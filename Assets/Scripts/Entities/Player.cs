using UnityEngine;

public class Player : Entity
{
    public Deck deck;
    public Hand hand;
    public ActionPointManager apManager;

    void Start()
    {
        deck.Shuffle();
        for (int i = 0; i < 5; i++) // Draw initial hand
        {
            CardData card = deck.DrawCard();
            if (card != null) hand.AddCard(card);
        }
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }

    public void PlayCard(CardData cardData)
    {
        if (apManager.SpendActionPoints(cardData.cost))
        {
            // Instantiate the appropriate card behavior
            Card card = cardData.type switch
            {
                CardType.Attack => ScriptableObject.CreateInstance<AttackCard>(),
                CardType.Defense => ScriptableObject.CreateInstance<DefenseCard>(),
                CardType.Effect => ScriptableObject.CreateInstance<EffectCard>(),
                _ => null
            };
            if (card != null)
            {
                card.cardName = cardData.cardName;
                card.cost = cardData.cost;
                card.description = cardData.description;
                card.artwork = cardData.artwork;
                if (card is AttackCard attack) attack.damage = cardData.value;
                if (card is DefenseCard defense) defense.block = cardData.value;
                if (card is EffectCard effect) effect.effect = cardData.effect;

                card.Play();
                hand.RemoveCard(cardData);
            }
        }
    }
}