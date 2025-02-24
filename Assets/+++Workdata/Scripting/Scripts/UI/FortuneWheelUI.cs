using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("Velocity Floats")] 
    private const float SpinPower = 1250;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;
    
    private const int FortuneWheelPieCount = 5;

    [SerializeField] private GameObject firstFortuneWheelSelected;

    private Rigidbody2D rb;
    [HideInInspector] public bool canGetPrize;

    //Gets and Sets the fortune wheel to a random rotation
    private void OnEnable()
    {
        Player.Instance.isInteracting = true;
        
        EventSystem.current.SetSelectedGameObject(firstFortuneWheelSelected);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
    }

    private void Update()
    {
        FortuneWheelUpdate();
    }

    //Slows down the fortune wheel over time after being span
    private void FortuneWheelUpdate()
    {
        var angularVelocity = rb.angularVelocity;

        if (angularVelocity > 0)
        {
            angularVelocity -= Random.Range(minStopPower, maxStopPower) * Time.deltaTime;
            if (angularVelocity < 5)
            {
                angularVelocity = 0;
            }
            rb.angularVelocity = angularVelocity;

            rb.angularVelocity = Mathf.Clamp(angularVelocity, 0, maxAngularVelocity);

            canGetPrize = true;
        }

        if (angularVelocity > 0 || !canGetPrize) 
            return;
        
        GetRewardPosition();
    }
    
    //Spins the fortune wheel by adding torque to it
    public void SpinWheel()
    {
        if (rb.angularVelocity > 0) 
            return;
        
        rb.AddTorque(SpinPower);

        Player.Instance.isInteracting = true;
    }

    //Gets the Rotation of where the fortune wheel stopped spinning
    private void GetRewardPosition()
    {
        var rotationAngle = transform.eulerAngles.z;
        const float pieSize = (360f / FortuneWheelPieCount);
        int priceIndex = Mathf.FloorToInt((rotationAngle + 41f) / pieSize) % Player.Instance.allWeaponPrizes.Count;
        GetWeaponPrize(Player.Instance.allWeaponPrizes[priceIndex]);
    }
    
    //Gets the weapon prize and sets every weapon specification to the player
    private void GetWeaponPrize(WeaponObjectSO weapon)
    {
        Player.Instance.GetWeapon(weapon);
        
        Player.Instance.isInteracting = false;

        GameSaveStateManager.Instance.saveGameDataManager.AddWeapon(weapon.weaponName);

        gameObject.transform.parent.gameObject.SetActive(false);
        canGetPrize = false;

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
