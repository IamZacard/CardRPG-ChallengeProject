using UnityEngine;

/// <summary>
/// Represents a targetable combat entity (like a player or enemy) in a card-based turn system.
/// </summary>
public interface ICombatTarget
{
    /// <summary>
    /// Is the target currently alive (HP > 0)?
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// The current block value that absorbs damage.
    /// </summary>
    int CurrentBlock { get; }

    /// <summary>
    /// The target's current health.
    /// </summary>
    int CurrentHP { get; }

    /// <summary>
    /// The target's maximum health.
    /// </summary>
    int MaxHP { get; }

    /// <summary>
    /// Reference to the GameObject (for visuals or logging).
    /// </summary>
    GameObject GetGameObject();

    /// <summary>
    /// Deals raw damage to the target (after applying block and effects).
    /// </summary>
    /// <param name="amount">Damage amount</param>
    void TakeDamage(int amount);

    /// <summary>
    /// Heals the target for a given amount (clamped to max HP).
    /// </summary>
    /// <param name="amount">Heal amount</param>
    void Heal(int amount);

    /// <summary>
    /// Grants block that lasts until the start of the next turn.
    /// </summary>
    /// <param name="amount">Block to add</param>
    void GainBlock(int amount);

    /// <summary>
    /// Adds a status effect like Poison, Strength, etc.
    /// </summary>
    /// <param name="effect">Status effect to apply</param>
    void AddStatusEffect(StatusEffect effect);

    /// <summary>
    /// Removes all block (typically at the end of turn).
    /// </summary>
    void ClearBlock();

    /// <summary>
    /// Gets a specific status effect on this target, or null if not present.
    /// </summary>
    StatusEffect GetStatusEffect(StatusEffectType type);

    /// <summary>
    /// Removes a specific status effect from the target.
    /// </summary>
    void RemoveStatusEffect(StatusEffectType type);
}
