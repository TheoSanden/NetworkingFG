using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ConnectionUI : MonoBehaviour 
{
    private UIDocument connectionUIDocument;
    private VisualElement root;
    private Button clientJoinButton;
    private Button hostButton;
    private Button lobbiesButton;
    private TextField joinCodeTextField;

    private void OnDisable()
    {
        clientJoinButton.clicked -= ConnectAsClient;
        hostButton.clicked -= StartHost;
        lobbiesButton.clicked -= GoToLobbyMenu;
    }

    private void OnEnable() 
    {
        Initialize();
        clientJoinButton.clicked += ConnectAsClient;
        hostButton.clicked += StartHost;
        lobbiesButton.clicked += GoToLobbyMenu;
    }

    void Initialize() 
    {
        connectionUIDocument = GetComponent<UIDocument>();
        if (connectionUIDocument == null) {
            return;
        }

        root = connectionUIDocument.rootVisualElement;
        clientJoinButton = root.Q<Button>("JoinWithCode");
        hostButton = root.Q<Button>("host");
        joinCodeTextField = root.Q<TextField>("JoinCodeField");
        lobbiesButton = root.Q<Button>("ViewLobby");
    }

    public void GoToLobbyMenu() {
        SceneManager.LoadScene("LobbyMenu");
    }
    public async void ConnectAsClient() 
    {
        await JoinWithCode();
    }
    public async Task JoinWithCode() 
    {
        await ClientSingleton.GetInstance().StartClientAsync(joinCodeTextField.value);
    }

    private async void StartHost() 
    {
        Debug.LogWarning("Trying to start host");
        await HostSingleton.GetInstance().StartHost();
    }
}
