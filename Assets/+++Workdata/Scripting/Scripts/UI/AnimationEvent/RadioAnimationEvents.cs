using UnityEngine;

public class RadioAnimationEvents : MonoBehaviour
{
    public void ShowDialogueBox()
    {
        InGameUIManager.Instance.dialogueBoxAnim.SetTrigger("DialogueBoxOn");
    }
}
