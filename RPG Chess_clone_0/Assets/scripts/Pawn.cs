using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public Pawn(){
        maxHealth = 15;
        health = maxHealth;
        attack = 13;
        defense = 8;
        position = new Vector3(0,0,0);
        value = 5;
        cost = 10;

        movementCooldown = 3;
        attackCooldown = 3;
        attackPattern = new Vector3[] {new Vector3(1,0,0), new Vector3(-1,0,0), new Vector3(0,0,1), new Vector3(0,0,-1)};
        movementPattern = new Vector3[] {new Vector3(0,0,1), new Vector3(0,0,-1)};
    }

}
