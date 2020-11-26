using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    public Queen(){
        maxHealth = 75;
        health = maxHealth;
        attack = 15;
        defense = 0;
        position = new Vector3(0,0,0);
        value = 30;
        cost = 60;

        movementCooldown = 3;
        attackCooldown = 10;
        attackPattern = generateAttackPattern();
        movementPattern = generateMovementPattern();
        aoePattern = generateAOEPattern();
    }

    private Vector3[] generateAttackPattern(){
        List<Vector3> output = new List<Vector3>();
        for(int x = -1; x <=  1; x++){
            for(int z = -1; z <=  1; z++){
                output.Add(new Vector3(x,0,z));
            }
        }

        output.Add(new Vector3(2,0,0));
        output.Add(new Vector3(-2,0,0));
        output.Add(new Vector3(0,0,2));
        output.Add(new Vector3(0,0,-2));

        return output.ToArray();  
    }

    private Vector3[] generateMovementPattern(){
        List<Vector3> output = new List<Vector3>();
        for(int i = -2; i <=  2; i++){
            if(i == 0)
                continue;

            output.Add(new Vector3(i,0,0));
            output.Add(new Vector3(0,0,i));
        }

        return output.ToArray();  
    }

    private Vector3[] generateAOEPattern(){
        List<Vector3> output = new List<Vector3>();
        
        for(int i = -1; i <=  1; i++){
            if(i==0)
                continue;
            output.Add(new Vector3(i,0,0));
            output.Add(new Vector3(0,0,i));
        }

        return output.ToArray();  
    }
}
