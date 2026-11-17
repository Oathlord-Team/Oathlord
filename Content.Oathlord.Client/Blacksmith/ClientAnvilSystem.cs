// SPDX-License-Identifier: AGPL-3.0-or-later

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

    protected override void UpdateUi()
    {
        base.UpdateUi();
        UpdateWindow?.Invoke(this, EventArgs.Empty);
    }
}
