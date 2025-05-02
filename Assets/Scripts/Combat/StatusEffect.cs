using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class StatusEffect : ScriptableObject
{
    public string effectName;
    public int duration;

    public void Apply(Entity target)
    {
        Debug.Log($"Applying {effectName} to {target.entityName} for {duration} turns.");
    }
}