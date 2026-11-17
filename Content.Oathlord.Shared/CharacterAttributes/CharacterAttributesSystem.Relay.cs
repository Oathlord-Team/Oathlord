// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Melee.Events;

namespace Content.Oathlord.Shared.CharacterAttributes;

public sealed partial class CharacterAttributesSystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<CharacterAttributesComponent, GetMeleeDamageEvent>(RefRelayCharAttributeEvent);
    }

    private void RefRelayCharAttributeEvent<T>(
        EntityUid uid,
        CharacterAttributesComponent component,
        ref T args) where T : struct
    {
        RelayEvent((uid, component), ref args);
    }

    private void RelayCharAttributeEvent<T>(
        EntityUid uid,
        CharacterAttributesComponent component,
        ref T args) where T : class
    {
        RelayEvent((uid, component), args);
    }

    private void RelayEvent<T>(Entity<CharacterAttributesComponent> ent, ref T args) where T : struct
    {
        var ev = new CharAttributeRelayedEvent<T>(args);
        foreach (var attr in GetAttributes(ent.AsNullable()))
        {
            RaiseLocalEvent(attr, ref ev);
        }

        args = ev.Args;
    }

    private void RelayEvent<T>(Entity<CharacterAttributesComponent> ent, T args) where T : class
    {
        var ev = new CharAttributeRelayedEvent<T>(args);
        foreach (var attr in GetAttributes(ent.AsNullable()))
        {
            RaiseLocalEvent(attr, ref ev);
        }
    }
}

/// <summary>
/// Relay event used for character attribute entities (e.g. strength) for relaying events to them.
/// </summary>
[ByRefEvent]
public record struct CharAttributeRelayedEvent<TEvent>(TEvent Args);
