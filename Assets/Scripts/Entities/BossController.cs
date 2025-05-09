using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class BossController : MonoBehaviour
{
    [System.Serializable]
    public class BossPhase
    {
        public float healthPercentageThreshold;
        public List<EnemyAction> phaseActions = new List<EnemyAction>();
        public EnemyAction phaseTransitionAction;
    }

    [Header("Boss Phases")]
    public List<BossPhase> phases = new List<BossPhase>();

    private Enemy enemy;
    private int currentPhaseIndex = -1;
    private bool transitioning = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        // Sort phases by threshold from highest to lowest (e.g., 0.7, 0.4, 0.1)
        phases.Sort((a, b) => b.healthPercentageThreshold.CompareTo(a.healthPercentageThreshold));

        // Subscribe to health changes to detect phase transitions
        enemy.OnHealthChanged += CheckPhaseTransition;
    }

    private void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.OnHealthChanged -= CheckPhaseTransition;
        }
    }

    private void CheckPhaseTransition(int currentHealth)
    {
        if (transitioning || enemy.MaxHealth <= 0)
            return;

        float healthPercent = (float)currentHealth / enemy.MaxHealth;

        // Check if we need to transition to a new phase
        for (int i = currentPhaseIndex + 1; i < phases.Count; i++)
        {
            if (healthPercent <= phases[i].healthPercentageThreshold)
            {
                TransitionToPhase(i);
                break;
            }
        }
    }

    private void TransitionToPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= phases.Count || phaseIndex <= currentPhaseIndex)
            return;

        transitioning = true;
        currentPhaseIndex = phaseIndex;
        BossPhase phase = phases[phaseIndex];

        // Execute transition action if available
        if (phase.phaseTransitionAction != null)
        {
            enemy.SetNextAction(phase.phaseTransitionAction);

            // Modify enemy's possible actions pool for this phase
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                // Add phase-specific actions to appropriate AI state pools
                ai.specialActions.Clear();
                ai.specialActions.AddRange(phase.phaseActions);
            }
            else
            {
                // If no AI component, directly modify the enemy's action pool
                enemy.possibleActions.Clear();
                enemy.possibleActions.AddRange(phase.phaseActions);
            }
        }

        Debug.Log($"Boss {enemy.entityName} transitioned to phase {phaseIndex + 1} at {(phase.healthPercentageThreshold * 100)}% health");
        transitioning = false;
    }
}