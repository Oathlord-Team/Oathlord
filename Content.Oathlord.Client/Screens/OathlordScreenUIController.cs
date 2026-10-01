using Content.Client.UserInterface.Screens;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Oathlord.Client.UserInterface.Systems.Mana;
using Content.Oathlord.Client.UserInterface.Systems.Mana.Widgets;
using Content.Oathlord.Client.UserInterface.Systems.ScrollingMovement;
using Content.Oathlord.Client.UserInterface.Systems.Spells;
using Content.Oathlord.Client.UserInterface.Systems.Spells.Widgets;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;

namespace Content.Oathlord.Client.Screens;

/// <summary>
/// If you need to add an element to the game's HUD, you came to the right place
/// For future reference, adding a UI element goes like this:
/// - Make a UI Widget
/// - Make a UI controller to handle updating your widget via GetActiveWidgetOrNull (this gets the active screen's widget)
/// - Add it here via the methods relating to widgets (RemoveWidget, GetOrAddWidget etc)
/// and don't forget to account for both separated (ss13) + default (ss14) screens
///
/// ps: I'd prefer doing an all gwyn ds1 run than working with ss14 ui
/// </summary>
[UsedImplicitly]
public sealed partial class OathlordScreenUIController : UIController
{
    [Dependency] private ManaUIController _mana = default!;
    [Dependency] private SpellsUIController _spells = default!;

    private ManaBar? _manaBar;
    private SpellsButton? _spellsButton;
    private ScrollingMovementWidget? _scrollingMovement;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;
    }

    private void OnScreenUnload()
    {
        _manaBar = null;
        _spellsButton = null;
        _scrollingMovement = null;

        _spells.UnloadGui();

        switch (UIManager.ActiveScreen)
        {
            case DefaultGameScreen screen:
                ClearWidgets(screen);
                break;
            case SeparatedChatGameScreen separated:
                ClearWidgets(separated);
                break;
        }
    }

    private void OnScreenLoad()
    {
        switch (UIManager.ActiveScreen)
        {
            case DefaultGameScreen screen:
                SetupMana(screen, false);
                SetupSpells(screen, false);
                SetupScrollingMovement(screen);
                break;
            case SeparatedChatGameScreen separated:
                SetupMana(separated, true);
                SetupSpells(separated, true);
                SetupScrollingMovement(separated);
                break;
        }

        // Registering events for widgets in the hud must be done in here, and not in the respective UI controllers
        // because the UI controllers usually request the active widget, which may not exist at that time.
        //
        // e.g. You can't click a button widget because ui controller couldn't register the event handler bcuz it was null at that time
        _spells.LoadGui();
    }

    #region Widget Setup

    private void SetupMana(InGameScreen screen, bool separated)
    {
        _manaBar = screen.GetOrAddWidget<ManaBar>();
        _mana.SyncMana();

        if (!separated)
        {
            LayoutContainer.SetAnchorAndMarginPreset(_manaBar, LayoutContainer.LayoutPreset.BottomWide, margin: 40);
            return;
        }

        LayoutContainer.SetAnchorAndMarginPreset(_manaBar, LayoutContainer.LayoutPreset.BottomWide, margin: 40);
        LayoutContainer.SetMarginLeft(_manaBar, -460);
    }

    private void SetupSpells(InGameScreen screen, bool separated)
    {
        _spellsButton = screen.GetOrAddWidget<SpellsButton>();

        if (!separated)
        {
            LayoutContainer.SetAnchorAndMarginPreset(_spellsButton, LayoutContainer.LayoutPreset.BottomWide, margin: 20);
            LayoutContainer.SetMarginLeft(_spellsButton, 450);
            return;
        }

        LayoutContainer.SetAnchorAndMarginPreset(_spellsButton, LayoutContainer.LayoutPreset.BottomWide, margin: 20);
        LayoutContainer.SetMarginLeft(_spellsButton, 200);
    }

    private void SetupScrollingMovement(InGameScreen screen)
    {
        _scrollingMovement = screen.GetOrAddWidget<ScrollingMovementWidget>();
    }

    #endregion

    private void ClearWidgets(InGameScreen screen)
    {
        screen.RemoveWidget<ManaBar>();
        screen.RemoveWidget<SpellsButton>();
        screen.RemoveWidget<ScrollingMovementWidget>();
    }
}
