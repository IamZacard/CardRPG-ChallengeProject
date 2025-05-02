using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public string entityName;
    public int maxHealth;
    public int currentHealth;
    public int block;

    public virtual void TakeDamage(int damage)
    {
        int damageAfterBlock = damage - block;
        block = Mathf.Max(0, block - damage); // Reduce block first
        if (damageAfterBlock > 0)
        {
            currentHealth -= damageAfterBlock;
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public virtual void Die()
    {
        Debug.Log($"{entityName} has died.");
    }
}