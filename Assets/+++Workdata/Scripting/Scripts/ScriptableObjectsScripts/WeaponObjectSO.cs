using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class WeaponObjectSO : ScriptableObject
{
    //Scriptable Object for every unique weapon with unique perks
    [FormerlySerializedAs("inventoryWeaponVisual")] [FormerlySerializedAs("inGameWeaponVisual")] public Sprite uiWeaponVisual;
    public Sprite inGameWeaponVisual;
    public string weaponName;
    public int upgradeTier;
    [TextArea(3, 10)] public string weaponDescription;
    [FormerlySerializedAs("weaponStats")] [TextArea(3, 10)] public string weaponAbilityDescription;
    public float shootDelay;
    public float bulletDamage;
    public float weaponSpread;
    [FormerlySerializedAs("knockBack")] public float playerKnockBack;
    [FormerlySerializedAs("enemyKnockBackPerBullet")] public float enemyKnockBack;
    public float screenShake;
    public Vector3 weaponScale;
    public Vector2 bulletSize;
    public int penetrationCount;
    public int bulletsPerShot;
    public int clipSize;
    public int ammunitionInClip;
    public int ammunitionInBackUp;
    [FormerlySerializedAs("showAbilityFill")] public bool hasAbilityUpgrade;
}
