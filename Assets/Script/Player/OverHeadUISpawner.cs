using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OverHeadUISpawner : NetworkBehaviour 
{
    private static OverHeadUISpawner Instance => instance;
    private static OverHeadUISpawner instance;
    [SerializeField] private OverHeadUI namePlatePrefab;

    private Dictionary<ulong, OverHeadUI> nameplates = new Dictionary<ulong, OverHeadUI>();
    
    private void Awake() 
    {
        if (OverHeadUISpawner.Instance != null) {
            Destroy(this);
        }
        else {
            instance = this;
        }
    }

    private void OnEnable() 
    {
        PlayerSpawner.Instance.OnPlayerRegistered += SpawnNamePlate;
        PlayerSpawner.Instance.OnPlayerDeregistered += DestroyNameplate;
    }

    private void OnDisable() {
        PlayerSpawner.Instance.OnPlayerRegistered -= SpawnNamePlate;
        PlayerSpawner.Instance.OnPlayerDeregistered -= DestroyNameplate;
    }

    public void SpawnNamePlate(NetworkObject player,string name) 
    {
        if (player == null) {
            return;
        }

        string localName = player.IsOwner? "You" : name; 
        OverHeadUI overHeadUI = GameObject.Instantiate(namePlatePrefab,this.transform);
        overHeadUI.Initialize();
        overHeadUI.SetName(localName);
        overHeadUI.AttachTo(player.gameObject);
        nameplates.Add(player.NetworkObjectId,overHeadUI);
    }
    public void DestroyNameplate(NetworkObject player,string name)
    {
        Debug.Log("Trying to destroy nameplate");
        OverHeadUI overHeadUI = nameplates[player.NetworkObjectId];
        nameplates.Remove(player.NetworkObjectId);
        Destroy(overHeadUI.gameObject);
    }
}
