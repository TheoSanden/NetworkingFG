using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : Collideable
{
    [SerializeField] private int healthGain;
    protected override void OnCollision(Collider2D other) 
    {
        print("Colliding");
        if (IsHost && other.gameObject.TryGetComponent<Health>(out Health playerHealth))
        {
            playerHealth.ChangeHealth(healthGain);                   
        }       
    }
}
