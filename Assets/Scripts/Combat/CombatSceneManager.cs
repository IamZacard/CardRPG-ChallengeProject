using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene transitions for combat encounters, handling asynchronous scene loading
/// and maintaining persistent data between scenes.
/// </summary>
public class CombatSceneManager : MonoBehaviour
{
    // Singleton pattern
    public static CombatSceneManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string combatSceneName = "Combat";
    [SerializeField] private string rewardsSceneName = "Rewards";

    [Header("Transition Settings")]
    [SerializeField] private GameObject transitionPrefab;
    [SerializeField] private float transitionDuration = 1.0f;

    // Events for scene transitions
    public event Action BeforeSceneUnload;
    public event Action AfterSceneLoaded;

    // Track current loading operation
    private AsyncOperation currentLoadOperation;
    private GameObject currentTransition;

    private void Awake()
    {
        // Singleton implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Initiates transition to the combat scene with provided combat data
    /// </summary>
    /// <param name="combatData">Data needed to initialize the combat</param>
    public void LoadCombatScene(CombatData combatData)
    {
        // Save combat data to GameManager for retrieval after scene load
        GameManager.Instance.SetPendingCombatData(combatData);

        // Begin scene transition
        StartCoroutine(TransitionToCombatScene());
    }

    /// <summary>
    /// Returns to the map from combat scene
    /// </summary>
    public void ReturnToMap()
    {
        StartCoroutine(LoadSceneWithTransition(mapSceneName));
    }

    /// <summary>
    /// Proceeds to rewards scene after combat
    /// </summary>
    /// <param name="rewardData">Rewards earned from combat</param>
    public void ProceedToRewards(RewardData rewardData)
    {
        // Save reward data to GameManager
        GameManager.Instance.SetPendingRewardData(rewardData);

        StartCoroutine(LoadSceneWithTransition(rewardsSceneName));
    }

    /// <summary>
    /// Returns to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        StartCoroutine(LoadSceneWithTransition(mainMenuSceneName));
    }

    /// <summary>
    /// Coroutine that handles the transition to combat scene
    /// </summary>
    private IEnumerator TransitionToCombatScene()
    {
        yield return LoadSceneWithTransition(combatSceneName);
    }

    /// <summary>
    /// Core scene loading coroutine with transition effects
    /// </summary>
    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        // Show transition effect
        ShowTransition();

        // Wait for transition animation
        yield return new WaitForSeconds(transitionDuration);

        // Notify listeners about scene unload
        BeforeSceneUnload?.Invoke();

        // Begin async scene loading
        currentLoadOperation = SceneManager.LoadSceneAsync(sceneName);

        // Optional: prevent scene activation until fully loaded
        // currentLoadOperation.allowSceneActivation = false;

        // Wait until scene is loaded
        while (!currentLoadOperation.isDone)
        {
            // Update loading progress if needed
            float progress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
            Debug.Log($"Loading progress: {progress * 100}%");

            yield return null;
        }

        // Scene has been loaded
        Debug.Log($"Scene {sceneName} loaded successfully");

        // Notify listeners about scene load completion
        AfterSceneLoaded?.Invoke();

        // Wait a moment before hiding transition
        yield return new WaitForSeconds(0.5f);

        // Hide transition effect
        HideTransition();
    }

    /// <summary>
    /// Shows the transition effect
    /// </summary>
    private void ShowTransition()
    {
        if (transitionPrefab != null && currentTransition == null)
        {
            currentTransition = Instantiate(transitionPrefab);
            DontDestroyOnLoad(currentTransition);
        }
    }

    /// <summary>
    /// Hides the transition effect
    /// </summary>
    private void HideTransition()
    {
        if (currentTransition != null)
        {
            Destroy(currentTransition);
            currentTransition = null;
        }
    }

    /// <summary>
    /// Emergency method to handle loading errors
    /// </summary>
    public void HandleLoadingError()
    {
        Debug.LogError("Scene loading error detected!");

        // Clean up transition
        HideTransition();

        // Attempt to return to a safe state
        SceneManager.LoadScene(mainMenuSceneName);
    }
}