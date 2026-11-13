using System.Linq;
using Content.Oathlord.Shared.Spellcasting.Components;
using Content.Oathlord.Shared.Spellcasting.Systems;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Server.Spellcasting.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed partial class AddSpellCommand : LocalizedEntityCommands
{
    [Dependency] private SpellcastingSystem _spellcasting = default!;
    [Dependency] private IComponentFactory _factory = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    public override string Command => "addspell";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("cmd-addaction-invalid-args"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetUidNet) || !EntityManager.TryGetEntity(targetUidNet, out var targetEntity))
        {
            shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!EntityManager.HasComponent<SpellsComponent>(targetEntity))
        {
            shell.WriteError("Entity does not have spells component");
            return;
        }

        if (!_prototypeManager.TryIndex<EntityPrototype>(args[1], out var proto)
            || !proto.HasComp<SpellComponent>(_factory))
        {
            shell.WriteError($"Spell not found: {args[1]}");
            return;
        }

        if (_spellcasting.AddSpell(targetEntity.Value, args[1]) == null)
        {
            shell.WriteError("Failed to add spell. You probably already know the spell...");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.Components<SpellsComponent>(args[0]),
                "<EntityUid>");
        }

        if (args.Length != 2)
            return CompletionResult.Empty;

        var spellPrototypes = _prototypeManager.EnumeratePrototypes<EntityPrototype>()
            .Where(p => p.HasComp<SpellComponent>(_factory))
            .Select(p => p.ID)
            .Order();

        return CompletionResult.FromHintOptions(
            spellPrototypes,
            "<SpellProto>");
    }
}
