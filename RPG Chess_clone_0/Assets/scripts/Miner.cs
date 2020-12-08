using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miner : Piece
{
    public Miner(){
        maxHealth = 15;
        health = maxHealth;
        attack = 5;
        defense = 0;
        position = new Vector3(0,0,0);
        value = 8;
        cost = 20;
        income = 1;

        movementCooldown = 1;
        attackCooldown = 5;
        attackPattern = new Vector3[] {new Vector3(0,0,0)};
        movementPattern = new Vector3[] {};
    }

    override public void UpdateAnimation(){
        Animator anim = GetComponent<Animator>();
        if(GetAttackCooldownPercentage() == 1){
            anim.SetBool("ready", true);
        }else
            anim.SetBool("ready", false);
    }

}
