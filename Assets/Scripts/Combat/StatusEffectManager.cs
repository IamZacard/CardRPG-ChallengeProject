using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class StatusEffectManager : MonoBehaviour
{
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    public void AddEffect(StatusEffect effect)
    {
        activeEffects.Add(effect);
        effect.Apply(GetComponent<Entity>());
    }

    public void RemoveEffect(StatusEffect effect)
    {
        activeEffects.Remove(effect);
    }

    public void UpdateEffects()
    {
        // Placeholder; would decrease duration and remove expired effects
        foreach (var effect in activeEffects.ToArray())
        {
            Debug.Log($"Updating {effect.effectName}.");
        }
    }
}