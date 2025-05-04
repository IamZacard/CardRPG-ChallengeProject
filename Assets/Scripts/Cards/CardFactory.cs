using UnityEngine;

public class CardFactory : MonoBehaviour
{
    public Card CreateCard(CardData data)
    {
        Card card = data.Type switch
        {
            CardType.Attack => gameObject.AddComponent<AttackCard>(),
            CardType.Defense => gameObject.AddComponent<DefenseCard>(),
            //CardType.Effect => gameObject.AddComponent<EffectCard>(),
            _ => throw new System.ArgumentException("Unknown card type")
        };
        card.Initialize(data);
        return card;
    }

    public void UpgradeCard(Card card, int damageIncrease)
    {
        card.Data.Damage += damageIncrease; // Simplified upgrade example
    }
}