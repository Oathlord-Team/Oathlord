using Content.Oathlord.Common.Input;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Oathlord.Shared.ItemSpecial;

/// <summary>
/// System that handles the input interaction of SpecialItemAction keybind.
///
/// In terms of what it actually does, it basically acts like the ActivateInHand keybind, except it's not as complex.
/// This should be used for items that should have special interactions, such as spellcasting items, or melee weapons with special abilities.
/// </summary>
public sealed partial class ItemSpecialSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(OathlordKeyFunctions.SpecialItemAction, new PointerInputCmdHandler(HandleSpecialAction))
            .Register<ItemSpecialSystem>();
    }

    private bool HandleSpecialAction(ICommonSession? playerSession, EntityCoordinates coordinates, EntityUid entity)
    {
        if (playerSession?.AttachedEntity is not {Valid: true} player || !Exists(player) || !coordinates.IsValid(EntityManager))
            return true;

        TryItemSpecial(player, coordinates, entity);
        return false;
    }

    #region Public Api

    /// <summary>
    /// Tries to do an item special interaction
    /// </summary>
    /// <param name="uid">The user of the interaction</param>
    /// <param name="coords">The coordinates where the click interaction happened</param>
    /// <param name="target">The target of this interaction</param>
    public void TryItemSpecial(EntityUid uid, EntityCoordinates coords, EntityUid target)
    {
        if (!_hands.TryGetActiveItem(uid, out var item) || item is not { } activeItem)
            return;

        // todo: check for other blockers (probably not use delay?)
        DoItemSpecial(uid, activeItem, coords, target);
    }

    /// <summary>
    /// Does an item special interaction
    /// </summary>
    /// <param name="user">The user of the interaction</param>
    /// <param name="used">The item that is part of the interaction</param>
    /// <param name="coords">The coordinates where the click interaction happened</param>
    /// <param name="target">The target of this interaction</param>
    public void DoItemSpecial(EntityUid user, EntityUid used, EntityCoordinates coords, EntityUid target)
    {
        if (TerminatingOrDeleted(user) || TerminatingOrDeleted(used))
            return;

        var ev = new ItemSpecialEvent(user, coords, target.Valid ? target : null);
        RaiseLocalEvent(used, ref ev);
    }

    #endregion
}

/// <summary>
/// Raised on the item when the user triggers the keybind for doing an item special
/// </summary>
[ByRefEvent]
public record struct ItemSpecialEvent(EntityUid User, EntityCoordinates Coords, EntityUid? Target);
