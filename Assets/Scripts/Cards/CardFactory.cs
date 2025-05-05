// CardFactory.cs - Factory for creating card instances
using UnityEngine;
using System.Collections.Generic;

public static class CardFactory
{
    public static Card CreateCard(CardData cardData)
    {
        switch (cardData.cardType)
        {
            case CardType.Attack:
                return new AttackCard(cardData);
            case CardType.Defense:
                return new DefenseCard(cardData);
            case CardType.Effect:
                return new EffectCard(cardData);
            default:
                Debug.LogError($"Unknown card type for card {cardData.cardName}");
                return null;
        }
    }

    public static List<Card> CreateStarterDeck()
    {
        List<Card> starterDeck = new List<Card>();

        // Load starter cards from Resources
        CardData[] starterCardData = Resources.LoadAll<CardData>("CardData/StarterCards");

        foreach (var cardData in starterCardData)
        {
            // Add appropriate number of each card
            int count = 1;
            if (cardData.cardName == "Strike" || cardData.cardName == "Defend")
            {
                count = 5; // Standard starter deck often has multiple basic cards
            }

            for (int i = 0; i < count; i++)
            {
                starterDeck.Add(CreateCard(cardData));
            }
        }

        return starterDeck;
    }
}