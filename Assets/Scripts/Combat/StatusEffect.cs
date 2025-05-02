using System;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Enumeration of possible status effect types
/// </summary>
public enum StatusEffectType
{
    // Negative effects
    Poison,      // Damage at end of turn
    Burn,        // Damage at start of turn
    Stun,        // Skip next turn
    Weak,        // Reduced damage output
    Vulnerable,  // Take increased damage
    Frail,       // Reduced block gained

    // Positive effects
    Strength,    // Increased damage output
    Dexterity,   // Increased block gained
    Regen,       // Heal at end of turn
    Thorns,      // Deal damage when attacked
    Artifact,    // Block next negative status effect

    // Special effects
    Custom       // Custom effect defined by individual cards
}

/// <summary>
/// Data class representing an active status effect on a combat entity
/// </summary>
[Serializable]
public class StatusEffect
{
    // Basic properties
    public StatusEffectType Type { get; private set; }
    public int Duration { get; private set; }
    public int Intensity { get; private set; }
    public bool IsPersistent { get; private set; }
    public ICombatTarget Target { get; private set; }

    // Callback for custom effects
    private Action<ICombatTarget, int> customEffect;

    // Visual representation
    public Sprite Icon { get; private set; }
    public string DisplayName { get; private set; }
    public string Description { get; private set; }

    /// <summary>
    /// Creates a new status effect
    /// </summary>
    /// <param name="type">The type of status effect</param>
    /// <param name="target">The target the effect is applied to</param>
    /// <param name="intensity">How strong the effect is</param>
    /// <param name="duration">How many turns the effect lasts (-1 for until combat ends)</param>
    /// <param name="isPersistent">Whether the effect persists between combats</param>
    public StatusEffect(StatusEffectType type, ICombatTarget target, int intensity, int duration = -1, bool isPersistent = false)
    {
        Type = type;
        Target = target;
        Intensity = intensity;
        Duration = duration;
        IsPersistent = isPersistent;

        // Setup display properties based on type
        SetupDisplayProperties();
    }

    /// <summary>
    /// Creates a custom status effect with a specific callback
    /// </summary>
    /// <param name="name">Display name of the custom effect</param>
    /// <param name="description">Description of what the effect does</param>
    /// <param name="icon">Visual representation</param>
    /// <param name="target">Target the effect is applied to</param>
    /// <param name="intensity">Effect strength</param>
    /// <param name="duration">Duration in turns</param>
    /// <param name="effect">Callback to execute when effect triggers</param>
    public StatusEffect(string name, string description, Sprite icon, ICombatTarget target, int intensity, int duration, Action<ICombatTarget, int> effect)
    {
        Type = StatusEffectType.Custom;
        Target = target;
        Intensity = intensity;
        Duration = duration;
        IsPersistent = false;

        DisplayName = name;
        Description = description;
        Icon = icon;
        customEffect = effect;
    }

    /// <summary>
    /// Set up display properties based on the effect type
    /// </summary>
    private void SetupDisplayProperties()
    {
        switch (Type)
        {
            case StatusEffectType.Poison:
                DisplayName = "Poison";
                Description = $"Takes {Intensity} damage at the end of turn. Decreases by 1 each turn.";
                // Icon would be assigned from a sprite sheet or resources
                break;

            case StatusEffectType.Burn:
                DisplayName = "Burn";
                Description = $"Takes {Intensity} damage at the start of turn.";
                break;

            case StatusEffectType.Stun:
                DisplayName = "Stunned";
                Description = "Cannot take actions for the duration.";
                break;

            case StatusEffectType.Weak:
                DisplayName = "Weak";
                Description = "Deals 25% less damage.";
                break;

            case StatusEffectType.Vulnerable:
                DisplayName = "Vulnerable";
                Description = "Takes 50% more damage.";
                break;

            case StatusEffectType.Frail:
                DisplayName = "Frail";
                Description = "Gains 25% less Block.";
                break;

            case StatusEffectType.Strength:
                DisplayName = "Strength";
                Description = $"Deals {Intensity} additional damage.";
                break;

            case StatusEffectType.Dexterity:
                DisplayName = "Dexterity";
                Description = $"Gains {Intensity} additional Block.";
                break;

            case StatusEffectType.Regen:
                DisplayName = "Regeneration";
                Description = $"Heals {Intensity} HP at the end of turn.";
                break;

            case StatusEffectType.Thorns:
                DisplayName = "Thorns";
                Description = $"Returns {Intensity} damage when attacked.";
                break;

            case StatusEffectType.Artifact:
                DisplayName = "Artifact";
                Description = "Negates the next negative status effect.";
                break;

            default:
                DisplayName = "Unknown Effect";
                Description = "This effect is not properly configured.";
                break;
        }
    }

    /// <summary>
    /// Process the effect at the start of a turn
    /// </summary>
    /// <returns>True if the effect should be removed after processing</returns>
    public bool ProcessStartOfTurn()
    {
        bool shouldRemove = false;

        // Process effects that trigger at start of turn
        switch (Type)
        {
            case StatusEffectType.Burn:
                // Apply burn damage
                if (Target != null && Target.IsAlive)
                {
                    Target.TakeDamage(Intensity);
                    Debug.Log($"Burn effect deals {Intensity} damage to {Target.GetGameObject().name}");
                }
                break;

            case StatusEffectType.Custom:
                // Execute custom effect if it's a start-of-turn effect
                customEffect?.Invoke(Target, Intensity);
                break;
        }

        // Count down duration for timed effects
        if (Duration > 0)
        {
            Duration--;
            shouldRemove = Duration <= 0;
        }

        return shouldRemove;
    }

