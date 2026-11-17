using System.Linq;
using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Tests.Interaction;
using Content.Oathlord.Shared.Blacksmith.Anvil;
using Content.Oathlord.Shared.Blacksmith.Anvil.Prototypes;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Oathlord;

public sealed class AnvilTest : InteractionTest
{
    [SidedDependency(Side.Server)] private AnvilSystem _anvil = default!;
    [SidedDependency(Side.Server)] private SharedStorageSystem _storage = default!;
    [SidedDependency(Side.Server)] private SharedContainerSystem _container = default!;

    private static readonly EntProtoId Anvil = "Anvil";
    private static readonly EntProtoId Hammer = "Hammer";
    private static readonly EntProtoId Metal = "DebugWorkable";
    private static readonly ProtoId<AnvilRecipePrototype> TestRecipe = "DebugAnvilRecipe";

    [Test]
    [Description("Test that ensures the anvil smithing process works as intended")]
    public async Task TestAnvil()
    {
        var playerUid = SEntMan.GetEntity(Player);
        var xform = SComp<TransformComponent>(playerUid);
        Entity<AnvilComponent> anvil = default;
        var metal = EntityUid.Invalid;

        await Server.WaitPost(() =>
        {
            anvil = SSpawn(Anvil);
            var anvilComp = SEntMan.GetComponent<AnvilComponent>(anvil);
            anvil = (anvil, anvilComp);
            metal = SSpawn(Metal);
        });

        await Server.WaitAssertion(() =>
        {
            // Make sure the player's hand starts empty
            var heldItem = HandSys.GetActiveItem((playerUid, Hands));
            Assert.That(heldItem, Is.Null, $"Player is holding an item ({SEntMan.ToPrettyString(heldItem)}) at start of test.");

            // Add a hammer to the user's hand so the anvil recipe can be worked on
            var hammer = SSpawnAtPosition(Hammer, xform.Coordinates);
            Assert.That(HandSys.TryPickupAnyHand(playerUid, hammer), Is.True, "Could not pickup hammer item");

            // Force set a recipe on the anvil
            _anvil.SetSelectedRecipe(anvil.AsNullable(), TestRecipe);
            Assert.That(anvil.Comp.SelectedRecipe, Is.Not.Null);

            // Try to insert a metal
            _storage.Insert(anvil, metal, out _);
            var container = _container.TryGetContainer(anvil, StorageComponent.ContainerId, out var cont);
            Assert.Multiple(() =>
            {
                Assert.That(container, Is.True);
                Assert.That(cont, Is.Not.Null);
                Assert.That(cont.Count, Is.EqualTo(1));
            });

            // Try to do some hits and finish the recipe
            // todo: have a force variable in the function to ensure those numbers dont have to exist for the hit
            _anvil.DoHit(anvil.AsNullable(), 9);
            _anvil.DoHit(anvil.AsNullable(), 1);
            Assert.That(anvil.Comp.SelectedRecipe, Is.Null, "The anvil could not complete a test recipe. Something is wrong the with the API logic");
        });
    }

    [Test]
    [Description("Test that checks that all anvil recipes have a valid pattern")]
    public async Task TestAnvilRecipes()
    {
        var pair = Pair;
        var anvilComp = Factory.CompName<AnvilComponent>();

        var recipes = SProtoMan.EnumeratePrototypes<AnvilRecipePrototype>()
            .Where(p => !pair.IsTestPrototype(p))
            .ToList();

        var anvils = SProtoMan.EnumeratePrototypes<EntityPrototype>()
            .Where(p => !p.Abstract)
            .Where(p => p.HasComp(anvilComp))
            .ToList();

        await Server.WaitAssertion(() =>
        {
            foreach (var recipe in recipes)
            {
                Assert.Multiple(() =>
                {
                    // Only check patterns if we have a pattern
                    if (recipe.Pattern.Any())
                    {
                        Assert.That(
                            recipe.Pattern.Count,
                            Is.GreaterThan(1),
                            $"The anvil recipe: {recipe.ID}, has a pattern with an invalid amount ({recipe.Metals.Count}) of hit numbers. You need at least 2 hit numbers for a pattern.");
                    }

                    Assert.That(
                        recipe.WorkRequired,
                        Is.LessThanOrEqualTo(100),
                        $"The anvil recipe: {recipe}, has a work required number higher than 100. It must be under, or equal to 100 for now.");

                    Assert.That(
                        recipe.WorkRequired,
                        Is.GreaterThan(0),
                        $"The anvil recipe: {recipe}, has a work required number that is 0, or lower. It must be between 100 and 1.");
                });
            }

            foreach (var anvil in anvils)
            {
                if (!anvil.TryComp<AnvilComponent>(anvilComp, out var anvilComponent))
                    return;

                var anvilNums = anvilComponent.Numbers;
                foreach (var recipe in recipes)
                {
                    if (!recipe.Pattern.Any())
                        continue;

                    foreach (var patterNum in recipe.Pattern)
                    {
                        Assert.That(anvilNums, Contains.Item(patterNum), $"{anvil.ID} has hit numbers that do not support the pattern of {recipe.ID}");
                    }
                }
            }
        });
    }
}
