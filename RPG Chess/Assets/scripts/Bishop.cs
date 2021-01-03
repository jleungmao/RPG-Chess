using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    public Bishop(){
        maxHealth = 25;
        health = maxHealth;
        attack = 8;
        defense = 0;
        position = new Vector3(0,0,0);
        value = 8;
        cost = 25;

        movementCooldown = 3;
        attackCooldown = 5;
        attackPattern = generateAttackPattern();
        movementPattern = new Vector3[] {new Vector3(1,0,1), new Vector3(-1,0,-1), new Vector3(-1,0,1), new Vector3(1,0,-1)};
    }

    private Vector3[] generateAttackPattern(){
        List<Vector3> output = new List<Vector3>();
        for(int x = -2; x <=  2; x++){
            for(int z = -2; z <=  2; z++){
                output.Add(new Vector3(x,0,z));
            }
        }

        return output.ToArray();  
    }

}
