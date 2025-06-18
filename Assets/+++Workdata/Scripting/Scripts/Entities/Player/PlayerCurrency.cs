using System;
using TMPro;
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public TextMeshProUGUI currencyText;
    public GameObject currencyBackground;
    [NonSerialized] private int currency;
    [SerializeField] private float timeBetweenAddingNumbers;

    [Header("ShowCurrencyNumberByNumber")] 
    [SerializeField] private int maxCurrencyNumberByNumberMultiplier = 10;
    [SerializeField] private int divisionNumberPerMultiplier = 100;
    private float currentTimeBetweenAddingNumbers;
    private int currencyAmount;

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
        currencyText.text = currencyAmount.ToString();
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
        if (currencyAmount == currency)
            return;
        
        float _addNumberMultiplier = Mathf.Clamp(Mathf.Abs(currency - currencyAmount) / divisionNumberPerMultiplier, 1, maxCurrencyNumberByNumberMultiplier);

        if (currentTimeBetweenAddingNumbers < 0)
        {
            //Mathf Sign to subtract or add numbers if needed - so if it is -1, it decreases number accordingly
            int _step = Mathf.CeilToInt(_addNumberMultiplier * Mathf.Sign(currency - currencyAmount));
            currencyAmount += _step;
            AudioManager.Instance.Play("CurrencyAdd");

            if ((_step > 0 && currencyAmount > currency) || (_step < 0 && currencyAmount < currency) || currencyAmount == currency)
            {
                currencyAmount = currency;

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
