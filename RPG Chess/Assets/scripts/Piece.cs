using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Piece : NetworkBehaviour
{
    // Start is called before the first frame update
    public static Color[] playerColors = {new Color(0.8f,0.8f,0.8f,1f), new Color(0.2f,0.2f,0.2f,1f)};

    protected int maxHealth;
    [SyncVar(hook = nameof(UpdateHealth))] protected int health;
    protected int attack;
    protected int defense;
    [SyncVar] protected Vector3 position;
    protected Vector3[] movementPattern;
    protected Vector3[] attackPattern;
    protected Vector3[] aoePattern;
    protected float movementCooldown;
    protected float attackCooldown;
    [SyncVar(hook = nameof(UpdateMovementCooldown))] protected float currentMovementCooldown=0;
    [SyncVar(hook = nameof(UpdateAttackCooldown))] protected float currentAttackCooldown=0;
    protected int income = 0;
    [SyncVar] protected int playerNumber;
    protected int value;
    protected int cost;
    protected int speed = 10;

    List<Material> colorMats = new List<Material>();

    void Start()
    {
        InitColorMats();
        Unhighlight();
    }

    private void InitColorMats(){
        Material[] allMats = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().materials;
        for(int i = 0; i <  allMats.Length; i++){
            String name = allMats[i].name;
            if(name == "armor (Instance)" || name == "armor.bishop (Instance)" || name == "armor.queen (Instance)" ||
                name == "skin (Instance)" || name == "skin.horse (Instance)"){
                colorMats.Add(allMats[i]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position != position){
            Move();
        }

        UpdateCooldown();
        UpdateAnimation();
    }

    virtual public void UpdateAnimation(){

    }

    public float GetHealthPercentage(){
        return ((float) health / maxHealth);
    }

    public float GetMoveCooldownPercentage(){
        return (float) (1 - currentMovementCooldown / movementCooldown);
    }

    public float GetAttackCooldownPercentage(){
        return (float) (1 - currentAttackCooldown / attackCooldown);
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

    public int GetAtk(){
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

    public void Attack(){
        StartAttackCooldown();
        try{
            gameObject.GetComponent<Animator>().Play("attack");
        }catch{}
    }

    public bool Damaged(int amount){
        bool val = ChangeHealth(amount);
        if(!val){
            try{
                gameObject.GetComponent<Animator>().Play("damaged");
            }catch{}
        }
        return val;
    }

    public void SetPlayer(int player){
        playerNumber = player;
    }

    private Vector3[] RotatePattern(Vector3[] pattern){

        List<Vector3> output = new List<Vector3>();
        if(pattern != null){
            foreach(Vector3 tile in pattern){
                output.Add(new Vector3(-1 * tile.x, -1 * tile.y, -1 * tile.z));
            }
        }
        return output.ToArray();
    }

    public void Highlight(){
        foreach(Material mat in colorMats){
            mat.color = Color.yellow;
        }
    }

    public void Unhighlight(){
        foreach(Material mat in colorMats){
            mat.color = playerColors[playerNumber];
        }
    }

    public int GetPlayer(){
        return playerNumber;
    }

    public Vector3[] GetMovementPattern(){
        if(playerNumber == 1)
            return RotatePattern(movementPattern);
        
        return movementPattern;
    }
    public Vector3[] GetAttackPattern(){
        if(playerNumber == 1)
            return RotatePattern(attackPattern);

        return attackPattern;
    }
    public Vector3[] GetAOEPattern(){
        if(playerNumber == 1)
            return RotatePattern(aoePattern);

        return aoePattern;
    }

    public Vector3 GetPosition(){
        return position;
    }

    // returns true if piece dies
    public bool ChangeHealth(int amount){
        Debug.Log(health);
        health =  Math.Max(health - amount, 0);
        if(health > maxHealth){
            health = maxHealth;
        }
        Debug.Log(health);
        if(health <= 0){
            return true;
        }
        return false;
    }

    IEnumerator  DieAnimation(){
        Animator anim = gameObject.GetComponent<Animator>();
        anim.Play("death");
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Destroy(this.gameObject);
    }

    public void Die(){
        StartCoroutine(DieAnimation());
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

    private void UpdateHealth(int oldValue, int newValue){
        health = newValue;
    }
    
    private void UpdateMovementCooldown(float oldValue, float newValue){
        currentMovementCooldown = newValue;
    }
    
    private void UpdateAttackCooldown(float oldValue, float newValue){
        currentAttackCooldown = newValue;
    }

    public int GetMaxHealth(){
        return maxHealth;
    }

    public float GetAtkCd(){
        return attackCooldown;
    }

    public float GetMoveCd(){
        return movementCooldown;
    }
}
