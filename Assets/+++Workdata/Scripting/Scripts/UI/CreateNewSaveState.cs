using System;
using UnityEngine;

public class CreateNewSaveState : MonoBehaviour
{
    //When new game is created in the loading screen animation, the game is saved with an animation event
    public void StartNewGame()
    {
        DateTime _dt = DateTime.Now;
        GameSaveStateManager.Instance.StartNewGame("SaveState               " + _dt.ToString("yyyy-MM-ddTHH-mm"));
        
        MainMenuUIManager.Instance.gameStateLoaded = false;
    }
}
