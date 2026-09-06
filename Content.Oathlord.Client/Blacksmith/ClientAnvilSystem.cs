using Content.Oathlord.Shared.Blacksmith.Anvil;

namespace Content.Oathlord.Client.Blacksmith;

public sealed partial class ClientAnvilSystem : AnvilSystem
{
    /// <summary>
    /// Updates the window
    /// </summary>
    public event EventHandler? UpdateWindow;

    [SubscribeLocalEvent]
    public void OnAutoHandleState(Entity<AnvilComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateWindow?.Invoke(this, EventArgs.Empty);
    }

    protected override void UpdateViews()
    {
        base.UpdateViews();

        UpdateWindow?.Invoke(this, EventArgs.Empty);
    }
}
