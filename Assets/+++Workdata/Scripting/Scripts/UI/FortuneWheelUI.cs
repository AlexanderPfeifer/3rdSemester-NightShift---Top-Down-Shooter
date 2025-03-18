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
    
    private const int FortuneWheelPieCount = 5;

    [SerializeField] private GameObject firstFortuneWheelButtonSelected;

    private Rigidbody2D rb;
    private bool wheelSpinning;
    private bool receivingWeapon;

    [SerializeField] private Image mark; 

    private void OnEnable()
    {
        Player.Instance.isInteracting = true;
        
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
        if (rb.angularVelocity > 0 || receivingWeapon || !Player.Instance.playerCurrency.SpendCurrency(spinPrice)) 
            return;
        
        rb.AddTorque(SpinPower);
    }

    private void GetRewardPosition()
    {
        const float pieSize = 360f / FortuneWheelPieCount;
        
        //The + 36f is there because the wheel of fortune starts in the middle of a field when on rotation 0,0,0 
        int _priceIndex = Mathf.FloorToInt((rb.transform.eulerAngles.z + 36f) / pieSize) % Player.Instance.allWeaponPrizes.Count;
        StartCoroutine(GetWeaponPrize(Player.Instance.allWeaponPrizes[_priceIndex]));
        
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
        
        Player.Instance.GetWeapon(weapon);
        GameSaveStateManager.Instance.saveGameDataManager.AddWeapon(weapon.weaponName);

        gameObject.SetActive(false);

        receivingWeapon = false;
    }

    private void OnDisable()
    {
        Player.Instance.isInteracting = false;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
