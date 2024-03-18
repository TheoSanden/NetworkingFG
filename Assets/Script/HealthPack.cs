using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HealthPack : Collideable
{
    [SerializeField] private int healthGain;
    [SerializeField] private float disableForSecondsOnCollision = 2;
    private Collider2D coll;
    private SpriteRenderer spriteRenderer;
    private void Start() {
        coll = this.GetComponent<Collider2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        DestroyOnCollision = false;
    }

    protected override void OnCollision(Collider2D other) 
    {
        print("Colliding");
        if (other.gameObject.TryGetComponent<Health>(out Health playerHealth)) {
            if (IsHost) 
            {
                playerHealth.ChangeHealth(healthGain);          
            }
            DisableFor(disableForSecondsOnCollision);
        }       
    }

    private async Task DisableFor(float seconds) 
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;

        await Task.Delay((int)(seconds * 1000));

        spriteRenderer.enabled = true;
        coll.enabled = true;
    }
}
