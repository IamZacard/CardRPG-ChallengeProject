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
    // Added events for StatusEffectManager
    public event Action OnCombatStart;
    public event Action OnCombatEnd;

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
        if (combatUI != null)
        {
            combatUI.OnCardSelected += HandleCardSelection;
            combatUI.OnEntitySelected += HandleEntitySelection;
            combatUI.OnEndTurnButtonClicked += EndPlayerTurn;
        }
    }

    public void StartCombat(List<Enemy> enemies)
    {
        SetCombatPhase(CombatPhase.Initializing);
        OnCombatStart?.Invoke(); // Invoke at the start of combat

        foreach (Transform child in enemyContainer)
        {
            Destroy(child.gameObject);
        }

        List<Enemy> spawnedEnemies = new List<Enemy>();
        foreach (var enemyPrefab in enemies)
        {
            Enemy enemy = Instantiate(enemyPrefab, enemyContainer);
            enemy.Initialize();
            spawnedEnemies.Add(enemy);
            enemy.OnDeath += () => CheckCombatEnd();
        }

        combatState.Player = player;
        combatState.Enemies = spawnedEnemies.ToArray();
        combatState.TurnNumber = 0;

        player.InitializeForCombat();
        player.OnDeath += HandlePlayerDeath;

        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        SetCombatPhase(CombatPhase.PlayerTurn);
        combatState.StartNewTurn(true);
        if (combatUI != null)
        {
            combatUI.UpdateUI(combatState);
        }
    }

    private void EndPlayerTurn()
    {
        combatState.EndCurrentTurn();
        StartCoroutine(ProcessEnemyTurn());
    }

    private IEnumerator ProcessEnemyTurn()
    {
        SetCombatPhase(CombatPhase.EnemyTurn);
        combatState.StartNewTurn(false);
        if (combatUI != null)
        {
            combatUI.UpdateUI(combatState);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var enemy in combatState.GetAliveEnemies())
        {
            EnemyAction action = enemy.nextAction;
            if (action != null)
            {
                if (combatUI != null)
                {
                    combatUI.HighlightEntity(enemy);
                }

                yield return new WaitForSeconds(enemyActionDelay);
                PerformEnemyAction(enemy, action);

                if (combatUI != null)
                {
                    combatUI.UpdateUI(combatState);
                }

                yield return new WaitForSeconds(0.3f);

                if (combatUI != null)
                {
                    combatUI.UnhighlightEntity(enemy);
                }
            }

            if (player.CurrentHealth <= 0)
            {
                HandlePlayerDeath();
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        combatState.EndCurrentTurn();
        StartPlayerTurn();
    }

    private void PerformEnemyAction(Enemy enemy, EnemyAction action)
    {
        if (action.damage > 0)
        {
            if (action.targetsAll)
            {
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

        enemy.SelectNextAction();
    }

    private void HandleCardSelection(Card card)
    {
        selectedCard = card;
        selectedTarget = null;

        if (card.cardData.target == CardTarget.SingleEnemy)
        {
            if (combatUI != null)
            {
                combatUI.EnableTargetMode(true);
            }
        }
        else
        {
            PlaySelectedCard();
        }
    }

    private void HandleEntitySelection(Entity entity)
    {
        if (selectedCard != null && selectedCard.cardData.target == CardTarget.SingleEnemy)
        {
            selectedTarget = entity;
            PlaySelectedCard();

            if (combatUI != null)
            {
                combatUI.EnableTargetMode(false);
            }
        }
    }

    private void PlaySelectedCard()
    {
        if (selectedCard == null) return;

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
                target = null;
                break;
        }

        bool success = player.PlayCard(selectedCard, target);
        if (success)
        {
            OnCardPlayed?.Invoke(selectedCard);
            StartCoroutine(DelayedCheckForCombatEnd());

            if (combatUI != null)
            {
                combatUI.UpdateUI(combatState);
            }
        }

        selectedCard = null;
        selectedTarget = null;
    }

    private IEnumerator DelayedCheckForCombatEnd()
    {
        yield return new WaitForSeconds(cardPlayAnimationDelay);
        CheckCombatEnd();
    }

    private void CheckCombatEnd()
    {
        if (combatState.AreAllEnemiesDead())
        {
            SetCombatPhase(CombatPhase.Victory);
            OnCombatVictory?.Invoke();
            OnCombatEnd?.Invoke(); // Invoke at victory
            return;
        }
    }

    private void HandlePlayerDeath()
    {
        SetCombatPhase(CombatPhase.Defeat);
        OnCombatDefeat?.Invoke();
        OnCombatEnd?.Invoke(); // Invoke at defeat
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
}