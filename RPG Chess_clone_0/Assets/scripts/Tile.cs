using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tile : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateTile))] public GameObject currentPiece;
    public Tile(){
        currentPiece = null;
    }

 
    public void SetPiece(GameObject piece){
        if(piece == null)
            currentPiece = null;
        else
            currentPiece = piece;
    }

    public Piece GetPiece(){
        if(currentPiece == null)
            return null;
        else
            return currentPiece.GetComponent<Piece>();
    }

    public void UpdateTile(GameObject oldValue, GameObject newValue){
        Debug.Log("updating a tile");
        currentPiece = newValue;
    }

}
