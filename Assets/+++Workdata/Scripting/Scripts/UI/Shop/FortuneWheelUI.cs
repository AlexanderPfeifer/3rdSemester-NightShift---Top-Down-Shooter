using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("Price Settings")] 
    [SerializeField] private UnityEvent[] prizes;
    [SerializeField] private int fortuneWheelPieCount = 5;
    [SerializeField] private float firstPieSliceBufferInDegree = 36f;
    [SerializeField] private int spinPrice;

    [Header("Price Dialogue")] 
    [SerializeField, TextArea(3, 10)] private string blankDialogue;
    [SerializeField, TextArea(3, 10)] private string currencyDialogue;
    [SerializeField, TextArea(3, 10)] private string largeCurrencyDialogue;

    [Header("Spinning Movement")] 
    [SerializeField] private Vector2Int timeUntilStop;
    [SerializeField] private Vector2Int fullSpinsUntilStop;
    [SerializeField] private Rigidbody2D rb;
    private bool receivingPrize;
    [SerializeField] private Image mark;
    
    [Header("EventSystem Controlling")]
    [SerializeField] private GameObject firstFortuneWheelButtonSelected;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstFortuneWheelButtonSelected);
        
        mark.transform.localScale = new Vector3(1, 1, 1);
    }

    private void Start()
    {
        rb.transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
    }

    private IEnumerator WheelOverTimeCoroutine()
    {
        float _timeUntilStop = Random.Range(timeUntilStop.x, timeUntilStop.y);
        int _randomPrize = Random.Range(0, prizes.Length);
        
        float _startRotation = rb.rotation % 360f;
        
        float _pieSize = 360f / fortuneWheelPieCount;
        
        int _fullSpins = Random.Range(fullSpinsUntilStop.x, fullSpinsUntilStop.y); 
        float _endRotation = _startRotation + _fullSpins * 360f + _randomPrize * _pieSize;
        
        float _elapsed = 0f;
        while (_elapsed < _timeUntilStop)
        {
            _elapsed += Time.deltaTime;
            float _t = Mathf.Sin((_elapsed / _timeUntilStop) * Mathf.PI * 0.5f); 
            float _currentRotation = Mathf.Lerp(_startRotation, _endRotation, _t);

            rb.MoveRotation(_currentRotation); 
            yield return null;
        }

        rb.MoveRotation(_endRotation);

        int _priceIndex = Mathf.FloorToInt((rb.transform.eulerAngles.z + firstPieSliceBufferInDegree) / _pieSize) % fortuneWheelPieCount;
        StartCoroutine(LocationHighlight(_priceIndex));
        
        receivingPrize = true;
    }
    
    public void SpinWheel()
    {
        if (rb.angularVelocity > 0 || receivingPrize || !PlayerBehaviour.Instance.playerCurrency.SpendCurrency(spinPrice)) 
            return;

        AllButtonsConfiguration.Instance.inGameUICanvasGroup.interactable = false;
        StartCoroutine(WheelOverTimeCoroutine());
    }

    private IEnumerator LocationHighlight(int priceIndex)
    {
        mark.transform.localScale = new Vector3(2, 2, 1);
        
        yield return new WaitForSeconds(.3f);
        
        mark.transform.localScale = new Vector3(1, 1, 1);

        yield return new WaitForSeconds(.3f);
        
        mark.transform.localScale = new Vector3(2, 2, 1);

        yield return new WaitForSeconds(.3f);
        
        mark.transform.localScale = new Vector3(1, 1, 1);
        
        yield return new WaitForSeconds(.3f);

        mark.transform.localScale = new Vector3(2, 2, 1);
        
        yield return new WaitForSeconds(.3f);
        
        mark.transform.localScale = new Vector3(1, 1, 1);
        
        yield return new WaitForSeconds(.3f);
        
        prizes[priceIndex]?.Invoke();

        receivingPrize = false;
        
        AllButtonsConfiguration.Instance.inGameUICanvasGroup.interactable = true;
    }

    public void WinWeapon(WeaponObjectSO weapon)
    {
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weapon);
        
        InGameUIManager.Instance.shopUI.ResetWeaponDescriptions();
    }
    
    public void WinBlank()
    {
        StartCoroutine(InGameUIManager.Instance.dialogueUI.TypeTextCoroutine(blankDialogue, null, InGameUIManager.Instance.dialogueUI.currentTextBox));
    }   
    
    public void WinMoney(int money)
    {
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(money, true);
        
        StartCoroutine(InGameUIManager.Instance.dialogueUI.TypeTextCoroutine(currencyDialogue, null, InGameUIManager.Instance.dialogueUI.currentTextBox));
    }    
    
    public void WinLargeCurrency(int money)
    {
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(money, true);
        
        StartCoroutine(InGameUIManager.Instance.dialogueUI.TypeTextCoroutine(largeCurrencyDialogue, null, InGameUIManager.Instance.dialogueUI.currentTextBox));
    }

    private void OnDisable()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
    
    [Header("Debugging Stuff")]
    [SerializeField] private float labelRadiusMultiplier = 0.8f;
    //Font size does not change through inspector
    private readonly int labelFontSize = 50;
    
#if UNITY_EDITOR
    private GUIStyle labelStyle;
#endif

    private void OnDrawGizmos()
    {
        RectTransform _rt = rb.GetComponent<RectTransform>();
        Vector3 _center = _rt.position;
        float _radius = _rt.rect.width * _rt.lossyScale.x * 0.5f;
        float _angleStep = 360f / fortuneWheelPieCount;
        float _imageRotation = _rt.eulerAngles.z;

#if UNITY_EDITOR
        labelStyle ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = labelFontSize,
            normal =
            {
                textColor = Color.white
            },
            alignment = TextAnchor.MiddleCenter
        };
#endif

        Gizmos.color = Color.white;

        for (int _i = 0; _i < fortuneWheelPieCount; _i++)
        {
            int _currentIndex = _i % fortuneWheelPieCount;
            //Add 90 degrees to make the fortune wheel start at "12 o'clock" otherwise unity default is pointing to right(3 o'clock)
            float _startAngle = -(_currentIndex * _angleStep) + _imageRotation + firstPieSliceBufferInDegree + 90f;
            float _midAngle = _startAngle - _angleStep / 2f;

            float _radStart = _startAngle * Mathf.Deg2Rad;
            float _radMid = _midAngle * Mathf.Deg2Rad;

            Vector3 _dirStart = new Vector3(Mathf.Cos(_radStart), Mathf.Sin(_radStart), 0f);
            Vector3 _dirMid = new Vector3(Mathf.Cos(_radMid), Mathf.Sin(_radMid), 0f);

            Gizmos.DrawLine(_center, _center + _dirStart * _radius);

#if UNITY_EDITOR
            Vector3 _labelPos = _center + _dirMid * _radius * labelRadiusMultiplier;
            Handles.Label(_labelPos, _currentIndex.ToString(), labelStyle);
#endif
        }
    }
}
