// CombatManager.cs - Manages the combat flow and state
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum CombatPhase
{
    Initializing,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public Transform enemyContainer;
    public CombatUI combatUI;

    [Header("Combat Settings")]
    public float enemyActionDelay = 1f;
    public float cardPlayAnimationDelay = 0.5f;

    private CombatState combatState;
    private CombatPhase currentPhase;
    private Entity selectedTarget;
    private Card selectedCard;

    // Events
    public event Action<CombatPhase> OnCombatPhaseChanged;
    public event Action<Card> OnCardPlayed;
    public event Action<Entity> OnEntityTargeted;
    public event Action OnCombatVictory;
    public event Action OnCombatDefeat;

    // Properties
    public CombatPhase CurrentPhase => currentPhase;
    public CombatState CurrentCombatState => combatState;

    private void Awake()
    {
        combatState = new CombatState
        {
            CombatManager = this
        };
    }

    private void Start()
    {
        // Register UI events
        if (combatUI != null)
        {
            combatUI.OnCardSelected += HandleCardSelection;
            combatUI.OnEntitySelected += HandleEntitySelection;
            combatUI.OnEndTurnButtonClicked += EndPlayerTurn;
        }
    }

    public void StartCombat(List<Enemy> enemies)
    {
        // Set phase to initializing
        SetCombatPhase(CombatPhase.Initializing);

        // Clear previous enemies if any
        foreach (Transform child in enemyContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate enemies
        List<Enemy> spawnedEnemies = new List<Enemy>();
        foreach (var enemyPrefab in enemies)
        {
            Enemy enemy = Instantiate(enemyPrefab, enemyContainer);
            enemy.Initialize();
            spawnedEnemies.Add(enemy);

            // Subscribe to enemy events
            enemy.OnDeath += () => CheckCombatEnd();
        }

        // Initialize combat state
        combatState.Player = player;
        combatState.Enemies = spawnedEnemies.ToArray();
        combatState.TurnNumber = 0;

        // Initialize player for combat
        player.InitializeForCombat();

        // Subscribe to player death
        player.OnDeath += HandlePlayerDeath;

        // Start the first turn
        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        SetCombatPhase(CombatPhase.PlayerTurn);

        // Process start of turn for the combat state
        combatState.StartNewTurn(true);

        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateUI(combatState);
        }
    }

    private void EndPlayerTurn()
    {
        // Process end of turn for the combat state
        combatState.EndCurrentTurn();

        // Start enemy turn
        StartCoroutine(ProcessEnemyTurn());
    }

    private IEnumerator ProcessEnemyTurn()
    {
        SetCombatPhase(CombatPhase.EnemyTurn);

        // Process start of turn for enemies
        combatState.StartNewTurn(false);

        // Update UI
        if (combatUI != null)
        {
            combatUI.UpdateUI(combatState);
        }

        // Allow a brief pause before enemy actions
        yield return new WaitForSeconds(0.5f);

        // Process each enemy's turn
        foreach (var enemy in combatState.GetAliveEnemies())
        {
            // Select and perform action
            EnemyAction action = enemy.nextAction;

            if (action != null)
            {
                // Visually indicate the enemy is acting
                if (combatUI != null)
                {
                    combatUI.HighlightEntity(enemy);
                }

                yield return new WaitForSeconds(enemyActionDelay);

                // Perform the action
                PerformEnemyAction(enemy, action);

                // Update UI after action
                if (combatUI != null)
                {
                    combatUI.UpdateUI(combatState);
                }

                yield return new WaitForSeconds(0.3f);

                // Un-highlight the enemy
                if (combatUI != null)
                {
                    combatUI.UnhighlightEntity(enemy);
                }
            }

            // Check if player is dead after this enemy's action
            if (player.CurrentHealth <= 0)
            {
                HandlePlayerDeath();
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        // End enemy turn
        combatState.EndCurrentTurn();

        // Start player turn
        StartPlayerTurn();
    }

    private void PerformEnemyAction(Enemy enemy, EnemyAction action)
    {
        // Process the enemy action based on its properties
        if (action.damage > 0)
        {
            if (action.targetsAll)
            {
                // AoE damage not applicable in this simplified version
                player.TakeDamage(action.damage, enemy);
            }
            else
            {
                player.TakeDamage(action.damage, enemy);
            }
        }

        if (action.block > 0)
        {
            enemy.AddBlock(action.block);
        }

        if (action.statusEffectType.HasValue)
        {
            int amount = action.statusEffectAmount;
            if (action.selfEffect)
            {
                enemy.ApplyStatusEffect(action.statusEffectType.Value, amount);
            }
            else
            {
                player.ApplyStatusEffect(action.statusEffectType.Value, amount);
            }
        }

        // Choose next action for the enemy
        enemy.SelectNextAction();
    }

    private void HandleCardSelection(Card card)
    {
        selectedCard = card;

        // Clear previous targeting
        selectedTarget = null;

        // Determine if this card needs a target
        if (card.cardData.target == CardTarget.SingleEnemy)
        {
            // Enable targeting mode in the UI
            if (combatUI != null)
            {
                combatUI.EnableTargetMode(true);
            }
        }
        else
        {
            // Play the card immediately if no target is needed
            PlaySelectedCard();
        }
    }

    private void HandleEntitySelection(Entity entity)
    {
        // If we have a selected card that needs a target
        if (selectedCard != null && selectedCard.cardData.target == CardTarget.SingleEnemy)
        {
            selectedTarget = entity;
            PlaySelectedCard();

            // Disable targeting mode
            if (combatUI != null)
            {
                combatUI.EnableTargetMode(false);
            }
        }
    }

    private void PlaySelectedCard()
    {
        if (selectedCard == null) return;

        // Get the appropriate target
        Entity target = null;
        switch (selectedCard.cardData.target)
        {
            case CardTarget.SingleEnemy:
                target = selectedTarget;
                if (target == null || !(target is Enemy))
                {
                    Debug.LogWarning("Cannot play card: No valid enemy target selected.");
                    return;
                }
                break;

            case CardTarget.Self:
                target = player;
                break;

            case CardTarget.AllEnemies:
            case CardTarget.None:
                // No specific target needed
                target = null;
                break;
        }

        // Play the card
        bool success = player.PlayCard(selectedCard, target);

        if (success)
        {
            // Trigger card played event
            OnCardPlayed?.Invoke(selectedCard);

            // Check for combat end after card is played
            StartCoroutine(DelayedCheckForCombatEnd());

            // Update UI
            if (combatUI != null)
            {
                combatUI.UpdateUI(combatState);
            }
        }

        // Reset selection
        selectedCard = null;
        selectedTarget = null;
    }

    private IEnumerator DelayedCheckForCombatEnd()
    {
        // Wait for any animations to finish
        yield return new WaitForSeconds(cardPlayAnimationDelay);

        // Check if combat is over
        CheckCombatEnd();
    }

    private void CheckCombatEnd()
    {
        // Check for victory
        if (combatState.AreAllEnemiesDead())
        {
            SetCombatPhase(CombatPhase.Victory);
            OnCombatVictory?.Invoke();
            return;
        }

        // Check for defeat (should be handled by player death event)
    }

    private void HandlePlayerDeath()
    {
        SetCombatPhase(CombatPhase.Defeat);
        OnCombatDefeat?.Invoke();
    }

    private void SetCombatPhase(CombatPhase newPhase)
    {
        if (currentPhase != newPhase)
        {
            currentPhase = newPhase;
            OnCombatPhaseChanged?.Invoke(currentPhase);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (player != null)
        {
            player.OnDeath -= HandlePlayerDeath;
        }

        if (combatUI != null)
        {
            combatUI.OnCardSelected -= HandleCardSelection;
            combatUI.OnEntitySelected -= HandleEntitySelection;
            combatUI.OnEndTurnButtonClicked -= EndPlayerTurn;
        }
    }

    internal void SetCombatState(CombatState combatState)
    {
        throw new NotImplementedException();
    }
}