namespace Content.Shared.CombatMode;

public sealed partial class CombatModeComponent
{
    /// <summary>
    /// If this is true, it means you can toggle combat mode.
    /// Otherwise, you are not able to toggle it (e.g. pacified).
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanToggle = true;
}
