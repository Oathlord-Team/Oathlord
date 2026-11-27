using Content.Oathlord.Shared.Economy.Components;
using Content.Shared.Stacks;
using Robust.Shared.Physics.Events;

namespace Content.Oathlord.Shared.Economy.Systems;

public sealed partial class EconomyVaultSystem : EntitySystem
{
    [Dependency] private OathlordEconomySystem _economy = default!;
    [Dependency] private EconomyValueSystem _value = default!;
    [Dependency] private SharedStackSystem _stack = default!;

    [Dependency] private EntityQuery<EconomyValueComponent> _economyValueQuery = default!;
    [Dependency] private EntityQuery<EconomyCurrencyComponent> _economyCurrencyQuery = default!;
    [Dependency] private EntityQuery<ActiveVaultEntityComponent> _activeVaultQuery = default!;

    [SubscribeLocalEvent]
    private void OnStartCollide(Entity<VaultTileComponent> ent, ref StartCollideEvent args)
    {
        var other = args.OtherEntity;
        if (_activeVaultQuery.HasComp(other))
            return; // don't do any operations if we are colliding with another vault tile

        if (!_economyValueQuery.TryComp(args.OtherEntity, out var value) || _economy.GetCurrentEconomy(other) is not { } economy)
            return;

        ent.Comp.Stored.Add(other);
        Dirty(ent);

        EnsureComp<ActiveVaultEntityComponent>(other);

        // If it's a currency, we must register it to the economy
        if (_economyCurrencyQuery.TryComp(other, out var currency))
        {
            var amount = _stack.GetCount(other);
            _economy.AdjustStoredCurrencies(economy.AsNullable(), currency.Currency, amount);

            return;
        }

        // Otherwise, just register it to the total economy amount
        var actual = _value.GetValue((args.OtherEntity, value));
        _economy.AdjustEconomyTotal(ent, actual);
    }

    [SubscribeLocalEvent]
    private void OnEndCollide(Entity<VaultTileComponent> ent, ref EndCollideEvent args)
    {
        var other = args.OtherEntity;
        if (!_economyValueQuery.TryComp(other, out var value) || _economy.GetCurrentEconomy(other) is not { } economy)
            return;

        if (!ent.Comp.Stored.Contains(other))
            return;

        RemCompDeferred<ActiveVaultEntityComponent>(other);

        ent.Comp.Stored.Remove(other);
        Dirty(ent);

        // Unregister everything from here on below
        if (_economyCurrencyQuery.TryComp(other, out var currency))
        {
            var amount = _stack.GetCount(other);
            _economy.AdjustStoredCurrencies(economy.AsNullable(), currency.Currency, -amount);

            return;
        }

        var actual = _value.GetValue((args.OtherEntity, value));
        _economy.AdjustEconomyTotal(ent, -actual);
    }
}
