using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Manages the overall combat flow, integrating all subsystems
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private HandManager handManager;
    [SerializeField] private CombatLoader combatLoader;

    [Header("UI References")]
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private Button endTurnButton;

    [Header("Combat Settings")]
    [SerializeField] private int initialDrawCount = 5;
    [SerializeField] private int normalDrawCount = 5;

    // Combat state
    private int currentTurn = 0;
    private bool isPlayerTurn = true;
    private bool isGameOver = false;

    // Events
    public event Action OnTurnStart;
    public event Action OnTurnEnd;
    public event Action<int> OnTurnChanged;
    public event Action OnPlayerTurnStart;
    public event Action OnPlayerTurnEnd;
    public event Action OnEnemyTurnStart;
    public event Action OnEnemyTurnEnd;
    public event Action OnCombatStart;
    public event Action OnCombatEnd;
    public event Action<bool> OnCombatResult; // true = victory, false = defeat

    private void Awake()
    {
        SetupReferences();
        RegisterEvents();
    }

    private void Start()
    {
        // Start the combat sequence after a short delay
        StartCoroutine(StartCombatCoroutine());
    }

    /// <summary>
    /// Finds and sets up references if not assigned in inspector
    /// </summary>
    private void SetupReferences()
    {
        if (deckManager == null)
            deckManager = FindObjectOfType<DeckManager>();

        if (handManager == null)
            handManager = FindObjectOfType<HandManager>();

        if (combatLoader == null)
            combatLoader = FindObjectOfType<CombatLoader>();

        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(EndPlayerTurn);
    }

    /// <summary>
    /// Registers events from other systems
    /// </summary>
    private void RegisterEvents()
    {
        if (handManager != null)
        {
            handManager.OnEnergyChanged += UpdateEnergyUI;
        }
    }

    /// <summary>
    /// Coroutine to start the combat sequence
    /// </summary>
    private IEnumerator StartCombatCoroutine()
    {
        // Wait for combat setup to complete
        yield return new WaitForSeconds(0.5f);

        // Initialize the player's deck
        deckManager.InitializeDefaultDeck();

        // Signal combat start
        isGameOver = false;
        OnCombatStart?.Invoke();

        // Start the first turn
        StartCoroutine(StartNewTurn());
    }

    /// <summary>
    /// Starts a new turn sequence
    /// </summary>
    private IEnumerator StartNewTurn()
    {
        currentTurn++;
        OnTurnChanged?.Invoke(currentTurn);
        OnTurnStart?.Invoke();

        // Update turn UI
        if (turnText != null)
            turnText.text = $"Turn {currentTurn}";

        if (isPlayerTurn)
        {
            yield return StartCoroutine(StartPlayerTurn());
        }
        else
        {
            yield return StartCoroutine(StartEnemyTurn());
        }
    }

    /// <summary>
    /// Starts the player's turn
    /// </summary>
    private IEnumerator StartPlayerTurn()
    {
        Debug.Log("Starting player turn " + currentTurn);

        // Enable end turn button
        if (endTurnButton != null)
            endTurnButton.interactable = true;

        // Reset player energy
        handManager.ResetEnergy();

        OnPlayerTurnStart?.Invoke();

        // Draw cards (different amount on first turn)
        if (currentTurn == 1)
        {
            handManager.DrawCards(initialDrawCount);
        }
        else
        {
            handManager.DrawCards(normalDrawCount);
        }

        // Wait for player to end their turn
        // This is handled by the EndPlayerTurn method

        yield break;
    }

    /// <summary>
    /// Called when the player ends their turn
    /// </summary>
    public void EndPlayerTurn()
    {
        // Only allow ending turn if it's the player's turn and combat isn't over
        if (!isPlayerTurn || isGameOver) return;

        StartCoroutine(EndPlayerTurnCoroutine());
    }

    /// <summary>
    /// Coroutine for ending the player's turn
    /// </summary>
    private IEnumerator EndPlayerTurnCoroutine()
    {
        Debug.Log("Ending player turn");

        // Disable end turn button during transition
        if (endTurnButton != null)
            endTurnButton.interactable = false;

        // Discard hand
        handManager.DiscardHand();

        // Notify listeners
        OnPlayerTurnEnd?.Invoke();
        OnTurnEnd?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Switch to enemy turn
        isPlayerTurn = false;

        // Start enemy turn
        StartCoroutine(StartNewTurn());
    }

    /// <summary>
    /// Starts the enemy's turn
    /// </summary>
    private IEnumerator StartEnemyTurn()
    {
        Debug.Log("Starting enemy turn " + currentTurn);

        OnEnemyTurnStart?.Invoke();

        // Wait a moment for visual clarity
        yield return new WaitForSeconds(0.5f);

        // Get enemies from combat state (you'd get these from your CombatLoader)
        CombatState combatState = GetCombatState();
        if (combatState != null)
        {
            List<Enemy> enemies = combatState.GetEnemies();

            // Let each enemy take their turn
            foreach (Enemy enemy in enemies)
            {
                if (isGameOver) break;

                yield return StartCoroutine(ExecuteEnemyTurn(enemy));

                // Check for game over condition
                if (isGameOver) break;
            }
        }

        // End enemy turn
        yield return StartCoroutine(EndEnemyTurnCoroutine());
    }

    /// <summary>
    /// Executes a turn for a single enemy
    /// </summary>
    private IEnumerator ExecuteEnemyTurn(Enemy enemy)
    {
        // Let the enemy execute its action
        // This would typically involve the enemy selecting and executing an action
        // For now, we'll just wait a moment to simulate the enemy's turn

        yield return new WaitForSeconds(1f);

        // Implement enemy action logic here...
        Debug.Log($"Enemy {enemy.entityName} takes its turn");

        // Wait a moment after the action
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Coroutine for ending the enemy's turn
    /// </summary>
    private IEnumerator EndEnemyTurnCoroutine()
    {
        Debug.Log("Ending enemy turn");

        // Notify listeners
        OnEnemyTurnEnd?.Invoke();
        OnTurnEnd?.Invoke();

        yield return new WaitForSeconds(0.5f);

        // Switch back to player turn
        isPlayerTurn = true;

        // Start new player turn
        StartCoroutine(StartNewTurn());
    }

    /// <summary>
    /// Updates the energy UI text
    /// </summary>
    private void UpdateEnergyUI(int current, int max)
    {
        if (energyText != null)
        {
            energyText.text = $"{current}/{max}";
            //energyText.text = $"Energy: {current}/{max}";
        }
    }

    /// <summary>
    /// Gets the combat state from the combat loader
    /// </summary>
    private CombatState GetCombatState()
    {
        // This would depend on how your CombatLoader provides access to the combat state
        // For now, we're just returning null
        return null;
    }

    /// <summary>
    /// Ends the combat with a result
    /// </summary>
    public void EndCombat(bool victory)
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log(victory ? "Combat ended in victory!" : "Combat ended in defeat!");

        OnCombatResult?.Invoke(victory);
        OnCombatEnd?.Invoke();

        // You would transition to a results screen or back to the map here
    }
}