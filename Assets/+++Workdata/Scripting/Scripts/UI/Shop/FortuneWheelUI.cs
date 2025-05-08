using System.Collections;
using System.Linq;
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

    [Header("Spinning Movement")] 
    [SerializeField] private Vector2Int timeUntilStop;
    [SerializeField] private float spinPower = 1250;
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
        float _elapsed = 0f;
        float _timeUntilStop = Random.Range(timeUntilStop.x, timeUntilStop.y);

        while (_elapsed < _timeUntilStop)
        {
            _elapsed += Time.deltaTime;

            float _currentVelocity = Mathf.Lerp(spinPower, 0f, _elapsed / _timeUntilStop);

            rb.angularVelocity = _currentVelocity;

            yield return null;
        }

        rb.angularVelocity = 0f;

        GetRewardPosition();
    }
    
    public void SpinWheel()
    {
        if (rb.angularVelocity > 0 || receivingPrize || !PlayerBehaviour.Instance.playerCurrency.SpendCurrency(spinPrice)) 
            return;
        
        rb.AddTorque(spinPower);
        StartCoroutine(WheelOverTimeCoroutine());
    }

    private void GetRewardPosition()
    {
        float _pieSize = 360f / fortuneWheelPieCount;
        
        //The + 36f is there because the wheel of fortune starts in the middle of a field when on rotation 0,0,0 
        int _priceIndex = Mathf.FloorToInt((rb.transform.eulerAngles.z + firstPieSliceBufferInDegree) / _pieSize) % PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Count;
        StartCoroutine(LocationHighlight(_priceIndex));
        
        receivingPrize = true;
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
    }

    public void WinWeapon(WeaponObjectSO weapon)
    {
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weapon);
    }

    public void WinMoney(int money)
    {
        PlayerBehaviour.Instance.playerCurrency.AddCurrency(money, true);
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
