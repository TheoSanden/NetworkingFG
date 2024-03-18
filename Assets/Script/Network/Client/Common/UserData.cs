using System.Collections;
using System.Collections.Generic;using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class UserData :INetworkSerializable
{
    public string userName;
    public string authID;
    public ulong networkID;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter 
    {
        serializer.SerializeValue(ref userName);
        serializer.SerializeValue(ref authID);
        serializer.SerializeValue(ref networkID);
    }
}

public static class UserDataUtil 
{
    public static byte[] PayLoadInBytes() 
    {
        UserData userData = new UserData()
        {
            userName = PlayerPrefs.GetString("userName", "NONAME"),
            authID = AuthenticationService.Instance.PlayerId,
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadByte = System.Text.Encoding.UTF8.GetBytes(payload);
        return payloadByte;
    }
}