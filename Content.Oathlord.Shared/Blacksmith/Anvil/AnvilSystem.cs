using System.Linq;
using Content.Oathlord.Shared.Blacksmith.Anvil.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Oathlord.Shared.Blacksmith.Anvil;

/// <summary>
/// Handles anything related to anvil smithing.
///
/// An anvil allows you to turn metals into other shapes and forms, which will then allow you to create cool objects,
/// such as weapons, armors and other things.
///
/// Anvils use storage as a container for metals (subject to change), and usually have limited metals they can have at any time.
///
/// They also have their own minigame, which is heavily inspired by TFC anvil's (great minecraft mod, check it out... TFG is a good start).
/// Minigame Explanation:
/// - Each anvil has a list of numbers. Each number represents a "hit" the player can do with their hammer.
/// - Two progress bars appear on the screen once the player selects a recipe. One is red, and it shows a number (e.g. 60), and the other one is green and starts at 0.
/// - The player's goal is to reach the red progress bar by pressing the correct combination of numbers.
/// - Once done, the player will be rewarded with the recipe. However, there's still other things to account for such as:
///     - Metals need to be at correct temp (not implemented yet cause it requires cooling/heating mechanics into blacksmith)
///     - Patterns that need to be performed
/// </summary>
public abstract partial class AnvilSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [Dependency] private EntityQuery<AnvilComponent> _anvilQuery = default!;
    [Dependency] private EntityQuery<MetalWorkableComponent> _metalQuery = default!;

    /// <summary>
    /// A list of all metals matched with their respective recipes, for fast lookups
    /// </summary>
    [ViewVariables]
    public List<ProtoId<AnvilRecipePrototype>> Recipes = new();

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<AnvilComponent>(AnvilUiKey.Key,
            subs =>
            {
                subs.Event<AnvilRecipeSelectedMessage>(OnRecipeSelected);
                subs.Event<AnvilHitMessage>(OnHit);
            });

        LoadMetalRecipes();
    }

    [SubscribeLocalEvent]
    private void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<AnvilRecipePrototype>())
            return;

        LoadMetalRecipes();
    }

    [SubscribeLocalEvent]
    private void InsertAttempt(Entity<AnvilComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (args.Container.ID != StorageComponent.ContainerId)
            return;

        if (args.Cancelled || args.Container.Count < ent.Comp.AllowedWorkables)
            return;

        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void EntRemoved(Entity<AnvilComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        ResetAnvil(ent, args.Container.ID);
    }

    [SubscribeLocalEvent]
    private void EntInserted(Entity<AnvilComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        ResetAnvil(ent, args.Container.ID);
    }

    private void OnRecipeSelected(Entity<AnvilComponent> ent, ref AnvilRecipeSelectedMessage args)
    {
        var recipe = args.Recipe;
        if (ent.Comp.SelectedRecipe == recipe || !_container.TryGetContainer(ent, StorageComponent.ContainerId, out var container))
            return;

        var metals = container.ContainedEntities.ToList();
        var recipes = GetRecipes(metals);
        if (!recipes.Contains(recipe))
        {
            // malf recipe
            Log.Error($"Requested invalid anvil recipe: {recipe}");
            return;
        }

        SetSelectedRecipe(ent.AsNullable(), recipe);
    }

    private void OnHit(Entity<AnvilComponent> ent, ref AnvilHitMessage args)
    {
        var actor = args.Actor;
        var attemptEv = new CanOperateAnvilAttemptEvent();
        RaiseLocalEvent(actor, ref attemptEv);
        if (!attemptEv.Handled)
        {
            _popup.PopupCursor("Hold a hammer before operating on the anvil!", actor, PopupType.MediumCaution);
            return;
        }

        DoHit(ent.AsNullable(), args.Number);

        var ev = new HammerHitDoneEvent(ent, actor);
        RaiseLocalEvent(actor, ref ev);
    }
}
