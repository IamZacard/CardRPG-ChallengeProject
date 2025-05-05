// EnemyFactory.cs - Factory for creating enemy instances
using UnityEngine;
using System.Collections.Generic;

public static class EnemyFactory
{
    public static Enemy CreateEnemy(EnemyData enemyData, Transform parent = null)
    {
        // Get enemy prefab from Resources
        GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Entities/BaseEnemy");

        if (enemyPrefab == null)
        {
            Debug.LogError("Base enemy prefab not found in Resources!");
            return null;
        }

        // Instantiate enemy
        GameObject enemyObject = Object.Instantiate(enemyPrefab, parent);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError("Enemy component not found on prefab!");
            return null;
        }

        // Configure the enemy with data
        enemy.entityName = enemyData.enemyName;
        enemy.maxHealth = enemyData.maxHealth;
        enemy.currentHealth = enemyData.maxHealth;
        enemy.baseBlock = enemyData.baseBlock;
        enemy.isElite = enemyData.isElite;
        enemy.isBoss = enemyData.isBoss;
        enemy.goldReward = enemyData.goldReward;
        enemy.experienceReward = enemyData.experienceReward;

        // Set up actions
        enemy.possibleActions.Clear();
        foreach (var action in enemyData.possibleActions)
        {
            enemy.possibleActions.Add(action);
        }

        // Set up AI
        EnemyAI ai = enemyObject.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.aggressiveThreshold = enemyData.aggressiveThreshold;
            ai.defensiveThreshold = enemyData.defensiveThreshold;
            ai.healThreshold = enemyData.healThreshold;

            // Split actions by type (could be more sophisticated)
            ai.aggressiveActions = enemyData.possibleActions.FindAll(a => a.damage > 0);
            ai.defensiveActions = enemyData.possibleActions.FindAll(a => a.block > 0);
            ai.healActions = enemyData.possibleActions.FindAll(a =>
                a.statusEffectType.HasValue &&
                a.selfEffect &&
                (a.statusEffectType == StatusEffectType.Strength || a.damage == 0));
        }

        // Handle boss setup if applicable
        if (enemyData.isBoss && enemyData.bossPhases.Count > 0)
        {
            BossController bossController = enemyObject.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.phases.Clear();

                foreach (var phaseData in enemyData.bossPhases)
                {
                    BossController.BossPhase phase = new BossController.BossPhase
                    {
                        healthPercentageThreshold = phaseData.healthPercentageThreshold,
                        phaseTransitionAction = phaseData.phaseTransitionAction
                    };

                    phase.phaseActions.AddRange(phaseData.phaseActions);
                    bossController.phases.Add(phase);
                }
            }
        }

        // Initialize the enemy
        enemy.Initialize();

        return enemy;
    }
}