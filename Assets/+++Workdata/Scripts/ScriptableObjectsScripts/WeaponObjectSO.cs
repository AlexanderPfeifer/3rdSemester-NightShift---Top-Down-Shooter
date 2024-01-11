using UnityEngine;

[CreateAssetMenu()]
public class WeaponObjectSO : ScriptableObject
{
    public Sprite inGameWeaponVisual;
    public string weaponName;
    public string weaponDescription;
    public float shootDelay;
    public int penetrationCount;
    public float bulletDamage;
    public float activeAbilityGain;
    public float weaponSpread;
    public Vector3 weaponScale;
    public int bulletsPerShot;
}
