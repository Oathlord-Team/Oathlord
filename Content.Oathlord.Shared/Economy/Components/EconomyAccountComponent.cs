using Content.Oathlord.Shared.Economy.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Economy.Components;

/// <summary>
/// Component that is applied to the player once they spawn as a job.
/// Allows them to be part of the economy, by initializing a new bank account in all maps with <see cref="EconomyMapComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class EconomyAccountComponent : Component
{
    /// <summary>
    /// The current amount of currency this entity has stored in the bank.
    /// For our purposes, this variable is considered the lowest tier currency (which is copper aka Na).
    /// However, it can be converted into silver and gold via other functions (100 copper = 1 silver, 100 silver = 1 gold).
    /// IMPORTANT: This indicates how much "ownership" the user has over the bank's vault/treasury! It does not act as a mini-bank.
    ///
    /// This is a <see cref="int"/> because float values generally add an extra layer of complexity we don't need.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Stored;

    /// <summary>
    /// A list of loans this account owns.
    /// Check <see cref="LoanData"/> for more information about what is considered a loan in the economy.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<LoanData> Loans = new();

    /// <summary>
    /// A list of transactions this entity has done. Used as a way to track their transaction history.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<TransactionData> Transactions = new();
}

/// <summary>
/// A loan can be granted by the economy in order to gain extra money, however they must be paid within a specified timeframe.
/// The final loan to be paid is calculated like this:
///
/// <see cref="Amount"/> + (<see cref="Amount"/> * <see cref="EconomyMapComponent.LoanInterest"/>)
/// </summary>
[Serializable, NetSerializable, DataRecord]
public partial record struct LoanData
{
    /// <summary>
    /// todo: this should be an in-game day once we add those, not timespan...
    /// Specifies a timeframe in which we should pay this loan
    /// This does not really have any use case, but in-game it should act as a crime to not pay your loans in time
    /// </summary>
    public TimeSpan DueTime;

    /// <summary>
    /// How much this loan is in the economy's main currency
    /// This does not include the final amount with the interest rate
    /// </summary>
    public int Amount;

    /// <summary>
    /// Whether this loan is paid, or not
    /// </summary>
    public bool Paid;

    /// <summary>
    /// The reason as to why this loan exists. Appears on the bank machine's UI for the specified account
    /// It can be nullable because it is optional to specify one
    /// </summary>
    public string? Reason;
}

[Serializable, NetSerializable, DataRecord]
public partial record struct TransactionData
{
    /// <summary>
    /// The amount that was present in this transaction
    /// </summary>
    public int Amount;

    /// <summary>
    /// The entity that initiated this transaction
    /// </summary>
    public string Initiator = "Unknown";

    /// <summary>
    /// The type of transaction it was
    /// </summary>
    public EconomyTransaction Type = EconomyTransaction.Deposit;
}
