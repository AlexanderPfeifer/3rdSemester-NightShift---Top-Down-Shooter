using System;
using TMPro;
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
    public TextMeshProUGUI playerCurrencyText;
    public GameObject currencyBackground;
    [NonSerialized] private int playerCurrency;
    [SerializeField] private float timeBetweenAddingNumbers;

    [Header("ShowCurrencyNumberByNumber")] 
    [SerializeField] private int maxCurrencyNumberByNumberMultiplier = 10;
    [SerializeField] private int divisionNumberPerMultiplier = 100;
    private float currentTimeBetweenAddingNumbers;
    private int countedCurrency;

    private bool playTschaTschingSFX;

    private void Start()
    {
        playerCurrencyText.text = countedCurrency.ToString();

        currentTimeBetweenAddingNumbers = timeBetweenAddingNumbers;
    }

    private void Update()
    {
        UpdateCurrencyTextNumberByNumber(playerCurrency, ref countedCurrency, playerCurrencyText);
    }

    public void AddCurrency(int amount, bool playTschaTschingSFX)
    {
        playerCurrency += amount;
        
        if (playTschaTschingSFX)
        {
            this.playTschaTschingSFX = true;
        }
    }

    public bool SpendCurrency(int amount)
    {
        if (playerCurrency >= amount)
        {
            playerCurrency -= amount;
            return true;
        }
        
        return false;
    }

    public bool CheckEnoughCurrency(int amount)
    {
        if (playerCurrency >= amount)
        {
            return true;
        }
        
        return false;
    }

    public void UpdateCurrencyTextNumberByNumber(int targetCurrency, ref int currentCurrency, TextMeshProUGUI currencyText, float currentTimeBetweenAdding)
    {
        if (currentCurrency == targetCurrency)
            return;
        
        float _addNumberMultiplier = Mathf.Clamp(Mathf.Abs(targetCurrency - currentCurrency) / divisionNumberPerMultiplier, 1, maxCurrencyNumberByNumberMultiplier);

        if (currentTimeBetweenAddingNumbers < 0)
        {
            //Mathf Sign to subtract or add numbers if needed - so if it is -1, it decreases number accordingly
            int _step = Mathf.CeilToInt(_addNumberMultiplier * Mathf.Sign(targetCurrency - currentCurrency));
            currentCurrency += _step;
            AudioManager.Instance.Play("CurrencyAdd");

            if ((_step > 0 && currentCurrency > targetCurrency) || (_step < 0 && currentCurrency < targetCurrency) || currentCurrency == targetCurrency)
            {
                currentCurrency = targetCurrency;

                if (playTschaTschingSFX)
                {
                    AudioManager.Instance.Play("GotRideMoney");
                    playTschaTschingSFX = false;
                }
            }

            currencyText.text = currentCurrency.ToString();
            currentTimeBetweenAddingNumbers = timeBetweenAddingNumbers / _addNumberMultiplier;
        }
        else
        {
            currentTimeBetweenAddingNumbers -= Time.deltaTime;
        }
    }
}
