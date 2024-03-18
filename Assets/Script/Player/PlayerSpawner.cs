using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerSpawner : NetworkBehaviour 
{
    public delegate void OnNetworkChange(NetworkObject networkObject, string name);
    public OnNetworkChange OnPlayerRegistered;
    public OnNetworkChange OnPlayerDeregistered;

    private static PlayerSpawner instance;

    public static PlayerSpawner Instance => instance;
    
    [SerializeField] private GameObject playerPrefab;
    private Dictionary<ulong, NetworkObject> players = new Dictionary<ulong, NetworkObject>();
    public void Start()
    {
        if (Instance == null) 
        {
            instance = this;
        }
        else {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }
    public override void OnNetworkSpawn() {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
    }

    public void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        
        if ((!IsHost || !IsServer) || sceneName != "GameScene") {
            Debug.Log("Rejecting spawning players");
            return;
        }

        Debug.Log("Spawning players");
        foreach (var id in clientsCompleted)
        {
            // NetworkManager.Singleton.
            NetworkObject player = Instantiate(playerPrefab).GetComponent<NetworkObject>();
            player.SpawnAsPlayerObject(id, true);
            RegisterPlayerObjectClientRpc(player.GetComponent<NetworkObject>(),SavedClientInformationManager.GetUserDataFrom(player.OwnerClientId).userName);
        }
        
    }

    [ClientRpc]
    private void RegisterPlayerObjectClientRpc(NetworkObjectReference player, string name) 
    {
        if (player.TryGet(out NetworkObject playerNetworkObject)) 
        {
            OnPlayerRegistered.Invoke(playerNetworkObject, name);
            players.Add(playerNetworkObject.OwnerClientId,playerNetworkObject);
        }
        else 
        {
            Debug.LogWarning("Player object not found on this client. ID: " + player.NetworkObjectId);   
        }
    }

    void OnPlayerDisconnected(ulong ID) 
    {
      
        NetworkObject netObj = players[ID];
        OnPlayerDeregistered.Invoke(netObj,"null");
        players.Remove(ID);
    }
}
