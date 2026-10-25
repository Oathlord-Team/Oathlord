using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.MenuBar;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Oathlord.Client.UserInterface.Systems.CharAttributes.Widgets;
using Content.Oathlord.Client.UserInterface.Systems.CharAttributes.Windows;
using Content.Oathlord.Common.Input;
using Content.Oathlord.Shared.CharacterAttributes;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;

namespace Content.Oathlord.Client.UserInterface.Systems.CharAttributes;

[UsedImplicitly]
public sealed partial class CharacterAttributesUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private IPlayerManager _player = default!;

    private CharacterAttributeWindow? _menu;
    private CharAttributeMenuButton? _charButton;

    public override void Initialize()
    {
        base.Initialize();

        GameTopMenuBarUIController.OnLoad += OnLoad;
        GameTopMenuBarUIController.OnUnload += OnUnload;
    }

    private void OnUnload(GameTopMenuBar bar)
    {
        UnloadButton(bar);
    }

    private void OnLoad(GameTopMenuBar bar)
    {
        LoadButton(bar);
    }

    public void OnStateEntered(GameplayState state)
    {
        CommandBinds.Builder.Bind(OathlordKeyFunctions.OpenAttributesMenu,
                InputCmdHandler.FromDelegate(_ => ToggleCharAttributeMenu()))
            .Register<CharacterAttributesUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<CharacterAttributesUIController>();
    }

    private void LoadButton(GameTopMenuBar bar)
    {
        _charButton = new CharAttributeMenuButton();
        _menu = new CharacterAttributeWindow();

        var btn = _charButton.CharAttributeButton;

        _charButton.RemoveChild(btn);

        btn.OnPressed += OnPressed;
        bar.AddChild(btn);
        btn.SetPositionLast();

        _menu.OnClose += OnMenuClosed;
        _menu.OnOpen += OnMenuOpened;
        _menu.ApplyChanges += MenuOnApplyChanges;
    }

    private void UnloadButton(GameTopMenuBar bar)
    {
        foreach (var child in bar.Children)
        {
            if (child == _charButton?.CharAttributeButton)
            {
                bar.RemoveChild(child);
                break;
            }
        }

        if (_menu is not null)
        {
            _menu.Close();
            _menu.OnClose -= OnMenuClosed;
            _menu.OnOpen -= OnMenuOpened;
            _menu.ApplyChanges -= MenuOnApplyChanges;

            _menu = null;
        }

        _charButton?.CharAttributeButton.OnPressed -= OnPressed;
    }

    private void OnPressed(BaseButton.ButtonEventArgs obj)
    {
        ToggleCharAttributeMenu();
    }

    private void ToggleCharAttributeMenu()
    {
        if (_menu == null)
            return;

        if (_menu.IsOpen)
        {
            _menu.Close();
            return;
        }

        _menu.OpenCenteredRight();
        
        PopulateMenu();
    }


    private void MenuOnApplyChanges(Dictionary<NetEntity, int> changedPoints)
    {
        var ev = new ApplyCharacterAttributesEvent(changedPoints);
        EntityManager.RaisePredictiveEvent(ev);

        PopulateMenu();
    }

    private void PopulateMenu()
    {
        if (_player.LocalEntity is not { } player)
            return;

        _menu?.Populate(player, EntityManager);
    }

    private void OnMenuClosed()
    {
        if (_charButton is not { } charBtn)
            return;

        charBtn.CharAttributeButton.SetClickPressed(false);
    }

    private void OnMenuOpened()
    {
        if (_charButton is not { } charBtn)
            return;

        charBtn.CharAttributeButton.SetClickPressed(true);
    }
}
