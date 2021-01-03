using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PieceInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public Text text;
    private GameObject pieceObject;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(pieceObject != null){
            transform.localScale = new Vector3(1f,1f,1f);
            Piece piece = pieceObject.GetComponent<Piece>();
            int health = piece.GetMaxHealth();
            int defense = piece.GetDefense();
            int attack = piece.GetAtk();
            float atkCd = piece.GetAtkCd();
            float moveCd = piece.GetMoveCd();
            text.text = "Health: "+health+"\nDefense: "+defense+"\nAttack: "+attack+"\nAttack Cooldown: "+atkCd+"\nMove Cooldown: "+moveCd;
        }else{
            transform.localScale = new Vector3(0f,0f,0f);
        }
    }

    public void SetPiece(GameObject piece){
        pieceObject = piece;
    }

    public void SetX(float x){
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
