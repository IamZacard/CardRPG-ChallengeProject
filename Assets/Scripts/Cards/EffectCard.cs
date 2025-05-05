using UnityEngine;

public class EffectCard : Card
{
    public EffectCard(CardData data) : base(data)
    {
        if (data.cardType != CardType.Effect)
        {
            Debug.LogWarning($"Card {data.cardName} is not an Effect card but using EffectCard class");
        }
    }

    // Special effect card functionality
}