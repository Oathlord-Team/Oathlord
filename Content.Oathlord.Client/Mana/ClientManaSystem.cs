using Content.Oathlord.Shared.Mana;
using Content.Shared.FixedPoint;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Oathlord.Client.Mana;

public sealed partial class ClientManaSystem : ManaSystem
{
    [Dependency] private IPlayerManager _player = default!;

    public event EventHandler<(FixedPoint2, FixedPoint2, bool)>? SyncMana;
    public event EventHandler? EnableMana;
    public event EventHandler? DisableMana;

    // TODO: Add LocalPlayerAttached and Detached events and show/hide the mana widget in the respective event

    [SubscribeLocalEvent]
    private void OnPlayerAttached(Entity<ManaUserComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (_player.LocalEntity == ent.Owner)
        {
            EnableMana?.Invoke(this, EventArgs.Empty);
            SyncMana?.Invoke(this, (ent.Comp.CurrentMana, ent.Comp.MaxMana, ent.Comp.CanUse));
        }
    }

    [SubscribeLocalEvent]
    private void OnPlayerDetached(Entity<ManaUserComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        DisableMana?.Invoke(this, EventArgs.Empty);
    }

    [SubscribeLocalEvent]
    private void OnAfterAutoHandle(Entity<ManaUserComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        // This is mostly needed for stuff that is being called in the server (like a command)
        UpdateHud(ent);
    }

    [SubscribeLocalEvent]
    private void OnRemove(Entity<ManaUserComponent> ent, ref ComponentRemove args)
    {
        if (_player.LocalEntity == ent.Owner)
            SyncMana?.Invoke(this, (1, 1, false)); // to make the bar look "unavailable" in the ui
    }

    protected override void UpdateHud(Entity<ManaUserComponent> ent)
    {
        if (_player.LocalEntity == ent.Owner)
            SyncMana?.Invoke(this, (ent.Comp.CurrentMana, ent.Comp.MaxMana, ent.Comp.CanUse));
    }
}
