
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkTransformClientAuthority : NetworkTransform
{
   protected override bool OnIsServerAuthoritative() {
      return false;
   }

   public override void OnNetworkSpawn() {
      base.OnNetworkSpawn();
      CanCommitToTransform = IsOwner;
   }

   protected override void Update()
   {
      base.Update();
      if (!IsOwner || IsHost || IsServer) return;

      CanCommitToTransform = IsOwner;
      
      if (NetworkManager.IsConnectedClient && CanCommitToTransform) 
      {
         TryCommitTransformToServer(transform,NetworkManager.LocalTime.Time);   
      }
   }
}
