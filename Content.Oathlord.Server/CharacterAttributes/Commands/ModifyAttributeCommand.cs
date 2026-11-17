// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Oathlord.Shared.CharacterAttributes;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Server.CharacterAttributes.Commands;

/// <summary>
/// Command that modifies your character attributes based on a prototype.
/// This is silent (no popups) to the user.
/// </summary>
[AdminCommand(AdminFlags.Admin)]
public sealed partial class ModifyAttributeCommand : LocalizedEntityCommands
{
    [Dependency] private CharacterAttributesSystem _attributes = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IComponentFactory _factory = default!;

    public override string Command => "modifyattribute";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError(Loc.GetString("cmd-addaction-invalid-args"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetUidNet) || !EntityManager.TryGetEntity(targetUidNet, out var targetEntity) || targetEntity is not { } target)
        {
            shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        if (!EntityManager.HasComponent<CharacterAttributesComponent>(target))
        {
            shell.WriteError("Entity does not have character attributes component");
            return;
        }

        if (!_prototypeManager.TryIndex<EntityPrototype>(args[1], out var proto) || !proto.HasComp<CharacterAttributeComponent>(_factory) || !_attributes.AllAttributes.Contains(proto))
        {
            shell.WriteError($"Attribute not found: {args[1]}");
            return;
        }

        if (!int.TryParse(args[2], out var attributePoints))
        {
            shell.WriteError("Could not parse attribute points. Must be an integer.");
            return;
        }

        if (!_attributes.TryAdjustAttribute(target, proto, attributePoints))
        {
            shell.WriteError("Could not adjust attribute. Possibly not found on the user's container.");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.Components<CharacterAttributesComponent>(args[0]),
                "<EntityUid>");
        }

        if (args.Length != 2)
            return CompletionResult.Empty;

        var characterAttributePrototypes = _prototypeManager.EnumeratePrototypes<EntityPrototype>()
            .Where(p => p.HasComp<CharacterAttributeComponent>(_factory))
            .Select(p => p.ID)
            .Order();

        return CompletionResult.FromHintOptions(
            characterAttributePrototypes,
            "<CharacterAttributeProto>");
    }
}
