using Robust.Shared.Configuration;

namespace Content.Oathlord.Common.CCVar;

[CVarDefs]
public sealed partial class OathlordCVars
{
    #region  Hitbox Combat

    /// <summary>
    /// The range which enlarges the hitbox's AABB search range for checking against lag-compensated targets, for hitbox prediction.
    /// </summary>
    public static readonly CVarDef<float> HitboxLagRange =
        CVarDef.Create("oathlord.hitbox_lag_range", 2.5f, CVar.SERVER);

    #endregion
}
