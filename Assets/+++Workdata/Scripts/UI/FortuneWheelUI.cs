using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("Velocity Floats")]
    [SerializeField] private float minSpinPower, maxSpinPower;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;

    [Header("Weapon Prize List")]
    [SerializeField] private List<WeaponObjectSO> weaponPrizes;

    private const int FortuneWheelPieCount = 5;

    private Player player;
    private Rigidbody2D rb;
    [HideInInspector] public bool canGetPrize;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }
    
    private void OnEnable()
    {
        player.isInteracting = true;
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
        int priceIndex = Mathf.FloorToInt((rotationAngle+22.5f) / pieSize) % weaponPrizes.Count;
        GetWeaponPrize(weaponPrizes[priceIndex]);
    }

    private void GetWeaponPrize(WeaponObjectSO weapon)
    {
        player.bulletPrefab = weapon.bulletPrefab;
        player.weaponVisual.GetComponent<SpriteRenderer>().sprite = weapon.inGameWeaponVisual;
        player.weaponVisual.SetActive(true);
        canGetPrize = false;
        gameObject.transform.parent.gameObject.SetActive(false);
        GameSaveStateManager.instance.saveGameDataManager.AddWeapon(weapon.weaponName);
        weaponPrizes.Remove(weapon);
        player.fortuneWheelGotUsed = true;
        //Assign ability to player
    }

    private void OnDisable()
    {
        player.isInteracting = false;
    }
}
