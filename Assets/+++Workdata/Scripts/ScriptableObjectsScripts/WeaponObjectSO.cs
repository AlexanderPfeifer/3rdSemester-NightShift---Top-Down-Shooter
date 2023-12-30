using UnityEngine;

[CreateAssetMenu()]
public class WeaponObjectSO : ScriptableObject
{
    public Bullet bulletPrefab;
    public Sprite weaponVisual;
    public string weaponName;
    public string weaponDescription;
}
