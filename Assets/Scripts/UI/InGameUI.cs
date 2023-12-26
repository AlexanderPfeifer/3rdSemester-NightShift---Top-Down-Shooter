using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI collectibleText;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DisplayCollectedLetters();
        }
    }

    private void DisplayCollectedLetters()
    {
        string text = "";
        
        var letterHeader = "Header";
        var letter = "The text that is displayed";

        text += letterHeader.ToUpper() + "\n";
        text += letter + "\n\n";

        StartCoroutine(LetterByLetterTextCoroutine(collectibleText, text));
    }

    private IEnumerator LetterByLetterTextCoroutine(TextMeshProUGUI textField, string text)
    {
        //an implementation for displaying a text letter by letter.
        string currentText = "";
        for (int index = 0; index < text.Length; index++)
        {
            currentText += text[index];
            textField.text = currentText;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
