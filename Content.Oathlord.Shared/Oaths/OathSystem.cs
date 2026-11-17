using Content.Oathlord.Common.Oaths;
using Content.Shared.Body;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Oaths;

/// <summary>
/// System that provides a public API for handling oaths
/// </summary>
public abstract partial class OathSystem : CommonOathSystem
{
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    [Dependency] private EntityQuery<OathComponent> _oathQuery = default!;

    /// <summary>
    /// List of all oaths loaded into the game
    /// </summary>
    [ViewVariables]
    public List<ProtoId<OathPrototype>> AllOaths = new();

    /// <summary>
    /// List of all roundstart oaths loaded into the game
    /// </summary>
    [ViewVariables]
    public List<ProtoId<OathPrototype>> AllRoundstartOaths = new();

    public override void Initialize()
    {
        base.Initialize();

        LoadOaths();
    }

    [SubscribeLocalEvent]
    public void OnProtoReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<OathPrototype>())
            return;

        LoadOaths();
    }

    /// <summary>
    /// Returns the oath active in the entity's brain
    /// </summary>
    /// <param name="uid">The user</param>
    /// <returns>Null if user has no organs with oath component</returns>
    public ProtoId<OathPrototype>? GetOath(EntityUid uid)
    {
        // Return early if it's a brain
        if (_oathQuery.TryComp(uid, out var oath))
            return oath.Oath;

        var organs = _body.EnumerateOrgans(uid, _oathQuery);
        foreach (var (_, _, oathOrgan) in organs)
        {
            return oathOrgan.Oath;
        }

        return null;
    }

    /// <summary>
    /// Sets the oath on a brain
    /// </summary>
    /// <param name="ent">The brain</param>
    /// <param name="oath">The oath to set it to</param>
    public bool SetOath(Entity<OathComponent?> ent, [ForbidLiteral] ProtoId<OathPrototype> oath) =>
        SetOath(ent, oath, null);

    /// <summary>
    /// Sets the oath on a brain
    /// </summary>
    /// <param name="ent">The brain</param>
    /// <param name="oath">The oath to set it to</param>
    /// <param name="user">The body entity</param>
    /// <param name="runEffects">If user is non-null, whether we should run the entity effects when setting the oath</param>
    /// <param name="isPreference">If true, it means you called this from the character customization. Some oaths may not appear in there.</param>
    /// <returns>True if the oath was set, false otherwise. Does not matter if oath effects were run, or not.</returns>
    public bool SetOath(Entity<OathComponent?> ent, [ForbidLiteral] ProtoId<OathPrototype> oath, EntityUid? user, bool runEffects = true, bool isPreference = false)
    {
        if (!_oathQuery.Resolve(ent.Owner, ref ent.Comp) || !ProtoMan.Resolve(oath, out var oathProto))
            return false;

        if (isPreference && !oathProto.Preference)
        {
            // the oath can only be found in world... this should not happen ever
            Log.Error($"Tried to set an oath ({oathProto.ID}) that was not meant to be set from the humanoid profile editor");
            return false;
        }

        var prevOath = ent.Comp.Oath;
        if (prevOath == oath)
            return false;

        ent.Comp.Oath = oath;
        Dirty(ent);

        if (!runEffects || user is not { } usr)
            return true;

        // Apply both the benefits of the new oath, and the curse of the previous oath (if it had any)
        // Swapping oaths should not be free game...
        ApplyOathCurseEffects(usr, prevOath);
        ApplyOathEffects(usr, ent.Comp.Oath);

        return true;
    }

    /// <summary>
    /// Sets the oath on a brain
    /// </summary>
    /// <param name="user">The user entity</param>
    /// <param name="oath">The oath to apply</param>
    /// <param name="runEffects">Whether to run effects, or not</param>
    /// <param name="isPreference">If true, it means you called this from the character customization.</param>
    public bool SetOath(EntityUid user, [ForbidLiteral] ProtoId<OathPrototype> oath, bool runEffects = true, bool isPreference = false)
    {
        // It's a brain...
        if (_oathQuery.TryComp(user, out var oathComp))
        {
            return SetOath(
                ent: (user, oathComp),
                oath: oath,
                user: null,
                runEffects: runEffects,
                isPreference: isPreference);
        }

        var organs = _body.EnumerateOrgans(user, _oathQuery);
        foreach (var (oathUid, _, oathOrgan) in organs)
        {
            // Apply to first found only
            return SetOath(
                ent: (oathUid, oathOrgan),
                oath: oath,
                user: user,
                runEffects: runEffects,
                isPreference: isPreference);
        }

        return false;
    }

    /// <summary>
    /// Returns the <see cref="OathPrototype.Effect"/>
    /// </summary>
    /// <param name="oath">The oath prototype</param>
    /// <returns>Null if prototype was not resolved</returns>
    public ProtoId<EntityEffectPrototype>? GetEffects([ForbidLiteral] ProtoId<OathPrototype> oath)
    {
        if (!ProtoMan.TryIndex(oath, out var oathProto))
            return null;

        return oathProto.Effect;
    }

    /// <summary>
    /// Returns the <see cref="OathPrototype.CurseEffect"/>
    /// </summary>
    /// <param name="oath">The oath prototype</param>
    /// <returns>Null if prototype was not resolved, or there were no curse effects</returns>
    public ProtoId<EntityEffectPrototype>? GetCurseEffects([ForbidLiteral] ProtoId<OathPrototype> oath)
    {
        if (!ProtoMan.TryIndex(oath, out var oathProto) || oathProto.CurseEffect is not { } curseEffect)
            return null;

        return curseEffect;
    }

    public override void ApplyOath(EntityUid target, [ForbidLiteral] ProtoId<OathPrototype> oath)
    {
        // Called from humanoid profile editor, so it's a preference
        SetOath(target, oath, isPreference: true);
    }

    /// <summary>
    /// Helper to apply oath effects
    /// </summary>
    /// <param name="target">The target to apply the effects to</param>
    /// <param name="oath">The oath</param>
    private void ApplyOathEffects(EntityUid target, ProtoId<OathPrototype> oath)
    {
        // todo: delay benefits to apply when curse ends, in case curse is a status effect etc...?
        var effect = GetEffects(oath);
        if (effect is { } entEffect)
            _effects.TryApplyEffect(target, entEffect);
    }

    /// <summary>
    /// Helper to apply curse effects
    /// </summary>
    /// <param name="target">The target to apply the effects to</param>
    /// <param name="oath">The oath</param>
    private void ApplyOathCurseEffects(EntityUid target, ProtoId<OathPrototype> oath)
    {
        var curse = GetCurseEffects(oath);
        if (curse is { } entCurse)
            _effects.TryApplyEffect(target, entCurse);
    }

    private void LoadOaths()
    {
        AllOaths.Clear();
        AllRoundstartOaths.Clear();
        foreach (var oath in ProtoMan.EnumeratePrototypes<OathPrototype>())
        {
            AllOaths.Add(oath);

            if (oath.Preference)
                AllRoundstartOaths.Add(oath);
        }
    }
}
