using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private Animator radioAnim;
    [FormerlySerializedAs("dialogueBoxAnim")] public Animator walkieTalkieDialogueBoxAnim;
    public Dialogues[] dialogue;
    [HideInInspector] public TextMeshProUGUI currentTextBox;
    [FormerlySerializedAs("dialogueText")] [SerializeField] private TextMeshProUGUI walkieTalkieText;
    [SerializeField] private TextMeshProUGUI shopText;
    [SerializeField] private float standardTextDisplaySpeed = 0.05f;
    [HideInInspector] public float textDisplaySpeed;
    [HideInInspector] public int dialogueCount;
    public float maxTextDisplaySpeed = 0.00005f;
    private int dialogueTextCount;
    private bool usingShopDialogueBox;
    
    [HideInInspector] public DialogueState dialogueState = DialogueState.DialogueNotPlaying;

    public enum DialogueState
    {
        DialogueNotPlaying,
        DialoguePlaying,
        DialogueAbleToGoNext,
        DialogueAbleToEnd,
    }

    public void SetDialogueBox(bool inShop)
    {
        currentTextBox = inShop ? shopText : walkieTalkieText;

        usingShopDialogueBox = inShop;
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
        currentTextBox.text = "";
        radioAnim.Rebind();
        walkieTalkieDialogueBoxAnim.Rebind();
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
        currentTextBox.text = "";
        currentTextBox.textWrappingMode = TextWrappingModes.Normal;

        var _words = text.Split(' ');
        string _displayText = ""; 
        float _availableWidth = currentTextBox.rectTransform.rect.width;

        foreach (var _word in _words)
        {
            string _testLine = _displayText + _word + " ";
            currentTextBox.text = _testLine;
            currentTextBox.ForceMeshUpdate();
            float _textWidth = currentTextBox.preferredWidth;

            if (_textWidth > _availableWidth) // If word doesn't fit, move to a new line before typing
            {
                _displayText += "\n";
            }

            // Update the text for each word, not each character, for performance reasons
            foreach (char _letter in _word + " ")
            {
                _displayText += _letter;
                currentTextBox.text = _displayText;
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

    public void SetDialogueBoxState(bool radioOn, bool putOn)
    {
        if (!usingShopDialogueBox)
        {
            radioAnim.SetBool("RadioOn", radioOn);
            radioAnim.SetBool("PutOn", putOn);

            if (!radioOn)
            {
                walkieTalkieDialogueBoxAnim.SetBool("DialogueBoxOn", false);   
            }
        }
        else
        {
            
        }
    }

    public void EndDialogue()
    {
        dialogueTextCount = 0;
        dialogueCount++;
        currentTextBox.text = "";
        
        SetDialogueBoxState(false, true);
        
        dialogueState = DialogueState.DialogueNotPlaying;

        if (dialogueCount == dialogue.Length)
        {
            InGameUIManager.Instance.EndScreen();
        }
        if (dialogueCount == 1)
        {
            TutorialManager.Instance.GetFirstWeaponAndWalkieTalkie();
        }
        else if (dialogueCount == 5)
        {
            InGameUIManager.Instance.currencyUI.GetCurrencyText().gameObject.SetActive(true);
        }
    }

    public void PlayNextDialogue()
    {
        dialogueState = DialogueState.DialoguePlaying;
        currentTextBox.text = "";
        textDisplaySpeed = standardTextDisplaySpeed;

        DisplayDialogue();
    }
}
