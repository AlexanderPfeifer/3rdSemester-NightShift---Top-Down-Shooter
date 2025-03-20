using System;
using System.Collections;
using UnityEngine;

public class AbilityBehaviour : MonoBehaviour
{
    [Header("Weapon Ability")]
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
    
    private void GameInputManagerOnUsingAbilityAction(object sender, EventArgs e)
    {
        if (currentAbilityTime >= maxAbilityTime && InGameUIManager.Instance.fightScene.activeSelf && hasAbilityUpgrade)
        {
            StartCoroutine(StartWeaponAbility());
        }
    }
    
    private IEnumerator StartWeaponAbility()
    {
        canGetAbilityGain = false;
        InGameUIManager.Instance.pressSpace.SetActive(false);

        currentActiveAbility = PlayerBehaviour.Instance.weaponBehaviour.myWeapon switch
        {
            WeaponBehaviour.MyWeapon.AssaultRifle => CurrentAbility.FastBullets,
            WeaponBehaviour.MyWeapon.Magnum => CurrentAbility.FreezeBullets,
            WeaponBehaviour.MyWeapon.Pistol => CurrentAbility.ExplosiveBullets,
            WeaponBehaviour.MyWeapon.HuntingRifle => CurrentAbility.PenetrationBullets,
            WeaponBehaviour.MyWeapon.Shotgun => CurrentAbility.StickyBullets,
            _ => throw new ArgumentOutOfRangeException()
        };

        while (currentAbilityTime > 0)
        {
            currentAbilityTime -= Time.deltaTime;
            InGameUIManager.Instance.abilityProgressImage.fillAmount = currentAbilityTime / maxAbilityTime;
            yield return null; 
        }
        
        currentActiveAbility = CurrentAbility.None;

        canGetAbilityGain = true;
    }

    public void AddAbilityFill(float enemyAbilityGainForPlayer)
    {
        if (canGetAbilityGain && hasAbilityUpgrade)
        {
            currentAbilityTime += enemyAbilityGainForPlayer;
            InGameUIManager.Instance.abilityProgressImage.fillAmount = currentAbilityTime / maxAbilityTime;

            if (currentAbilityTime >= maxAbilityTime)
            {
                InGameUIManager.Instance.pressSpace.SetActive(true);
            }
        }
    }
}
