// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Oathlord.Common.Input;
using Content.Shared.CombatMode;
using Content.Shared.Popups;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Oathlord.Shared.Combat;

/// <summary>
/// System that handles Oathlord's changes to how combat mode functions.
///
/// Oathlord uses a keybind instead of an action for toggling combat mode,
/// due to action combat mode having a lot of problems, like getting messed up in order.
/// </summary>
public sealed partial class OathlordCombatModeSystem : EntitySystem
{
    [Dependency] private SharedCombatModeSystem _combat = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [Dependency] private EntityQuery<CombatModeComponent> _combatModeQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(OathlordKeyFunctions.ToggleCombatMode,
                InputCmdHandler.FromDelegate(ToggleCombatMode, handle: false, outsidePrediction: false))
            .Register<OathlordCombatModeSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<OathlordCombatModeSystem>();
    }

    private void ToggleCombatMode(ICommonSession? session)
    {
        if (session?.AttachedEntity is not { Valid: true } attachedEnt || !_combatModeQuery.TryComp(attachedEnt, out var combatMode))
            return;

        ToggleCombatMode((attachedEnt, combatMode));
    }

    private void ToggleCombatMode(Entity<CombatModeComponent> ent)
    {
        var toggle = !ent.Comp.IsInCombatMode;
        _combat.SetInCombatMode(ent, toggle, ent.Comp);

        var msg = ent.Comp.IsInCombatMode ? "action-popup-combat-enabled" : "action-popup-combat-disabled";
        _popup.PopupEntity(Loc.GetString(msg), ent, ent);
    }
}
