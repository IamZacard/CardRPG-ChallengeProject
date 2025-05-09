using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class CombatLoader : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private string encounterId;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private GameObject enemyUITemplate;
    [SerializeField] private Transform uiContainer;

    [Header("Debug Settings")]
    [SerializeField] private EnemyData debugEnemyData;
    [SerializeField] private bool useDebugEnemy = true;

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

        InitializeCombatState();
    }

    private void Start()
    {
        StartCoroutine(InitializeCombat());
    }

    private void InitializeCombatState()
    {
        combatState = new CombatState();

        // Here we would load player data from GameManager or similar persistent object
        // For now, we'll initialize only enemy-related systems
    }

    private IEnumerator InitializeCombat()
    {
        // 1. Load encounter data (for now, using debug enemy)
        List<EnemyData> enemiesData = LoadEncounterEnemies();

        // 2. Spawn enemies
        SpawnEnemies(enemiesData);

        // 3. Create UI for enemies
        CreateEnemyUI();

        // 4. Connect enemy events to UI updates
        ConnectEnemyEvents();

        // 5. Initialize combat systems (turn manager, etc. - not implemented yet)
        // InitializeCombatSystems();

        yield return new WaitForSeconds(1f); // Give time for animations if needed

        // 6. Start first turn or enter player selection state
        // We'll stop here for now
        Debug.Log("Combat initialized and ready to begin");
    }

    private List<EnemyData> LoadEncounterEnemies()
    {
        List<EnemyData> enemies = new List<EnemyData>();

        if (useDebugEnemy && debugEnemyData != null)
        {
            enemies.Add(debugEnemyData);
            return enemies;
        }

        // In a real implementation, we would load encounter data based on encounterId
        // For example:
        // EncounterData encounter = EncounterDatabase.GetEncounter(encounterId);
        // return encounter.enemies;

        // For now, we'll load a default enemy from Resources
        EnemyData defaultEnemy = Resources.Load<EnemyData>("Enemies/DefaultEnemy");
        if (defaultEnemy != null)
        {
            enemies.Add(defaultEnemy);
        }
        else
        {
            Debug.LogError("No default enemy found! Add one to Resources/Enemies/ or use debug enemy.");
        }

        return enemies;
    }

    private void SpawnEnemies(List<EnemyData> enemiesData)
    {
        activeEnemies.Clear();

        // Simple positioning logic - can be enhanced for multiple enemies
        float spacing = 3f;
        float startX = enemiesData.Count > 1 ? -(spacing * (enemiesData.Count - 1)) / 2 : 0f;

        for (int i = 0; i < enemiesData.Count; i++)
        {
            Vector3 position = enemySpawnPoint.position + new Vector3(startX + i * spacing, 0, 0);
            Enemy enemy = EnemyFactory.CreateEnemy(enemiesData[i], position);

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
                combatState.AddEnemy(enemy);
            }
        }
    }

    private void CreateEnemyUI()
    {
        if (enemyUITemplate == null)
        {
            Debug.LogError("Enemy UI template is not assigned!");
            return;
        }

        foreach (Enemy enemy in activeEnemies)
        {
            // Instantiate the UI element.
            GameObject uiObject = Instantiate(enemyUITemplate, uiContainer);

            // Get the RectTransform of the UI object.
            RectTransform uiRectTransform = uiObject.GetComponent<RectTransform>();

            // Get the Canvas.
            Canvas canvas = uiObject.GetComponentInParent<Canvas>();

            // Position the UI element.
            if (canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    Vector2 enemyScreenPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
                    uiRectTransform.anchoredPosition = new Vector2(0 , 115f); // Set Y to 115
                }
                else if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    Vector3 enemyWorldPosition = enemy.transform.position;
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(enemyWorldPosition);
                    uiRectTransform.anchoredPosition = new Vector2(0, 115f);
                }
            }
            else
            {
                Debug.LogError("Canvas is null! UI will not be positioned correctly.");
            }

            // Store reference to update later.
            enemyUIs[enemy.entityID] = uiObject;

            // Initialize UI values.
            UpdateEnemyUI(enemy);
        }
    }

    private void ConnectEnemyEvents()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            // Subscribe to health and block changes
            enemy.OnHealthChanged += _ => UpdateEnemyUI(enemy);
            enemy.OnBlockChanged += _ => UpdateEnemyUI(enemy);
            enemy.OnStatusEffectApplied += (_, __) => UpdateEnemyUI(enemy);
            enemy.OnStatusEffectRemoved += (_, __) => UpdateEnemyUI(enemy);

            // Enemy action selection handling
            enemy.OnActionSelected += action => UpdateEnemyIntent(enemy, action);
        }
    }

    private void UpdateEnemyUI(Enemy enemy)
    {
        if (!enemyUIs.TryGetValue(enemy.entityID, out GameObject uiObject))
            return;

        // Update health text
        TMP_Text healthText = uiObject.transform.Find("HealthText")?.GetComponent<TMP_Text>();
        if (healthText)
        {
            healthText.text = $"{enemy.CurrentHealth}/{enemy.MaxHealth}";
        }

        // Update health bar
        Slider healthBar = uiObject.transform.Find("HealthBar")?.GetComponent<Slider>();
        if (healthBar)
        {
            healthBar.maxValue = enemy.MaxHealth;
            healthBar.value = enemy.CurrentHealth;
        }

        // Update block display
        GameObject blockObject = uiObject.transform.Find("BlockDisplay")?.gameObject;
        TMP_Text blockText = blockObject?.GetComponentInChildren<TMP_Text>();
        if (blockObject && blockText)
        {
            blockObject.SetActive(enemy.CurrentBlock > 0);
            blockText.text = enemy.CurrentBlock.ToString();
        }

        // Update status effects (simplified - would need a proper UI system for status icons)
        Transform statusContainer = uiObject.transform.Find("StatusEffectsContainer");
        if (statusContainer)
        {
            // Clear existing status indicators (simplified approach)
            foreach (Transform child in statusContainer)
            {
                Destroy(child.gameObject);
            }

            // Create status effect indicators
            Dictionary<StatusEffectType, int> effects = enemy.GetAllStatusEffects();
            foreach (var effect in effects)
            {
                if (effect.Value <= 0) continue;

                // Here you would instantiate a status effect icon from a template
                // For now, we'll just log it
                Debug.Log($"Enemy {enemy.entityName} has {effect.Key} {effect.Value}");
            }
        }
    }

    private void UpdateEnemyIntent(Enemy enemy, EnemyAction action)
    {
        if (!enemyUIs.TryGetValue(enemy.entityID, out GameObject uiObject))
            return;

        // Update intent icon and text
        Transform intentDisplay = uiObject.transform.Find("IntentDisplay");
        if (intentDisplay)
        {
            Image intentIcon = intentDisplay.Find("IntentIcon")?.GetComponent<Image>();
            TMP_Text intentText = intentDisplay.Find("IntentText")?.GetComponent<TMP_Text>();

            if (intentIcon && intentText)
            {
                // Set visibility based on whether the action is telegraphed
                intentDisplay.gameObject.SetActive(action.isTelegraphed);

                if (action.isTelegraphed)
                {
                    // Determine intent type and set icon (attack, block, buff, debuff)
                    string intentDescription = DetermineIntentText(action);
                    intentText.text = intentDescription;

                    // You would set the appropriate sprite based on intent type
                    // intentIcon.sprite = GetIntentSprite(action);
                }
            }
        }
    }

    private string DetermineIntentText(EnemyAction action)
    {
        if (action.damage > 0)
        {
            return $"Attack: {action.damage}";
        }
        else if (action.block > 0)
        {
            return $"Block: {action.block}";
        }
        else if (action.statusEffectType.HasValue)
        {
            string target = action.selfEffect ? "Self" : "You";
            return $"{action.statusEffectType}: {action.statusEffectAmount} ? {target}";
        }

        return action.actionName;
    }
}

// Combat state container class
public class CombatState
{
    private List<Enemy> enemies = new List<Enemy>();
    // Would also contain player, deck, etc.

    public void AddEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public List<Enemy> GetEnemies()
    {
        return enemies;
    }

    // Would have methods for managing combat state, turn handling, etc.
}