using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ChessNetworkManager : NetworkManager
{

    [SerializeField] private int minPlayers = 1;
    [Scene] [SerializeField] private string menuScene = string.Empty;

    [Header("Room")] 
    [SerializeField] private RoomPlayer roomPlayerPrefab = null;

    [Header("Game")] 
    [SerializeField] private GamePlayer gamePlayerPrefab = null;
    [SerializeField] private GameObject playerSpawnSystem = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;


    public List<RoomPlayer> RoomPlayers {get;} = new List<RoomPlayer>();
    public List<GamePlayer> GamePlayers {get;} = new List<GamePlayer>();


    public override void OnStartServer(){
        Debug.Log("Server Started");
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList<GameObject>();
    }

    public override void OnStartClient(){
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach(var prefab in spawnablePrefabs){
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnStopServer(){
        Debug.Log("Server Stopped");
        RoomPlayers.Clear();
    }

    public override void OnClientConnect(NetworkConnection conn){
        Debug.Log("Client Connected");
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn){
        Debug.Log("Disconnected from the server!");
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn){
        if(numPlayers >= maxConnections){
            Debug.Log("Too many people connecting");
            conn.Disconnect();
            return;
        }
        if(SceneManager.GetActiveScene().path != menuScene){
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn){
        if(conn.identity != null){
            Debug.Log("A client was disconnected from the server");
            var player = conn.identity.GetComponent<RoomPlayer>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn){
        if(SceneManager.GetActiveScene().path == menuScene){
            bool isLeader = RoomPlayers.Count == 0;

            RoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;
    
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public void NotifyPlayersOfReadyState(){
        foreach(var player in RoomPlayers){
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    public bool IsReadyToStart(){
        if(numPlayers < minPlayers){
            return false;
        }

        foreach(var player in RoomPlayers){
            if(!player.IsReady){
                return false;
            }
        }

        return true;
    }

    public void StartGame(){
        if(SceneManager.GetActiveScene().path == menuScene){
            if(!IsReadyToStart())
                return;
            
            // ServerChangeScene("Asss/Scenes/GameScene.unity");
            ServerChangeScene("GameScene");
        }
    }

    public override void ServerChangeScene(string newSceneName){
        if(SceneManager.GetActiveScene().path == menuScene && newSceneName == "GameScene"){
            for(int i = RoomPlayers.Count-1; i >= 0; i--){
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                gamePlayerInstance.SetPlayerNumber(i);

                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject, true);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName){
        if(sceneName == "GameScene"){
            foreach(GameObject prefab in spawnPrefabs){
                if(prefab.name == "Board"){
                    GameObject board = Instantiate(prefab);
                    NetworkServer.Spawn(board);
                }
            }
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance);
            
        }
    }

    public override void OnServerReady(NetworkConnection conn){
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }


    public void SpawnPiece(GameObject pieceObject,GameObject gamePlayer){
        // Debug.Log("Connection Number: " + gamePlayer.GetComponent<NetworkIdentity>().connectionToClient + "Spawning "+ pieceObject);
        NetworkServer.Spawn(pieceObject, gamePlayer.GetComponent<NetworkIdentity>().connectionToClient);
    }
}
