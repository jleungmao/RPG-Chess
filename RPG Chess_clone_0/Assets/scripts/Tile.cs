using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Piece currentPiece;
    public Tile(){
        currentPiece = null;
    }

    public void SetPiece(Piece piece){
        currentPiece = piece;
    }

    public Piece GetPiece(){
        return currentPiece;
    }

}
