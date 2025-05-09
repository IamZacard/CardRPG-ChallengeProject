using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages the player's card deck during combat, including draw and discard piles
/// </summary>
public class DeckManager : MonoBehaviour
{
    [Header("Default Deck")]
    [SerializeField] private List<CardData> defaultCards = new List<CardData>();

    // Card piles
    private List<Card> drawPile = new List<Card>();
    private List<Card> discardPile = new List<Card>();
    private List<Card> exhaustPile = new List<Card>();

    // Events
    public event Action OnDrawPileChanged;
    public event Action OnDiscardPileChanged;
    public event Action OnExhaustPileChanged;
    public event Action<Card> OnCardDrawn;
    public event Action<Card> OnCardDiscarded;
    public event Action<Card> OnCardExhausted;
    public event Action OnDeckShuffled;

    private void Awake()
    {
        // For debugging purposes
        if (defaultCards.Count == 0)
        {
            Debug.LogWarning("DeckManager: No default cards assigned!");
        }
    }

    /// <summary>
    /// Initializes the deck with default cards
    /// </summary>
    public void InitializeDefaultDeck()
    {
        // Clear all piles
        drawPile.Clear();
        discardPile.Clear();
        exhaustPile.Clear();

        // Create the default cards
        foreach (CardData cardData in defaultCards)
        {
            Card card = CardFactory.CreateCard(cardData);
            if (card != null)
            {
                drawPile.Add(card);
            }
        }

        // Shuffle the draw pile
        ShuffleDrawPile();

        Debug.Log($"Initialized default deck with {drawPile.Count} cards");
        OnDrawPileChanged?.Invoke();
    }

    /// <summary>
    /// Initializes the deck with a custom set of cards
    /// </summary>
    public void InitializeDeck(List<CardData> cardDataList)
    {
        // Clear all piles
        drawPile.Clear();
        discardPile.Clear();
        exhaustPile.Clear();

        // Create cards from the data
        List<Card> cards = CardFactory.CreateCards(cardDataList);
        drawPile.AddRange(cards);

        // Shuffle the draw pile
        ShuffleDrawPile();

        Debug.Log($"Initialized deck with {drawPile.Count} cards");
        OnDrawPileChanged?.Invoke();
    }

    /// <summary>
    /// Shuffles the current draw pile
    /// </summary>
    public void ShuffleDrawPile()
    {
        // Fisher-Yates shuffle algorithm
        System.Random rng = new System.Random();
        int n = drawPile.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card temp = drawPile[k];
            drawPile[k] = drawPile[n];
            drawPile[n] = temp;
        }

        Debug.Log("Draw pile shuffled");
        OnDeckShuffled?.Invoke();
    }

    /// <summary>
    /// Reshuffles the discard pile into the draw pile
    /// </summary>
    public void ReshuffleDiscardIntoDraw()
    {
        // Add all cards from discard pile to draw pile
        drawPile.AddRange(discardPile);
        discardPile.Clear();

        // Shuffle the draw pile
        ShuffleDrawPile();

        Debug.Log("Reshuffled discard pile into draw pile");
        OnDiscardPileChanged?.Invoke();
        OnDrawPileChanged?.Invoke();
    }

    /// <summary>
    /// Draws a specified number of cards from the draw pile
    /// </summary>
    /// <param name="count">Number of cards to draw</param>
    /// <returns>List of drawn cards</returns>
    public List<Card> DrawCards(int count)
    {
        List<Card> drawnCards = new List<Card>();

        for (int i = 0; i < count; i++)
        {
            // Check if draw pile is empty
            if (drawPile.Count == 0)
            {
                // If discard pile is also empty, we can't draw any more cards
                if (discardPile.Count == 0)
                {
                    Debug.Log("No more cards to draw");
                    break;
                }

                // Reshuffle discard pile into draw pile
                ReshuffleDiscardIntoDraw();
            }

            // Draw the top card
            Card card = drawPile[0];
            drawPile.RemoveAt(0);
            drawnCards.Add(card);

            // Notify listeners
            OnCardDrawn?.Invoke(card);
        }

        if (drawnCards.Count > 0)
        {
            Debug.Log($"Drew {drawnCards.Count} cards");
            OnDrawPileChanged?.Invoke();
        }

        return drawnCards;
    }

    /// <summary>
    /// Discards a card to the discard pile
    /// </summary>
    /// <param name="card">Card to discard</param>
    public void DiscardCard(Card card)
    {
        if (card == null) return;

        discardPile.Add(card);
        Debug.Log($"Discarded card: {card.cardData.cardName} (ID: {card.instanceID})");

        OnCardDiscarded?.Invoke(card);
        OnDiscardPileChanged?.Invoke();
    }

    /// <summary>
    /// Discards multiple cards to the discard pile
    /// </summary>
    /// <param name="cards">List of cards to discard</param>
    public void DiscardCards(List<Card> cards)
    {
        if (cards == null || cards.Count == 0) return;

        foreach (Card card in cards)
        {
            DiscardCard(card);
        }
    }

    /// <summary>
    /// Moves a card to the exhaust pile (removed from play)
    /// </summary>
    /// <param name="card">Card to exhaust</param>
    public void ExhaustCard(Card card)
    {
        if (card == null) return;

        exhaustPile.Add(card);
        Debug.Log($"Exhausted card: {card.cardData.cardName} (ID: {card.instanceID})");

        OnCardExhausted?.Invoke(card);
        OnExhaustPileChanged?.Invoke();
    }

    /// <summary>
    /// Adds a new card directly to the discard pile
    /// </summary>
    /// <param name="cardData">Card data to create</param>
    public void AddCardToDiscard(CardData cardData)
    {
        Card card = CardFactory.CreateCard(cardData);
        if (card != null)
        {
            discardPile.Add(card);
            Debug.Log($"Added card to discard: {card.cardData.cardName} (ID: {card.instanceID})");
            OnDiscardPileChanged?.Invoke();
        }
    }

    /// <summary>
    /// Adds a new card directly to the draw pile
    /// </summary>
    /// <param name="cardData">Card data to create</param>
    /// <param name="shuffle">Whether to shuffle the draw pile afterward</param>
    public void AddCardToDraw(CardData cardData, bool shuffle = true)
    {
        Card card = CardFactory.CreateCard(cardData);
        if (card != null)
        {
            drawPile.Add(card);
            Debug.Log($"Added card to draw pile: {card.cardData.cardName} (ID: {card.instanceID})");

            if (shuffle)
            {
                ShuffleDrawPile();
            }
            else
            {
                OnDrawPileChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets all cards in the draw pile
    /// </summary>
    public List<Card> GetDrawPile()
    {
        return new List<Card>(drawPile);
    }

    /// <summary>
    /// Gets all cards in the discard pile
    /// </summary>
    public List<Card> GetDiscardPile()
    {
        return new List<Card>(discardPile);
    }

    /// <summary>
    /// Gets all cards in the exhaust pile
    /// </summary>
    public List<Card> GetExhaustPile()
    {
        return new List<Card>(exhaustPile);
    }

    /// <summary>
    /// Gets the number of cards in the draw pile
    /// </summary>
    public int GetDrawPileCount()
    {
        return drawPile.Count;
    }

    /// <summary>
    /// Gets the number of cards in the discard pile
    /// </summary>
    public int GetDiscardPileCount()
    {
        return discardPile.Count;
    }

    /// <summary>
    /// Gets the number of cards in the exhaust pile
    /// </summary>
    public int GetExhaustPileCount()
    {
        return exhaustPile.Count;
    }
}