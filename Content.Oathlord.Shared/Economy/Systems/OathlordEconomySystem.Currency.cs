using Content.Oathlord.Shared.Economy.Prototypes;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Oathlord.Shared.Economy.Systems;

/// <summary>
/// Public API for anything related to <see cref="EconomyCurrencyPrototype"/>
/// </summary>
public sealed partial class OathlordEconomySystem
{
    [Dependency] private SharedStackSystem _stack = default!;

    /// <summary>
    /// Caching the values of the currencies, because economy is gonna use them a lot
    /// </summary>
    private Dictionary<ProtoId<EconomyCurrencyPrototype>, int> _allCurrencyValues = new();

    private void InitializeCurrency()
    {
        LoadValues();
    }

    #region Event Handlers

    [SubscribeLocalEvent]
    private void OnReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<EconomyCurrencyPrototype>())
            return;

        LoadValues();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Gets the total amount of the currency, by multiplying the currency's value with a specified amount.
    /// </summary>
    public int GetCurrencyTotal([ForbidLiteral] ProtoId<EconomyCurrencyPrototype> currencyProto, int amount)
    {
        if (!_allCurrencyValues.TryGetValue(currencyProto, out var value))
            return 0;

        return value * amount;
    }

    /// <summary>
    /// Gets the value of a currency prototype
    /// </summary>
    public int GetCurrencyValue([ForbidLiteral] ProtoId<EconomyCurrencyPrototype> currencyProto)
    {
        if (!_allCurrencyValues.TryGetValue(currencyProto, out var value))
            return 0;

        return value;
    }

    #endregion

    /// <summary>
    /// Spawns the physical <see cref="EntProtoId"/> of the currencies provided
    /// </summary>
    private void SpawnPhysicalFromCurrencies(EntityUid target, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> currencies)
    {
        foreach (var (cur, amount) in currencies)
        {
            // todo: if this gets called a lot, we can cache it later
            if (!ProtoMan.Resolve(cur, out var currency) || amount <= 0)
                continue;

            var remaining = amount;
            var maxCount = _stack.GetMaxCount(currency.EntityProto);

            // Since coins have maximum of X (where X is the maxCount) coins per stack,
            // we need to spawn more stack entities than usual if the machine requests more than X coins
            while (remaining > 0)
            {
                PredictedTrySpawnNextTo(currency.EntityProto, target, out var spawned);
                if (spawned is not { } spawnedCurrency)
                    break;

                var stackAmount = Math.Min(remaining, maxCount);
                _stack.SetCount(spawnedCurrency, stackAmount);
                remaining -= stackAmount;
            }
        }
    }

    private void LoadValues()
    {
        _allCurrencyValues.Clear();
        foreach (var proto in ProtoMan.EnumeratePrototypes<EconomyCurrencyPrototype>())
        {
            _allCurrencyValues.Add(proto, proto.Value);
        }
    }
}
