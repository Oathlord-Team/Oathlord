using Content.Client.Lobby.UI;

namespace Content.Client.Lobby;

public partial class LobbyUIController
{
    public static event Action<HumanoidProfileEditor>? OnProfileEditorCreated;
}
