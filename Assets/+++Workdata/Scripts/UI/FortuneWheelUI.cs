using UnityEngine;
using Random = UnityEngine.Random;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("Velocity Floats")]
    [SerializeField] private float minSpinPower, maxSpinPower;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;
    
    private const int FortuneWheelPieCount = 5;
    private float randomRotation;

    private Player player;
    private Rigidbody2D rb;
    [HideInInspector] public bool canGetPrize;

    private void OnEnable()
    {
        player = FindObjectOfType<Player>();
        
        player.isInteracting = true;

        randomRotation = Random.Range(0, 360);
        var transformEulerAngles = transform.eulerAngles;
        transformEulerAngles.z = randomRotation;
        transform.eulerAngles = transformEulerAngles;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        FortuneWheelUpdate();
    }

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
    
    public void SpinWheel()
    {
        if (rb.angularVelocity > 0) 
            return;
        
        rb.AddTorque(Random.Range(minSpinPower, maxSpinPower));
    }

    private void GetRewardPosition()
    {
        var rotationAngle = transform.eulerAngles.z;
        const float pieSize = (360f / FortuneWheelPieCount);
        int priceIndex = Mathf.FloorToInt((rotationAngle + 40) / pieSize) % player.allWeaponPrizes.Count;
        GetWeaponPrize(player.allWeaponPrizes[priceIndex]);
    }
    
    private void GetWeaponPrize(WeaponObjectSO weapon)
    {
        player.GetWeapon(weapon);

        GameSaveStateManager.instance.saveGameDataManager.AddWeapon(weapon.weaponName);

        gameObject.transform.parent.gameObject.SetActive(false);
        player.fortuneWheelGotUsed = true;
        canGetPrize = false;
        
        player.SearchInteractionObject(player.wheelOfFortuneLayer).GetComponent<FortuneWheel>().ride.GetComponent<Ride>().canActivateRide = true;

        player.SearchInteractionObject(player.wheelOfFortuneLayer).GetComponent<FortuneWheel>().DeactivateFortuneWheel();
    }

    private void OnDisable()
    {
        player.isInteracting = false;
    }
}
