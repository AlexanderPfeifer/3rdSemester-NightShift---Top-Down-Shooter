using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;

    public TextMeshProUGUI GetCurrencyText()
    {
        return currencyText;
    }
}
