using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    public Rook(){
        maxHealth = 25;
        health = maxHealth;
        attack = 15;
        defense = 0;
        position = new Vector3(0,0,0);
        value = 8;
        cost = 20;

        movementCooldown = 4;
        attackCooldown = 4;
        attackPattern = generateAttackPattern();
        movementPattern = new Vector3[] {new Vector3(1,0,0), new Vector3(-1,0,0), new Vector3(0,0,1), new Vector3(0,0,-1), new Vector3(0,0,-2)};
    }

    private Vector3[] generateAttackPattern(){
        List<Vector3> output = new List<Vector3>();
        for(int z = 0; z <=  3; z++){
            output.Add(new Vector3(0,0,z));
        }
        return output.ToArray();  
    }
}
