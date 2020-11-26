using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    // Start is called before the first frame update
    public Player player;
    public ShopEntry selectedEntry;
    public int gold = 0;
    public int income = 0;

    private float incomeCooldown = 10;
    private float currentIncomeCooldown;
    public Text goldText;
    public Text incomeText;
    
    public GameObject myEventSystem;

    void Start(){
        ChangeGold(50);
        ChangeIncome(1);
        myEventSystem = GameObject.Find("EventSystem");
        currentIncomeCooldown = incomeCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        GenerateGoldPassively();
        if(selectedEntry!=null){
            selectedEntry.gameObject.GetComponent<Button>().Select();
        }
        if(NameSpaces.Mode.PLACE != player.GetMode()){
            myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        }
    }

    public void SelectPiece(string entryName){
        player.Deselect();
        selectedEntry = gameObject.transform.Find("ShopEntries").transform.Find(entryName).GetComponent<ShopEntry>();
        if(gold >= selectedEntry.GetCost()){
            player.SetMode(NameSpaces.Mode.PLACE);
            selectedEntry.gameObject.GetComponent<Button>().Select();
        }
    }

    public void DeselectPiece(NameSpaces.Mode mode){
        selectedEntry = null;
        player.SetMode(mode);
    }

    public void BuyPiece(Vector3 position){
        ChangeGold(-1*selectedEntry.GetPiece().GetComponent<Piece>().GetCost());
        player.PlacePiece(selectedEntry.GetPiece(),position);
        DeselectPiece(NameSpaces.Mode.MOVE);
    }

    public void ChangeGold(int amount){
        gold += amount;
        goldText.text = "Current Gold: " + gold;
    }

    public bool IsShopping(){
        return selectedEntry != null;
    }
    

    public void ChangeIncome(int amount){
        income += amount;
    }

    public void GenerateGoldPassively(){
        if(currentIncomeCooldown <= 0){
            ChangeGold(income);
            currentIncomeCooldown = incomeCooldown;
        }else{
            currentIncomeCooldown -= Time.deltaTime;
            if(((int)currentIncomeCooldown+1) < 10){
                incomeText.text = "Income ( " + ((int)currentIncomeCooldown+1) + " ): " + income;
            }else{
                incomeText.text = "Income (" + ((int)currentIncomeCooldown+1) + "): " + income;
            }
        }
    }

}
