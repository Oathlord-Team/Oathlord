namespace Content.Oathlord.Shared.CharacterAttributes;

public sealed partial class CharacterAttributesSystem
{
    private void InitializeRelay()
    {

    }

    private void RefRelayCharAttributeEvent<T>(
        EntityUid uid,
        CharacterAttributesComponent component,
        ref T args) where T : struct
    {
        RelayEvent((uid, component), ref args);
    }

    private void RelayCharAttributeEvent<T>(
        EntityUid uid,
        CharacterAttributesComponent component,
        ref T args) where T : class
    {
        RelayEvent((uid, component), args);
    }

    public void RelayEvent<T>(Entity<CharacterAttributesComponent> ent, ref T args) where T : struct
    {
        var ev = new CharAttributeRelayedEvent<T>(args);
        foreach (var attr in ent.Comp.Container.ContainedEntities)
        {
            RaiseLocalEvent(attr, ref ev);
        }

        args = ev.Args;
    }

    public void RelayEvent<T>(Entity<CharacterAttributesComponent> ent, T args) where T : class
    {
        var ev = new CharAttributeRelayedEvent<T>(args);
        foreach (var attr in ent.Comp.Container.ContainedEntities)
        {
            RaiseLocalEvent(attr, ref ev);
        }
    }
}

[ByRefEvent]
public record struct CharAttributeRelayedEvent<TEvent>(TEvent Args)
{
    public TEvent Args = Args;
};
