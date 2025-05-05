// BossController.cs
using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Enemy))]
public class BossController : MonoBehaviour
{
    [Serializable]
    public class BossPhase
    {
        [Tooltip("Switch into this phase when HP% <= threshold (0 to 1)")]
        public float healthPercentageThreshold;
        public List<EnemyAction> phaseActions = new List<EnemyAction>();
        public EnemyAction phaseTransitionAction;
        [HideInInspector] public bool hasTriggered = false;
    }

    [Header("Boss Phases (in descending threshold order)")]
    public List<BossPhase> phases = new List<BossPhase>();

    [Header("Telegraph Settings")]
    [Tooltip("How many turns before the action to telegraph (not yet implemented)")]
    public int telegraphTurnsAhead = 1;
    public bool alwaysTelegraphSpecialAttacks = true;

    private Enemy enemy;
    private int currentPhaseIndex = 0;
    private List<EnemyAction> actionSequence = new List<EnemyAction>();
    private int sequencePosition = 0;
    private int turnCounter = 0;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        // Sort so highest thresholds come first (e.g. 0.8, then 0.5, then 0.2)
        phases.Sort((a, b) => b.healthPercentageThreshold.CompareTo(a.healthPercentageThreshold));

        // Subscribe to the new events
        enemy.OnTurnStarted += OnEnemyTurnStarted;
        enemy.OnTurnEnded += OnEnemyTurnEnded;
    }

    private void Start()
    {
        // Initialize first phase
        SetupCurrentPhase();
    }

    private void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.OnTurnStarted -= OnEnemyTurnStarted;
            enemy.OnTurnEnded -= OnEnemyTurnEnded;
        }
    }

    private void OnEnemyTurnStarted()
    {
        turnCounter++;
        CheckPhaseTransition();
    }

    private void OnEnemyTurnEnded()
    {
        // Advance through the current phase's sequence
        if (actionSequence.Count > 0)
            sequencePosition = (sequencePosition + 1) % actionSequence.Count;

        SelectNextAction();
    }

    private void CheckPhaseTransition()
    {
        float hpPct = (float)enemy.CurrentHealth / enemy.MaxHealth;

        // Look for the next untriggered phase whose threshold we’ve crossed
        for (int i = currentPhaseIndex; i < phases.Count; i++)
        {
            var phase = phases[i];
            if (!phase.hasTriggered && hpPct <= phase.healthPercentageThreshold)
            {
                phase.hasTriggered = true;

                // Fire the transition action first, if any
                if (phase.phaseTransitionAction != null)
                    enemy.SetNextAction(phase.phaseTransitionAction);

                // Move into that phase
                currentPhaseIndex = i;
                SetupCurrentPhase();
                return;
            }
        }
    }

    private void SetupCurrentPhase()
    {
        if (currentPhaseIndex < 0 || currentPhaseIndex >= phases.Count)
        {
            Debug.LogWarning("BossController: no valid phase to set up!");
            return;
        }

        var phase = phases[currentPhaseIndex];
        actionSequence.Clear();
        actionSequence.AddRange(phase.phaseActions);
        sequencePosition = 0;

        SelectNextAction();
    }

    private void SelectNextAction()
    {
        if (actionSequence.Count == 0)
        {
            Debug.LogWarning("BossController: action sequence is empty!");
            return;
        }

        var next = actionSequence[sequencePosition];

        if (alwaysTelegraphSpecialAttacks)
            next.isTelegraphed = true;

        enemy.SetNextAction(next);
    }
}
