using UnityEngine;

public class EffectCard : Card
{
    public StatusEffect effect;

    public override void Play()
    {
        // Placeholder logic; would apply the effect to a target
        Debug.Log($"Playing {cardName}, applying {effect.effectName} effect.");
    }
}