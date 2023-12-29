using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void StartNewGame()
    {
        GameSaveStateManager.instance.StartNewGame();
    }

    public void LoadGame()
    {
        GameSaveStateManager.instance.LoadFromSave("NightShiftSaveState");
    }
}
