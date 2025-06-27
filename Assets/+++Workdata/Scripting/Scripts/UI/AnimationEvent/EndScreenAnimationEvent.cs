using UnityEngine;

public class EndScreenAnimationEvent : MonoBehaviour
{
    private void GoToMainMenuAfterGameEnd()
    {
        InGameUIManager.Instance.changeLight = false;

        GameSaveStateManager.Instance.gameGotFinished = true;
        
        GameSaveStateManager.Instance.GoToMainMenu();
    }
}
