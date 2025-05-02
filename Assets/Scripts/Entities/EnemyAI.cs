using UnityEngine;
using System.Collections.Generic;

public enum EnemyAIState
{
    Aggressive,
    Defensive,
    Buff,
    Heal,
    Special
}

[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public EnemyAIState defaultState = EnemyAIState.Aggressive;
    public float aggressiveThreshold = 0.8f; // % of health where enemy is aggressive
    public float defensiveThreshold = 0.4f; // % of health where enemy gets defensive
    public float healThreshold = 0.3f; // % of health where enemy prioritizes healing
    public int buffInterval = 3; // Turn interval for buffing

    [Header("Actions")]
    public List<EnemyAction> aggressiveActions = new List<EnemyAction>();
    public List<EnemyAction> defensiveActions = new List<EnemyAction>();
    public List<EnemyAction> buffActions = new List<EnemyAction>();
    public List<EnemyAction> healActions = new List<EnemyAction>();
    public List<EnemyAction> specialActions = new List<EnemyAction>();

    private Enemy enemy;
    private EnemyAIState currentState;
    private int turnCounter = 0;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        currentState = defaultState;

        // Subscribe to enemy events
        enemy.OnEndTurnEvent += OnEnemyEndTurn; // Subscribe to the new event
        enemy.OnHealthChanged += OnEnemyHealthChanged;
    }

    private void Start()
    {
        // Configure possible actions
        ConfigureEnemyActions();

        // Select first action based on AI state
        SelectActionBasedOnState();
    }

    private void ConfigureEnemyActions()
    {
        // Clear existing actions
        enemy.possibleActions.Clear();

        // Add all possible actions from our lists
        enemy.possibleActions.AddRange(aggressiveActions);
        enemy.possibleActions.AddRange(defensiveActions);
        enemy.possibleActions.AddRange(buffActions);
        enemy.possibleActions.AddRange(healActions);
        enemy.possibleActions.AddRange(specialActions);
    }

    private void OnEnemyEndTurn()
    {
        turnCounter++;
        UpdateAIState();
        SelectActionBasedOnState();
    }

    private void OnEnemyHealthChanged(int newHealth)
    {
        UpdateAIState();
    }

    private void UpdateAIState()
    {
        // Calculate health percentage
        float healthPercentage = (float)enemy.CurrentHealth / enemy.MaxHealth;

        // Determine state based on health and other factors
        if (healthPercentage <= healThreshold && healActions.Count > 0)
        {
            currentState = EnemyAIState.Heal;
        }
        else if (healthPercentage <= defensiveThreshold && defensiveActions.Count > 0)
        {
            currentState = EnemyAIState.Defensive;
        }
        else if (turnCounter % buffInterval == 0 && buffActions.Count > 0)
        {
            currentState = EnemyAIState.Buff;
        }
        else if (specialConditionsMet() && specialActions.Count > 0)
        {
            currentState = EnemyAIState.Special;
        }
        else
        {
            currentState = EnemyAIState.Aggressive;
        }
    }

    private bool specialConditionsMet()
    {
        // Override in specific enemy classes for special behaviors
        // For example: boss phases, specific player conditions, etc.
        return false;
    }

    private void SelectActionBasedOnState()
    {
        List<EnemyAction> actionsForState = new List<EnemyAction>();

        // Get relevant actions for current state
        switch (currentState)
        {
            case EnemyAIState.Aggressive:
                actionsForState = aggressiveActions.Count > 0 ? aggressiveActions : enemy.possibleActions;
                break;

            case EnemyAIState.Defensive:
                actionsForState = defensiveActions;
                break;

            case EnemyAIState.Buff:
                actionsForState = buffActions;
                break;

            case EnemyAIState.Heal:
                actionsForState = healActions;
                break;

            case EnemyAIState.Special:
                actionsForState = specialActions;
                break;
        }

        // If we don't have specific actions for this state, use default ones
        if (actionsForState.Count == 0)
        {
            enemy.SelectNextAction();
            return;
        }

        // Select a random action from the appropriate list
        int actionIndex = Random.Range(0, actionsForState.Count);
        enemy.nextAction = actionsForState[actionIndex];

        // Use SelectNextAction to notify listeners
        enemy.SelectNextAction();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (enemy != null)
        {
            enemy.OnEndTurnEvent -= OnEnemyEndTurn; // Unsubscribe from the new event
            enemy.OnHealthChanged -= OnEnemyHealthChanged;
        }
    }
}