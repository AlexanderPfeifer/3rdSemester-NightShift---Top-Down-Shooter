using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class WeaponObjectSO : ScriptableObject
{
    public string weaponName;
    
    [Header("Upgrades")]
    public int upgradeTier;
    
    [Header("UpgradableValues")]
    public float bulletDamage;
    public float shootDelay;
    public int clipSize;
    public float reloadTime;
    
    [Header("Descriptions")]
    [TextArea(3, 10)] public string weaponDescription;
    [FormerlySerializedAs("weaponStats")] [TextArea(3, 10)] public string weaponAbilityDescription;
    
    [Header("Knock Back")]
    [FormerlySerializedAs("knockBack")] public float playerKnockBack;
    [FormerlySerializedAs("enemyKnockBack")] public float enemyKnockBackPerBullet;
    
    [Header("Bullets")]
    public int bulletsPerShot;
    [Range(0,1)] public float weaponBulletSpread;
    public int penetrationCount;
    
    [Header("Visuals")]
    [FormerlySerializedAs("inventoryWeaponVisual")] [FormerlySerializedAs("inGameWeaponVisual")] public Sprite uiWeaponVisual;
    public Sprite inGameWeaponVisual;
    public float screenShake;
    public Vector3 weaponScale;
    public Vector2 bulletSize;
    public Sprite bulletSprite;

    [Header("Ammo")]
    public int ammunitionInClip;
    [FormerlySerializedAs("ammunitionInBackUp")] public int ammunitionBackUpSize;
    public int ammunitionInBackUp;
    
    [Header("Ability")]
    [FormerlySerializedAs("showAbilityFill")] public bool hasAbilityUpgrade;
    public Sprite abilityBulletSprite;

    [Header("Sounds")] 
    public AudioResource shotSound;
    public AudioResource reloadSound;
    public AudioResource repetitionSound;
}
