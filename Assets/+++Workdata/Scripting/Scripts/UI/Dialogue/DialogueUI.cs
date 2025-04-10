using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private Animator radioAnim;
    public Animator dialogueBoxAnim;
    public Dialogues[] dialogue;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float standardTextDisplaySpeed = 0.05f;
    [HideInInspector] public float textDisplaySpeed;
    [HideInInspector] public int dialogueCount;
    public float maxTextDisplaySpeed = 0.00005f;
    private int dialogueTextCount;
    
    [HideInInspector] public DialogueState dialogueState = DialogueState.DialogueNotPlaying;

    public enum DialogueState
    {
        DialogueNotPlaying,
        DialoguePlaying,
        DialogueAbleToGoNext,
        DialogueAbleToEnd,
    }
    
    public void DisplayDialogue()
    {
        StartCoroutine(TypeTextCoroutine(dialogue[dialogueCount].dialogues[dialogueTextCount]));
        
        dialogueTextCount++;
    }

    public void ResetDialogueElements()
    {
        dialogueCount = 0;
        dialogueTextCount = 0;
        dialogueText.text = "";
        radioAnim.Rebind();
        dialogueBoxAnim.Rebind();
        dialogueState = DialogueState.DialogueNotPlaying;
    }

    public void SetDialogueState()
    {
        switch (dialogueState)
        {
            case DialogueState.DialoguePlaying:
                textDisplaySpeed = maxTextDisplaySpeed;
                break;
            case DialogueState.DialogueAbleToGoNext:
                PlayNextDialogue();
                break;
            case DialogueState.DialogueAbleToEnd:
                EndDialogue();
                break;
            case DialogueState.DialogueNotPlaying:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool IsDialoguePlaying()
    {
        return dialogueState != DialogueState.DialogueNotPlaying;
    }
    
    private IEnumerator TypeTextCoroutine(string text)
    {
        dialogueState = DialogueState.DialoguePlaying;
        dialogueText.text = "";
        dialogueText.textWrappingMode = TextWrappingModes.Normal;

        var _words = text.Split(' ');
        string _displayText = ""; 
        float _availableWidth = dialogueText.rectTransform.rect.width;

        foreach (var _word in _words)
        {
            string _testLine = _displayText + _word + " ";
            dialogueText.text = _testLine;
            dialogueText.ForceMeshUpdate();
            float _textWidth = dialogueText.preferredWidth;

            if (_textWidth > _availableWidth) // If word doesn't fit, move to a new line before typing
            {
                _displayText += "\n";
            }

            // Update the text for each word, not each character, for performance reasons
            foreach (char _letter in _word + " ")
            {
                _displayText += _letter;
                dialogueText.text = _displayText;
                yield return new WaitForSeconds(textDisplaySpeed);
            }
        }
        
        //This checks for the current dialogue that is playing, whether they talked until the end or not
        if (dialogueTextCount == dialogue[dialogueCount].dialogues.Count)
        {
            dialogueState = DialogueState.DialogueAbleToEnd;
        }
        else
        {
            dialogueState = DialogueState.DialogueAbleToGoNext;
        }
    }

    public void SetRadioState(bool radioOn, bool putOn)
    {
        radioAnim.SetBool("RadioOn", radioOn);
        radioAnim.SetBool("PutOn", putOn);

        if (!radioOn)
        {
            dialogueBoxAnim.SetBool("DialogueBoxOn", false);   
        }
    }

    public void EndDialogue()
    {
        dialogueTextCount = 0;
        dialogueCount++;
        dialogueText.text = "";
        SetRadioState(false, true);
        dialogueState = DialogueState.DialogueNotPlaying;

        if (dialogueCount == dialogue.Length)
        {
            InGameUIManager.Instance.EndScreen();
        }
        else if (dialogueCount == 1)
        {
            TutorialManager.Instance.GetFirstWeaponAndWalkieTalkie();
        }
        else if (dialogueCount == 5)
        {
            InGameUIManager.Instance.currencyUI.gameObject.SetActive(true);
        }
    }

    public void PlayNextDialogue()
    {
        dialogueState = DialogueState.DialoguePlaying;
        dialogueText.text = "";
        textDisplaySpeed = standardTextDisplaySpeed;

        DisplayDialogue();
    }
}
