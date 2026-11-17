using Robust.Shared.Prototypes;

namespace Content.Oathlord.Common.Oaths;

/// <summary>
/// A common system for oaths, since it is required for the humanoid profile editor
/// </summary>
public abstract partial class CommonOathSystem : EntitySystem
{
    /// <summary>
    /// Applies an oath to the user's brain
    /// </summary>
    /// <param name="target">The user</param>
    /// <param name="oath">The oath to apply</param>
    public abstract void ApplyOath(EntityUid target, [ForbidLiteral] ProtoId<OathPrototype> oath);
}
