using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages all status effects in combat, including application, removal, and processing effects at the appropriate times.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    // Dictionary of active effects for each target
    private Dictionary<ICombatTarget, List<StatusEffect>> activeEffects = new Dictionary<ICombatTarget, List<StatusEffect>>();

    // Persistent effects that carry between combats
    private List<StatusEffect> persistentEffects = new List<StatusEffect>();

    // Cached references
    private CombatManager combatManager;

    private void Awake()
    {
        combatManager = GetComponent<CombatManager>();
    }

    private void OnEnable()
    {
        if (combatManager != null)
        {
            combatManager.OnCombatStart += ClearNonPersistentEffects;
            combatManager.OnCombatEnd += CleanupEffects;
        }
    }

    private void OnDisable()
    {
        if (combatManager != null)
        {
            combatManager.OnCombatStart -= ClearNonPersistentEffects;
            combatManager.OnCombatEnd -= CleanupEffects;
        }
    }

    /// <summary>
    /// Apply a status effect to a target
    /// </summary>
    /// <param name="effectType">Type of effect to apply</param>
    /// <param name="target">Target to receive the effect</param>
    /// <param name="intensity">Strength of the effect</param>
    /// <param name="duration">How many turns the effect lasts (-1 for permanent)</param>
    /// <param name="isPersistent">Whether effect persists between combats</param>
    /// <returns>The applied effect, or null if blocked</returns>
    public StatusEffect ApplyEffect(StatusEffectType effectType, ICombatTarget target, int intensity, int duration = -1, bool isPersistent = false)
    {
        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning("Attempted to apply status effect to null or dead target");
            return null;
        }

        // Check if this is a negative effect that can be blocked
        bool isNegative = IsNegativeEffect(effectType);
        if (isNegative && TryBlockNegativeEffect(target))
        {
            Debug.Log($"{effectType} effect was blocked on {target.GetGameObject().name}");
            return null;
        }

        // Check if target already has this effect
        StatusEffect existingEffect = GetExistingEffect(target, effectType);

        if (existingEffect != null)
        {
            // Increase intensity of existing effect
            existingEffect.ModifyIntensity(intensity);

            // Reset or extend duration if applicable
            if (duration > 0 && existingEffect.ModifyDuration(duration) <= 0)
            {
                // If duration is now 0, remove the effect
                RemoveEffect(target, existingEffect);
                return null;
            }

            Debug.Log($"Increased {effectType} on {target.GetGameObject().name} to intensity {existingEffect.Intensity}");
            return existingEffect;
        }
        else
        {
            // Create and add new effect
            StatusEffect newEffect = new StatusEffect(effectType, target, intensity, duration, isPersistent);
            AddEffectToTarget(target, newEffect);

            Debug.Log($"Applied {effectType} to {target.GetGameObject().name} with intensity {intensity}");
            return newEffect;
        }
    }

    /// <summary>
    /// Apply a custom status effect with a specific action
    /// </summary>
    /// <param name="name">Display name of the effect</param>
    /// <param name="description">Description of what the effect does</param>
    /// <param name="icon">Visual icon for the effect</param>
    /// <param name="target">Target to receive the effect</param>
    /// <param name="intensity">Strength of the effect</param>
    /// <param name="duration">How many turns it lasts</param>
    /// <param name="effect">Callback action to execute</param>
    /// <returns>The applied custom effect</returns>
    public StatusEffect ApplyCustomEffect(string name, string description, Sprite icon, ICombatTarget target,
                                         int intensity, int duration, System.Action<ICombatTarget, int> effect)
    {
        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning("Attempted to apply custom status effect to null or dead target");
            return null;
        }

        // Create and add the custom effect
        StatusEffect customEffect = new StatusEffect(name, description, icon, target, intensity, duration, effect);
        AddEffectToTarget(target, customEffect);

        Debug.Log($"Applied custom effect '{name}' to {target.GetGameObject().name}");
        return customEffect;
    }

    /// <summary>
    /// Process status effects at the start of a turn
    /// </summary>
    /// <param name="isPlayerTurn">Whether it's the player's turn starting</param>
    /*public void ProcessStartOfTurnEffects(bool isPlayerTurn)
    {
        // Get all relevant targets
        List<ICombatTarget> targets = GetTargetsForTurn(isPlayerTurn);

        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            ProcessEffectsForTarget(target, true);
        }
    }

    /// <summary>
    /// Process status effects at the end of a turn
    /// </summary>
    /// <param name="isPlayerTurn">Whether it's the player's turn ending</param>
    public void ProcessEndOfTurnEffects(bool isPlayerTurn)
    {
        // Get all relevant targets
        List<ICombatTarget> targets = GetTargetsForTurn(isPlayerTurn);

        foreach (var target in targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            ProcessEffectsForTarget(target, false);
        }
    }*/

    /// <summary>
    /// Process effects for a specific target
    /// </summary>
    /// <param name="target">Target whose effects to process</param>
    /// <param name="isStartOfTurn">True for start of turn, false for end of turn</param>
    private void ProcessEffectsForTarget(ICombatTarget target, bool isStartOfTurn)
    {
        if (!activeEffects.TryGetValue(target, out List<StatusEffect> effects) || effects.Count == 0)
            return;

        // Create a copy of the list because we might modify it during iteration
        List<StatusEffect> effectsToProcess = new List<StatusEffect>(effects);

        foreach (var effect in effectsToProcess)
        {
            bool shouldRemove = false;

            if (isStartOfTurn)
                shouldRemove = effect.ProcessStartOfTurn();
            else
                shouldRemove = effect.ProcessEndOfTurn();

            if (shouldRemove)
            {
                RemoveEffect(target, effect);
            }
        }
    }

    /// <summary>
    /// Remove a specific effect from a target
    /// </summary>
    /// <param name="target">Target to remove effect from</param>
    /// <param name="effect">Effect to remove</param>
    public void RemoveEffect(ICombatTarget target, StatusEffect effect)
    {
        if (target == null || effect == null)
            return;

        if (activeEffects.TryGetValue(target, out List<StatusEffect> effects))
        {
            effects.Remove(effect);
            Debug.Log($"Removed {effect.DisplayName} effect from {target.GetGameObject().name}");

            // Remove target entry if no more effects
            if (effects.Count == 0)
            {
                activeEffects.Remove(target);
            }
        }
    }

    /// <summary>
    /// Remove all effects of a certain type from a target
    /// </summary>
    /// <param name="target">Target to clear effects from</param>
    /// <param name="effectType">Type of effect to remove</param>
    public void RemoveEffectsOfType(ICombatTarget target, StatusEffectType effectType)
    {
        if (target == null)
            return;

        if (activeEffects.TryGetValue(target, out List<StatusEffect> effects))
        {
            // Find all effects of specified type
            List<StatusEffect> effectsToRemove = effects.Where(e => e.Type == effectType).ToList();

            // Remove them
            foreach (var effect in effectsToRemove)
            {
                effects.Remove(effect);
                Debug.Log($"Removed {effectType} effect from {target.GetGameObject().name}");
            }

            // Remove target entry if no more effects
            if (effects.Count == 0)
            {
                activeEffects.Remove(target);
            }
        }
    }

    /// <summary>
    /// Clear all non-persistent effects from all targets
    /// </summary>
    public void ClearNonPersistentEffects()
    {
        // Create a copy of the keys to avoid modification during iteration
        List<ICombatTarget> targets = new List<ICombatTarget>(activeEffects.Keys);

        foreach (var target in targets)
        {
            if (activeEffects.TryGetValue(target, out List<StatusEffect> effects))
            {
                // Find all non-persistent effects
                List<StatusEffect> effectsToRemove = effects.Where(e => !e.IsPersistent).ToList();

                // Remove them
                foreach (var effect in effectsToRemove)
                {
                    effects.Remove(effect);
                }

                // Remove target entry if no more effects
                if (effects.Count == 0)
                {
                    activeEffects.Remove(target);
                }
            }
        }

        Debug.Log("Cleared all non-persistent status effects");
    }

    /// <summary>
    /// Apply persistent effects that carry over from previous combats
    /// </summary>
    public void ApplyPersistentEffects()
    {
        foreach (var effect in persistentEffects)
        {
            // Skip if the target is null or not alive
            if (effect.Target == null || !effect.Target.IsAlive)
                continue;

            AddEffectToTarget(effect.Target, effect.Clone());
            Debug.Log($"Reapplied persistent {effect.DisplayName} to {effect.Target.GetGameObject().name}");
        }
    }

    /// <summary>
    /// Save effects that should persist between combats
    /// </summary>
    public void SavePersistentEffects()
    {
        persistentEffects.Clear();

        foreach (var targetEffects in activeEffects)
        {
            foreach (var effect in targetEffects.Value)
            {
                if (effect.IsPersistent)
                {
                    persistentEffects.Add(effect.Clone());
                    Debug.Log($"Saved persistent {effect.DisplayName} effect for {effect.Target.GetGameObject().name}");
                }
            }
        }
    }

    /// <summary>
    /// Clean up all effects at the end of combat
    /// </summary>
    public void CleanupEffects()
    {
        // Save persistent effects first
        SavePersistentEffects();

        // Then clear all active effects
        activeEffects.Clear();
        Debug.Log("Cleaned up all active status effects");
    }

    /// <summary>
    /// Get all active effects for a target
    /// </summary>
    /// <param name="target">Target to get effects for</param>
    /// <returns>List of active effects, or empty list if none</returns>
    public List<StatusEffect> GetEffectsForTarget(ICombatTarget target)
    {
        if (target == null || !activeEffects.TryGetValue(target, out List<StatusEffect> effects))
        {
            return new List<StatusEffect>();
        }

        return new List<StatusEffect>(effects);
    }

    /// <summary>
    /// Check if a target has a specific effect
    /// </summary>
    /// <param name="target">Target to check</param>
    /// <param name="effectType">Type of effect to check for</param>
    /// <returns>True if target has the effect</returns>
    public bool HasEffect(ICombatTarget target, StatusEffectType effectType)
    {
        return GetExistingEffect(target, effectType) != null;
    }

    /// <summary>
    /// Get an existing effect of a specific type on a target
    /// </summary>
    /// <param name="target">Target to check</param>
    /// <param name="effectType">Type of effect to find</param>
    /// <returns>The effect if found, null otherwise</returns>
    public StatusEffect GetExistingEffect(ICombatTarget target, StatusEffectType effectType)
    {
        if (target == null || !activeEffects.TryGetValue(target, out List<StatusEffect> effects))
        {
            return null;
        }

        return effects.FirstOrDefault(e => e.Type == effectType);
    }

    /// <summary>
    /// Check if an effect type is considered negative
    /// </summary>
    /// <param name="effectType">Type to check</param>
    /// <returns>True if it's a negative/debuff effect</returns>
    private bool IsNegativeEffect(StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Poison:
            case StatusEffectType.Burn:
            case StatusEffectType.Stun:
            case StatusEffectType.Weak:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Frail:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Try to block a negative effect with Artifact
    /// </summary>
    /// <param name="target">Target receiving the effect</param>
    /// <returns>True if effect was blocked</returns>
    private bool TryBlockNegativeEffect(ICombatTarget target)
    {
        StatusEffect artifact = GetExistingEffect(target, StatusEffectType.Artifact);

        if (artifact != null)
        {
            bool blocked = artifact.TryBlockNegativeEffect();

            // Remove artifact if consumed
            if (blocked && artifact.Intensity <= 0)
            {
                RemoveEffect(target, artifact);
            }

            return blocked;
        }

        return false;
    }

    /// <summary>
    /// Get all targets that should be processed for a turn
    /// </summary>
    /// <param name="isPlayerTurn">Whether it's the player's turn</param>
    /// <returns>List of targets to process</returns>
    /*private List<ICombatTarget> GetTargetsForTurn(bool isPlayerTurn)
    {
        List<ICombatTarget> targets = new List<ICombatTarget>();

        if (combatManager != null)
        {
            // Use Player instead of PlayerController
            Player player = combatManager.player; // Access the public player field
            if (player != null && player.IsAlive)
            {
                targets.Add(player);
            }

            // Add enemies if it's the enemy's turn
            if (!isPlayerTurn)
            {
                foreach (var enemy in combatManager.CurrentCombatState.GetAliveEnemies())
                {
                    if (enemy != null && enemy.IsAlive)
                    {
                        targets.Add(enemy);
                    }
                }
            }
        }

        return targets;
    }*/

    /// <summary>
    /// Add an effect to a target's active effects list
    /// </summary>
    /// <param name="target">Target to add effect to</param>
    /// <param name="effect">Effect to add</param>
    private void AddEffectToTarget(ICombatTarget target, StatusEffect effect)
    {
        if (!activeEffects.TryGetValue(target, out List<StatusEffect> effects))
        {
            effects = new List<StatusEffect>();
            activeEffects[target] = effects;
        }

        effects.Add(effect);
    }

    /// <summary>
    /// Calculate modified damage based on all effects
    /// </summary>
    /// <param name="source">Entity dealing damage</param>
    /// <param name="target">Entity receiving damage</param>
    /// <param name="baseDamage">Original damage amount</param>
    /// <returns>Modified damage amount</returns>
    public int CalculateModifiedDamage(ICombatTarget source, ICombatTarget target, int baseDamage)
    {
        int modifiedDamage = baseDamage;

        // Apply source effects (like Strength, Weak)
        if (source != null)
        {
            var sourceEffects = GetEffectsForTarget(source);
            foreach (var effect in sourceEffects)
            {
                modifiedDamage = effect.ModifyDamageDealt(modifiedDamage);
            }
        }

        // Apply target effects (like Vulnerable)
        if (target != null)
        {
            var targetEffects = GetEffectsForTarget(target);
            foreach (var effect in targetEffects)
            {
                modifiedDamage = effect.ModifyDamageReceived(modifiedDamage);
            }
        }

        return Mathf.Max(0, modifiedDamage);
    }

    /// <summary>
    /// Calculate modified block based on effects
    /// </summary>
    /// <param name="target">Entity gaining block</param>
    /// <param name="baseBlock">Original block amount</param>
    /// <returns>Modified block amount</returns>
    public int CalculateModifiedBlock(ICombatTarget target, int baseBlock)
    {
        int modifiedBlock = baseBlock;

        if (target != null)
        {
            var targetEffects = GetEffectsForTarget(target);
            foreach (var effect in targetEffects)
            {
                modifiedBlock = effect.ModifyBlockGained(modifiedBlock);
            }
        }

        return Mathf.Max(0, modifiedBlock);
    }

    /// <summary>
    /// Trigger on-attacked effects when a target is hit
    /// </summary>
    /// <param name="target">Entity that was attacked</param>
    /// <param name="attacker">Entity that attacked</param>
    public void TriggerOnAttackedEffects(ICombatTarget target, ICombatTarget attacker)
    {
        if (target == null)
            return;

        var targetEffects = GetEffectsForTarget(target);
        foreach (var effect in targetEffects)
        {
            effect.OnAttacked(attacker);
        }
    }
}