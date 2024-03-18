using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


//clean this code 
public class ClientDisconnect 
{
    private NetworkManager networkManager;

    public ClientDisconnect(NetworkManager networkManager) {
        this.networkManager = networkManager;
        networkManager.OnClientStarted += OnClientStarted;
    }

    private void OnClientStarted() {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientID)
    {
        //Checking to see if not host
        if (networkManager.IsClient  && networkManager.IsConnectedClient &&  networkManager.LocalClientId == clientID && clientID != 0)
        {
            networkManager.Shutdown();
            SceneManager.LoadScene("S_MainMenu");
        }
    }
}
