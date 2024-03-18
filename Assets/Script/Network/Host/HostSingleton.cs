using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HostSingleton : Singleton<HostSingleton> {
  public HostManager hostManager;
  public bool keepLobbyOpen = true;
  public async Task<bool> InitServerAsync() 
  {
      return true;
  }

  public async Task StartHost() 
  {
    hostManager = ScriptableObject.CreateInstance<HostManager>();
    await hostManager.StartHostAsync();
    StartCoroutine(PingServer());
  }

  public string GetLobbyID() 
  {
      return hostManager.lobbyID;
  }

  private IEnumerator PingServer() {
    while (keepLobbyOpen) 
    {
      Task task = hostManager.PingServer();
      yield return new WaitUntil(() => task.IsCompleted);
    }
  }
}
