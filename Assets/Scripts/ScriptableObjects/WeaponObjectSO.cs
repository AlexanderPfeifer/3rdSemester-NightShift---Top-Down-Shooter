using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WeaponObjectSO : ScriptableObject
{
    public Transform bulletPrefab;
    public Transform weaponPrefab;
    public Sprite sprite;
    public float bulletDamage;
    public string objectName;
}
