using Content.Oathlord.Shared.Economy.Components;
using Content.Shared.Popups;

namespace Content.Oathlord.Shared.Economy.Systems;

/// <summary>
/// Handles BUI events for the economy machine (depositing, withdrawing etc).
/// </summary>
public sealed partial class OathlordEconomySystem
{
    private void InitializeMachine()
    {
        Subs.BuiEvents<EconomyMachineComponent>(EconomyMachineUiKey.Key,
            subs =>
            {
                subs.Event<EconomyTransactionMessage>(OnTransaction);
                subs.Event<EconomyAddLoanMessage>(OnAddLoan);
                subs.Event<EconomyPayLoanMessage>(OnPayLoan);
            });
    }

    #region Event Handlers

    private void OnTransaction(Entity<EconomyMachineComponent> ent, ref EconomyTransactionMessage args)
    {
        switch (args.Type)
        {
            case EconomyTransaction.Withdraw:
                Withdraw(ent, args);
                break;
            case EconomyTransaction.Deposit:
                Deposit(ent, args);
                break;
        }
    }

    private void OnAddLoan(Entity<EconomyMachineComponent> ent, ref EconomyAddLoanMessage args)
    {
        if (GetTransactEntity(args.TransactEntity) is not { } user)
            return;

        AddLoan(user, args.Loan);
    }

    private void OnPayLoan(Entity<EconomyMachineComponent> ent, ref EconomyPayLoanMessage args)
    {
        if (GetTransactEntity(args.TransactEntity) is not { } user|| args.Actor is not { Valid: true } actor)
            return;

        if (!TryPayLoan(user, args.Loan))
        {
            _popup.PopupCursor("Denied loan payment. Is there enough money in the account?", actor, PopupType.SmallCaution);
            _audio.PlayPredicted(ent.Comp.FailSound, ent, actor);

            return;
        }

        _audio.PlayPredicted(ent.Comp.SuccessSound, ent, actor);
        _popup.PopupCursor("The loan has been successfully paid!", actor, PopupType.Medium);
    }

    #endregion

    private void Withdraw(Entity<EconomyMachineComponent> ent, EconomyTransactionMessage args)
    {
        if (GetTransactEntity(args.TransactEntity) is not { } user || args.Actor is not { Valid: true } actor)
            return;

        var toTransact = args.ToTransact;
        if (!WithdrawFromAccount(user, toTransact, actor))
        {
            _popup.PopupCursor("Transaction denied. Invalid input, or not enough money in the bank.", actor, PopupType.SmallCaution);
            _audio.PlayPredicted(ent.Comp.FailSound, ent, actor);

            return;
        }

        SpawnPhysicalFromCurrencies(ent.Owner, toTransact);

        _popup.PopupCursor("Transaction successful. The currencies should appear on the ground!", actor, PopupType.Medium);
        _audio.PlayPredicted(ent.Comp.WithdrawSound, ent, actor);
    }

    private void Deposit(Entity<EconomyMachineComponent> ent, EconomyTransactionMessage args)
    {
        if (GetTransactEntity(args.TransactEntity) is not { } user || args.Actor is not { Valid: true } actor)
            return;

        if (!DepositToAccount(user, args.ToTransact, actor))
        {
            _popup.PopupCursor("Transaction denied. Invalid input, or not enough money in the bank.", actor, PopupType.SmallCaution);
            _audio.PlayPredicted(ent.Comp.FailSound, ent, actor);

            return;
        }

        _popup.PopupCursor("Transaction successful. The account has been updated!", actor, PopupType.Medium);
        _audio.PlayPredicted(ent.Comp.SuccessSound, ent, actor);
    }

    /// <summary>
    /// Boilerplate to convert the transact entity from NetEntity to EntityUid
    /// </summary>
    private EntityUid? GetTransactEntity(NetEntity? entity)
    {
        if (entity is not { } withdrawEntity)
            return null;

        return GetEntity(withdrawEntity);
    }
}
