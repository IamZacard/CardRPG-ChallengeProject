using UnityEngine;

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

public abstract class StatusEffect : MonoBehaviour
{
    public int Duration { get; protected set; }

    public string effectName { get; protected set; }
    public abstract void Apply(Entity target);
    public abstract void Remove(Entity target);
}