using UnityEngine;

public class RadioAnimationEvents : MonoBehaviour
{
    public void ShowDialogueBox()
    {
        InGameUIManager.Instance.dialogueUI.walkieTalkieDialogueBoxAnim.SetBool("DialogueBoxOn", true);
    }

    public void PlayRadioSound()
    {
        AudioManager.Instance.Play("WalkieTalkie");
    }
}
