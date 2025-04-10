using UnityEngine;

public class RadioAnimationEvents : MonoBehaviour
{
    public void ShowDialogueBox()
    {
        InGameUIManager.Instance.dialogueUI.dialogueBoxAnim.SetBool("DialogueBoxOn", true);
    }

    public void PlayRadioSound()
    {
        AudioManager.Instance.Play("WalkieTalkie");
    }
}
