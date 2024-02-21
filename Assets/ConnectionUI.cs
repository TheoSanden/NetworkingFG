using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ConnectionUI : MonoBehaviour 
{
    private UIDocument connectionUIDocument;
    private VisualElement root;
    private Button clientJoinButton;
    private Button hostButton;

    private void Start() {
     
    }

    private void OnDisable() {
        clientJoinButton.clicked -= ConnectAsClient;
        hostButton.clicked -= StartHost;
    }

    private void OnEnable() 
    {
        Initialize();
        clientJoinButton.clicked += ConnectAsClient;
        hostButton.clicked += StartHost;
    }

    void Initialize() {
        connectionUIDocument = GetComponent<UIDocument>();
        if (connectionUIDocument == null) {
            return;
        }

        root = connectionUIDocument.rootVisualElement;
        clientJoinButton = root.Q<Button>("JoinAsClient");
        hostButton = root.Q<Button>("host");
    }

    void ConnectAsClient() {
       bool success =  NetworkManager.Singleton.StartClient();

       if (!success)
       {
            Debug.LogWarning("Failed to join server as client.");
            return;
       }

       connectionUIDocument.enabled = false;
    }

    private void StartHost() 
    {
        bool success =  NetworkManager.Singleton.StartHost();

        if (!success) {
            Debug.LogWarning("Failed to start host.");
            return;
        }
        connectionUIDocument.enabled = false;
    }
}
