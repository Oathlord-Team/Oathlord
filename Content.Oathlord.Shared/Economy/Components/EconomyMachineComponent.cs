using Content.Oathlord.Shared.Economy.Prototypes;
using Content.Oathlord.Shared.Economy.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Economy.Components;

/// <summary>
/// Component that is used for objects, to allow withdrawing and depositing money via UI
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EconomyMachineComponent : Component
{
    /// <summary>
    /// Sound that plays when a transaction has been accepted
    /// </summary>
    [DataField]
    public SoundSpecifier SuccessSound = new SoundPathSpecifier("/Audio/_Oathlord/Economy/success_transaction.ogg");

    /// <summary>
    /// Sound that plays when a transaction has been denied
    /// </summary>
    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/_Oathlord/Economy/fail_transaction.ogg");

    /// <summary>
    /// Sound that plays when withdrawing money from an account
    /// </summary>
    [DataField]
    public SoundSpecifier WithdrawSound = new SoundCollectionSpecifier("WithdrawSounds");
}

[Serializable, NetSerializable]
public sealed class EconomyTransactionMessage : BoundUserInterfaceMessage
{
    /// <summary>
    /// The entity that is the target of this transaction
    /// </summary>
    public NetEntity? TransactEntity;

    /// <summary>
    /// The amount of currencies to withdraw from this account.
    /// </summary>
    public Dictionary<ProtoId<EconomyCurrencyPrototype>, int> ToTransact;

    /// <summary>
    /// The type of transaction we're commiting (e.g. withdraw, depositing)
    /// </summary>
    public EconomyTransaction Type;

    public EconomyTransactionMessage(NetEntity? transactEntity, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> toTransact, EconomyTransaction type)
    {
        TransactEntity = transactEntity;
        ToTransact = toTransact;
        Type = type;
    }
}

[Serializable, NetSerializable]
public sealed class EconomyAddLoanMessage : BoundUserInterfaceMessage
{
    /// <summary>
    /// The entity that is the target of this transaction
    /// </summary>
    public NetEntity? TransactEntity;

    /// <summary>
    /// The loan to add to this account
    /// </summary>
    public LoanData Loan;

    public EconomyAddLoanMessage(LoanData loan, NetEntity? transactEntity)
    {
        TransactEntity = transactEntity;
        Loan = loan;
    }
}

[Serializable, NetSerializable]
public sealed class EconomyPayLoanMessage : BoundUserInterfaceMessage
{
    /// <summary>
    /// The entity that is the target of this transaction
    /// </summary>
    public NetEntity? TransactEntity;

    /// <summary>
    /// The loan to pay from this account
    /// </summary>
    public LoanData Loan;

    public EconomyPayLoanMessage(LoanData loan, NetEntity? transactEntity)
    {
        TransactEntity = transactEntity;
        Loan = loan;
    }
}


[Serializable, NetSerializable]
public enum EconomyMachineUiKey : byte
{
    Key,
}
