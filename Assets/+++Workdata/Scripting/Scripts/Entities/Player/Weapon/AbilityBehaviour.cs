using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilityBehaviour : MonoBehaviour
{
    [Header("UI")]
    public GameObject pressSpace;
    public Image abilityProgressImage;

    [Header("Ability Time")]
    public float maxAbilityTime;
    public float fastBulletsDelay = 0.075f;
    [HideInInspector] public bool canGetAbilityGain = true;
    private float currentAbilityTime;
    [HideInInspector] public bool hasAbilityUpgrade;
    
    [HideInInspector] public CurrentAbility currentActiveAbility = CurrentAbility.None;

    public enum CurrentAbility
    {
        FastBullets,
        StickyBullets,
        FreezeBullets,
        ExplosiveBullets,
        PenetrationBullets,
        None,
    }
    
    private void OnEnable()
    {
        GameInputManager.Instance.OnUsingAbilityAction += GameInputManagerOnUsingAbilityAction;
    }
    
    private void OnDisable()
    {
        GameInputManager.Instance.OnUsingAbilityAction -= GameInputManagerOnUsingAbilityAction;
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStickButton.isPressed && Gamepad.current.rightStickButton.isPressed)
            {
                StartCoroutine(StartWeaponAbility());
            }
        }
    }

    private void GameInputManagerOnUsingAbilityAction(object sender, EventArgs e)
    {
        if (currentAbilityTime >= maxAbilityTime && Ride.Instance.waveStarted && hasAbilityUpgrade)
        {
            StartCoroutine(StartWeaponAbility());
        }
    }
    
    private IEnumerator StartWeaponAbility()
    {
        canGetAbilityGain = false;
        pressSpace.SetActive(false);
        
        foreach (var _bullet in BulletPoolingManager.Instance.GetBulletList())
        {
            _bullet.GetComponentInChildren<SpriteRenderer>().sprite = PlayerBehaviour.Instance.weaponBehaviour.GetCurrentWeaponObjectSO().abilityBulletSprite;
        }

        currentActiveAbility = PlayerBehaviour.Instance.weaponBehaviour.myWeapon switch
        {
            WeaponBehaviour.MyWeapon.AssaultRifle => CurrentAbility.FastBullets,
            WeaponBehaviour.MyWeapon.Magnum => CurrentAbility.FreezeBullets,
            WeaponBehaviour.MyWeapon.PopcornLauncher => CurrentAbility.ExplosiveBullets,
            WeaponBehaviour.MyWeapon.HuntingRifle => CurrentAbility.PenetrationBullets,
            WeaponBehaviour.MyWeapon.Shotgun => CurrentAbility.StickyBullets,
            _ => throw new ArgumentOutOfRangeException()
        };

        while (currentAbilityTime > 0)
        {
            currentAbilityTime -= Time.deltaTime;
            abilityProgressImage.fillAmount = currentAbilityTime / maxAbilityTime;
            yield return null; 
        }
        
        currentActiveAbility = CurrentAbility.None;
        
        foreach (var _bullet in BulletPoolingManager.Instance.GetBulletList())
        {
            _bullet.GetComponentInChildren<SpriteRenderer>().sprite = PlayerBehaviour.Instance.weaponBehaviour.GetCurrentWeaponObjectSO().bulletSprite;
        }

        canGetAbilityGain = true;
    }

    public void AddAbilityFill(float enemyAbilityGainForPlayer)
    {
        if (canGetAbilityGain && hasAbilityUpgrade)
        {
            currentAbilityTime += enemyAbilityGainForPlayer;

            if (currentAbilityTime >= maxAbilityTime)
            {
                pressSpace.SetActive(true);
                currentAbilityTime = maxAbilityTime;
            }
            
            abilityProgressImage.fillAmount = currentAbilityTime / maxAbilityTime;
        }
    }
}
