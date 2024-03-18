using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Collideable : NetworkBehaviour 
{
    [SerializeField]protected bool DestroyOnCollision = true;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.transform.tag == "Player")
        {
            OnCollision(other);
            if (DestroyOnCollision) 
            {
                Destroy(this.gameObject);
            }
        }
    }
    protected virtual void OnCollision(Collider2D other) 
    {
            
    }
}
