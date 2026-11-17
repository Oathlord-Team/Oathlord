using Content.IntegrationTests.Fixtures.Attributes;
using Content.IntegrationTests.Tests.Interaction;
using Content.Oathlord.Shared.Spellcasting.Components;
using Content.Oathlord.Shared.Spellcasting.Systems;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Oathlord;

public sealed class SpellTest : InteractionTest
{
    [SidedDependency(Side.Server)] private SpellcastingSystem _spellcasting = default!;
    [SidedDependency(Side.Server)] private SpellcasterSystem _spellcaster = default!;

    private const string SpellTestId = "EntitySpellTest";
    private const string SpellcasterTestId = "EntitySpellcasterTest";

    [TestPrototypes]
    private const string TestSpell = $@"
- type: entity
  parent: BaseSpell
  id: {SpellTestId}
  name: spell
";

    [TestPrototypes]
    private const string TestSpellcaster = $@"
- type: entity
  parent: BaseItem
  id: {SpellcasterTestId}
  name: spellcaster
  components:
  - type: Spellcaster
";

    [Test]
    [Description("Tests basic spell operations like transferring, adding, removing etc")]
    public async Task TestSpells()
    {
        var playerUid = SEntMan.GetEntity(Player);
        var xform = SComp<TransformComponent>(playerUid);

        await Server.WaitAssertion(() =>
        {
            // Make the user able to cast spells first
            var spells = SEntMan.EnsureComponent<SpellsComponent>(playerUid);
            Entity<SpellsComponent> spellUser = (playerUid, spells);

            // Make sure the player's hand starts empty
            var heldItem = HandSys.GetActiveItem((playerUid, Hands));
            Assert.That(heldItem, Is.Null, $"Player is holding an item ({SEntMan.ToPrettyString(heldItem)}) at start of test.");

            // Add the spell to the player, return if insertion failed
            // ignore needed because test prototype isn't registered in AllSpells
            var spell = _spellcasting.AddSpell(spellUser, SpellTestId);
            Assert.That(spell, Is.Not.Null, $"The spell {TestSpell} returned null. Either player already owns the spell, or spell not valid");

            var spellComp = SEntMan.GetComponent<SpellComponent>(spell.Value);

            // Try to transfer the spell to the active category
            _spellcasting.TryTransferSpell(spellUser, spell.Value, SpellTransfer.Active);
            Assert.That(spellComp.Active, Is.True, $"The spell {TestSpell} was not active, even though we transferred it to active spells.");

            // Add the spellcaster item to the user's hand so spells can be casted
            var item = SSpawnAtPosition(SpellcasterTestId, xform.Coordinates);
            Assert.That(HandSys.TryPickupAnyHand(playerUid, item), Is.True, "Could not pickup spellcaster item");

            // Check that we can cast the test spell
            Assert.That(_spellcaster.CanCast(item, spell.Value), Is.True, "The spellcaster failed to pass the conditions to cast the spell");

            // Check we have an active spell
            Assert.That(_spellcasting.GetActiveSpell(spellUser), Is.Not.Null, "There was no active spell in the user's container");

            // Remove the spell
            _spellcasting.RemoveSpell(spellUser, spell.Value);
            Assert.That(spellUser.Comp.Container.Count, Is.Zero, "The spell was not removed from the container");
        });
    }
}
