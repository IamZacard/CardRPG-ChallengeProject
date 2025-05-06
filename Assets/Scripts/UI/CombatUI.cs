using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all UI elements related to combat including cards, player/enemy status, energy, etc.
/// </summary>
public class CombatUI : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private GameObject playerStatusPanel;
    [SerializeField] private Image playerHealthBar;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerBlockText;
    [SerializeField] private Image[] energyOrbs;
    [SerializeField] private GameObject[] statusEffectIcons;

    [Header("Enemy UI")]
    [SerializeField] private Transform enemyStatusContainer;
    [SerializeField] private GameObject enemyStatusPrefab;

    [Header("Cards UI")]
    [SerializeField] private Transform handContainer;
    [SerializeField] private Transform drawPileTransform;
    [SerializeField] private Transform discardPileTransform;
    [SerializeField] private TextMeshProUGUI drawPileCountText;
    [SerializeField] private TextMeshProUGUI discardPileCountText;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private float cardSpacing = 30f;
    [SerializeField] private float selectedCardOffset = 20f;

    [Header("Turn UI")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI turnCounterText;
    [SerializeField] private GameObject playerTurnIndicator;
    [SerializeField] private GameObject enemyTurnIndicator;

    [Header("Target Selection")]
    [SerializeField] private GameObject targetCursor;
    [SerializeField] private Color targetHighlightColor = Color.red;
    [SerializeField] private Color defaultEntityColor = Color.white;

    // Callbacks for interaction
    public Action<Card> OnCardSelected { get; set; }
    public Action<Entity> OnEntitySelected { get; set; }
    public Action OnEndTurnButtonClicked { get; set; }

    // State tracking
    private CombatState combatState;
    private bool isTargetingMode = false;
    private Dictionary<Enemy, GameObject> enemyStatusObjects = new Dictionary<Enemy, GameObject>();
    private List<GameObject> cardObjects = new List<GameObject>();
    private Card selectedCard;
    private Entity highlightedEntity;

    private void Awake()
    {
        // Set up event listeners
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(HandleEndTurnClicked);
        }
    }

    private void Start()
    {
        // Hide targeting UI elements initially
        if (targetCursor != null)
            targetCursor.SetActive(false);
    }

    private void Update()
    {
        // Handle player input for target selection
        if (isTargetingMode && Input.GetMouseButtonDown(0))
        {
            // Perform raycast to detect entity selection
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                // Check if hit entity is valid target
                Entity entity = hit.collider.GetComponent<Entity>();
                if (entity != null)
                {
                    OnEntitySelected?.Invoke(entity);
                }
            }
        }

        // Handle escape to cancel targeting mode
        if (isTargetingMode && Input.GetKeyDown(KeyCode.Escape))
        {
            EnableTargetMode(false);
        }
    }

    /// <summary>
    /// Prepare the UI for a new combat encounter
    /// </summary>
    public void PrepareForCombat()
    {
        // Clear any existing UI elements
        ClearCardHand();
        ClearEnemyStatusObjects();

        // Reset UI state
        isTargetingMode = false;
        if (targetCursor != null)
            targetCursor.SetActive(false);

        // Enable end turn button
        if (endTurnButton != null)
            endTurnButton.interactable = true;

        // Set initial turn counter
        if (turnCounterText != null)
            turnCounterText.text = "Turn 1";

        // Initialize player status panel
        UpdatePlayerStatus(null, 0, 0, 0);

        Debug.Log("Combat UI prepared for new encounter");
    }

    /// <summary>
    /// Connect to the combat state to receive updates
    /// </summary>
    public void ConnectToCombatState(CombatState state)
    {
        combatState = state;

        // Create enemy status UI elements
        foreach (Enemy enemy in state.Enemies)
        {
            CreateEnemyStatusUI(enemy);
        }

        // Update all UI elements with initial state
        UpdateUI(state);

        Debug.Log("Combat UI connected to combat state");
    }

    /// <summary>
    /// Update all UI elements based on current combat state
    /// </summary>
    public void UpdateUI(CombatState state)
    {
        if (state == null)
            return;

        // Update player status
        UpdatePlayerStatus(
            state.Player,
            state.Player.CurrentHealth,
            state.Player.MaxHealth,
            state.Player.CurrentBlock
        );

        // Update energy display
        UpdateEnergyOrbs(state.Player.CurrentActionPoints, state.Player.BaseActionPoints);
        //UpdateEnergyOrbs(state.Player.CurrentActionPoints, state.Player.MaxActionPoints);

        // Update enemy status displays
        foreach (Enemy enemy in state.Enemies)
        {
            UpdateEnemyStatus(enemy);
        }

        // Update card counts
        if (drawPileCountText != null && state.DrawPile != null)
            drawPileCountText.text = state.DrawPile.Count.ToString();

        if (discardPileCountText != null && state.DiscardPile != null)
            discardPileCountText.text = state.DiscardPile.Count.ToString();

        // Update turn indicator
        if (playerTurnIndicator != null)
            playerTurnIndicator.SetActive(state.IsPlayerTurn);

        if (enemyTurnIndicator != null)
            enemyTurnIndicator.SetActive(!state.IsPlayerTurn);

        // Update turn counter
        if (turnCounterText != null)
            turnCounterText.text = $"Turn {state.TurnNumber}";

        // Update hand display if we have references
        UpdateCardHand(state);
    }

    /// <summary>
    /// Enable/disable target selection mode
    /// </summary>
    public void EnableTargetMode(bool enabled)
    {
        isTargetingMode = enabled;

        if (targetCursor != null)
            targetCursor.SetActive(enabled);

        // Disable card interaction during targeting
        SetCardsInteractable(!enabled);

        // Disable end turn button during targeting
        if (endTurnButton != null)
            endTurnButton.interactable = !enabled;

        Debug.Log($"Target mode {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Highlight an entity for targeting
    /// </summary>
    public void HighlightEntity(Enemy enemy)
    {
        if (enemy == null)
            return;

        // Reset previous highlight if any
        if (highlightedEntity != null && highlightedEntity is Enemy previousEnemy)
        {
            UnhighlightEntity(previousEnemy);
        }

        // Set new highlight
        highlightedEntity = enemy;

        // Apply visual highlight effect
        SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = targetHighlightColor;
        }

        // Move target cursor to enemy if available
        if (targetCursor != null)
        {
            targetCursor.transform.position = enemy.transform.position + Vector3.up * 0.5f;
        }
    }

    /// <summary>
    /// Remove highlight from an entity
    /// </summary>
    public void UnhighlightEntity(Enemy enemy)
    {
        if (enemy == null)
            return;

        // Reset visual highlight
        SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = defaultEntityColor;
        }

        // Reset tracking if this is the currently highlighted entity
        if (highlightedEntity == enemy)
        {
            highlightedEntity = null;
        }
    }

    /// <summary>
    /// Update the card hand display based on current player hand
    /// </summary>
    private void UpdateCardHand(CombatState state)
    {
        // This method needs to be implemented once you have a hand system
        // For now, we just display placeholder cards

        // Clear existing cards
        ClearCardHand();

        // Get cards from the player's hand - this will need to be updated based on your implementation
        List<Card> playerHand = GetPlayerHand(state);

        // Create card UI objects
        if (handContainer != null && cardPrefab != null && playerHand != null)
        {
            float totalWidth = playerHand.Count * cardSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < playerHand.Count; i++)
            {
                Card card = playerHand[i];

                // Instantiate card object
                GameObject cardObject = Instantiate(cardPrefab, handContainer);

                // Position card
                RectTransform rectTransform = cardObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float xPos = startX + i * cardSpacing;
                    rectTransform.anchoredPosition = new Vector2(xPos, 0);
                }

                // Set up card UI with card data
                CardUI cardUI = cardObject.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.SetupCard(card);

                    // Add click handler
                    Button cardButton = cardObject.GetComponent<Button>();
                    if (cardButton != null)
                    {
                        int index = i; // Capture index for the lambda
                        cardButton.onClick.AddListener(() => OnCardClicked(playerHand[index]));
                    }
                }

                // Add to tracking list
                cardObjects.Add(cardObject);
            }
        }
    }

    // This is a placeholder method until you implement your hand system
    private List<Card> GetPlayerHand(CombatState state)
    {
        // This should be replaced with actual implementation
        // This is just a placeholder that returns an empty list
        return new List<Card>();
    }

    /// <summary>
    /// Handle card selection
    /// </summary>
    private void OnCardClicked(Card card)
    {
        if (isTargetingMode)
            return;

        selectedCard = card;

        // Raise event for combat manager
        OnCardSelected?.Invoke(card);

        // Visual feedback (raise selected card)
        VisuallySelectCard(card);
    }

    /// <summary>
    /// Handle end turn button click
    /// </summary>
    private void HandleEndTurnClicked()
    {
        // Invoke the callback
        OnEndTurnButtonClicked?.Invoke();

        Debug.Log("End turn button clicked");
    }

    /// <summary>
    /// Update player status UI elements
    /// </summary>
    private void UpdatePlayerStatus(Player player, int currentHealth, int maxHealth, int block)
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.fillAmount = (float)currentHealth / maxHealth;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text = $"{currentHealth}/{maxHealth}";
        }

        if (playerBlockText != null)
        {
            playerBlockText.text = block > 0 ? block.ToString() : "";
            playerBlockText.gameObject.SetActive(block > 0);
        }

        // Update status effects (would require additional implementation)
    }

    /// <summary>
    /// Update the energy/action points display
    /// </summary>
    private void UpdateEnergyOrbs(int current, int max)
    {
        if (energyOrbs == null || energyOrbs.Length == 0)
            return;

        // Enable orbs based on current energy
        for (int i = 0; i < energyOrbs.Length; i++)
        {
            if (energyOrbs[i] != null)
            {
                energyOrbs[i].gameObject.SetActive(i < max);

                // Change color/fill based on whether energy is spent
                Color orbColor = (i < current) ? Color.yellow : Color.gray;
                energyOrbs[i].color = orbColor;
            }
        }
    }

    /// <summary>
    /// Create UI element for an enemy
    /// </summary>
    private void CreateEnemyStatusUI(Enemy enemy)
    {
        if (enemy == null || enemyStatusContainer == null || enemyStatusPrefab == null)
            return;

        // Create status object
        GameObject statusObject = Instantiate(enemyStatusPrefab, enemyStatusContainer);

        // Position above enemy
        statusObject.transform.position = enemy.transform.position + Vector3.up * 1.2f;

        // Track in dictionary
        enemyStatusObjects[enemy] = statusObject;

        // Initial update
        UpdateEnemyStatusUI(enemy, statusObject);
    }

    /// <summary>
    /// Update the status UI for a specific enemy
    /// </summary>
    private void UpdateEnemyStatus(Enemy enemy)
    {
        if (enemy == null || !enemyStatusObjects.ContainsKey(enemy))
            return;

        GameObject statusObject = enemyStatusObjects[enemy];
        UpdateEnemyStatusUI(enemy, statusObject);
    }

    /// <summary>
    /// Update an enemy status UI with current data
    /// </summary>
    private void UpdateEnemyStatusUI(Enemy enemy, GameObject statusObject)
    {
        if (enemy == null || statusObject == null)
            return;

        // Get components
        Image healthBar = statusObject.transform.Find("HealthBar")?.GetComponent<Image>();
        TextMeshProUGUI healthText = statusObject.transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI blockText = statusObject.transform.Find("BlockText")?.GetComponent<TextMeshProUGUI>();
        Image intentIcon = statusObject.transform.Find("IntentIcon")?.GetComponent<Image>();

        // Update health
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)enemy.CurrentHealth / enemy.MaxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{enemy.CurrentHealth}/{enemy.MaxHealth}";
        }

        // Update block
        if (blockText != null)
        {
            blockText.text = enemy.CurrentBlock > 0 ? enemy.CurrentBlock.ToString() : "";
            blockText.gameObject.SetActive(enemy.CurrentBlock > 0);
        }

        // Update intent (would require additional implementation)
        /*if (intentIcon != null && enemy.CurrentIntent != null)
        {
            // Example of updating intent icon based on enemy intent
            // intentIcon.sprite = GetIntentSprite(enemy.CurrentIntent);
        }*/
    }

    /// <summary>
    /// Clear all card objects from the hand
    /// </summary>
    private void ClearCardHand()
    {
        foreach (GameObject cardObject in cardObjects)
        {
            Destroy(cardObject);
        }

        cardObjects.Clear();
    }

    /// <summary>
    /// Clear all enemy status objects
    /// </summary>
    private void ClearEnemyStatusObjects()
    {
        foreach (GameObject statusObject in enemyStatusObjects.Values)
        {
            Destroy(statusObject);
        }

        enemyStatusObjects.Clear();
    }

    /// <summary>
    /// Visually select a card (move it up)
    /// </summary>
    private void VisuallySelectCard(Card card)
    {
        // Find the card object
        for (int i = 0; i < cardObjects.Count; i++)
        {
            CardUI cardUI = cardObjects[i].GetComponent<CardUI>();
            if (cardUI != null && cardUI.Card == card)
            {
                // Move this card up
                RectTransform rectTransform = cardObjects[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition += new Vector2(0, selectedCardOffset);
                }
            }
            else
            {
                // Reset other cards
                RectTransform rectTransform = cardObjects[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Vector2 position = rectTransform.anchoredPosition;
                    rectTransform.anchoredPosition = new Vector2(position.x, 0);
                }
            }
        }
    }

    /// <summary>
    /// Enable/disable interaction with cards
    /// </summary>
    private void SetCardsInteractable(bool interactable)
    {
        foreach (GameObject cardObject in cardObjects)
        {
            Button cardButton = cardObject.GetComponent<Button>();
            if (cardButton != null)
            {
                cardButton.interactable = interactable;
            }
        }
    }
}