using Content.Shared.Preferences;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    /// <summary>
    /// Raised when the profile is being set,
    /// so we can update our stuff without touching upstream
    /// </summary>
    public event Action<HumanoidCharacterProfile?>? OnSetProfile;
}
