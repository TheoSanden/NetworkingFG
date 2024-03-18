using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkServer: ScriptableObject
{
   //Dictionary<ulong,ulong>
  private NetworkManager networkManager;
  public void Initalize(NetworkManager networkManager) 
  { 
     this.networkManager = networkManager;
     networkManager.ConnectionApprovalCallback += ConnectionApproval;
     networkManager.OnClientDisconnectCallback += ClientDisconnected;
  }

  public void OnDestroy() 
  {
      Debug.Log("Im Getting Destroyed");
      networkManager.ConnectionApprovalCallback -= ConnectionApproval;
      networkManager.OnClientDisconnectCallback -= ClientDisconnected;
  }


  private void ClientDisconnected(ulong networkID) 
  {
      SavedClientInformationManager.RemoveUser(networkID);
  }

  private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) 
  {
      string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
      UserData userData = JsonUtility.FromJson<UserData>(payload);
      userData.networkID = request.ClientNetworkId;
        
      //Make a loading scene  and set response.createPlayerObject to true
      //Appr
      response.Approved = true;
      SavedClientInformationManager.RegisterUser(userData);
      response.CreatePlayerObject = false;
  }
}
