using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Color = UnityEngine.Color;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class MovementRaycaster : NetworkBehaviour {
    [SerializeField] private LayerMask obstacleRaycastLayer;
    private float horizontalMovementInput;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidBody;

    private Vector2 velocity;

    [SerializeField] private float gravity;
    [SerializeField] private float terminalVelocity;
    [SerializeField] private float verticalSpeed = 1;
    [SerializeField] private float jumpForce = 5;

    private int verticalRaycastSegments = 20;
    private int horizontalRaycastSegments = 15;
    private Vector2 ColliderOrigo => (Vector2)transform.position + boxCollider.offset;


    [SerializeField] private bool enableDebug = false;

    private Vector2 raycastEdgeIntersection;

    private NetworkVariable<Vector2> newPosition = new NetworkVariable<Vector2>(Vector2.zero,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<Vector2> replicatedVelocity = new NetworkVariable<Vector2>(Vector2.zero,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<float> lastUpdatedServerTime  = new NetworkVariable<float>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Vector2 predictedPosition;
    private Vector2 previousPosition;
    private Vector2 predictedVelocityDelta;

    private float extrapolatePositionErrorThreshold = 0.5f;
    private void Awake() 
    {
        boxCollider = this.GetComponent<BoxCollider2D>();
        rigidBody = this.GetComponent<Rigidbody2D>();
        if (!IsServer || !IsOwner) 
        {
            newPosition.OnValueChanged += UpdatePositionFromServer;   
        }
        VerifyComponents();
    }

    private void VerifyComponents() {
        if (!rigidBody && !boxCollider) {
            Debug.LogWarning("Rigid body: " + ((!rigidBody) ? " null" : "not null"));
            Debug.LogWarning("Box Collider: " + ((!boxCollider) ? " null" : "not null"));
            Debug.LogWarning("Inputs will not work correctly.");
        }
    }


    public void UpdateMovementInput(float input) {
        horizontalMovementInput = input;
    }

    private void UpdatePositionFromServer(Vector2 prev,Vector2 curr) {
        transform.position = curr;
    }

    public void Jump() {
        bool grounded = RaycastInCardinalDirection(Vector2.down, verticalRaycastSegments, 0.1f);
        if (!grounded) return;
        velocity.y += jumpForce;
    }

    private void FixedUpdate() 
    {
        if (IsOwner) 
        {
            
            bool isGrounded = RaycastInCardinalDirection(Vector2.down, verticalRaycastSegments, 0.1f);
            velocity.x = horizontalMovementInput;

            if (!isGrounded) 
            {
                ApplyGravity();
            }
            else {
                velocity.y = 0;
            }

            ClampVelocity();
            UpdateNetworkVariables();
        }
        else 
        {
            ExtrapolateMovement();
        }
    }

    public void ExtrapolateMovement() 
    {
        /*NetworkTime networkTime = new NetworkTime();
        predictedPosition = (Vector2)transform.position + replicatedVelocity.Value * (networkTime.TimeAsFloat - lastUpdatedServerTime.Value);

        /*Vector2 positionError = predictedPosition - (Vector2)transform.position;

        if (positionError.magnitude > extrapolatePositionErrorThreshold) {
            transform.position = Vector2.Lerp(transform.position, predictedPosition, Time.deltaTime * 10);
        }*/

        velocity = replicatedVelocity.Value;
        transform.position = GetTranslatedPosition();
    }

    private void UpdateNetworkVariables() 
    {
        NetworkTime networkTime = new NetworkTime();
        lastUpdatedServerTime.Value = networkTime.TimeAsFloat;
        replicatedVelocity.Value = velocity;
        newPosition.Value = transform.position = GetTranslatedPosition();
    }

    Vector2 GetTranslatedPosition() 
    {
        float xVelocity = velocity.x;
        float yVelocity = velocity.y;

        Vector2 horizontalDirection = new Vector2(Mathf.Sign(xVelocity), 0);
        Vector2 verticalDirection = new Vector2(0, Mathf.Sign(yVelocity));

        RaycastHit2D verticalhit = new RaycastHit2D();
        RaycastHit2D horizontalhit = new RaycastHit2D();

        if (verticalDirection != Vector2.zero) {
            verticalhit = RaycastInCardinalDirection(verticalDirection, verticalRaycastSegments, Mathf.Abs(yVelocity));
        }

        if (horizontalDirection != Vector2.zero) {
            horizontalhit =
                RaycastInCardinalDirection(horizontalDirection, horizontalRaycastSegments, Mathf.Abs(xVelocity));
        }

        Vector2 transformedPosition;
        if (verticalhit || (verticalhit && horizontalhit)) {
            transformedPosition = (Vector2)transform.position + (verticalhit.point - raycastEdgeIntersection);
        }

        if (horizontalhit && !verticalhit) {
            transformedPosition = transform.position + new Vector3(0, yVelocity);
        }
        else {
            transformedPosition = (Vector2)transform.position + velocity;
        }

        if (horizontalhit) {
            velocity.x = 0;
        }

        if (verticalhit) {
            velocity.y = 0;
        }

        return transformedPosition;
    }

    private void ApplyGravity() {
        velocity.y -= Time.fixedDeltaTime * gravity;
    }

    private RaycastHit2D RaycastInCardinalDirection(Vector2 direction, int segments, float addedCastLength = 0) 
    {
        if (direction.normalized != Vector2.left && direction.normalized != Vector2.right &&
            direction.normalized != Vector2.up && direction.normalized != Vector2.down) {
            Debug.LogError(
                "Tried to pass: " + direction + " .RaycastInCardinalDirection requires one of the axis vectors.", this);
            return new RaycastHit2D();
        }


        Vector2 spacingDirection =
            (direction == Vector2.right || direction == Vector2.left) ? Vector2.up : Vector2.left;

        float size = (direction == Vector2.right || direction == Vector2.left)
            ? boxCollider.size.y
            : boxCollider.size.x;

        Vector2 spacing = spacingDirection * (size / segments);

        float raycastDistance = (direction == Vector2.right || direction == Vector2.left)
            ? boxCollider.size.x / 2
            : boxCollider.size.y / 2;

        Vector2 startOffset = (direction == Vector2.up || direction == Vector2.down) ? Vector2.right : Vector2.down;

        Vector2 start = ColliderOrigo + (startOffset * (size / 2)) + velocity;

        raycastDistance = raycastDistance + addedCastLength;

        for (int i = 0; i < segments; i++) {
            RaycastHit2D hitGround =
                Physics2D.Raycast(start + spacing * i, direction, raycastDistance, obstacleRaycastLayer);

            Color color = (i == 0) ? Color.cyan : (hitGround) ? Color.green : Color.red;
            if (enableDebug)
                Debug.DrawRay(start + spacing * i, direction * raycastDistance, color, Time.fixedDeltaTime);
            if (hitGround) {
                //Save the position of where the raycast intersects with the edge of the player collider

                raycastEdgeIntersection = ColliderOrigo + velocity + (direction * (raycastDistance - addedCastLength));
                Debug.DrawLine(raycastEdgeIntersection, hitGround.point, Color.cyan, 10);
                return hitGround;
            }
        }

        return new RaycastHit2D();
    }

    private void ClampVelocity() {
        if (velocity.magnitude > terminalVelocity) {
            velocity = velocity.normalized * terminalVelocity;
        }
    }
}
