using Content.Oathlord.Shared.Economy.Components;
using Content.Oathlord.Shared.Economy.Prototypes;
using Content.Oathlord.Shared.Economy.Systems;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Oathlord.Client.Economy.UI;

[UsedImplicitly]
public sealed partial class EconomyMachineBoundInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private EconomyMachineWindow? _window;

    protected override void Open()
    {
        base.Open();

       _window = this.CreateWindow<EconomyMachineWindow>();
       _window.SetOwner(Owner);

       _window.Populate();

       _window.RequestTransaction += RequestTransaction;

       _window.RequestGrantLoan += RequestGrantLoan;
       _window.RequestPayLoan += RequestPayLoan;
    }

    private void RequestPayLoan(NetEntity? entity, LoanData loan)
    {
        SendPredictedMessage(new EconomyPayLoanMessage(loan, entity));
        _window?.UpdateInfo();
    }

    private void RequestGrantLoan(NetEntity? entity, LoanData loan)
    {
        SendPredictedMessage(new EconomyAddLoanMessage(loan, entity));
        _window?.UpdateInfo();
    }

    private void RequestTransaction(NetEntity? entity, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> currencies, EconomyTransaction type)
    {
        SendPredictedMessage(new EconomyTransactionMessage(entity, currencies, type));
        _window?.UpdateInfo();
    }
}
