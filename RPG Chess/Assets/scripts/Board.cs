using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NameSpaces;
using Mirror;
public class Board : NetworkBehaviour
{
    public static int rows = 8; // x
    public static int columns = 12; // z
    public Tile[,] board = new Tile[rows,columns]; 
    public List<List<Tile>> syncBoard = new List<List<Tile>>();
    public GameObject tile;
    public Material material1;
    public Material material2;
    private ChessNetworkManager room;
    private ChessNetworkManager Room{
        get{
            if(room != null){
                return room;
            }
            return room = NetworkManager.singleton as ChessNetworkManager;
        }
    }


    void Start()
    {
        DontDestroyOnLoad(gameObject);
        CreateBoard();
        
        for(int i = 0; i<Room.GamePlayers.Count; i++){
            Room.GamePlayers[i].Setup();
        }
        for(int i = 0; i<Room.GamePlayers.Count; i++){
            Room.GamePlayers[i].GameSetup();
        }

    }

    public void CreateBoard()
    {
        for(int i = 0; i < rows; i++){
            syncBoard.Add(new List<Tile>());
        }

        for(int i = 0; i < rows * columns; i++){
            int x = i % rows;
            int z = (int) Math.Floor((double)(i / rows));
            GameObject obj = Instantiate(tile, new Vector3(x,0,z), Quaternion.identity);
            obj.name = x + "," + z;
            if((x + z)%2 == 0){
                obj.GetComponent<MeshRenderer>().material = material1;
            }else{
                obj.GetComponent<MeshRenderer>().material = material2;
            }
            obj.transform.SetParent(this.transform);
            syncBoard[x].Add(obj.GetComponent<Tile>());
            board[x,z] = obj.GetComponent<Tile>();
        }
    }

    public Piece GetPiece(Vector3 position){
        // return board[(int) position.x, (int) position.z].GetPiece();
        return syncBoard[(int) position.x][(int) position.z].GetPiece();
    }

    [ClientRpc]
    public void RpcPlacePiece(GameObject pieceObject, Vector3 position){
        if(IsFree(position)){
            Piece piece = pieceObject.GetComponent<Piece>();
            piece.SetPosition(position);
            
            syncBoard[(int) position.x][(int) position.z].SetPiece(pieceObject);
            board[(int) position.x,(int) position.z].SetPiece(pieceObject);
        }
    }

    [ClientRpc]
    public void RpcRemovePiece(Vector3 position){
        // board[(int) position.x, (int) position.z].SetPiece(null);
        Debug.Log(position);
        syncBoard[(int) position.x][(int) position.z].SetPiece(null);
    }

    [Server]
    public int AttackSquare(Piece selected, Mode mode, Vector3 position){
        // Piece target = board[(int)position.x, (int)position.z].GetPiece();
        Piece target = syncBoard[(int)position.x][(int)position.z].GetPiece();
        if(target){
            if(mode == Mode.ATTACK){
                if(selected.GetComponent<Miner>() != null){
                    selected.Attack();
                    return selected.GetAttack();
                }
                if(selected.GetComponent<Bishop>() != null){
                    if(selected.GetAOEPattern() == null){
                        selected.Attack();
                        target.Damaged(-1*selected.GetAttack());
                    }else{
                        Piece[] tilesToHealArray = GenerateAOETiles(selected, mode, position);
                        for(int i = 0; i<tilesToHealArray.Length;i++){
                            target = tilesToHealArray[i];
                            target.Damaged(-1*selected.GetAttack());
                        }
                        selected.Attack();
                    }
                    return 0;
                }
                if(selected.GetAOEPattern() == null){
                    return AttackPiece(selected,target);
                }else{
                    Piece[] tilesToAttackArray = GenerateAOETiles(selected, mode, position);
                    int sum = 0;
                    for(int i = 0; i<tilesToAttackArray.Length;i++){
                        target = tilesToAttackArray[i];
                        int check = AttackPiece(selected, target);
                        if(check == -1)
                            return -1;
                        sum += check;
                    }
                    return sum;
                }
            }
        }
        return 0;
    }

