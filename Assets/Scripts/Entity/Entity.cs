// Entity.cs - Base class for all entities (player and enemies)
using UnityEngine;
using System;
using System.Collections.Generic;

public enum StatusEffectType
{
    Strength,
    Dexterity,
    Poison,
    Vulnerable,
    Weak,
    Burn,
    Stun,
    Frail,
    Thorns
}

public abstract class Entity : MonoBehaviour
{
    [Header("Base Stats")]
    public string entityName;
    public int maxHealth;
    public int currentHealth;
    public int baseBlock;

    [Header("Status")]
    protected Dictionary<StatusEffectType, int> statusEffects = new Dictionary<StatusEffectType, int>();
    protected int currentBlock;

    // Events
    public event Action<int> OnHealthChanged;
    public event Action<int> OnBlockChanged;
    public event Action<StatusEffectType, int> OnStatusEffectApplied;
    public event Action<StatusEffectType, int> OnStatusEffectRemoved;
    public event Action OnDeath;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentBlock => currentBlock;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void Initialize()
    {
        currentHealth = maxHealth;
        currentBlock = 0;
        statusEffects.Clear();
    }

    public virtual void TakeDamage(int amount, Entity source)
    {
        // Apply vulnerabilities
        if (HasStatusEffect(StatusEffectType.Vulnerable))
        {
            amount = Mathf.FloorToInt(amount * 1.5f);
        }

        // Reduce damage by current block
        if (currentBlock > 0)
        {
            int blockDamage = Mathf.Min(amount, currentBlock);
            amount -= blockDamage;
            SetBlock(currentBlock - blockDamage);
        }

        // Apply remaining damage to health
        if (amount > 0)
        {
            SetHealth(currentHealth - amount);

            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public virtual void Heal(int amount)
    {
        SetHealth(Mathf.Min(currentHealth + amount, maxHealth));
    }

    public virtual void AddBlock(int amount)
    {
        // Apply frail status effect
        if (HasStatusEffect(StatusEffectType.Frail))
        {
            amount = Mathf.FloorToInt(amount * 0.75f);
        }

        SetBlock(currentBlock + amount);
    }

    public virtual void ApplyStatusEffect(StatusEffectType type, int amount)
    {
        if (amount <= 0) return;

        // Some status effects don't stack, they replace
        if (!statusEffects.ContainsKey(type))
        {
            statusEffects[type] = 0;
        }

        statusEffects[type] += amount;
        OnStatusEffectApplied?.Invoke(type, statusEffects[type]);
    }

    public virtual void RemoveStatusEffect(StatusEffectType type, int amount = 0)
    {
        if (!statusEffects.ContainsKey(type)) return;

        if (amount <= 0 || amount >= statusEffects[type])
        {
            int previousAmount = statusEffects[type];
            statusEffects.Remove(type);
            OnStatusEffectRemoved?.Invoke(type, previousAmount);
        }
        else
        {
            statusEffects[type] -= amount;
            OnStatusEffectRemoved?.Invoke(type, amount);
        }
    }

    public bool HasStatusEffect(StatusEffectType type)
    {
        return statusEffects.ContainsKey(type) && statusEffects[type] > 0;
    }

    public int GetStatusEffectAmount(StatusEffectType type)
    {
        return statusEffects.ContainsKey(type) ? statusEffects[type] : 0;
    }

    public virtual void OnStartTurn()
    {
        // Process status effects that trigger at the start of the turn
        ProcessStatusEffects(true);
    }

    public virtual void OnEndTurn()
    {
        // Process status effects that trigger at the end of the turn
        ProcessStatusEffects(false);

        // Reset block at the end of the turn
        SetBlock(0);
    }

    protected virtual void ProcessStatusEffects(bool isStartOfTurn)
    {
        List<StatusEffectType> effectsToRemove = new List<StatusEffectType>();

        foreach (var effect in statusEffects)
        {
            StatusEffectType type = effect.Key;
            int value = effect.Value;

            if (isStartOfTurn)
            {
                // Effects that trigger at the start of turn
                switch (type)
                {
                    case StatusEffectType.Poison:
                        TakeDamage(value, this); // Poison damages the entity
                        statusEffects[type]--; // Reduce poison by 1
                        break;

                    case StatusEffectType.Burn:
                        TakeDamage(value, this); // Burn damages the entity
                        break;
                }
            }
            else
            {
                // Effects that trigger at the end of turn
                switch (type)
                {
                    case StatusEffectType.Vulnerable:
                    case StatusEffectType.Weak:
                    case StatusEffectType.Frail:
                        statusEffects[type]--; // Reduce duration
                        break;

                    case StatusEffectType.Stun:
                        statusEffects[type]--; // Reduce stun duration
                        break;
                }
            }

            // Check if the effect should be removed
            if (statusEffects[type] <= 0)
            {
                effectsToRemove.Add(type);
            }
        }

        // Remove expired effects
        foreach (var type in effectsToRemove)
        {
            RemoveStatusEffect(type);
        }
    }

    public int GetDamageModifier()
    {
        int modifier = 0;

        // Add strength
        if (HasStatusEffect(StatusEffectType.Strength))
        {
            modifier += GetStatusEffectAmount(StatusEffectType.Strength);
        }

        // Add other damage modifiers

        return modifier;
    }

    public int GetBlockModifier()
    {
        int modifier = 0;

        // Add dexterity
        if (HasStatusEffect(StatusEffectType.Dexterity))
        {
            modifier += GetStatusEffectAmount(StatusEffectType.Dexterity);
        }

        // Add other block modifiers

        return modifier;
    }

    protected virtual void SetHealth(int newHealth)
    {
        int oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        if (oldHealth != currentHealth)
        {
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    protected virtual void SetBlock(int newBlock)
    {
        int oldBlock = currentBlock;
        currentBlock = Mathf.Max(0, newBlock);

        if (oldBlock != currentBlock)
        {
            OnBlockChanged?.Invoke(currentBlock);
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
    }

    public virtual void Reset()
    {
        SetHealth(maxHealth);
        SetBlock(0);
        statusEffects.Clear();
    }

    public Dictionary<StatusEffectType, int> GetAllStatusEffects()
    {
        return new Dictionary<StatusEffectType, int>(statusEffects);
    }

    public void SetStatusEffects(Dictionary<StatusEffectType, int> effects)
    {
        statusEffects.Clear();
        foreach (var effect in effects)
        {
            statusEffects[effect.Key] = effect.Value;
            OnStatusEffectApplied?.Invoke(effect.Key, effect.Value);
        }
    }
}