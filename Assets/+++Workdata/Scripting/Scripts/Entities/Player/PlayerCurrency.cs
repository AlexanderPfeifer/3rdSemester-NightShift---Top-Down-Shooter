using System;
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    [NonSerialized] private int currency;
    [SerializeField] private float timeBetweenAddingNumbers;

    [Header("ShowCurrencyNumberByNumber")] 
    [SerializeField] private int maxCurrencyNumberByNumberMultiplier = 10;
    [SerializeField] private int divisionNumberPerMultiplier = 100;
    private float currentTimeBetweenAddingNumbers;
    private int currencyText;

    private bool playTschaTschingSFX;

    private void Start()
    {
        SetCurrencyText();

        currentTimeBetweenAddingNumbers = timeBetweenAddingNumbers;
    }

    private void Update()
    {
        UpdateCurrencyTextNumberByNumber();
    }

    public void AddCurrency(int amount, bool playTschaTschingSFX)
    {
        currency += amount;
        
        if (playTschaTschingSFX)
        {
            this.playTschaTschingSFX = true;
        }
    }

    private void SetCurrencyText()
    {
        InGameUIManager.Instance.currencyUI.GetCurrencyText().text = currencyText.ToString();
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

    public bool CheckEnoughCurrency(int amount)
    {
        if (currency >= amount)
        {
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
            AudioManager.Instance.Play("CurrencyAdd");

            if ((_step > 0 && currencyText > currency) || (_step < 0 && currencyText < currency) || currencyText == currency)
            {
                currencyText = currency;

                if (playTschaTschingSFX)
                {
                    AudioManager.Instance.Play("GotRideMoney");
                    playTschaTschingSFX = false;
                }
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
