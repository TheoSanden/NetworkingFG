using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : Singleton<ClientSingleton> {
   public ClientManager clientManager;
   public bool authenticated;
   public string lobbyName = "";
   public async Task InitClientAsync() 
   {
       clientManager = ScriptableObject.CreateInstance<ClientManager>();
       authenticated = await clientManager.InitAsync();
   }

   public async Task StartClientAsync(string joinCode){
       await clientManager.StartClientAsync(joinCode);
   }
}