    [Server]
    public int AttackPiece(Piece selected, Piece target){
        selected.Attack();
		Debug.Log("Will attack");
        bool isDead = target.Damaged(selected.GetAttack()-target.GetDefense());
        if(isDead){
            Debug.Log($"{target} DIED with {target.GetHealthPercentage()}% health left");
            target.Die();
            RpcRemovePiece(target.GetPosition());
            return target.GetValue();
        }
        return 0;
    }

    [ClientRpc]
    public void RpcKillAndDestroy(GameObject targetObject){
        Piece target = targetObject.GetComponent<Piece>();
        target.Die();
    }

    public bool IsFree(Vector3 position){
        // return board[(int)position.x,(int)position.z].GetPiece() == null;
        return syncBoard[(int)position.x][(int)position.z].GetPiece() == null;
    }

    public bool OnBoard(int x, int z){
        return (x < rows && x >= 0 && z < columns && z >= 0);
    }

    public Vector3[] MarkTiles(Piece piece, Mode mode, Vector3[] placeTiles){
        Vector3[] tiles = new Vector3[0];

        if(mode == Mode.ATTACK){
            if((piece.GetComponent<Miner>() != null)){
                tiles = GenerateMineValidTiles(piece);
                HighlightTiles(tiles,new Color32(255,0,0,255));
            }else if((piece.GetComponent<Bishop>() != null)){
                tiles = GenerateHealValidTiles(piece);
                HighlightTiles(tiles, new Color32(0,255,0,255));
            }else{
                tiles = GenerateAttackValidTiles(piece);
                HighlightTiles(tiles, new Color32(255,0,0,255));
            }
            return tiles;
        }else if(mode == Mode.MOVE){
            tiles = GenerateMovementValidTiles(piece);
            HighlightTiles(tiles, new Color32(255,255,0,255));
            return tiles;
        }else if(mode == Mode.PLACE){
            tiles = GeneratePlaceValidTiles(placeTiles);
            HighlightTiles(tiles, new Color32(0,0,255,255));
        }

        return tiles;
    }

    public void HighlightTiles(Vector3[] tileArray, Color32 highlight){
        //highlight the tiles in tileArray
        foreach(Vector3 tilePosition in tileArray){
            // board[(int)tilePosition.x, (int)tilePosition.z].gameObject.GetComponent<Renderer>().material.color = highlight;
            syncBoard[(int)tilePosition.x][(int)tilePosition.z].gameObject.GetComponent<MeshRenderer>().material.color = highlight;
        }
    }

    public void UnhighlightTiles(Vector3[] tileArray){
        //sets pieces to their original color
        foreach(Vector3 tilePosition in tileArray){
            if(((int)tilePosition.x + (int)tilePosition.z)%2 == 0){
                // board[(int)tilePosition.x, (int)tilePosition.z].gameObject.GetComponent<MeshRenderer>().material = material1;
                syncBoard[(int)tilePosition.x][(int)tilePosition.z].gameObject.GetComponent<MeshRenderer>().material = material1;
            }else{
                // board[(int)tilePosition.x, (int)tilePosition.z].gameObject.GetComponent<MeshRenderer>().material = material2;
                syncBoard[(int)tilePosition.x][(int)tilePosition.z].gameObject.GetComponent<MeshRenderer>().material = material2;
            }
        }
    }

    private Vector3[] GenerateMovementValidTiles(Piece piece){
        Vector3 position = piece.GetPosition();
        Vector3[] tileArray = piece.GetMovementPattern();
        List<Vector3> output = new List<Vector3>();
        for(int i = 0; i <  tileArray.Length; i++){
            int newX = (int) position.x  + (int)  tileArray[i].x;
            int newZ = (int) position.z +  (int) tileArray[i].z;

            if(OnBoard(newX, newZ)){
                Vector3 newPosition = new Vector3(newX, 0, newZ);
                if(IsFree(newPosition))
                    output.Add(new Vector3(newX, 0, newZ));
            }
        }

        return output.ToArray();
    }

