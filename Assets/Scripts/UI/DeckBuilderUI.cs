using UnityEngine;

public class DeckBuilderUI : MonoBehaviour
{
    public void DisplayDeck(Deck deck)
    {
        Debug.Log($"Displaying deck with {deck.cards.Count} cards.");
        // Show deck contents
    }

    public void AddCardToDeck(CardData card)
    {
        FindObjectOfType<Deck>().cards.Add(card);
    }

    public void RemoveCardFromDeck(CardData card)
    {
        FindObjectOfType<Deck>().cards.Remove(card);
    }
}