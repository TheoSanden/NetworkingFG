using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : ScriptableObject
{
    JoinAllocation allocation;
    internal async Task<bool> InitAsync() 
    {
        await UnityServices.InitializeAsync();
        Authenticator.AuthState authenticationState = await Authenticator.Authenticate(5);
        
        bool isAuth = authenticationState == Authenticator.AuthState.Authorized;

        ClientDisconnect clientDisconnect = new ClientDisconnect(NetworkManager.Singleton);
        return isAuth;
    }

    public async Task StartClientAsync(string joinCode)
    {
        allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        if (allocation != null) joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
        
        UnityTransport transport;
        Debug.Log("JoinCode: " + joinCode);
        if (NetworkManager.Singleton.TryGetComponent<UnityTransport>(out transport))
        {
            RelayServerData relayServerData = new RelayServerData(allocation, "udp");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = UserDataUtil.PayLoadInBytes();
            //Move this to UI
            NetworkManager.Singleton.StartClient();
        }
    }

    public async Task DisconnectFromLobby() 
    {
        SceneManager.LoadScene("S_MainMenu");
        NetworkManager.Singleton.Shutdown();
        return;
    }
}
