using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("Price Settings")]
    [SerializeField] private int fortuneWheelPieCount = 5;
    [SerializeField] private float firstPieSliceBufferInDegree = 36f;
    [SerializeField] private int spinPrice;

    [Header("Spinning Movement")] 
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;
    private const float SpinPower = 1250;
    [SerializeField] private Rigidbody2D rb;
    private bool wheelSpinning;
    private bool receivingWeapon;
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

    private void Update()
    {
        FortuneWheelUpdate();
    }

    private void FortuneWheelUpdate()
    {
        var _angularVelocity = rb.angularVelocity;

        if (_angularVelocity > 0)
        {
            _angularVelocity -= Random.Range(minStopPower, maxStopPower) * Time.deltaTime;

            rb.angularVelocity = Mathf.Clamp(_angularVelocity, 0, maxAngularVelocity);

            wheelSpinning = true;
        }

        if (_angularVelocity > 0 || !wheelSpinning) 
            return;
        
        GetRewardPosition();
    }
    
    public void SpinWheel()
    {
        if (rb.angularVelocity > 0 || receivingWeapon || !PlayerBehaviour.Instance.playerCurrency.SpendCurrency(spinPrice)) 
            return;
        
        rb.AddTorque(SpinPower);
    }

    private void GetRewardPosition()
    {
        float _pieSize = 360f / fortuneWheelPieCount;
        
        //The + 36f is there because the wheel of fortune starts in the middle of a field when on rotation 0,0,0 
        int _priceIndex = Mathf.FloorToInt((rb.transform.eulerAngles.z + firstPieSliceBufferInDegree) / _pieSize) % PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Count;
        StartCoroutine(GetWeaponPrize(PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes[_priceIndex]));
        
        wheelSpinning = false;
        receivingWeapon = true;
    }
    
    private IEnumerator GetWeaponPrize(WeaponObjectSO weapon)
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
        
        PlayerBehaviour.Instance.weaponBehaviour.GetWeapon(weapon);
        
        receivingWeapon = false;
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
