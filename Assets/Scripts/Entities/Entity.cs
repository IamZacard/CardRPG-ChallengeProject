using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int Health { get; protected set; } = 100;
    public int Block { get; protected set; } = 0;
    public int Energy { get; protected set; } = 3;

    public void TakeDamage(int damage)
    {
        int remainingDamage = damage - Block;
        Block = Mathf.Max(0, Block - damage);
        if (remainingDamage > 0)
            Health = Mathf.Max(0, Health - remainingDamage);
    }

    public void AddBlock(int amount)
    {
        Block += amount;
    }

    public void Heal(int amount)
    {
        Health = Mathf.Min(100, Health + amount); // Cap at max health (100)
    }
}