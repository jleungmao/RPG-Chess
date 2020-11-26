using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NameSpaces;
public class Board : MonoBehaviour
{
    public static int rows = 8; // x
    public static int columns = 15; // z
    public Tile[,] board = new Tile[rows,columns]; 
    public GameObject tile;
    public Material material1;
    public Material material2;

    void Start()
    {
        Debug.Log("RUNNNING");
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
            board[x,z] = obj.GetComponent<Tile>();
        }

        tile.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Piece GetPiece(Vector3 position){
        return board[(int) position.x, (int) position.z].GetPiece();
    }

    public void PlacePiece(Piece piece, Vector3 position){
        if(IsFree(position)){
            board[(int) position.x, (int) position.z].SetPiece(piece);
            piece.SetPosition(position);
        }
    }

    public void RemovePiece(Vector3 newPosition){
        board[(int) newPosition.x, (int) newPosition.z].SetPiece(null);
    }

    public void MovePiece(Piece piece, Vector3 newPosition){
        if(IsFree(newPosition)){
            Vector3 oldPosition = piece.GetPosition();
            RemovePiece(oldPosition);
            PlacePiece(piece, newPosition);
            piece.StartMovementCooldown();
        }
    }

    public int AttackSquare(Piece selected, Mode mode, Vector3 position){
        Piece target = board[(int)position.x, (int)position.z].GetPiece();
        if(target){
            if(mode == Mode.ATTACK){
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
                
            }else if(mode == Mode.MINE){
                selected.StartAttackCooldown();
                return selected.GetAttack();
            }else if(mode == Mode.HEAL){
                if(selected.GetAOEPattern() == null){
                    selected.StartAttackCooldown();
                    target.ChangeHealth(-1*selected.GetAttack());
                }else{
                    Piece[] tilesToHealArray = GenerateAOETiles(selected, mode, position);
                    for(int i = 0; i<tilesToHealArray.Length;i++){
                        target = tilesToHealArray[i];
                        target.ChangeHealth(-1*selected.GetAttack());
                    }
                    selected.StartAttackCooldown();
                }
                return 0;
            }
        }
        return 0;
    }

    public int AttackPiece(Piece selected, Piece target){
        selected.StartAttackCooldown();
        bool isDead = target.ChangeHealth(selected.GetAttack()-target.GetDefense());
        if(isDead){
            Debug.Log("DIED");
            target.Die();
            RemovePiece(target.GetPosition());
            return target.GetValue();
        }
        return 0;
    }


    public bool IsFree(Vector3 position){
        return board[(int)position.x,(int)position.z].GetPiece() == null;
    }

    public bool OnBoard(int x, int z){
        return (x < rows && x >= 0 && z < columns && z >= 0);
    }

    public Vector3[] MarkTiles(Piece piece, Mode mode, Vector3[] placeTiles){
        Vector3[] tiles = new Vector3[0];

        if(mode == Mode.ATTACK){
            tiles = GenerateAttackValidTiles(piece);
            HighlightTiles(tiles, new Color32(255,0,0,255));
            return tiles;
        }else if(mode == Mode.MOVE){
            tiles = GenerateMovementValidTiles(piece);
            HighlightTiles(tiles, new Color32(255,255,0,255));
            return tiles;
        }else if(mode == Mode.PLACE){
            tiles = GeneratePlaceValidTiles(placeTiles);
            HighlightTiles(tiles, new Color32(0,0,255,255));
        }else if(mode == Mode.MINE){
            tiles = GenerateMineValidTiles(piece);
            HighlightTiles(tiles,new Color32(255,0,0,255));
        }else if(mode == Mode.HEAL){
            tiles = GenerateHealValidTiles(piece);
            HighlightTiles(tiles, new Color32(0,255,0,255));
        }

        return tiles;
    }

    public void HighlightTiles(Vector3[] tileArray, Color32 highlight){
        //highlight the tiles in tileArray
        foreach(Vector3 tilePosition in tileArray){
            board[(int)tilePosition.x, (int)tilePosition.z].gameObject.GetComponent<Renderer>().material.color = highlight;
        }
    }

    public void UnhighlightTiles(Vector3[] tileArray){
        //sets pieces to their original color
        foreach(Vector3 tilePosition in tileArray){
            if(((int)tilePosition.x + (int)tilePosition.z)%2 == 0){
                board[(int)tilePosition.x, (int)tilePosition.z].gameObject.GetComponent<MeshRenderer>().material = material1;
            }else{
                board[(int)tilePosition.x, (int)tilePosition.z].gameObject.GetComponent<MeshRenderer>().material = material2;
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
                if(IsFree(newPosition) || board[newX,newZ].GetPiece().GetPlayer() != piece.GetPlayer())
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
                if(IsFree(newPosition) || board[newX,newZ].GetPiece().GetPlayer() == piece.GetPlayer())
                    output.Add(new Vector3(newX, 0, newZ));
            }
        }

        return output.ToArray();
    }

    private Piece[] GenerateAOETiles(Piece selected, Mode mode, Vector3 position){
        Piece target = board[(int)position.x, (int)position.z].GetPiece();
        Vector3[] aoeArray = selected.GetAOEPattern();
        List<Piece> tilesToAttack = new List<Piece>();
        tilesToAttack.Add(target);
        for(int i = 0; i <  aoeArray.Length; i++){
            int newX = (int) position.x  + (int) aoeArray[i].x;
            int newZ = (int) position.z +  (int) aoeArray[i].z;

            if(OnBoard(newX, newZ)){
                Vector3 newPosition = new Vector3(newX, 0, newZ);
                if(mode == Mode.HEAL){
                    if(!IsFree(newPosition) && board[newX,newZ].GetPiece().GetPlayer() == selected.GetPlayer())
                        tilesToAttack.Add(board[newX,newZ].GetPiece());
                }else{
                    if(!IsFree(newPosition) && board[newX,newZ].GetPiece().GetPlayer() != selected.GetPlayer())
                        tilesToAttack.Add(board[newX,newZ].GetPiece());
                }
            }
        }
    
        return tilesToAttack.ToArray();
    }

}
