using System;
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    [NonSerialized] private int currency;
    [SerializeField] private float timeBetweenAddingNumbers;
    private float currentTimeBetweenAddingNumbers;

    [Header("ShowCurrencyNumberByNumber")] 
    [SerializeField] private int maxCurrencyNumberByNumberMultiplier = 10;
    [SerializeField] private int divisionNumberPerMultiplier = 100;
    private int currencyText;

    private void Start()
    {
        SetCurrencyText();

        currentTimeBetweenAddingNumbers = timeBetweenAddingNumbers;
    }

    private void Update()
    {
        UpdateCurrencyTextNumberByNumber();
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
    }

    private void SetCurrencyText()
    {
        InGameUIManager.Instance.currencyUI.GetCurrencyText().text = "Currency:\n" + currencyText;
    }

    public bool SpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            return true;
        }
        
        return false;
    }

    private void UpdateCurrencyTextNumberByNumber()
    {
        if (currencyText == currency)
            return;
        
        float _addNumberMultiplier = Mathf.Clamp(Mathf.Abs(currency - currencyText) / divisionNumberPerMultiplier, 1, maxCurrencyNumberByNumberMultiplier);

        if (currentTimeBetweenAddingNumbers < 0)
        {
            //Mathf Sign to subtract or add numbers if needed - so if it is -1, it decreases number accordingly
            int _step = Mathf.CeilToInt(_addNumberMultiplier * Mathf.Sign(currency - currencyText));
            currencyText += _step;

            if ((_step > 0 && currencyText > currency) || (_step < 0 && currencyText < currency))
            {
                currencyText = currency;
            }

            SetCurrencyText();
            currentTimeBetweenAddingNumbers = timeBetweenAddingNumbers / _addNumberMultiplier;
        }
        else
        {
            currentTimeBetweenAddingNumbers -= Time.deltaTime;
        }
    }
}
