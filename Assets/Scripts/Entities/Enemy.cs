using UnityEngine;

public class Enemy : Entity
{
    public EnemyAI ai;

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }
}