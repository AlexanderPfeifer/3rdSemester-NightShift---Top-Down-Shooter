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
    [SerializeField] public List<WeaponObjectSO> currentWeaponPrizes;

    private const int FortuneWheelPieCount = 5;

    private Player player;
    private Rigidbody2D rb;
    [HideInInspector] public bool canGetPrize;

    private void OnEnable()
    {
        player = FindObjectOfType<Player>();
        
        player.isInteracting = true;
        
        for (int i = 0; i < currentWeaponPrizes.Count; i++)
        {
            currentWeaponPrizes.RemoveAt(0);
        }
        
        for (int i = 0; i < player.allWeaponPrizes.Count; i++)
        {
            currentWeaponPrizes.Add(player.allWeaponPrizes[i]);
        }
        
        for (int i = 0; i < currentWeaponPrizes.Count; i++)
        {
            if (GameSaveStateManager.instance.saveGameDataManager.HasWeaponInInventory(currentWeaponPrizes[i].weaponName))
            {
                currentWeaponPrizes.Remove(currentWeaponPrizes[i]);
            }
        }
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
        int priceIndex = Mathf.FloorToInt((rotationAngle+22.5f) / pieSize) % currentWeaponPrizes.Count;
        GetWeaponPrize(currentWeaponPrizes[priceIndex]);
    }
    
    private void GetWeaponPrize(WeaponObjectSO weapon)
    {
        player.weaponVisual.GetComponent<SpriteRenderer>().sprite = weapon.inGameWeaponVisual;
        player.weaponVisual.SetActive(true);
        player.bulletDamage = weapon.bulletDamage;
        player.maxPenetrationCount = weapon.penetrationCount;
        player.maxShootDelay = weapon.shootDelay;
        player.activeAbilityGain = weapon.activeAbilityGain;
        player.shootingSpread = weapon.weaponSpread;
        player.weaponVisual.transform.localScale = weapon.weaponScale;
        
        GameSaveStateManager.instance.saveGameDataManager.AddWeapon(weapon.weaponName);
        currentWeaponPrizes.Remove(weapon);

        switch (weapon.weaponName)
        {
            case "Magnum" :
                player.AbilityFunction = player.StartMagnumAbility;
                break;
            case "Assault Rifle" :
                player.AbilityFunction = player.StartAssaultRifleAbility;
                break;
            case "Shotgun" :
                player.AbilityFunction = player.StartShotgunAbility;
                break;
            case "Hunting Rifle" :
                player.AbilityFunction = player.StartHuntingRifleAbility;
                break;
            case "Pistol" :
                player.AbilityFunction = player.StartPistolAbility;
                break;
        }

        gameObject.transform.parent.gameObject.SetActive(false);
        player.fortuneWheelGotUsed = true;
        canGetPrize = false;
    }

    private void OnDisable()
    {
        player.isInteracting = false;
    }
}
