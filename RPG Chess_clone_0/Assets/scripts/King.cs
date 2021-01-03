using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    // Start is called before the first frame update
    public King(){
        maxHealth = 150;
        health = maxHealth;
        attack = 0;
        defense = 0;
        position = new Vector3(0,0,0);
        value = -1;
        cost = 0;
        income = 0;

        movementCooldown = 1;
        attackCooldown = 1;
        attackPattern = new Vector3[] {};
        movementPattern = new Vector3[] {};
    }
}
