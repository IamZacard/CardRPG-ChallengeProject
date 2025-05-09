using UnityEngine;
using System.Collections.Generic;

public class CombatTestInitializer : MonoBehaviour
{
    public EnemyData[] testEnemies;

    void Start()
    {
        // Only use this in development scenes for testing
        if (GameManager.Instance.GetPendingCombatData() == null)
        {
            CreateTestCombat();
        }
    }

    void CreateTestCombat()
    {
        CombatData combatData = new CombatData("test_encounter", CombatDifficulty.Normal);

        // Add test enemies
        foreach (var enemyData in testEnemies)
        {
            combatData.AddEnemy(enemyData);
        }

        // Set rewards
        combatData.RewardData = new RewardData(10, 5);

        // Set combat data in GameManager
        GameManager.Instance.SetPendingCombatData(combatData);
    }
}