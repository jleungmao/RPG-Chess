using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // Start is called before the first frame update
    public static Color[] playerColors = {Color.red, Color.blue};

    protected int maxHealth;
    protected int health;
    protected int attack;
    protected int defense;
    protected Vector3 position;
    protected Vector3[] movementPattern;
    protected Vector3[] attackPattern;
    protected Vector3[] aoePattern;
    protected float movementCooldown;
    protected float attackCooldown;
    protected float currentMovementCooldown=0;
    protected float currentAttackCooldown=0;
    protected int income = 0;
    protected int playerNumber;
    protected int value;
    protected int cost;
    protected int speed = 10;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position != position){
            Move();
        }

        UpdateCooldown();
    }

    private void UpdateCooldown(){
        if(currentMovementCooldown > 0){
            currentMovementCooldown -= Time.deltaTime;
            if(currentMovementCooldown < 0)
                currentMovementCooldown = 0;
        }

        if(currentAttackCooldown > 0){
            currentAttackCooldown -= Time.deltaTime;
            if(currentAttackCooldown < 0)
                currentAttackCooldown = 0;
        }
    }

    public void Move(){
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, position, step);
    }

    public int GetAttack(){
        currentAttackCooldown = attackCooldown;
        return attack;
    }

    public int GetDefense(){
        return defense;
    }

    public void SetPosition(Vector3 newPosition){
        position = newPosition;
    }

    public void StartMovementCooldown(){
        currentMovementCooldown = movementCooldown;
    }

    public void StartAttackCooldown(){
        currentAttackCooldown = attackCooldown;
    }
    public void SetPlayer(int player){
        playerNumber = player;
        gameObject.GetComponent<Renderer>().materials[1].color = playerColors[playerNumber];
    }

    public void Highlight(){
        gameObject.GetComponent<Renderer>().materials[1].color = Color.magenta;
    }

    public void Unhighlight(){
        gameObject.GetComponent<Renderer>().materials[1].color = playerColors[playerNumber];
    }

    public int GetPlayer(){
        return playerNumber;
    }

    public Vector3[] GetMovementPattern(){
        return movementPattern;
    }
    public Vector3[] GetAttackPattern(){
        return attackPattern;
    }
    public Vector3[] GetAOEPattern(){
        return aoePattern;
    }

    public Vector3 GetPosition(){
        return position;
    }

    // returns true if piece dies
    public bool ChangeHealth(int amount){
        health = health - amount;
        if(health > maxHealth){
            health = maxHealth;
        }
        if(health <= 0){
            return true;
        }
        return false;
    }

    public void Die(){
        Destroy(this.gameObject);
    }

    public int GetValue(){
        return value;
    }

    public int GetCost(){
        return cost;
    }

    public float GetMovementCooldown(){
        return currentMovementCooldown;
    }

    public float GetAttackCooldown(){
        return currentAttackCooldown;
    }

    public bool CanMove(){
        return currentMovementCooldown <= 0;
    }
    public bool CanAttack(){
        return currentAttackCooldown <= 0;
    }

    public int GetIncome(){
        return income;
    }
}
