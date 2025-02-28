using System.Collections.Generic;
using UnityEngine;

public class BulletPoolingManager : MonoBehaviour
{
    public static BulletPoolingManager Instance;
    private readonly List<Bullet> poolableBullets = new();
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int bulletAmountToPool;
    public ParticleSystem impactParticles;
    public ParticleSystem popcornParticles;

    private void Awake()
    {
        if (Instance == null)
        { 
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        for (int _poolIndex = 0; _poolIndex < bulletAmountToPool; _poolIndex++)
        {
            var _bullet = Instantiate(bulletPrefab, transform);
            _bullet.gameObject.SetActive(false);
            poolableBullets.Add(_bullet);
        }
    }
    
    public Bullet GetInactiveBullet()
    {
        for (int _pooledIndex = 0; _pooledIndex < bulletAmountToPool; _pooledIndex++)
        {
            if (!poolableBullets[_pooledIndex].gameObject.activeInHierarchy)
            {
                return poolableBullets[_pooledIndex];
            }
        }
        
        return null;
    }

    public List<Bullet> GetBulletList()
    {
        return poolableBullets;
    }
}
