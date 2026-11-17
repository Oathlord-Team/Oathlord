using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Oathlord.Client.Spellcasting;
using Content.Oathlord.Client.UserInterface.Systems.Spells.Controls;
using Content.Oathlord.Client.UserInterface.Systems.Spells.Widgets;
using Content.Oathlord.Client.UserInterface.Systems.Spells.Windows;
using Content.Oathlord.Common.Input;
using Content.Oathlord.Shared.Spellcasting.Components;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;

namespace Content.Oathlord.Client.UserInterface.Systems.Spells;

[UsedImplicitly]
public sealed partial class SpellsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<ClientSpellcastingSystem>
{
    [Dependency] private IPlayerManager _player = default!;
    [UISystemDependency] private readonly ClientSpellcastingSystem _spellcasting = default!;

    private EntityQuery<SpellsComponent> _spellsQuery;

    /// <summary>
    /// Exists to prevent ghosts or non-spell users opening the window with the keybind
    /// </summary>
    private bool _canUseSpells = true;

    private SpellsWindow? _window;
    private SpellsButton? UI => UIManager.GetActiveUIWidgetOrNull<SpellsButton>();

    /// <summary>
    /// Event that gets triggered when the user clicks on a spell slot, in order to transfer it to active or learned slots.
    /// </summary>
    public Action<EntityUid, int>? TransferSpellRequest;

    public void OnStateEntered(GameplayState state)
    {
        _window = UIManager.CreateWindow<SpellsWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.Center);

        var builder = CommandBinds.Builder;
        builder
            .Bind(OathlordKeyFunctions.OpenSpellsMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<SpellsUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        CommandBinds.Unregister<SpellsUIController>();
    }

    public override void Initialize()
    {
        base.Initialize();

        _spellsQuery = EntityManager.GetEntityQuery<SpellsComponent>();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += LoadGui;
        gameplayStateLoad.OnScreenUnload += UnloadGui;
    }

    public void LoadGui()
    {
        if (UI == null)
            return;

        UI.SelectedSpellSlot.SpellTexButton.OnPressed += SpellsButtonOnOnPressed;
    }

    public void UnloadGui()
    {
        if (UI == null)
            return;

        UI.SelectedSpellSlot.SpellTexButton.OnPressed -= SpellsButtonOnOnPressed;
    }

    public void OnSystemLoaded(ClientSpellcastingSystem system)
    {
        system.SelectedSpellChanged += OnSelectedSpellChanged;
        system.UpdateSpellWindow += SystemOnUpdateSpellWindow;
        system.EnableSpells += SystemOnEnableSpells;
        system.DisableSpells += SystemOnDisableSpells;
    }

    public void OnSystemUnloaded(ClientSpellcastingSystem system)
    {
        system.SelectedSpellChanged -= OnSelectedSpellChanged;
        system.UpdateSpellWindow -= SystemOnUpdateSpellWindow;
        system.EnableSpells -= SystemOnEnableSpells;
        system.DisableSpells -= SystemOnDisableSpells;
    }

    private void SystemOnUpdateSpellWindow(object? sender, EventArgs e)
    {
        UpdateWindow();
    }

    private void SystemOnEnableSpells(object? sender, EventArgs e)
    {
        UI?.Visible = true;
        _canUseSpells = true;
        UpdateWindow();
    }

    private void SystemOnDisableSpells(object? sender, EventArgs e)
    {
        UI?.Visible = false;
        _canUseSpells = false;
    }

    private void OnSelectedSpellChanged(object? sender, int spellIndex)
    {
        if (UI == null || _player.LocalEntity is not { } player)
            return;

        var activeSpell = _spellcasting.GetActiveSpell(player);
        UI.UpdateSpellWidget(activeSpell);
    }

    private void SpellsButtonOnOnPressed(BaseButton.ButtonEventArgs obj)
    {
        ToggleWindow();
    }

    public void UpdateWindow()
    {
        if (_window == null || _player.LocalEntity is not { } player || !_spellsQuery.TryComp(player, out var spells))
            return;

        var maxLearned = spells.MaxLearned;
        var currentSlots = spells.CurrentSlots;
        var learnedSpellContainer = _window.LearnedSpellsContainer;
        var activeSpellContainer = _window.ActiveSpellsContainer;

        SetupSpells(learnedSpellContainer, maxLearned);
        SetupSpells(activeSpellContainer, currentSlots, active: true);

        AddSpellsToContainers(learnedSpellContainer, player, active: false);
        AddSpellsToContainers(activeSpellContainer, player, active: true);
    }

    private void ToggleWindow()
    {
        if (!_canUseSpells || _window == null)
            return;

        if (_window.IsOpen)
        {
            _window.Close();
            return;
        }

        _window = UIManager.CreateWindow<SpellsWindow>();
        UpdateWindow();

        _window.Open();
    }

    private void SetupSpells(Control parent, int amount, bool active = false)
    {
        parent.Children.Clear();
        for (int i = 0; i < amount; i++)
        {
            var spell = new SpellSlot();
            spell.Active = active;
            parent.AddChild(spell);

            spell.SpellTexButton.OnPressed += _ => SpellTexButtonOnOnPressed(spell);
        }
    }

    private void AddSpellsToContainers(Control container, EntityUid user, bool active)
    {
        var spells = _spellcasting.GetSpells(user, active);
        foreach (var spell in spells)
        {
            foreach (var spellControl in container.Children)
            {
                if (spellControl is not SpellSlot spellSlot || spellSlot.Spell != null)
                    continue;

                spellSlot.AddSpell(spell);
                break;
            }
        }
    }

    /// <summary>
    /// Moves a spell from learned to active, and vice-versa
    /// </summary>
    private void SpellTexButtonOnOnPressed(SpellSlot slot)
    {
        if (_window is not { } window)
            return;

        // If the spell is learned, we move it to the active section
        if (!slot.Active)
        {
            TransferSpell(slot, window.ActiveSpellsContainer, SpellTransfer.Active);
            return;
        }

        // If the spell is active, we move it to learned...
        TransferSpell(slot, window.LearnedSpellsContainer, SpellTransfer.Learned);
    }

    /// <summary>
    /// Transfers a <see cref="SpellSlot"/>'s spell from the first available spell slot in another container.
    /// The other spell slot must not have a spell.
    /// </summary>
    private void TransferSpell(SpellSlot from, Control container, SpellTransfer type)
    {
        if (from.Spell is not { } fromSpell)
            return;

        SpellSlot? selected = null;
        foreach (var child in container.Children)
        {
            // We don't want to overwrite the spell on this slot, so just skip
            if (child is not SpellSlot spellSlot || spellSlot.Spell != null)
                continue;

            selected = spellSlot;
            break;
        }

        if (selected is not { } selectedSlot)
            return;

        var netEnt = EntityManager.GetNetEntity(fromSpell);
        var ev = new SpellTransferEvent(netEnt, type);
        EntityManager.RaisePredictiveEvent(ev);
        if (ev.Cancelled)
            return;

        selectedSlot.AddSpell(fromSpell);
        from.RemoveSpell();
    }
}
