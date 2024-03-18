using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostManager : ScriptableObject 
{
    public string joinCode;
    private const int MaxConnections = 20;

    private Allocation allocation;
    public string lobbyID;
    public string lobbyName;

    private Lobby currentLobby;

    public Lobby CurrentLobby => currentLobby;
    public NetworkServer server;
    public async Task StartHostAsync() 
    {
        allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        if (allocation != null) joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
        UnityTransport transport;
        Debug.Log("JoinCode: " + joinCode);
        if (NetworkManager.Singleton.TryGetComponent<UnityTransport>(out transport)) 
        {
            RelayServerData relayServerData = new RelayServerData(allocation, "udp");
            transport.SetRelayServerData(relayServerData);
            
            lobbyName = PlayerPrefs.GetString("userName") + "'s Lobby";
            
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions();
            createLobbyOptions.IsLocked = false;
            createLobbyOptions.IsPrivate = false;
            createLobbyOptions.Data = new Dictionary<string, DataObject>() 
            {
                {"JoinCode", new DataObject(DataObject.VisibilityOptions.Public,joinCode)},
                {"LobbyName", new DataObject(DataObject.VisibilityOptions.Public,lobbyName)}
            };
            
            
            var lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, MaxConnections, createLobbyOptions);
            lobbyID = lobby.Id;

            server = ScriptableObject.CreateInstance<NetworkServer>();
            server.Initalize(NetworkManager.Singleton);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = UserDataUtil.PayLoadInBytes();
                
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
            NetworkManager.Singleton.SceneManager.LoadScene("S_Lobby",LoadSceneMode.Single);
        }
        else 
        {
            Debug.LogWarning("No UnityTransport component found on networkManager");
            return;
        }
    }
    public async Task DisconnectFromLobby() 
    {
       
        Destroy(server);
        Debug.Log("Deleting lobby");
        await LobbyService.Instance.DeleteLobbyAsync(lobbyID);
        Debug.Log("Finish deleting lobby");
        SavedClientInformationManager.ClearAll();
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.SceneManager.LoadScene("S_MainMenu",LoadSceneMode.Single);
        return;
    }

    public void OnPlayerDisconnected(ulong ID) 
    {
        if (currentLobby != null) 
        {
            LobbyService.Instance.RemovePlayerAsync(lobbyID, SavedClientInformationManager.GetUserDataFrom(ID).authID);
        }    
    }

    public async Task LockLobby() 
    {
        if (currentLobby == null) {
            return;
        }
        UpdateLobbyOptions updatedLobbyOptions = new UpdateLobbyOptions() {
            IsLocked = true
        };

        var updatedLobby = await Lobbies.Instance.UpdateLobbyAsync(currentLobby.Id,updatedLobbyOptions);
        currentLobby = updatedLobby;
    }
    public string GetJoinCode() 
    {
        return joinCode;
    }

    public async Task PingServer() 
    {
        await Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
        return;
    }
   
}
