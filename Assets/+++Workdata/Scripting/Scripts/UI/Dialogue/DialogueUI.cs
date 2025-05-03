using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class DialogueUI : MonoBehaviour
{
    [Header("DialogueType")]
    [FormerlySerializedAs("dialogue")] public Dialogues[] dialogueShop;
    public Dialogues[] dialogueWalkieTalkie;
    [SerializeField] private TextMeshProUGUI shopText;
    [FormerlySerializedAs("dialogueText")] [SerializeField] private TextMeshProUGUI walkieTalkieText;
    [HideInInspector] public TextMeshProUGUI currentTextBox;
    [HideInInspector] public int dialogueCountShop;
    [HideInInspector] public int dialogueCountWalkieTalkie;
    [HideInInspector] public int currentDialogueCount;
    
    [Header("Typing Settings")]
    [SerializeField] private float standardTextDisplaySpeed = 0.05f;
    [HideInInspector] public float textDisplaySpeed;
    public float maxTextDisplaySpeed = 0.00005f;
    
    [Header("Animations")]
    [SerializeField] private Animator radioAnim;
    [FormerlySerializedAs("dialogueBoxAnim")] public Animator walkieTalkieDialogueBoxAnim;
    
    [Header("DialogueState")]
    private int dialogueTextCount;
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
        if (inShop)
        {
            currentTextBox = shopText;
            currentDialogueCount = dialogueCountShop;
        }
        else
        {
            currentTextBox = walkieTalkieText;
            currentDialogueCount = dialogueCountWalkieTalkie;
        }
    }
    
    public void DisplayDialogue()
    {
        if (currentTextBox == shopText)
        {
            if (dialogueShop.Length >= currentDialogueCount)
            {
                StartCoroutine(TypeTextCoroutine(dialogueShop[currentDialogueCount].dialogues[dialogueTextCount], dialogueShop));
            }
        }
        else
        {
            if (dialogueWalkieTalkie.Length >= currentDialogueCount)
            {
                StartCoroutine(TypeTextCoroutine(dialogueWalkieTalkie[currentDialogueCount].dialogues[dialogueTextCount], dialogueWalkieTalkie));
            }
        }
        
        dialogueTextCount++;
    }

    public void ResetDialogueElements()
    {
        dialogueCountShop = 0;
        dialogueCountWalkieTalkie = 0;
        currentDialogueCount = 0;
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
    
    private IEnumerator TypeTextCoroutine(string text, Dialogues[] currentDialogue)
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
        
        CheckDialogueEnd(currentDialogue);
    }

    private void CheckDialogueEnd(IReadOnlyList<Dialogues> currentDialogue)
    {
        if (dialogueTextCount == currentDialogue[currentDialogueCount].dialogues.Count)
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
        if (currentTextBox != shopText)
        {
            radioAnim.SetBool("RadioOn", radioOn);
            radioAnim.SetBool("PutOn", putOn);

            if (!radioOn)
            {
                walkieTalkieDialogueBoxAnim.SetBool("DialogueBoxOn", false);   
                PlayerBehaviour.Instance.SetPlayerBusy(false);
            }
            else
            {
                PlayerBehaviour.Instance.SetPlayerBusy(true);
            }
        }
        else
        {
            //Also some animation for the text box in the shop
        }
    }

    public void EndDialogue()
    {
        dialogueTextCount = 0;
        currentDialogueCount++;
        currentTextBox.text = "";
        
        SetDialogueBoxState(false, true);
        
        dialogueState = DialogueState.DialogueNotPlaying;

        if (currentTextBox != shopText)
        {
            dialogueCountWalkieTalkie++;
            return;
        }
        
        dialogueCountShop++;

        if (currentDialogueCount == dialogueShop.Length)
        {
            //Need to find a way to end the game
            //InGameUIManager.Instance.EndScreen();
        }
        if (currentDialogueCount == 1)
        {
            TutorialManager.Instance.GetFirstWeaponAndWalkieTalkie();
        }
        else if (currentDialogueCount == 5)
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
