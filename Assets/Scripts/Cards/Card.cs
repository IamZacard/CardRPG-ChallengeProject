using UnityEngine;
using System;

public abstract class Card
{
    public CardData cardData;
    public int instanceId;

    // Events
    public event Action<Card> OnCardPlayed;
    public event Action<Card> OnCardDiscarded;
    public event Action<Card> OnCardExhausted;

    public Card(CardData data)
    {
        cardData = data;
        instanceId = GetHashCode();
    }

    public virtual bool CanBePlayed(CombatState combatState)
    {
        return combatState.Player.CurrentActionPoints >= cardData.cost;
    }

    public virtual void Play(CombatState combatState, Entity target)
    {
        // Deduct action points
        combatState.Player.SpendActionPoints(cardData.cost);

        // Apply card effects based on type
        ApplyCardEffects(combatState, target);

        // Trigger event
        OnCardPlayed?.Invoke(this);

        // Handle special card traits
        if (cardData.exhaust)
        {
            combatState.ExhaustPile.AddCard(this);
            OnCardExhausted?.Invoke(this);
        }
        else
        {
            combatState.DiscardPile.AddCard(this);
            OnCardDiscarded?.Invoke(this);
        }
    }

    protected virtual void ApplyCardEffects(CombatState combatState, Entity target)
    {
        // Base implementation for common effects
        if (cardData.damage > 0)
        {
            int damageToApply = cardData.damage;
            // Apply strength or other modifiers
            damageToApply += combatState.Player.GetDamageModifier();

            switch (cardData.target)
            {
                case CardTarget.SingleEnemy:
                    if (target != null && target != combatState.Player)
                    {
                        target.TakeDamage(damageToApply, combatState.Player);
                    }
                    break;
                case CardTarget.AllEnemies:
                    foreach (Enemy enemy in combatState.Enemies)
                    {
                        enemy.TakeDamage(damageToApply, combatState.Player);
                    }
                    break;
            }
        }

        if (cardData.block > 0)
        {
            int blockToApply = cardData.block;
            // Apply dexterity or other modifiers
            blockToApply += combatState.Player.GetBlockModifier();

            combatState.Player.AddBlock(blockToApply);
        }

        // Apply status effects
        foreach (var effectData in cardData.statusEffects)
        {
            switch (effectData.targetType)
            {
                case CardData.StatusEffectData.TargetType.Self:
                    combatState.Player.ApplyStatusEffect(effectData.type, effectData.amount);
                    break;
                case CardData.StatusEffectData.TargetType.Target:
                    if (target != null && target != combatState.Player)
                    {
                        target.ApplyStatusEffect(effectData.type, effectData.amount);
                    }
                    break;
                case CardData.StatusEffectData.TargetType.AllEnemies:
                    foreach (Enemy enemy in combatState.Enemies)
                    {
                        enemy.ApplyStatusEffect(effectData.type, effectData.amount);
                    }
                    break;
            }
        }
    }

    public virtual string GetCurrentDescription()
    {
        // Could be expanded to replace placeholders in description with current values
        return cardData.description;
    }
}