using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orchestrates the loading and initialization sequence for combat scenes.
/// Follows SRP by focusing only on coordinating the loading process.
/// </summary>
public class CombatLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private CombatUI combatUI;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform enemiesContainer;

    [Header("Configuration")]
    [SerializeField] private GameObject loadingScreenPrefab;
    [SerializeField] private float minimumLoadingTime = 1.5f; // Ensures loading screen appears for at least this duration

    // Combat data that can be passed from the map/encounter selection
    private CombatData pendingCombatData;

    // Track when critical components are ready
    private bool isPlayerReady = false;
    private bool areEnemiesReady = false;
    private bool isDeckReady = false;
    private bool isUIReady = false;

    // References to created objects
    private Player player;
    private List<Enemy> spawnedEnemies = new List<Enemy>();
    private Deck playerDeck;
    private GameObject loadingScreen;

    private void Awake()
    {
        // Ensure we have references to required components
        if (combatManager == null)
            combatManager = FindObjectOfType<CombatManager>();

        if (combatUI == null)
            combatUI = FindObjectOfType<CombatUI>();
    }

    private void Start()
    {
        // Get combat data from the game manager or scene parameters
        pendingCombatData = GameManager.Instance.GetPendingCombatData();

        // Begin the loading sequence
        StartCoroutine(LoadCombatSequence());
    }

    /// <summary>
    /// Main coroutine that orchestrates the entire loading sequence
    /// </summary>
    private IEnumerator LoadCombatSequence()
    {
        // Show loading screen immediately
        ShowLoadingScreen();

        // Record start time to ensure minimum loading time
        float startTime = Time.time;

        // Start async initialization processes
        StartCoroutine(InitializePlayer());
        StartCoroutine(InitializeEnemies());
        StartCoroutine(InitializeDeck());
        StartCoroutine(InitializeUI());

        // Wait until all systems are ready
        yield return StartCoroutine(WaitForAllSystemsReady());

        // Ensure minimum loading time has passed
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumLoadingTime)
        {
            yield return new WaitForSeconds(minimumLoadingTime - elapsedTime);
        }

        // Hide loading screen
        HideLoadingScreen();

        // Initialize combat state with all references
        InitializeCombatState();

        // Begin the first turn
        combatManager.StartCombat(spawnedEnemies);

        Debug.Log("Combat initialization sequence completed successfully");
    }

    private void ShowLoadingScreen()
    {
        if (loadingScreenPrefab != null)
        {
            loadingScreen = Instantiate(loadingScreenPrefab);
            loadingScreen.SetActive(true);
            Debug.Log("Loading screen displayed");
        }
        else
        {
            Debug.LogWarning("Loading screen prefab not assigned");
        }
    }

    private void HideLoadingScreen()
    {
        if (loadingScreen != null)
        {
            Destroy(loadingScreen);
            Debug.Log("Loading screen removed");
        }
    }

    /// <summary>
    /// Waits until all required systems report readiness
    /// </summary>
    private IEnumerator WaitForAllSystemsReady()
    {
        while (!isPlayerReady || !areEnemiesReady || !isDeckReady || !isUIReady)
        {
            yield return null;
        }

        Debug.Log("All systems ready");
    }

    /// <summary>
    /// Initializes the player character
    /// </summary>
    private IEnumerator InitializePlayer()
    {
        Debug.Log("Initializing player...");

        // Load player data from save or game state
        PlayerData playerData = GameManager.Instance.GetPlayerData();

        // Instantiate player at spawn point
        player = PlayerFactory.CreatePlayer(playerData, playerSpawnPoint.position);

        // Signal completion
        isPlayerReady = true;
        Debug.Log("Player initialized");

        yield return null;
    }

    /// <summary>
    /// Spawns enemies based on combat data
    /// </summary>
    private IEnumerator InitializeEnemies()
    {
        Debug.Log("Initializing enemies...");

        // Get enemy data from pending combat data
        List<EnemyData> enemyDataList = pendingCombatData.EnemyDataList;

        // Calculate spawn positions
        List<Vector3> spawnPositions = CalculateEnemyPositions(enemyDataList.Count);

        // Spawn each enemy
        for (int i = 0; i < enemyDataList.Count; i++)
        {
            Enemy enemy = EnemyFactory.CreateEnemy(enemyDataList[i], spawnPositions[i], enemiesContainer);
            spawnedEnemies.Add(enemy);

            // Small delay between spawns for visual effect
            yield return new WaitForSeconds(0.1f);
        }

        // Signal completion
        areEnemiesReady = true;
        Debug.Log($"Enemies initialized: {spawnedEnemies.Count} enemies spawned");
    }

    /// <summary>
    /// Initializes the player's deck, draw pile, and hand
    /// </summary>
    private IEnumerator InitializeDeck()
    {
        Debug.Log("Initializing deck...");

        // Get deck data from player save or game state
        List<CardData> deckCardData = GameManager.Instance.GetPlayerDeckData();

        // Create deck and initialize it
        playerDeck = new Deck();
        playerDeck.Initialize(deckCardData);

        // Signal completion
        isDeckReady = true;
        Debug.Log("Deck initialized");

        yield return null;
    }

    /// <summary>
    /// Sets up UI elements and connects them to gameplay objects
    /// </summary>
    private IEnumerator InitializeUI()
    {
        Debug.Log("Initializing UI...");

        // Prepare UI for the combat scene
        combatUI.PrepareForCombat();

        // Signal completion
        isUIReady = true;
        Debug.Log("UI initialized");

        yield return null;
    }

    /// <summary>
    /// Creates and populates the CombatState with all references
    /// </summary>
    private void InitializeCombatState()
    {
        Debug.Log("Initializing combat state...");

        // Create and populate combat state
        CombatState combatState = new CombatState
        {
            Player = player,
            Enemies = spawnedEnemies.ToArray(),
            PlayerDeck = playerDeck,
            TurnCount = 0,
            CombatDifficulty = pendingCombatData.Difficulty,
            IsPlayerTurn = true
        };

        // Set the state in CombatManager
        combatManager.SetCombatState(combatState);

        // Connect UI to combat state
        combatUI.ConnectToCombatState(combatState);

        Debug.Log("Combat state initialized");
    }

    /// <summary>
    /// Calculates positioned for enemies based on count
    /// </summary>
    private List<Vector3> CalculateEnemyPositions(int enemyCount)
    {
        List<Vector3> positions = new List<Vector3>();

        // Define the spawn area
        float spawnWidth = 5f;
        Vector3 basePosition = enemiesContainer.position;

        // Calculate spacing
        float spacing = spawnWidth / (enemyCount + 1);

        // Generate positions
        for (int i = 0; i < enemyCount; i++)
        {
            float xOffset = spacing * (i + 1) - spawnWidth / 2f;
            positions.Add(basePosition + new Vector3(xOffset, 0, 0));
        }

        return positions;
    }
}