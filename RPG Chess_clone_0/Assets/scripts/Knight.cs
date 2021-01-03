using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    public Knight(){
        maxHealth = 35;
        health = maxHealth;
        attack = 20;
        defense = 5;
        position = new Vector3(0,0,0);
        value = 8;
        cost = 20;

        movementCooldown = 3;
        attackCooldown = 5;
        attackPattern = generateAttackPattern();
        movementPattern = new Vector3[] {new Vector3(1,0,0), new Vector3(0,0,2), new Vector3(-1,0,0), new Vector3(0,0,1), new Vector3(0,0,-1)};
    }

    private Vector3[] generateAttackPattern(){
        List<Vector3> output = new List<Vector3>();
        for(int x = -1; x <=  1; x++){
            for(int z = -1; z <=  1; z++){
                output.Add(new Vector3(x,0,z));
            }
        }

        return output.ToArray();  
    }

}
