using UnityEngine;

public class AttackCard : Card
{
    public AttackCard(CardData data) : base(data)
    {
        if (data.cardType != CardType.Attack)
        {
            Debug.LogWarning($"Card {data.cardName} is not an Attack card but using AttackCard class");
        }
    }

    // Special attack card functionality
}