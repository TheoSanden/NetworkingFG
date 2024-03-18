using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LobbyWaitUI : NetworkBehaviour {

    private Label lobbyName;
    private Button startGameButton;
    private Button leaveGameButton;
    private UIDocument uiDocument;
    private ScrollView playerScrollView;
    [SerializeField] private VisualTreeAsset connectedPlayerContainer;
    private List<string> playerNames;

    private bool startingGame = false;
    void Start() 
    {
        uiDocument = GetComponent<UIDocument>();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        playerScrollView = uiDocument.rootVisualElement.Q<ScrollView>("ConnectedPlayers");
        leaveGameButton = uiDocument.rootVisualElement.Q<Button>("Leave");
        startGameButton = uiDocument.rootVisualElement.Q<Button>("Start");
        lobbyName = uiDocument.rootVisualElement.Q<Label>("LobbyName");
        if (!IsHost) 
        {
            startGameButton.parent.Remove(startGameButton);
            leaveGameButton.clicked += OnClientLeave;
            lobbyName.text = ClientSingleton.GetInstance().lobbyName;
        }
        else {
            startGameButton.clicked += StartGame;
            leaveGameButton.clicked += OnHostLeave;
            lobbyName.text = HostSingleton.GetInstance().hostManager.lobbyName;
        }
        UpdateLobbyUIClientRpc(SavedClientInformationManager.GetAllUserData());
    }

    async void OnHostLeave() 
    {
        DissconnectCallbacks();
        await HostSingleton.GetInstance().hostManager.DisconnectFromLobby();
    }

    async void OnClientLeave()
    {
        DissconnectCallbacks();
        await ClientSingleton.GetInstance().clientManager.DisconnectFromLobby();
    }

    private void DissconnectCallbacks() 
    {
        NetworkManager netManager = NetworkManager.Singleton;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnDisable() {
        DissconnectCallbacks();
    }

    void StartGame() 
    {
        StartGame_Task();
    }
    async Task StartGame_Task() {
        if (startingGame) 
        {
            return;
        }

        startingGame = true;
        await HostSingleton.GetInstance().hostManager.LockLobby();
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
        startingGame = false;
    }

    void OnClientConnected(ulong ID)
    {
        //Somehow get the connected players name
        //playerNames.Add();
        UserData[] data = SavedClientInformationManager.GetAllUserData();
        UpdateLobbyUI(data);
        UpdateLobbyUIClientRpc(SavedClientInformationManager.GetAllUserData());
    }

    void OnClientDisconnected(ulong ID)
    {
        //Get player name and remove it
       SavedClientInformationManager.RemoveUser(ID);
       UserData[] data = SavedClientInformationManager.GetAllUserData();
       UpdateLobbyUI(data);
       UpdateLobbyUIClientRpc(data);
    }
    
    [ClientRpc]
    void UpdateLobbyUIClientRpc(UserData[] userData)
    {
        UpdateLobbyUI(userData);
    }
    void UpdateLobbyUI(UserData[] userData)
    {
        playerScrollView.Clear();
        // UserData[] userData = SavedClientInformationManager.GetAllUserData();
        foreach (UserData data in userData)
        {
            CreateNameContainer(data.userName);    
        }
    }

    public void CreateNameContainer(string playerName) 
    {
        VisualElement nameContainer = connectedPlayerContainer.Instantiate();
        Label nameLabel = nameContainer.Q<Label>("Name");
        nameLabel.text = playerName;
        playerScrollView.Add(nameLabel);
    }

}
