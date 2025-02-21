using System;
using UnityEngine;

public class LoadScreenNewGame : MonoBehaviour
{
    //When new game is created, the game is saved after the time it was created
    public void StartNewGame()
    {
        DateTime dt = DateTime.Now;
        GameSaveStateManager.Instance.StartNewGame("SaveState               " + dt.ToString("yyyy-MM-ddTHH-mm"));
        
        FindObjectOfType<MainMenuUI>().gameStateLoaded = false;
    }
}
