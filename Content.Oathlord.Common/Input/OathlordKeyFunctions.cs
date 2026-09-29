using Robust.Shared.Input;

namespace Content.Oathlord.Common.Input;

[KeyFunctions]
public static class OathlordKeyFunctions
{
    #region Activation

    public static readonly BoundKeyFunction SpecialItemAction = "SpecialItemAction";

    #endregion

    #region Spellcasting

    public static readonly BoundKeyFunction OpenSpellsMenu = "OpenSpellsMenu";
    public static readonly BoundKeyFunction MoveSpellUp = "MoveSpellUp";
    public static readonly BoundKeyFunction MoveSpellDown = "MoveSpellDown";

    #endregion

    #region Movement

    public static readonly BoundKeyFunction MovementMod = "MovementMod";

    #endregion
}