    private Vector3[] GenerateAttackValidTiles(Piece piece){
        Vector3 position = piece.GetPosition();
        Vector3[] tileArray = piece.GetAttackPattern();
        List<Vector3> output = new List<Vector3>();
        for(int i = 0; i <  tileArray.Length; i++){
            int newX = (int) position.x  + (int)  tileArray[i].x;
            int newZ = (int) position.z +  (int) tileArray[i].z;

            if(OnBoard(newX, newZ)){
                Vector3 newPosition = new Vector3(newX, 0, newZ);
                // if(IsFree(newPosition) || board[newX,newZ].GetPiece().GetPlayer() != piece.GetPlayer())
                if(IsFree(newPosition) || syncBoard[newX][newZ].GetPiece().GetPlayer() != piece.GetPlayer())
                    output.Add(new Vector3(newX, 0, newZ));
            }
        }

        return output.ToArray();
    }

    private Vector3[] GeneratePlaceValidTiles(Vector3[] placeTiles){
        List<Vector3> output = new List<Vector3>();
        for(int i = 0; i <  placeTiles.Length; i++){
            if(IsFree(placeTiles[i]))
                output.Add(placeTiles[i]);
        }

        return output.ToArray();
    }

    private Vector3[] GenerateMineValidTiles(Piece piece){
        return new Vector3[]{piece.GetPosition()};
    }

    private Vector3[] GenerateHealValidTiles(Piece piece){
        Vector3 position = piece.GetPosition();
        Vector3[] tileArray = piece.GetAttackPattern();
        List<Vector3> output = new List<Vector3>();
        for(int i = 0; i <  tileArray.Length; i++){
            int newX = (int) position.x  + (int)  tileArray[i].x;
            int newZ = (int) position.z +  (int) tileArray[i].z;

            if(OnBoard(newX, newZ)){
                Vector3 newPosition = new Vector3(newX, 0, newZ);
                // if(IsFree(newPosition) || board[newX,newZ].GetPiece().GetPlayer() == piece.GetPlayer())
                if(IsFree(newPosition) || syncBoard[newX][newZ].GetPiece().GetPlayer() == piece.GetPlayer())
                    output.Add(new Vector3(newX, 0, newZ));
            }
        }

        return output.ToArray();
    }

    private Piece[] GenerateAOETiles(Piece selected, Mode mode, Vector3 position){
        // Piece target = board[(int)position.x, (int)position.z].GetPiece();
        Piece target = syncBoard[(int)position.x][(int)position.z].GetPiece();
        Vector3[] aoeArray = selected.GetAOEPattern();
        List<Piece> tilesToAttack = new List<Piece>();
        tilesToAttack.Add(target);
        for(int i = 0; i <  aoeArray.Length; i++){
            int newX = (int) position.x  + (int) aoeArray[i].x;
            int newZ = (int) position.z +  (int) aoeArray[i].z;

            if(OnBoard(newX, newZ)){
                Vector3 newPosition = new Vector3(newX, 0, newZ);
                // if(!IsFree(newPosition) && board[newX,newZ].GetPiece().GetPlayer() != selected.GetPlayer())
                //     tilesToAttack.Add(board[newX,newZ].GetPiece());
                if(selected.GetComponent<Bishop>()!= null){
                    if(!IsFree(newPosition) && syncBoard[newX][newZ].GetPiece().GetPlayer() == selected.GetPlayer())
                        tilesToAttack.Add(syncBoard[newX][newZ].GetPiece());
                }else{
                    if(!IsFree(newPosition) && syncBoard[newX][newZ].GetPiece().GetPlayer() != selected.GetPlayer())
                    tilesToAttack.Add(syncBoard[newX][newZ].GetPiece());
                }
                
            }
        }
    
        return tilesToAttack.ToArray();
    }

}
