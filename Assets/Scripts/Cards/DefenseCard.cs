using UnityEngine;

public class DefenseCard : Card
{
    public DefenseCard(CardData data) : base(data)
    {
        if (data.cardType != CardType.Defense)
        {
            Debug.LogWarning($"Card {data.cardName} is not a Defense card but using DefenseCard class");
        }
    }

    // Special defense card functionality
}