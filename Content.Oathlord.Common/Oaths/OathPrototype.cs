using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Oathlord.Common.Oaths;

/// <summary>
/// Prototype for oaths, which are basically "alignments" which give special effects to the users.
/// </summary>
[Prototype]
public sealed partial class OathPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of the oath
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// The description of the oath
    /// </summary>
    [DataField(required: true)]
    public string Description = string.Empty;

    /// <summary>
    /// Whether the player can set this oath before the round starts, in the lobby.
    /// </summary>
    [DataField]
    public bool Preference = true;

    /// <summary>
    /// EntityEffectPrototype that will run on the user, when an oath has been chosen
    /// </summary>
    /// <remarks>
    /// This is a string because we can't access entity effects from common,
    /// and we need this in common for the profile editor
    /// </remarks>
    [DataField(required: true)]
    public string Effect = string.Empty;

    /// <summary>
    /// If set, this oath will run effects when the user switches off from it
    /// </summary>
    [DataField]
    public string? CurseEffect;

    /// <summary>
    /// The icon of the oath to display in the UI.
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    /// <summary>
    /// The background icon of the oath to display in the UI.
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Background = default!;
}
