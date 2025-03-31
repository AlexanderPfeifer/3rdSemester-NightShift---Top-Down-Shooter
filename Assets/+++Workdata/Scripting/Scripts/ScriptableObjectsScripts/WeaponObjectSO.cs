using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class WeaponObjectSO : ScriptableObject
{
    public string weaponName;
    
    [Header("Upgrades")]
    public int upgradeTier;
    
    [Header("Descriptions")]
    [TextArea(3, 10)] public string weaponDescription;
    [FormerlySerializedAs("weaponStats")] [TextArea(3, 10)] public string weaponAbilityDescription;
    
    [Header("Knock Back")]
    [FormerlySerializedAs("knockBack")] public float playerKnockBack;
    [FormerlySerializedAs("enemyKnockBack")] public float enemyKnockBackPerBullet;
    
    [Header("Bullets")]
    public int bulletsPerShot;
    public float shootDelay;
    public float bulletDamage;
    public float weaponSpread;
    public int penetrationCount;
    
    [Header("Visuals")]
    [FormerlySerializedAs("inventoryWeaponVisual")] [FormerlySerializedAs("inGameWeaponVisual")] public Sprite uiWeaponVisual;
    public Sprite inGameWeaponVisual;
    public float screenShake;
    public Vector3 weaponScale;
    public Vector2 bulletSize;

    [Header("Ammo")]
    public int clipSize;
    public int ammunitionInClip;
    public int ammunitionInBackUp;
    
    [Header("Ability")]
    [FormerlySerializedAs("showAbilityFill")] public bool hasAbilityUpgrade;
}
