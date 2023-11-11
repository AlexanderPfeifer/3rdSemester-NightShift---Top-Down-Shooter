using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPointManager : MonoBehaviour
{
    [SerializeField] private int maximumHitPoints = 5;
    [SerializeField] private int currentHitPoints;

    private void Start()
    {
        currentHitPoints = maximumHitPoints;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHitPoints -= damageAmount;
        
        if(currentHitPoints <= 0)
            Destroy(gameObject);
    }
}
