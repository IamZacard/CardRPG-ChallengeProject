using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages the player's hand during combat, including card layout and interactions
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private Transform handContainer;
    [SerializeField] private GameObject cardUIPrefab;

    [Header("Hand Settings")]
    [SerializeField] private int maxHandSize = 10;
    [SerializeField] private float cardSpacing = 0.5f;
    [SerializeField] private float handCurveHeight = 0.3f;
    [SerializeField] private float handWidth = 5f;
    [SerializeField] private float cardRotationMax = 20f;

    [Header("Energy")]
    [SerializeField] private int maxEnergy = 3;
    private int currentEnergy;

    // Current hand state
    private List<Card> cardsInHand = new List<Card>();
    private Dictionary<int, CardUI> cardUIInstances = new Dictionary<int, CardUI>();

    // Selected card tracking
    private CardUI selectedCard = null;

    // Events
    public event Action<int, int> OnEnergyChanged; // Current, Max
    public event Action<Card> OnCardAdded;
    public event Action<Card> OnCardRemoved;
    public event Action<Card> OnCardPlayed;
    public event Action<int> OnHandSizeChanged;

    private void Awake()
    {
        if (deckManager == null)
        {
            deckManager = FindObjectOfType<DeckManager>();
            if (deckManager == null)
            {
                Debug.LogError("HandManager: DeckManager reference is missing!");
            }
        }

        if (handContainer == null)
        {
            Debug.LogError("HandManager: Hand container reference is missing!");
        }
    }

    private void Start()
    {
        // Reset energy at the start
        ResetEnergy();
    }

    /// <summary>
    /// Resets the player's energy to maximum
    /// </summary>
    public void ResetEnergy()
    {
        SetEnergy(maxEnergy);
    }

    /// <summary>
    /// Sets the player's current energy
    /// </summary>
    public void SetEnergy(int value)
    {
        int oldEnergy = currentEnergy;
        currentEnergy = Mathf.Clamp(value, 0, maxEnergy);

        if (oldEnergy != currentEnergy)
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            UpdateCardPlayability();
        }
    }

    /// <summary>
    /// Modifies the player's current energy
    /// </summary>
    public void ModifyEnergy(int amount)
    {
        SetEnergy(currentEnergy + amount);
    }

    /// <summary>
    /// Draws a specified number of cards into the hand
    /// </summary>
    public void DrawCards(int count)
    {
        // Calculate how many cards we can actually draw based on hand size limit
        int availableSpace = maxHandSize - cardsInHand.Count;
        int cardsToActuallyDraw = Mathf.Min(count, availableSpace);

        if (cardsToActuallyDraw <= 0)
        {
            Debug.Log("Hand is full, cannot draw more cards");
            return;
        }

        // Draw cards from the deck
        List<Card> drawnCards = deckManager.DrawCards(cardsToActuallyDraw);

        // Add each card to the hand
        foreach (Card card in drawnCards)
        {
            AddCardToHand(card);
        }

        // Arrange the cards in hand
        ArrangeCardsInHand();
    }

    /// <summary>
    /// Adds a single card to the hand and creates its UI representation
    /// </summary>
    public void AddCardToHand(Card card)
    {
        if (card == null) return;

        // Check if hand is full
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.Log("Hand is full, cannot add more cards");
            return;
        }

        // Add card to hand
        cardsInHand.Add(card);

        // Create UI for the card
        CreateCardUI(card);

        // Notify listeners
        OnCardAdded?.Invoke(card);
        OnHandSizeChanged?.Invoke(cardsInHand.Count);

        // Update card playability based on current energy
        UpdateCardPlayability();
    }

    /// <summary>
    /// Creates the UI representation for a card
    /// </summary>
    private void CreateCardUI(Card card)
    {
        // Create card UI using factory
        CardUI cardUI = CardFactory.CreateCardUI(card, cardUIPrefab, handContainer);

        if (cardUI != null)
        {
            // Store reference to the UI
            cardUIInstances[card.instanceID] = cardUI;

            // Set up event listeners
            cardUI.OnCardClicked += HandleCardClicked;
            cardUI.OnCardHoverStart += HandleCardHoverStart;
            cardUI.OnCardHoverEnd += HandleCardHoverEnd;
            cardUI.OnCardDragStart += HandleCardDragStart;
            cardUI.OnCardDragEnd += HandleCardDragEnd;
        }
    }

    /// <summary>
    /// Removes a card from the hand
    /// </summary>
    public void RemoveCardFromHand(Card card)
    {
        if (card == null || !cardsInHand.Contains(card)) return;

        // Remove card from hand
        cardsInHand.Remove(card);

        // Destroy the UI
        if (cardUIInstances.TryGetValue(card.instanceID, out CardUI cardUI))
        {
            // Clean up event listeners
            cardUI.OnCardClicked -= HandleCardClicked;
            cardUI.OnCardHoverStart -= HandleCardHoverStart;
            cardUI.OnCardHoverEnd -= HandleCardHoverEnd;
            cardUI.OnCardDragStart -= HandleCardDragStart;
            cardUI.OnCardDragEnd -= HandleCardDragEnd;

            // Destroy the GameObject
            Destroy(cardUI.gameObject);
            cardUIInstances.Remove(card.instanceID);
        }

        // Notify listeners
        OnCardRemoved?.Invoke(card);
        OnHandSizeChanged?.Invoke(cardsInHand.Count);

        // Re-arrange remaining cards
        ArrangeCardsInHand();
    }

    /// <summary>
    /// Discards a card from hand to the discard pile
    /// </summary>
    public void DiscardCardFromHand(Card card)
    {
        if (card == null || !cardsInHand.Contains(card)) return;

        // Remove from hand
        RemoveCardFromHand(card);

        // Add to discard pile
        deckManager.DiscardCard(card);
    }

    /// <summary>
    /// Plays a card from hand (applies effects and discards/exhausts it)
    /// </summary>
    public void PlayCard(Card card)
    {
        if (card == null || !cardsInHand.Contains(card)) return;

        // Check if we have enough energy
        if (card.CurrentCost > currentEnergy)
        {
            Debug.Log($"Not enough energy to play {card.cardData.cardName}");
            return;
        }

        // Spend energy
        ModifyEnergy(-card.CurrentCost);

        // Notify listeners before removing from hand
        OnCardPlayed?.Invoke(card);

        // Remove from hand
        RemoveCardFromHand(card);

        // Here you would apply the card's effects
        // ApplyCardEffects(card);

        // For now, just discard the card
        deckManager.DiscardCard(card);

        // Update playability of remaining cards
        UpdateCardPlayability();
    }

    /// <summary>
    /// Discards all cards in hand
    /// </summary>
    public void DiscardHand()
    {
        List<Card> cardsToDiscard = new List<Card>(cardsInHand);

        foreach (Card card in cardsToDiscard)
        {
            DiscardCardFromHand(card);
        }
    }

    /// <summary>
    /// Arranges cards in a curved layout in the hand
    /// </summary>
    public void ArrangeCardsInHand()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 0) return;

        // Calculate spacing based on hand width and card count
        float effectiveSpacing = handWidth / Mathf.Max(1, cardCount - 1);
        if (cardCount == 1) effectiveSpacing = 0;

        // Calculate start position (centered)
        float startX = -effectiveSpacing * (cardCount - 1) / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            Card card = cardsInHand[i];
            if (!cardUIInstances.TryGetValue(card.instanceID, out CardUI cardUI))
                continue;

            // Calculate position on a curved arc
            float xPos = startX + i * effectiveSpacing;

            // Calculate y position (curved layout using quadratic function)
            float normalizedX = i / (float)(Mathf.Max(1, cardCount - 1)); // 0 to 1
            float normalizedXCentered = normalizedX * 2 - 1; // -1 to 1
            float yPos = -handCurveHeight * (normalizedXCentered * normalizedXCentered);

            // Calculate rotation (cards at edges are rotated more)
            float rotationAngle = normalizedXCentered * cardRotationMax;

            // Set position and rotation
            Vector3 targetPosition = new Vector3(xPos, yPos, 0);
            Quaternion targetRotation = Quaternion.Euler(0, 0, rotationAngle);

            // Animate to position
            cardUI.AnimateToPosition(targetPosition, targetRotation);
        }
    }

    /// <summary>
    /// Updates playability status of all cards based on current energy
    /// </summary>
    private void UpdateCardPlayability()
    {
        foreach (Card card in cardsInHand)
        {
            if (cardUIInstances.TryGetValue(card.instanceID, out CardUI cardUI))
            {
                bool playable = card.CanPlayWithEnergy(currentEnergy);
                cardUI.SetPlayable(playable);
            }
        }
    }

    #region Event Handlers

    private void HandleCardClicked(CardUI cardUI)
    {
        if (cardUI == null) return;

        Card card = cardUI.GetCard();
        if (card.CanPlayWithEnergy(currentEnergy))
        {
            // If the card is playable, play it
            PlayCard(card);
        }
        else
        {
            Debug.Log($"Not enough energy to play {card.cardData.cardName}");
            // Could add a visual/audio feedback here
        }
    }

    private void HandleCardHoverStart(CardUI cardUI)
    {
        if (cardUI == null) return;

        // You could implement preview functionality here
        Debug.Log($"Card hovered: {cardUI.GetCard().cardData.cardName}");
    }

    private void HandleCardHoverEnd(CardUI cardUI)
    {
        if (cardUI == null) return;

        // Clean up any preview functionality
    }

    private void HandleCardDragStart(CardUI cardUI)
    {
        if (cardUI == null) return;

        selectedCard = cardUI;
        Debug.Log($"Started dragging card: {cardUI.GetCard().cardData.cardName}");
    }

    private void HandleCardDragEnd(CardUI cardUI)
    {
        if (cardUI == null) return;

        Debug.Log($"Stopped dragging card: {cardUI.GetCard().cardData.cardName}");

        // Check if card was dragged to a valid play area
        // This would depend on your specific UI layout

        // For now, we'll just play the card if it was selected and is playable
        if (selectedCard == cardUI && cardUI.GetCard().CanPlayWithEnergy(currentEnergy))
        {
            PlayCard(cardUI.GetCard());
        }

        selectedCard = null;
    }

    #endregion

    /// <summary>
    /// Gets the current number of cards in hand
    /// </summary>
    public int GetHandSize()
    {
        return cardsInHand.Count;
    }

    /// <summary>
    /// Gets the current energy value
    /// </summary>
    public int GetCurrentEnergy()
    {
        return currentEnergy;
    }

    /// <summary>
    /// Gets the maximum energy value
    /// </summary>
    public int GetMaxEnergy()
    {
        return maxEnergy;
    }

    /// <summary>
    /// Gets all cards currently in hand
    /// </summary>
    public List<Card> GetCardsInHand()
    {
        return new List<Card>(cardsInHand);
    }
}