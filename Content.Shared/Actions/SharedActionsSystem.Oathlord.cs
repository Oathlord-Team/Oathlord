using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.DoAfter;
using Robust.Shared.Map;

namespace Content.Shared.Actions;

public partial class SharedActionsSystem
{
    [Dependency] private EntityQuery<DoAfterArgsComponent> _doAfterArgsQuery = default!;
    [Dependency] private EntityQuery<DoAfterComponent> _doAfterQuery = default!;

    /// <summary>
    /// Validates a spell action so that it can be used, and also perform a doafter if it has one.
    /// This shouldn't exist in the first place, but action system is hardcoded as hell.
    /// Do note it will start a dofater, if the action has one
    /// </summary>
    /// <returns>True if the action is valid, false otherwise</returns>
    public void PerformSpellAction(EntityUid user, EntityUid actionEntity, EntityUid? target, EntityCoordinates coordinates, bool skipDoActionRequest = false)
    {
        if (GetAction(actionEntity) is not { } action)
            return;

        if (!action.Comp.Enabled)
            return;

        var curTime = GameTiming.CurTime;
        if (IsCooldownActive(action, curTime))
            return;

        var attemptEv = new ActionAttemptEvent(user, target);
        RaiseLocalEvent(action, ref attemptEv);
        if (attemptEv.Cancelled)
            return;

        // this is usually sent from the client to the server, but we have no other choice to make it here because of
        // how hardcoded ActionValidateEvent is to this specific event
        var input = new RequestPerformActionEvent(GetNetEntity(action), GetNetEntity(target), GetNetCoordinates(coordinates));
        var validation = new ActionValidateEvent
        {
            Input = input,
            User = user,
            Provider = user, // normally, this would be the container of the action, but spell actions dont have one
        };
        RaiseLocalEvent(action, ref validation);
        if (validation.Invalid)
            return;

        if (_doAfterArgsQuery.TryComp(action, out var actionDoAfterComp) && _doAfterQuery.TryComp(user, out var performerDoAfterComp) && !skipDoActionRequest)
        {
            TryStartActionDoAfter((action, actionDoAfterComp), (user, performerDoAfterComp), action.Comp.UseDelay, input);
            return;
        }

        PerformAction(user, action);
    }
}
