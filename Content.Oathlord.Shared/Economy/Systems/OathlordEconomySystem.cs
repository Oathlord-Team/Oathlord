using Content.Oathlord.Shared.Economy.Components;
using Content.Oathlord.Shared.Economy.Prototypes;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.Station;
using Content.Shared.Station.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Oathlord.Shared.Economy.Systems;

/// <summary>
/// This system-clutter handles all of the economic functions of Oathlord.
///
/// Oathlord has one main currency, Nar, which is used for the central economy.
/// In most cases, there can only be one economy (on the main map).
///
/// This is split into partial classes so it's not one big file with tons of methods.
/// Check the individual classes for what you need.
/// </summary>
public sealed partial class OathlordEconomySystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [Dependency] private EntityQuery<EconomyMapComponent> _econMapQuery = default!;
    [Dependency] private EntityQuery<EconomyAccountComponent> _econAccountQuery = default!;

    /// <summary>
    /// All active economies loaded, gets cleared when a round restarts
    /// </summary>
    [ViewVariables]
    private HashSet<Entity<EconomyMapComponent>> _activeEconomies = new();

    public override void Initialize()
    {
        base.Initialize();

        InitializeMachine();
        InitializeCurrency();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _activeEconomies.Clear();
    }

    [SubscribeLocalEvent]
    private void OnEconomyMapInit(Entity<EconomyMapComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.TotalStored = GetTotalEconomyStored(ent.AsNullable());
        DirtyField(ent.AsNullable(), nameof(EconomyMapComponent.TotalStored));

        _activeEconomies.Add(ent);
    }

    [SubscribeLocalEvent]
    private void OnEconomyShutdown(Entity<EconomyMapComponent> ent, ref ComponentShutdown args)
    {
        _activeEconomies.Remove(ent);
    }

    [SubscribeLocalEvent]
    private void OnReset(RoundRestartCleanupEvent ev)
    {
        _activeEconomies.Clear();
    }

    #region Public API

    /// <summary>
    /// Adds an account to all active economies.
    /// </summary>
    public void AddAccountToEconomy(EntityUid account)
    {
        foreach (var econ in _activeEconomies)
        {
            econ.Comp.ActiveAccounts.Add(account);
            DirtyField(econ.AsNullable(), nameof(EconomyMapComponent.ActiveAccounts));
        }
    }

    /// <summary>
    /// Removes an account from all active economies
    /// </summary>
    public void RemoveAccountFromEconomy(EntityUid account)
    {
        foreach (var econ in _activeEconomies)
        {
            econ.Comp.ActiveAccounts.Remove(account);
            DirtyField(econ.AsNullable(), nameof(EconomyMapComponent.ActiveAccounts));
        }
    }

    /// <summary>
    /// Returns the economy the user is residing in.
    /// If the user is in another map without an economy, then this will return null.
    /// </summary>
    public Entity<EconomyMapComponent>? GetCurrentEconomy(EntityUid user)
    {
        if (_station.GetOwningStation(user) is not { } station || !_econMapQuery.TryComp(station, out var mapEconomy))
            return null;

        return (station, mapEconomy);
    }

    /// <summary>
    /// Returns the total amount of currency this economy has stored
    /// </summary>
    public int GetTotalEconomyStored(Entity<EconomyMapComponent?> ent)
    {
        if (!_econMapQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0;

        var total = 0;
        foreach (var (currency, value) in ent.Comp.StoredCurrencies)
        {
            total += GetCurrencyTotal(currency, value);
        }

        ent.Comp.TotalStored = total;
        DirtyField(ent, nameof(EconomyMapComponent.TotalStored));

        return total;
    }

    /// <summary>
    /// Withdraws a specific amount of currencies from the economy.
    /// Does not accept negative values.
    /// </summary>
    public bool TryWithdrawFromEconomy(Entity<EconomyMapComponent?> ent, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> toWithdraw)
    {
        if (!_econMapQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        var total = GetTotalFromCurrencies(ent.Comp.TotalStored, toWithdraw);
        if (total == 0)
            return false;

        foreach (var (cur, storedValue) in ent.Comp.StoredCurrencies)
        {
            if (!toWithdraw.TryGetValue(cur, out var value) || value < 0 || value > storedValue)
            {
                return false;
            }

            ent.Comp.StoredCurrencies[cur] = Math.Clamp(ent.Comp.StoredCurrencies[cur] - toWithdraw[cur], 0, int.MaxValue);
        }

        DirtyField(ent, nameof(EconomyMapComponent.StoredCurrencies));
        return true;
    }

    /// <summary>
    /// Gets the loan interest specified by the economy
    /// </summary>
    public float GetLoanInterest(Entity<EconomyMapComponent?> ent)
    {
        if (!_econMapQuery.Resolve(ent.Owner, ref ent.Comp))
            return 0f;

        return ent.Comp.LoanInterest;
    }

    /// <summary>
    /// Gets the loan interest specified by the economy
    /// </summary>
    /// <param name="user">A user to get the economy from</param>
    public float GetLoanInterest(EntityUid user)
    {
        if (GetCurrentEconomy(user) is not { } economy)
            return 0f;

        return economy.Comp.LoanInterest;
    }

    /// <summary>
    /// Sets the stored currencies of the map's currency to a specific amount
    /// </summary>
    /// <param name="ent">The map economy</param>
    /// <param name="currencies">The currencies to set them to</param>
    public void SetStoredCurrencies(Entity<EconomyMapComponent?> ent, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> currencies)
    {
        if (!_econMapQuery.Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.StoredCurrencies = currencies;
        DirtyField(ent, nameof(EconomyMapComponent.StoredCurrencies));
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Helper to get the total amount from currencies, and check if its enough versus our stored amount.
    /// Does not accept negative values.
    /// </summary>
    /// <param name="stored">The total stored amount we have in our accounts or economy</param>
    /// <param name="currencies">The currencies to check against</param>
    /// <returns></returns>
    private int GetTotalFromCurrencies(int stored, Dictionary<ProtoId<EconomyCurrencyPrototype>, int> currencies)
    {
        var total = 0;
        foreach (var (cur, amount) in currencies)
        {
            if (amount < 0)
                continue;

            var curValue = GetCurrencyTotal(cur, amount);
            total += curValue;
        }

        if (total > stored)
            return 0;

        return total;
    }

    /// <summary>
    /// Helper to get the total amount from currencies
    /// Does not accept negative values.
    /// </summary>
    /// <param name="currencies">The currencies to check against</param>
    /// <returns></returns>
    private int GetTotalFromCurrencies(Dictionary<ProtoId<EconomyCurrencyPrototype>, int> currencies)
    {
        var total = 0;
        foreach (var (cur, amount) in currencies)
        {
            if (amount < 0)
                continue;

            var curValue = GetCurrencyTotal(cur, amount);
            total += curValue;
        }

        return total;
    }

    #endregion
}

/// <summary>
/// Enum that defines a transaction type.
/// </summary>
[Serializable, NetSerializable]
public enum EconomyTransaction : byte
{
    Withdraw,
    Deposit,
}
