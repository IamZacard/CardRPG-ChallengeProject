// Enemy.cs
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
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

    // Fired whenever nextAction is chosen
    public event Action<EnemyAction> OnActionSelected;

    // New turn-based events
    public event Action OnTurnStarted;
    public event Action OnTurnEnded;

    protected override void Awake()
    {
        base.Awake();
        // pick an initial action
        SelectNextAction();
    }

    public override void Initialize()
    {
        base.Initialize();
        SelectNextAction();
    }

    // Called by your turn manager or game loop
    public override void OnStartTurn()
    {
        base.OnStartTurn();
        OnTurnStarted?.Invoke();
    }

    // Called by your turn manager or game loop
    public override void OnEndTurn()
    {
        base.OnEndTurn();
        OnTurnEnded?.Invoke();
        SelectNextAction();
    }

    // Random fallback; AI can override entirely by calling SetNextAction()
    public virtual void SelectNextAction()
    {
        if (possibleActions.Count == 0)
        {
            Debug.LogWarning($"Enemy {entityName} has no actions defined!");
            return;
        }

        int idx = UnityEngine.Random.Range(0, possibleActions.Count);
        nextAction = possibleActions[idx];
        OnActionSelected?.Invoke(nextAction);
    }

    /// <summary>
    /// Let an external controller (like EnemyAI) choose the nextAction and fire the event.
    /// </summary>
    public void SetNextAction(EnemyAction action)
    {
        nextAction = action;
        OnActionSelected?.Invoke(nextAction);
    }

    protected override void Die()
    {
        base.Die();
        // e.g. drop gold/XP here…
    }
}
