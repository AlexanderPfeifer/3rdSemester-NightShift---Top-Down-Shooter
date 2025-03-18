using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    [SerializeField] private int currentCurrency;
    private int currencyBeforeChange;

    private void Start()
    {
        SetCurrencyText();
    }

    private void Update()
    {
        UpdateCurrencyTextNumberByNumber();
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
    }

    private void SetCurrencyText()
    {
        InGameUIManager.Instance.currencyUI.GetCurrencyText().text = "Currency:\n" + currencyBeforeChange;
    }

    public bool SpendCurrency(int amount)
    {
        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            return true;
        }
        
        return false;
    }

    private void UpdateCurrencyTextNumberByNumber()
    {
        if(currencyBeforeChange == currentCurrency)
            return;
        
        currencyBeforeChange += (int)Mathf.Sign(currentCurrency - currencyBeforeChange);
        SetCurrencyText();
    }
}
