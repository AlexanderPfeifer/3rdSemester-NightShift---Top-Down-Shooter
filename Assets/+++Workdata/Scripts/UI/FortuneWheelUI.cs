using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FortuneWheelUI : MonoBehaviour
{
    [Header("Velocity Floats")]
    [SerializeField] private float minSpinPower, maxSpinPower;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;

    [Header("Weapon Prize List")]
    [SerializeField] public List<WeaponObjectSO> currentWeaponPrizes;
    [SerializeField] public List<WeaponObjectSO> allWeaponPrizes;

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
        
        for (int i = 0; i < allWeaponPrizes.Count; i++)
        {
            currentWeaponPrizes.Add(allWeaponPrizes[i]);
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
        }

        gameObject.transform.parent.gameObject.SetActive(false);
        player.fortuneWheelGotUsed = true;
        canGetPrize = false;
    }

    private void GetWeaponOnGameStart(string weaponName)
    {
        var gotFirstWeapon = false;

        for (int i = 0; i < allWeaponPrizes.Count; i++)
        {
            if(GameSaveStateManager.instance.saveGameDataManager.HasWeaponInInventory(allWeaponPrizes[i].weaponName))
            {
                if (!gotFirstWeapon)
                {
                    player.weaponVisual.GetComponent<SpriteRenderer>().sprite = allWeaponPrizes[i].inGameWeaponVisual;
                    player.weaponVisual.SetActive(true);
                    player.bulletDamage = allWeaponPrizes[i].bulletDamage;
                    player.maxPenetrationCount = allWeaponPrizes[i].penetrationCount;
                    player.maxShootDelay = allWeaponPrizes[i].shootDelay;
                    player.activeAbilityGain = allWeaponPrizes[i].activeAbilityGain;
                
                    switch (allWeaponPrizes[i].weaponName)
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
                    }

                    gotFirstWeapon = true;
                }
                else
                {
                    player.secondWeaponVisual.GetComponent<SpriteRenderer>().sprite = allWeaponPrizes[i].inGameWeaponVisual;
                    player.secondBulletDamage = allWeaponPrizes[i].bulletDamage;
                    player.secondMaxPenetrationCount = allWeaponPrizes[i].penetrationCount;
                    player.secondMaxShootDelay = allWeaponPrizes[i].shootDelay;
                    player.secondActiveAbilityGain = allWeaponPrizes[i].activeAbilityGain;
                
                    switch (allWeaponPrizes[i].weaponName)
                    {
                        case "Magnum" :
                            player.SecondAbilityFunction = player.StartMagnumAbility;
                            break;
                        case "Assault Rifle" :
                            player.SecondAbilityFunction = player.StartAssaultRifleAbility;
                            break;
                        case "Shotgun" :
                            player.SecondAbilityFunction = player.StartShotgunAbility;
                            break;
                        case "Hunting Rifle" :
                            player.SecondAbilityFunction = player.StartHuntingRifleAbility;
                            break;
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        player.isInteracting = false;
    }
}
