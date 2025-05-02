using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class EnemyAction
{
    public string actionName;
    public int damage;
    public int block;
    public StatusEffectType? statusEffectType;
    public int statusEffectAmount;
    public bool targetsAll;
    public bool selfEffect;
    public bool isTelegraphed;
}

public class Enemy : Entity
{
    [Header("Enemy Stats")]
    public int goldReward;
    public int experienceReward;
    public bool isElite;
    public bool isBoss;

    [Header("Actions")]
    public List<EnemyAction> possibleActions = new List<EnemyAction>();
    public EnemyAction nextAction;

    // Events
    public event Action<EnemyAction> OnActionSelected;

    protected override void Awake()
    {
        base.Awake();

        // Select initial action
        SelectNextAction();
    }

    public override void Initialize()
    {
        base.Initialize();
        SelectNextAction();
    }

    public virtual void SelectNextAction()
    {
        if (possibleActions.Count == 0)
        {
            Debug.LogWarning($"Enemy {entityName} has no actions defined!");
            return;
        }

        // Simple random action selection (could be overridden for more complex patterns)
        int actionIndex = UnityEngine.Random.Range(0, possibleActions.Count);
        nextAction = possibleActions[actionIndex];

        // Notify listeners of the selected action
        OnActionSelected?.Invoke(nextAction);
    }

    public override void OnStartTurn()
    {
        base.OnStartTurn();

        // Additional enemy-specific start turn logic
    }

    public override void OnEndTurn()
    {
        base.OnEndTurn();

        // Select the next action for the next turn
        SelectNextAction();
    }

    protected override void Die()
    {
        base.Die();

        // Additional enemy death logic (e.g., drop rewards)
    }
}