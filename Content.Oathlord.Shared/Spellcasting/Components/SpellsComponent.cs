using Content.Oathlord.Shared.Spellcasting.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Spellcasting.Components;

/// <summary>
/// Component that handles spellcasting for the user.
/// It holds all learned spells, and active spells in a container on the entity.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SpellcastingSystem))]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class SpellsComponent : Component
{
    /// <summary>
    /// The current selected spell
    /// It is an index pointing at <see cref="Container.ContainedEntities"/>
    /// </summary>
    [DataField, AutoNetworkedField]
    public int SelectedSpell;

    /// <summary>
    /// How many active slots we currently have
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentSlots = 3;

    /// <summary>
    /// How many learned slots we are allowed to have
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxLearned = 10;

    public const string ContainerId = "spells";

    /// <summary>
    /// Container that holds all spells on the entity
    /// </summary>
    [ViewVariables]
    public Container Container = default!;
}

/// <summary>
/// Raised from the client before transferring a spell from learned to active, or vice-versa
/// </summary>
/// <param name="spell">The spell to transfer</param>
/// <param name="type">What transfer is it</param>
/// <param name="cancelled">Whether the transfer was cancelled</param>
[Serializable, NetSerializable]
public sealed class SpellTransferEvent(NetEntity spell, SpellTransfer type, bool cancelled = false) : EntityEventArgs
{
    public NetEntity Spell = spell;
    public SpellTransfer Type = type;
    public bool Cancelled = cancelled;
};

/// <summary>
/// Spell users have 2 categories, learned spells and active spells.
/// Transferring spells is done between those categories.
/// </summary>
/// <remarks>https://www.teamten.com/lawrence/programming/prefer-enums-over-booleans.html</remarks>
[NetSerializable, Serializable]
public enum SpellTransfer : byte
{
    Learned,
    Active,
}

/// <summary>
/// Enum that defines how to move the <see cref="SpellsComponent.SelectedSpell"/>
/// </summary>
[NetSerializable, Serializable]
public enum SpellMove : byte
{
    Up,
    Down,
}
