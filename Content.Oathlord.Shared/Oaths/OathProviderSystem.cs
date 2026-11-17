using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Oathlord.Shared.Oaths;

public sealed partial class OathProviderSystem : EntitySystem
{
    [Dependency] private OathSystem _oath = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [SubscribeLocalEvent]
    public void OnVerb(Entity<OathProviderComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!args.CanInteract || !args.CanAccess || _oath.GetOath(user) == null)
            return;

        if (!ProtoMan.Resolve(ent.Comp.Oath, out var oathProto))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Icon = oathProto.Icon,
            Text = "Pray to Avatar",
            Act = () =>
            {
                // note: should this be a do-after?
                if (_oath.SetOath(user, ent.Comp.Oath))
                {
                    _popup.PopupEntity("You break your previous oath, and fall into a new one...", user, PopupType.Medium);
                    return;
                }

                _popup.PopupEntity("You fail to align to this oath...", user, PopupType.MediumCaution);
            }
        });
    }
}
