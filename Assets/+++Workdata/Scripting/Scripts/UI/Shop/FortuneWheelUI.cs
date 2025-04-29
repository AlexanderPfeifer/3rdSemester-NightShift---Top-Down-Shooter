using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("SpinPrice")] 
    [SerializeField] private int spinPrice;
    
    [Header("SpinPower")] 
    private const float SpinPower = 1250;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;
    private Rigidbody2D rb;
    private bool wheelSpinning;
    private bool receivingWeapon;
    [SerializeField] private Image mark;

    private const int FortuneWheelPieCount = 5;

    [Header("Controller")]
    [SerializeField] private GameObject firstFortuneWheelButtonSelected;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstFortuneWheelButtonSelected);
        
        mark.transform.localScale = new Vector3(1, 1, 1);
    }

    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();

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
        const float pieSize = 360f / FortuneWheelPieCount;
        
        //The + 36f is there because the wheel of fortune starts in the middle of a field when on rotation 0,0,0 
        int _priceIndex = Mathf.FloorToInt((rb.transform.eulerAngles.z + 36f) / pieSize) % PlayerBehaviour.Instance.weaponBehaviour.allWeaponPrizes.Count;
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
}
