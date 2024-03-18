using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Task = System.Threading.Tasks.Task;

public class LobbyUI : MonoBehaviour {
    private bool isRefreshing = false;
    private VisualElement lobbyContainer;
    [SerializeField] private VisualTreeAsset lobbyContainerVisualTreeAsset;
    private UIDocument uiDocument;
    private ScrollView lobbyScrollView;
    private Button refreshButton;
    private Button backButton;

    //insert lobby name and get the join code;
    private Dictionary<string, string> lobbyCodes;

    private bool initialized = false;
    void Start() 
    {
        uiDocument = this.GetComponent<UIDocument>();
        lobbyScrollView = uiDocument.rootVisualElement.Q<ScrollView>("LobbyScrollView");
        lobbyScrollView.Clear();

        refreshButton = uiDocument.rootVisualElement.Q<Button>("Refresh");
        backButton = uiDocument.rootVisualElement.Q<Button>("Back");
        
        refreshButton.clicked += RefreshLobbies;
        backButton.clicked += Back;
        initialized = true;
        lobbyCodes = new Dictionary<string, string>();
        RefreshLobbies();
    }

    private void OnEnable() {
        if (!initialized) return;
        refreshButton.clicked += RefreshLobbies;
        backButton.clicked += Back;
    }

    private void OnDisable() 
    {
        refreshButton.clicked -= RefreshLobbies;
        backButton.clicked -= Back; 
    }

    private void Back() 
    {
        SceneManager.LoadScene("S_MainMenu");
    }
    // Update is called once per frame

    private void CreateLobbyContainer(string lobbyName, string joinCode, int currentPlayerCount, int maxPlayerCount)
    {
        VisualElement lobbyContainerInstance = lobbyContainerVisualTreeAsset.Instantiate();
        Label connectionsLobby = lobbyContainerInstance.Q<Label>("Connections");
        Button joinButton = lobbyContainerInstance.Q<Button>("Join");
        Label lobbyNameLabel = lobbyContainerInstance.Q<Label>("LobbyName");

        connectionsLobby.text = "" + currentPlayerCount + "/" + maxPlayerCount;
        lobbyNameLabel.text = lobbyName;
        joinButton.clicked += async () => {
            ClientSingleton.GetInstance().lobbyName = lobbyName;
            await ClientSingleton.GetInstance().StartClientAsync(joinCode);
        };
        
        lobbyScrollView.Add(lobbyContainerInstance);
    }
    
    private async void RefreshLobbies() 
    {
        if (isRefreshing) return;
        Debug.Log("Trying to refresh");
        isRefreshing = true;

        QueryLobbiesOptions options = new QueryLobbiesOptions();
        options.Count = 30;
        options.Filters = new List<QueryFilter>() 
        {
            new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
            new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ)
        };
        QueryResponse qLobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
        
        //clear the scrollview of lobbies 
        //loop over the qLobbies and create a new panel for each lobby 
        
        lobbyScrollView.Clear();
        lobbyCodes.Clear();
        foreach (var lobby in qLobbies.Results) 
        {
            Debug.Log("Lobby Name: " + lobby.Name);
            var lobbyInstance = await Lobbies.Instance.GetLobbyAsync(lobby.Id);
           // string joinCode = lobbyInstance.Data["JoinCode"].Value;
            string joinCode = lobbyInstance.Data["JoinCode"].Value;
            int playerCount = lobbyInstance.Players.Count;
            int maxPlayerCount = lobby.MaxPlayers;
            CreateLobbyContainer(lobby.Name,joinCode,playerCount,maxPlayerCount);
        }

        await Task.Delay(1000);
        isRefreshing = false;
    }
}
