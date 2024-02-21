using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Color = UnityEngine.Color;

[RequireComponent(typeof(BoxCollider2D),typeof(Rigidbody2D))]
public class MovementRaycaster : NetworkBehaviour 
{
    private float horizontalMovementInput;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidBody;

    private Vector2 velocity;
    
    [SerializeField] private float gravity;
    [SerializeField] private float terminalVelocity;
    [SerializeField] private float verticalSpeed = 1;

    private float verticalRaycastChecks = 3;
    private Vector2 ColliderOrigo => (Vector2)transform.position + boxCollider.offset;


    [SerializeField] private bool enableDebug = false;
    private void Awake() 
    {
        boxCollider = this.GetComponent<BoxCollider2D>();
        rigidBody = this.GetComponent<Rigidbody2D>();
        
        VerifyComponents();
    }
    private void VerifyComponents() 
    {
        if (!rigidBody && !boxCollider) 
        {
            Debug.LogWarning("Rigid body: " + ((!rigidBody)? " null" : "not null"));
            Debug.LogWarning("Box Collider: " + ((!boxCollider)? " null" : "not null"));
            Debug.LogWarning("Inputs will not work correctly.");
        }   
    }

    public void UpdateMovementInput(float input) 
    {
        horizontalMovementInput = input;
    }

    public void Jump() 
    {
        
    }
    private void FixedUpdate() 
    {
        if (!IsOwner) {
            return;
        }
        bool isGrounded = IsGrounded();
        
        velocity.x = horizontalMovementInput;
        
        if (!isGrounded) 
        {
            ApplyGravity();
        }
        else 
        {
            velocity.y = 0;
        }
        
        ClampVelocity();
        TranslateVelocity();
    }

    private void TranslateVelocity() 
    {
        transform.position = GetTranslatedPosition();
    }

    Vector2 GetTranslatedPosition() {
        Vector2 localPointOnEdge = LocalPointOnEdge(velocity.normalized);
        Vector2 pointOnEdge = ColliderOrigo + localPointOnEdge;
        RaycastHit2D hit = Physics2D.Raycast(pointOnEdge, velocity.normalized, velocity.magnitude);

        if(enableDebug)Debug.DrawRay(pointOnEdge,velocity, (hit)? Color.green:Color.red,Time.fixedDeltaTime);
        
        if (hit) 
        {
            return hit.point - localPointOnEdge - boxCollider.offset;
        }
        return (Vector2)transform.position + velocity;
    }
    public Vector2 LocalPointOnEdge(Vector2 direction)
    {
        // Calculate half extents of the rectangle
        float halfWidth = boxCollider.size.x / 2f;
        float halfHeight = boxCollider.size.y / 2f;

        // Calculate t values for intersection with horizontal and vertical edges
        float tHorizontal = Mathf.Abs(1f / direction.x);
        float tVertical = Mathf.Abs(1f / direction.y);

        // Calculate intersection points with horizontal and vertical edges
        Vector2 intersectionHorizontal = new Vector2(Mathf.Sign(direction.x) * halfWidth, 0f);
        Vector2 intersectionVertical = new Vector2(0f, Mathf.Sign(direction.y) * halfHeight);

        // Choose the intersection point with the smaller positive t value
        Vector2 intersectionPoint = tHorizontal < tVertical ? intersectionHorizontal : intersectionVertical;

        return intersectionPoint;
    }
    private void ApplyGravity() {
        
        velocity.y -= Time.fixedDeltaTime * gravity;
    }
    private bool IsGrounded() 
    {
        Vector2 start = ColliderOrigo + ((boxCollider.size.x/2) * Vector2.left);
        Vector2 spacing = Vector2.right  * ((boxCollider.size.x * 2) / verticalRaycastChecks);
        for (int i = 0; i < verticalRaycastChecks; i++) 
        {
            bool hitGround = Physics2D.Raycast(start + spacing * i, Vector2.down, boxCollider.size.y/2);
            
            if(enableDebug)Debug.DrawRay(start + spacing * i,Vector2.down * boxCollider.size.y /2, (hitGround)? Color.green: Color.red,Time.fixedDeltaTime);
            if (hitGround) 
            {
                return true;
            }   
        }
        return false;
    }
    private void ClampVelocity() {
        
        if (velocity.magnitude > terminalVelocity) 
        {
            velocity = velocity.normalized * terminalVelocity;
        }
    }
}
