using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class NetworkTransformServerAuthority : NetworkTransform
{
    protected override bool OnIsServerAuthoritative() {
        return true;
    }

    protected override void Update()
    {
        base.Update();
        if (!IsHost || !IsServer) {
            CanCommitToTransform = false;
            return;
        }
        CanCommitToTransform = true;
    }
}
