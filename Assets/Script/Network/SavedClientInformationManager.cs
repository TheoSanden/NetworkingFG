using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SavedClientInformationManager
{
   private static Dictionary<ulong, UserData> dataFromNetworkID = new Dictionary<ulong, UserData>();
   private static Dictionary<string, UserData> dataFromAuthID = new Dictionary<string, UserData>();
   
   public static void RegisterUser(UserData userData)
   {
       if (userData == null) {
           return;
       }
       dataFromNetworkID.Add(userData.networkID,userData);
       dataFromAuthID.Add(userData.authID,userData);
   }

   public static UserData GetUserDataFrom(ulong networkID) 
   {
       return dataFromNetworkID[networkID];
   }
   public static UserData GetUserDataFrom(string authID)
   {
       return dataFromAuthID[authID];
   }

   public static UserData[] GetAllUserData() 
   {
       List<UserData> userData = new List<UserData>();
       foreach (var data in dataFromAuthID) 
       {
            userData.Add(data.Value);
       }
       return userData.ToArray();
   }

   public static void ClearAll() {
       dataFromNetworkID.Clear();
       dataFromAuthID.Clear();
   }
   public static void RemoveUser(ulong networkID) {
       if (!dataFromNetworkID.ContainsKey(networkID)) {
           return;
       }

       dataFromNetworkID.Remove(networkID);
   }
}
