using UnityEngine;

[CreateAssetMenu]
public class WeaponObjectSO : ScriptableObject
{
    //Scriptable Object for every unique weapon with unique perks
    public Sprite inGameWeaponVisual;
    public string weaponName;
    public string weaponDescription;
    public string weaponAbilityDescription;
    public float shootDelay;
    public int penetrationCount;
    public float bulletDamage;
    public float weaponSpread;
    public Vector3 weaponScale;
    public int bulletsPerShot;
    public float knockBack;
    public float enemyKnockBackPerBullet;
    public Vector2 bulletSize;
    public int clipSize;
    public int ammunitionInClip;
    public int ammunitionInBackUp;
    public float screenShake;
}
