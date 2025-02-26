using UnityEngine;

public class EndScreenAnimationEvent : MonoBehaviour
{
    private void GoToMainMenuAfterGameEnd()
    {
        InGameUIManager.Instance.changeLight = false;

        GameSaveStateManager.Instance.gameGotFinished = true;
        
        InGameUIManager.Instance.endScreen.gameObject.SetActive(false);
        
        GameSaveStateManager.Instance.GoToMainMenu();
    }
}
