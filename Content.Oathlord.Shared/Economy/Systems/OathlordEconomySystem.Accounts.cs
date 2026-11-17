using Content.Oathlord.Shared.Economy.Components;
using Content.Oathlord.Shared.Economy.Prototypes;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Economy.Systems;

/// <summary>
/// Public API for anything related to entities with <see cref="EconomyAccountComponent"/>
///
/// Accounts are part of the economy and rely on the bank for money.
///
/// Withdrawing will take money out of the bank (and therefore, reduce the amount stored on the user's account)
/// to give the player a physical version of the money.
///
/// Depositing will increase the account's stored money.
///
/// Accounts don't store individual coins like the economy's bank, rather they store one value.
/// Just like euros, as an example. Your account has 5000euros, not 10 * 50 + 9 * 500 euro bills.
/// But when withdrawing, you can withdraw however many bill types you want.
///
/// The economy holds the physical version of the currencies in their bank (vault/treasury).
/// </summary>
public sealed partial class OathlordEconomySystem
{
    /*
        P.S. I am not sure if accounts should be components tied to entities, but too lazy to refactor it now
    */

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<EconomyAccountComponent> ent, ref MapInitEvent args)
    {
        AddAccountToEconomy(ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<EconomyAccountComponent> ent, ref ComponentShutdown args)
    {
        RemoveAccountFromEconomy(ent.Owner);
    }

    #region Public API

    /// <summary>
    /// Adds a specified amount of currency to an account
    /// </summary>
    /// <param name="ent">The account to deposit to</param>
    /// <param name="amount">The amount to adjust</param>
    public void AddCurrencyToAccount(Entity<EconomyAccountComponent?> ent, int amount)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp) || amount <= 0)
            return;

        if (GetCurrentEconomy(ent.Owner) is not { } economy || amount > GetTotalEconomyStored(economy.AsNullable()))
            return;

        ent.Comp.Stored += amount;
        DirtyField(ent, nameof(EconomyAccountComponent.Stored));
    }

    /// <summary>
    /// Adds a specified amount of currency to an account, with the economy specified
    /// </summary>
    public void AddCurrencyToAccount(Entity<EconomyAccountComponent?> ent, int amount, EntityUid economy)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp) || amount <= 0)
            return;

        if (amount > GetTotalEconomyStored(economy))
            return;

        ent.Comp.Stored += amount;
        DirtyField(ent, nameof(EconomyAccountComponent.Stored));
    }

    /// <summary>
    /// Withdraws a specified amount of currencies from an account.
    /// </summary>
    /// <param name="ent">The account to withdraw from</param>
    /// <param name="toGive">The amount of currencies to withdraw from the economy, to give to the player</param>
    /// <returns>If the withdrawing was successful</returns>
    public bool WithdrawFromAccount(Entity<EconomyAccountComponent?> ent, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> toGive) =>
        WithdrawFromAccount(ent, toGive, null);

    /// <summary>
    /// Withdraws a specified amount of currency to an account.
    /// </summary>
    /// <param name="ent">The account to withdraw from</param>
    /// <param name="amount">The amount to withdraw, make sure its positive as negative values or zero won't work</param>
    public void WithdrawFromAccount(Entity<EconomyAccountComponent?> ent, int amount) =>
        WithdrawFromAccount(ent, amount, null);

    /// <inheritdoc cref="WithdrawFromAccount(Entity{EconomyAccountComponent?}, Dictionary{ProtoId{EconomyCurrencyPrototype}, int})"/>
    public bool WithdrawFromAccount(Entity<EconomyAccountComponent?> ent, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> toGive, EntityUid? initiator)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        var total= GetTotalFromCurrencies(ent.Comp.Stored, toGive);
        if (total == 0)
            return false;

        // Accounts don't store individual currencies, only economy does.
        // Unlike depositing, withdrawing automatically also withdraws from the vault and gives you the physical coins directly (subject to change).
        if (GetCurrentEconomy(ent.Owner) is not { } economy || !TryWithdrawFromEconomy(economy.AsNullable(), toGive))
            return false;

        WithdrawFromAccount(ent, total, initiator);

        return true;
    }

    /// <inheritdoc cref="WithdrawFromAccount(Entity{EconomyAccountComponent?}, int)"/>
    public void WithdrawFromAccount(Entity<EconomyAccountComponent?> ent, int amount, EntityUid? initiator)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp) || amount <= 0)
            return;

        ent.Comp.Stored = Math.Clamp(ent.Comp.Stored - amount, 0, int.MaxValue); // TODO: There should be a cap instead of int.MaxValue
        DirtyField(ent, nameof(EconomyAccountComponent.Stored));

        if (initiator is not { } initiatorEnt)
            return;

        AddTransaction(ent, EconomyTransaction.Withdraw, amount, initiatorEnt);
    }

    /// <summary>
    /// Deposits a specified amount of currencies to an account.
    /// </summary>
    /// <param name="ent">The account to deposit to</param>
    /// <param name="toDeposit">The amount of currencies to withdraw from the economy, to deposit to the account</param>
    public void DepositToAccount(Entity<EconomyAccountComponent?> ent, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> toDeposit) =>
        DepositToAccount(ent, toDeposit, null);

    /// <summary>
    /// Deposits a specified amount of currencies to an account.
    /// </summary>
    /// <param name="ent">The account to deposit to</param>
    /// <param name="toDeposit">The amount of currencies to withdraw from the economy, to deposit to the account</param>
    /// <param name="depositee">The entity that initiated this deposit (usually the operator of the bank machine)</param>
    public bool DepositToAccount(
        Entity<EconomyAccountComponent?> ent,
        Dictionary<ProtoId<EconomyCurrencyPrototype>, int> toDeposit,
        EntityUid? depositee)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        // Here, unlike withdrawing, we don't have to check for how much we have stored, since it doesn't matter (we're adding up)
        var total= GetTotalFromCurrencies(toDeposit);
        if (total == 0)
            return false;

        // Prevents depositing insanely large amounts of coins. We just make the cap be the economy's total stored...
        if (GetCurrentEconomy(ent.Owner) is not { } economy || total > GetTotalEconomyStored(economy.AsNullable()))
            return false;

        ent.Comp.Stored = Math.Clamp(ent.Comp.Stored + total, 0, int.MaxValue);
        DirtyField(ent, nameof(EconomyAccountComponent.Stored));

        if (depositee is not { } depositeeEnt)
            return false;

        AddTransaction(ent, EconomyTransaction.Deposit, total, depositeeEnt);
        return true;
    }

    #endregion

    #region Loan API

    /// <summary>
    /// Fetches all loans in a specified account
    /// </summary>
    public IEnumerable<LoanData>? GetLoans(Entity<EconomyAccountComponent?> ent)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp))
            return null;

        return ent.Comp.Loans;
    }

    /// <summary>
    /// Validates and adds a loan to an account
    /// </summary>
    /// <param name="ent">The account</param>
    /// <param name="loan">The loan to add to the account</param>
    public void AddLoan(Entity<EconomyAccountComponent?> ent, LoanData loan)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        if (loan.Paid || loan.Amount <= 0 || loan.DueTime <= TimeSpan.Zero)
            return;

        ent.Comp.Loans.Add(loan);
        DirtyField(ent, nameof(EconomyAccountComponent.Loans));

        // Now we own the loan's amount to our account
        AddCurrencyToAccount(ent, loan.Amount);
    }

    /// <summary>
    /// Attempts to pay a loan with the account's stored amount.
    /// It uses the economy's current interest rate to calculate the final price
    /// </summary>
    /// <param name="ent">The account</param>
    /// <param name="loan">The loan to pay from the entity's account</param>
    /// <returns>True if the loan was paid, false otherwise</returns>
    public bool TryPayLoan(Entity<EconomyAccountComponent?> ent, LoanData loan)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp) || GetCurrentEconomy(ent.Owner) is not { } economy)
            return false;

        if (loan.Paid || loan.Amount <= 0)
            return false;

        var loanIndex = ent.Comp.Loans.IndexOf(loan);
        if (loanIndex == -1)
            return false;

        var storedLoan = ent.Comp.Loans[loanIndex];
        var interest = GetLoanInterest(economy.AsNullable());

        var toPay = storedLoan.Amount + (int)(interest * storedLoan.Amount);

        if (toPay > ent.Comp.Stored || storedLoan.Paid)
            return false;

        storedLoan.Paid = true;
        ent.Comp.Loans[loanIndex] = storedLoan;
        DirtyField(ent, nameof(EconomyAccountComponent.Loans));

        WithdrawFromAccount(ent, toPay);
        return true;
    }

    #endregion

    #region Transaction API

    /// <summary>
    /// Adds a transaction to this account's transaction history
    /// </summary>
    /// <param name="ent">The account</param>
    /// <param name="type">The type of transaction</param>
    /// <param name="amount">The amount that played in this transaction</param>
    /// <param name="initiator">The initiator of this transaction</param>
    public void AddTransaction(Entity<EconomyAccountComponent?> ent, EconomyTransaction type, int amount, EntityUid initiator)
    {
        if (!_econAccountQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        var data = new TransactionData { Amount = amount, Initiator = Identity.Name(initiator, EntityManager), Type =  type };
        ent.Comp.Transactions.Add(data);
        DirtyField(ent, nameof(EconomyAccountComponent.Transactions));
    }

    #endregion
}
