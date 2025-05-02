using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public SaveData currentSaveData;

    public void StartNewGame()
    {
        currentSaveData = new SaveData
        {
            playerName = "Player",
            playerHealth = 50,
            playerMaxHealth = 50,
            gold = 0,
            deckCardNames = new System.Collections.Generic.List<string>(),
            currentNodeIndex = 0
        };
        Debug.Log("New game started.");
    }

    public void LoadSavedGame()
    {
        SaveLoadManager saveLoad = GetComponent<SaveLoadManager>();
        currentSaveData = saveLoad.LoadGame();
        if (currentSaveData != null)
        {
            Debug.Log("Game loaded.");
            // Apply data to player, deck, map, etc.
        }
    }

    public void SaveGame()
    {
        SaveLoadManager saveLoad = GetComponent<SaveLoadManager>();
        saveLoad.SaveGame(currentSaveData);
    }
}