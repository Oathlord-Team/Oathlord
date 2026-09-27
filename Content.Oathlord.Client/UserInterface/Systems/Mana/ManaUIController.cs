using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Oathlord.Client.Mana;
using Content.Oathlord.Client.UserInterface.Systems.Mana.Widgets;
using Content.Oathlord.Shared.Mana;
using Content.Shared.FixedPoint;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Oathlord.Client.UserInterface.Systems.Mana;

public sealed partial class ManaUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<ClientManaSystem>
{
    [Dependency] private IPlayerManager _player = default!;
    [UISystemDependency] private readonly ClientManaSystem _clientMana = default!;

    private EntityQuery<ManaUserComponent> _manaQuery = default!;

    public ManaBar? UI => UIManager.GetActiveUIWidgetOrNull<ManaBar>();

    public override void Initialize()
    {
        base.Initialize();

        _manaQuery = EntityManager.GetEntityQuery<ManaUserComponent>();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        SyncMana();
    }

    public void OnSystemLoaded(ClientManaSystem system)
    {
        system.SyncMana += SystemOnSyncMana;
        system.EnableMana += SystemOnEnableMana;
        system.DisableMana += SystemOnDisableMana;
    }

    public void OnSystemUnloaded(ClientManaSystem system)
    {
        system.SyncMana -= SystemOnSyncMana;
        system.EnableMana -= SystemOnEnableMana;
        system.DisableMana -= SystemOnDisableMana;
    }

    private void SystemOnDisableMana(object? sender, EventArgs e)
    {
        UI?.Visible = false;
    }

    private void SystemOnEnableMana(object? sender, EventArgs e)
    {
        UI?.Visible = true;
    }

    public void OnStateEntered(GameplayState state)
    {
        SyncMana();
    }

    private void SystemOnSyncMana(object? sender, (FixedPoint2 current, FixedPoint2 max, bool canUse) mana)
    {
        UI?.SyncMana(mana.current, mana.max, mana.canUse);
    }

    public void SyncMana()
    {
        if (_player.LocalEntity is not { } player)
            return;

        if (!_manaQuery.TryComp(player, out var mana))
        {
            SystemOnSyncMana(_clientMana, (1, 1, false));
            return;
        }

        var manaUser = (player, mana);

        var current = _clientMana.GetMana(manaUser);
        var max = _clientMana.GetMaxMana(manaUser);
        var canUse = _clientMana.CanUseMana(manaUser);
        SystemOnSyncMana(_clientMana, (current, max, canUse));
    }
}