    /// <summary>
    /// Modify the intensity of the status effect
    /// </summary>
    /// <param name="amount">Amount to add (positive) or remove (negative)</param>
    /// <returns>The new intensity</returns>
    public int ModifyIntensity(int amount)
    {
        Intensity += amount;

        // Update description if needed
        SetupDisplayProperties();

        return Intensity;
    }

    /// <summary>
    /// Extend or reduce the duration of the status effect
    /// </summary>
    /// <param name="turns">Number of turns to add (positive) or remove (negative)</param>
    /// <returns>The new duration</returns>
    public int ModifyDuration(int turns)
    {
        // Only modify if not permanent
        if (Duration > 0)
        {
            Duration += turns;
            if (Duration < 0)
                Duration = 0;
        }

        return Duration;
    }

    /// <summary>
    /// Create a copy of this status effect with the same properties
    /// </summary>
    public StatusEffect Clone()
    {
        if (Type == StatusEffectType.Custom)
        {
            return new StatusEffect(DisplayName, Description, Icon, Target, Intensity, Duration, customEffect);
        }
        else
        {
            return new StatusEffect(Type, Target, Intensity, Duration, IsPersistent);
        }
    }

    /// <summary>
    /// Calculate the damage modifier from this effect
    /// </summary>
    /// <param name="baseDamage">The original damage amount</param>
    /// <returns>Modified damage amount</returns>
    public int ModifyDamageDealt(int baseDamage)
    {
        switch (Type)
        {
            case StatusEffectType.Weak:
                // 25% damage reduction
                return Mathf.FloorToInt(baseDamage * 0.75f);

            case StatusEffectType.Strength:
                // Add bonus damage
                return baseDamage + Intensity;

            default:
                return baseDamage;
        }
    }

    /// <summary>
    /// Calculate the damage modifier for incoming damage
    /// </summary>
    /// <param name="incomingDamage">Original damage amount</param>
    /// <returns>Modified damage amount</returns>
    public int ModifyDamageReceived(int incomingDamage)
    {
        switch (Type)
        {
            case StatusEffectType.Vulnerable:
                // 50% increased damage
                return Mathf.FloorToInt(incomingDamage * 1.5f);

            default:
                return incomingDamage;
        }
    }

    /// <summary>
    /// Calculate the block modifier from this effect
    /// </summary>
    /// <param name="baseBlock">Original block amount</param>
    /// <returns>Modified block amount</returns>
    public int ModifyBlockGained(int baseBlock)
    {
        switch (Type)
        {
            case StatusEffectType.Frail:
                // 25% block reduction
                return Mathf.FloorToInt(baseBlock * 0.75f);

            case StatusEffectType.Dexterity:
                // Add bonus block
                return baseBlock + Intensity;

            default:
                return baseBlock;
        }
    }

    /// <summary>
    /// Respond to being attacked (used for effects like Thorns)
    /// </summary>
    /// <param name="attacker">The entity that attacked</param>
    public void OnAttacked(ICombatTarget attacker)
    {
        switch (Type)
        {
            case StatusEffectType.Thorns:
                if (attacker != null && attacker.IsAlive)
                {
                    attacker.TakeDamage(Intensity);
                    Debug.Log($"Thorns deals {Intensity} damage to {attacker.GetGameObject().name}");
                }
                break;
        }
    }

    /// <summary>
    /// Check if this effect blocks a negative status effect
    /// </summary>
    /// <returns>True if the effect is consumed to block another effect</returns>
    public bool TryBlockNegativeEffect()
    {
        if (Type == StatusEffectType.Artifact && Intensity > 0)
        {
            Intensity--;
            Debug.Log($"Artifact blocked a negative status effect on {Target.GetGameObject().name}");
            return true;
        }

        return false;
    }


    /// <summary>
    /// Process the effect at the end of a turn
    /// </summary>
    /// <returns>True if the effect should be removed after processing</returns>
    public bool ProcessEndOfTurn()
    {
        bool shouldRemove = false;

        // Process effects that trigger at end of turn
        switch (Type)
        {
            case StatusEffectType.Poison:
                // Apply poison damage
                if (Target != null && Target.IsAlive)
                {
                    Target.TakeDamage(Intensity);
                    Debug.Log($"Poison effect deals {Intensity} damage to {Target.GetGameObject().name}");

                    // Reduce poison stack by 1
                    Intensity--;
                    shouldRemove = Intensity <= 0;
                }
                break;

            case StatusEffectType.Regen:
                // Apply healing
                if (Target != null && Target.IsAlive)
                {
                    Target.Heal(Intensity);
                    Debug.Log($"Regeneration heals {Target.GetGameObject().name} for {Intensity} HP");
                }
                break;

            case StatusEffectType.Custom:
                // Execute custom effect if it's an end-of-turn effect
                customEffect?.Invoke(Target, Intensity);
                break;
        }

        // Count down duration for timed effects
        if (Duration > 0)
        {
            Duration--;
            shouldRemove = shouldRemove || Duration <= 0;
        }

        return shouldRemove;
    }
}