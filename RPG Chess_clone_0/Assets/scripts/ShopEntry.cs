﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopEntry : MonoBehaviour
{
    public GameObject selectedPiece;
    private int cost;

    public Shop shop;

    // Start is called before the first frame update
    void Start()
    {
        // GameObject obj = Instantiate(selectedPiece, new Vector3(0,0,0), selectedPiece.transform.rotation);
        cost = selectedPiece.GetComponent<Piece>().GetCost();
        // Destroy(obj);
    }

    void Update(){
        gameObject.GetComponent<Button>().interactable= shop.gold>=cost;
    }

    public GameObject GetPiece(){
        return selectedPiece;
    }

    public int GetCost(){
        return cost;
    }

}
