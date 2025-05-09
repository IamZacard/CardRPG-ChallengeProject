using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles loading combat encounters, spawning enemies, and initializing enemy UI.
/// </summary>
public class CombatLoader : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private EncounterData encounterData; // Reference to the current encounter data
    [SerializeField] private Transform enemySpawnPoint;   // Where enemies will be spawned
    [SerializeField] private GameObject enemyUITemplate;  // Prefab for enemy UI
    [SerializeField] private Transform uiContainer;       // Parent for enemy UI elements

    // Combat state references
    private CombatState combatState;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private Dictionary<int, GameObject> enemyUIs = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (enemySpawnPoint == null)
        {
            Debug.LogError("Enemy spawn point is not assigned!");
            return;
        }

        if (encounterData == null)
        {
            Debug.LogError("EncounterData is not assigned! Please assign it in the inspector.");
            return;
        }

        InitializeCombatState();
    }

    private void Start()
    {
        StartCoroutine(InitializeCombat());
    }

    /// <summary>
    /// Initializes the combat state container.
    /// </summary>
    private void InitializeCombatState()
    {
        combatState = new CombatState();
        // Player and deck setup would go here if needed
    }

    /// <summary>
    /// Initializes combat: loads enemies, spawns them, sets up UI, and connects events.
    /// </summary>
    private IEnumerator InitializeCombat()
    {
        // 1. Load a random enemy from the encounter data
        List<EnemyData> enemiesData = LoadEncounterEnemies();

        // 2. Spawn the enemy
        SpawnEnemies(enemiesData);

        // 3. Create UI for the enemy
        CreateEnemyUI();

        // 4. Connect enemy events to UI updates
        ConnectEnemyEvents();

        yield return new WaitForSeconds(1f); // Optional: wait for spawn/animation

        Debug.Log("Combat initialized and ready to begin");
    }

    /// <summary>
    /// Loads a single random enemy from the EncounterData ScriptableObject.
    /// </summary>
    private List<EnemyData> LoadEncounterEnemies()
    {
        List<EnemyData> enemies = new List<EnemyData>();

        if (encounterData == null || encounterData.enemies == null || encounterData.enemies.Count == 0)
        {
            Debug.LogError("EncounterData has no enemies! Please add at least one EnemyData to the list.");
            return enemies;
        }

        // Pick one random enemy from the encounter's enemy list
        int randomIndex = Random.Range(0, encounterData.enemies.Count);
        EnemyData randomEnemy = encounterData.enemies[randomIndex];
        enemies.Add(randomEnemy);

        Debug.Log($"Loaded random enemy: {randomEnemy.name}");
        return enemies;
    }

    /// <summary>
    /// Spawns enemies at the designated spawn point.
    /// </summary>
    private void SpawnEnemies(List<EnemyData> enemiesData)
    {
        activeEnemies.Clear();

        for (int i = 0; i < enemiesData.Count; i++)
        {
            Vector3 position = enemySpawnPoint.position;
            Enemy enemy = EnemyFactory.CreateEnemy(enemiesData[i], position);

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
                combatState.AddEnemy(enemy);
            }
        }
    }

    /// <summary>
    /// Creates UI elements for each active enemy.
    /// </summary>
    private void CreateEnemyUI()
    {
        if (enemyUITemplate == null)
        {
            Debug.LogError("Enemy UI template is not assigned!");
            return;
        }

        foreach (Enemy enemy in activeEnemies)
        {
            GameObject uiObject = Instantiate(enemyUITemplate, uiContainer);

            RectTransform uiRectTransform = uiObject.GetComponent<RectTransform>();
            Canvas canvas = uiObject.GetComponentInParent<Canvas>();

            // Position UI above the enemy (simple Y offset)
            if (canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    uiRectTransform.anchoredPosition = new Vector2(0, 115f);
                }
                else if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    uiRectTransform.anchoredPosition = new Vector2(0, 115f);
                }
            }
            else
            {
                Debug.LogError("Canvas is null! UI will not be positioned correctly.");
            }

            enemyUIs[enemy.entityID] = uiObject;
            UpdateEnemyUI(enemy);
        }
    }

    /// <summary>
    /// Connects enemy events to their respective UI update methods.
    /// </summary>
    private void ConnectEnemyEvents()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            enemy.OnHealthChanged += _ => UpdateEnemyUI(enemy);
            enemy.OnBlockChanged += _ => UpdateEnemyUI(enemy);
            enemy.OnStatusEffectApplied += (_, __) => UpdateEnemyUI(enemy);
            enemy.OnStatusEffectRemoved += (_, __) => UpdateEnemyUI(enemy);
            enemy.OnActionSelected += action => UpdateEnemyIntent(enemy, action);
        }
    }

    /// <summary>
    /// Updates the UI for a specific enemy.
    /// </summary>
    private void UpdateEnemyUI(Enemy enemy)
    {
        if (!enemyUIs.TryGetValue(enemy.entityID, out GameObject uiObject))
            return;

        TMP_Text healthText = uiObject.transform.Find("HealthText")?.GetComponent<TMP_Text>();
        if (healthText)
            healthText.text = $"{enemy.CurrentHealth}/{enemy.MaxHealth}";

        Slider healthBar = uiObject.transform.Find("HealthBar")?.GetComponent<Slider>();
        if (healthBar)
        {
            healthBar.maxValue = enemy.MaxHealth;
            healthBar.value = enemy.CurrentHealth;
        }

        GameObject blockObject = uiObject.transform.Find("BlockDisplay")?.gameObject;
        TMP_Text blockText = blockObject?.GetComponentInChildren<TMP_Text>();
        if (blockObject && blockText)
        {
            blockObject.SetActive(enemy.CurrentBlock > 0);
            blockText.text = enemy.CurrentBlock.ToString();
        }

        Transform statusContainer = uiObject.transform.Find("StatusEffectsContainer");
        if (statusContainer)
        {
            foreach (Transform child in statusContainer)
                Destroy(child.gameObject);

            Dictionary<StatusEffectType, int> effects = enemy.GetAllStatusEffects();
            foreach (var effect in effects)
            {
                if (effect.Value <= 0) continue;
                // Here you would instantiate a status effect icon from a template
                Debug.Log($"Enemy {enemy.entityName} has {effect.Key} {effect.Value}");
            }
        }
    }

    /// <summary>
    /// Updates the intent display for an enemy's next action.
    /// </summary>
    private void UpdateEnemyIntent(Enemy enemy, EnemyAction action)
    {
        if (!enemyUIs.TryGetValue(enemy.entityID, out GameObject uiObject))
            return;

        Transform intentDisplay = uiObject.transform.Find("IntentDisplay");
        if (intentDisplay)
        {
            Image intentIcon = intentDisplay.Find("IntentIcon")?.GetComponent<Image>();
            TMP_Text intentText = intentDisplay.Find("IntentText")?.GetComponent<TMP_Text>();

            intentDisplay.gameObject.SetActive(action.isTelegraphed);

            if (action.isTelegraphed && intentIcon && intentText)
            {
                intentText.text = DetermineIntentText(action);
                // Set intentIcon.sprite here if you have different icons for each intent
            }
        }
    }

    /// <summary>
    /// Determines the text to display for an enemy's intent.
    /// </summary>
    private string DetermineIntentText(EnemyAction action)
    {
        if (action.damage > 0)
            return $"Attack: {action.damage}";
        else if (action.block > 0)
            return $"Block: {action.block}";
        else if (action.statusEffectType.HasValue)
        {
            string target = action.selfEffect ? "Self" : "You";
            return $"{action.statusEffectType}: {action.statusEffectAmount} → {target}";
        }
        return action.actionName;
    }
}

/// <summary>
/// Container for combat state (enemies, etc.).
/// </summary>
public class CombatState
{
    private List<Enemy> enemies = new List<Enemy>();

    public void AddEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public List<Enemy> GetEnemies()
    {
        return enemies;
    }
}
