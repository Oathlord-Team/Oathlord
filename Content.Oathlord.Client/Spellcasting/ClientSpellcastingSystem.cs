using Content.Oathlord.Shared.Spellcasting.Components;
using Content.Oathlord.Shared.Spellcasting.Systems;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Oathlord.Client.Spellcasting;

public sealed partial class ClientSpellcastingSystem : SpellcastingSystem
{
    [Dependency] private IPlayerManager _player = default!;

    public event EventHandler<int>? SelectedSpellChanged;
    public event EventHandler? UpdateSpellWindow;
    public event EventHandler? DisableSpells;
    public event EventHandler? EnableSpells;

    public EntityUid? ActiveSelectedSpell
    {
        get
        {
            var ent = _player.LocalEntity;
            return ent is { } player
                ? GetActiveSpell(player)
                : null;
        }
    }

    [SubscribeLocalEvent]
    public void OnAttached(Entity<SpellsComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (_player.LocalEntity == ent.Owner)
            EnableSpells?.Invoke(this, EventArgs.Empty);
    }

    [SubscribeLocalEvent]
    public void OnDetached(Entity<SpellsComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        DisableSpells?.Invoke(this, EventArgs.Empty);
    }

    protected override void UpdateUi(Entity<SpellsComponent> ent, bool refresh = false)
    {
        base.UpdateUi(ent, refresh);

        if (_player.LocalEntity != ent.Owner)
            return;

        SelectedSpellChanged?.Invoke(this, ent.Comp.SelectedSpell);

        if (refresh)
            UpdateSpellWindow?.Invoke(this, EventArgs.Empty);
    }
}
