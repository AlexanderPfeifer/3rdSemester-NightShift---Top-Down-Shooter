using UnityEngine;
using UnityEngine.EventSystems;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("SpinPower")] 
    private const float SpinPower = 1250;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;
    
    private const int FortuneWheelPieCount = 5;

    [SerializeField] private GameObject firstFortuneWheelButtonSelected;

    private Rigidbody2D rb;
    [HideInInspector] public bool wheelGotSpinned;

    private void OnEnable()
    {
        Player.Instance.isInteracting = true;
        
        EventSystem.current.SetSelectedGameObject(firstFortuneWheelButtonSelected);
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

            wheelGotSpinned = true;
        }

        if (_angularVelocity > 0 || !wheelGotSpinned) 
            return;
        
        GetRewardPosition();
    }
    
    public void SpinWheel()
    {
        if (rb.angularVelocity > 0) 
            return;
        
        rb.AddTorque(SpinPower);
    }

    private void GetRewardPosition()
    {
        const float pieSize = 360f / FortuneWheelPieCount;
        //Do not know anymore why I put +41 right now, but it works
        int _priceIndex = Mathf.FloorToInt((rb.transform.eulerAngles.z + 36f) / pieSize) % Player.Instance.allWeaponPrizes.Count;
        GetWeaponPrize(Player.Instance.allWeaponPrizes[_priceIndex]);
    }
    
    private void GetWeaponPrize(WeaponObjectSO weapon)
    {
        Player.Instance.GetWeapon(weapon);
        GameSaveStateManager.Instance.saveGameDataManager.AddWeapon(weapon.weaponName);

        gameObject.SetActive(false);
        wheelGotSpinned = false;

        if (Player.Instance.GetInteractionObjectInRange(Player.Instance.wheelOfFortuneLayer, out Collider2D _interactable))
        {
            var _wheelOfFortune = _interactable.GetComponent<FortuneWheel>();
            _wheelOfFortune.ride.GetComponent<Ride>().canActivateRide = true;
            _wheelOfFortune.DeactivateFortuneWheel();
        }
    }

    private void OnDisable()
    {
        Player.Instance.isInteracting = false;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
