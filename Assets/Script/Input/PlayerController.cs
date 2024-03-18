
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MovementRaycaster))]
public class PlayerController : NetworkBehaviour
{
   private PlayerInput input = null;
   private MovementRaycaster raycaster;
   
   public override void OnNetworkSpawn() 
   {
      base.OnNetworkSpawn();
      if (!IsOwner) {
         return;}
      input = new PlayerInput();
      raycaster = this.GetComponent<MovementRaycaster>();
      
      input.Enable();
      input.Player.Movement.performed += OnMovement;
      input.Player.Movement.canceled += OnMovementCanceled;
      input.Player.Jump.performed += Jump;
   }

   public override void OnNetworkDespawn() {
      base.OnNetworkDespawn();
      if (input == null) {
         return;}
      input.Disable();
      input.Player.Movement.performed -= OnMovement;
      input.Player.Movement.canceled -= OnMovementCanceled;
      input.Player.Jump.performed -= Jump;
   }
   

   private void OnDisable() 
   {
      if (!IsOwner) {
         return;}
   }

   private void Jump(InputAction.CallbackContext context) {
      raycaster.Jump();
   }

   private void OnMovement(InputAction.CallbackContext context) 
   {
      raycaster.UpdateMovementInput(context.ReadValue<float>());
   }

   private void OnMovementCanceled(InputAction.CallbackContext context) 
   {
      raycaster.UpdateMovementInput(0);
   }
   
}
