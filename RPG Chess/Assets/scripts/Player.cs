using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using NameSpaces;
namespace NameSpaces
{
    public enum Mode {MOVE, ATTACK, PLACE, HEAL, MINE}

    public enum GameStatus {WIN, LOSE, ONGOING}

}

public class Player : MonoBehaviour
{
    public Mode mode = Mode.MOVE;
    public int playerNumber; // 0 or 1
    public Board board;
    public Piece selectedPiece;
    public GameObject camera;
    
    public GameObject pawn;
    public GameObject knight;
    public GameObject bishop;
    public GameObject rook;
    public GameObject miner;
    public GameObject queen;
    public GameObject king;
    public GameObject[] pieces;

    public Vector3[] tileArray;
    public Shop shop;
    public GameStatus gameStatus = GameStatus.ONGOING;
    private Vector3[] shopTiles;

    private void GenerateShopTiles(){
        int playerStartRow = 0;
        if(playerNumber == 1)
            playerStartRow = Board.columns - 2;


        List<Vector3> output = new List<Vector3>();
        for(int i = 0; i < Board.rows; i++){
            output.Add(new Vector3(i, 0, playerStartRow));
            output.Add(new Vector3(i, 0, playerStartRow+1));
        }

        shopTiles = output.ToArray();
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateShopTiles();
        pieces = new GameObject[] {pawn, knight, bishop, rook, miner, queen, king};
        for(int i = 0; i < pieces.Length; i++){
            pieces[i].SetActive(false);
        }

        PlacePiece(king, new Vector3(0, 0, 0));
        PlacePiece(miner, new Vector3(2, 0, 0));
        PlacePiece(rook, new Vector3(0, 0, 1));
        PlacePiece(rook, new Vector3(Board.rows-1, 0, 1));
        PlacePiece(bishop, new Vector3(1, 0, 1));
        PlacePiece(bishop, new Vector3(Board.rows-2, 0, 1));
        PlacePiece(knight, new Vector3(2, 0, 1));
        PlacePiece(knight, new Vector3(Board.rows-3, 0, 1));
        for(int i = 0; i < Board.rows; i++){
            PlacePiece(pawn, new Vector3(i, 0, 2));
        }

        // temporary to add enemy pawn on board
        playerNumber = 1;
        for(int i = 0; i < Board.rows; i++){
            PlacePiece(pawn, new Vector3(i, 0, Board.columns - 3));
        }
        PlacePiece(king, new Vector3(Board.rows-1,0,Board.columns- 1));
        playerNumber = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if(shop.selectedEntry == null){
            board.UnhighlightTiles(shopTiles);
        }
        if(gameStatus == GameStatus.ONGOING){
            GetActionTiles();
            Select();
            GetKeyPress();    
        }else{
            EndGame();
        }
    }

    public void PlacePiece(GameObject piece, Vector3 position){
        GameObject obj = Instantiate(piece, position, piece.transform.rotation);
        board.PlacePiece(obj.GetComponent<Piece>(), position);
        obj.GetComponent<Piece>().SetPlayer(playerNumber);
        shop.ChangeIncome(obj.GetComponent<Piece>().GetIncome());
        obj.SetActive(true);
    }

    public void GetActionTiles(){
        if(selectedPiece && (mode == Mode.MOVE || mode == Mode.ATTACK || mode == Mode.MINE || mode == Mode.HEAL)){
            tileArray = board.MarkTiles(selectedPiece, mode, null);
            if((!selectedPiece.CanAttack() && (mode == Mode.ATTACK || mode == Mode.MINE || mode == Mode.HEAL)) || 
            (!selectedPiece.CanMove() && mode == Mode.MOVE)){
                board.UnhighlightTiles(tileArray);
            }
        }else if(mode == Mode.PLACE){
            tileArray = board.MarkTiles(null , mode, shopTiles);
        }
    }

    public void Select(){
        Transform clickedObject = GetClick();
        if(!clickedObject) return;
        Vector3 objectPosition = clickedObject.position;
        Piece clickedPiece = board.GetPiece(objectPosition);

        if(clickedPiece){
            SelectPiece(clickedPiece);
            MoveCamera(objectPosition);
            // if(clickedPiece.GetComponent<Miner>() != null){
            if(mode == Mode.MINE){
                SelectMine(objectPosition);
            }else if(mode == Mode.HEAL){
                SelectHeal(objectPosition);
            }else if(mode == Mode.ATTACK){
                SelectAttack(objectPosition);
            }else{
                shop.DeselectPiece(Mode.MOVE);
            }
        }else{
            if(mode == Mode.PLACE){
                SelectPlace(objectPosition);
            }else if(mode == Mode.MOVE && selectedPiece != null){
                shop.DeselectPiece(Mode.MOVE);
                SelectMove(objectPosition);
            }
        }
    }

    private void SelectPiece(Piece piece){ //PICKING A NEW PIECE
        if(piece.GetPlayer() == playerNumber){ 
            if(mode == Mode.MINE){
                if(!Array.Exists(tileArray, location => location == piece.GetPosition())){
                    Deselect();
                    if(piece.GetComponent<Miner>() == null)
                        SetMode(Mode.MOVE);
                    selectedPiece = piece;
                    selectedPiece.Highlight();
                }
            }else if(mode == Mode.HEAL){
                if(!Array.Exists(tileArray, location => location == piece.GetPosition()) || !selectedPiece.CanAttack()){
                    Deselect();
                    if(piece.GetComponent<Miner>() == null)
                        SetMode(Mode.ATTACK);
                    else
                        SetMode(Mode.MINE);
                    selectedPiece = piece;
                    selectedPiece.Highlight();
                }
            }else{
                Deselect();
                
                if(piece.GetComponent<Miner>() != null){
                    SetMode(Mode.MINE);
                }else if(piece.GetComponent<Bishop>() != null){
                    if(mode == Mode.ATTACK){
                        SetMode(Mode.HEAL);
                    }
                }
                selectedPiece = piece;
                selectedPiece.Highlight();
                if(mode == Mode.PLACE){
                    board.UnhighlightTiles(shopTiles);
                    SetMode(Mode.MOVE);
                }
            }
        }
    }

    private void SelectMine(Vector3 tilePosition){
        Piece clickedPiece = board.GetPiece(tilePosition);
        if((clickedPiece.GetComponent<Miner>() != null) && (clickedPiece.GetPlayer() == playerNumber) && clickedPiece.CanAttack()){
            if(tileArray != null && Array.Exists(tileArray, attack => attack == tilePosition)){
                int valueChanges = board.AttackSquare(clickedPiece, mode, tilePosition);
                shop.ChangeGold(valueChanges);
            }
        }
    }

    private void SelectHeal(Vector3 tilePosition){
        Piece clickedPiece = board.GetPiece(tilePosition);
        if((selectedPiece.GetComponent<Bishop>() != null) && (clickedPiece.GetPlayer() == playerNumber) && selectedPiece.CanAttack()){
            if(tileArray != null && Array.Exists(tileArray, attack => attack == tilePosition)){
                board.AttackSquare(selectedPiece, mode, tilePosition);
            }
        }
    }

    private void SelectMove(Vector3 tilePosition){
        if(tileArray != null && Array.Exists(tileArray, move => move == tilePosition)){
            if(selectedPiece.CanMove()){
                board.UnhighlightTiles(tileArray);
                board.MovePiece(selectedPiece, tilePosition);
            }
        }
    }

    private void SelectAttack(Vector3 tilePosition){
        Piece clickedPiece = board.GetPiece(tilePosition);
        if(clickedPiece && clickedPiece.GetPlayer() != playerNumber && selectedPiece.CanAttack()){
            if(tileArray != null && Array.Exists(tileArray, attack => attack == tilePosition)){
                board.UnhighlightTiles(tileArray);
                int value = board.AttackSquare(selectedPiece, mode, tilePosition);
                if(value == -1){
                    gameStatus = GameStatus.WIN;
                }else{
                    shop.ChangeGold(value);
                }
            }
        }
    }
    
    private void SelectPlace(Vector3 tilePosition){
        if(Array.Exists(tileArray, place => place == tilePosition)){
            board.UnhighlightTiles(shopTiles);
            shop.BuyPiece(tilePosition);
            SetMode(Mode.MOVE);
        }
    }

    public void Deselect(){
        if(selectedPiece){
            selectedPiece.Unhighlight();
            selectedPiece = null;
            if(tileArray != null)
                board.UnhighlightTiles(tileArray);
            tileArray=null;
        }
    }

    private void MoveCamera(Vector3 position){
        // Vector3 newPosition = new Vector3(position.x, camera.transform.position.y, position.z);
        // camera.transform.position = newPosition;
    }

    private Transform GetClick(){
        if(Input.GetMouseButtonDown(0) && !IsMouseOverUI())
        {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            RaycastHit hit;
            if( Physics.Raycast( ray, out hit, 100) )
            {
                Debug.Log( hit.transform.position );
                return hit.transform;
            }
        }

        return null;
    }

    public void SetMode(Mode mode){
        this.mode = mode;
        if(mode != Mode.PLACE){
            board.UnhighlightTiles(shopTiles);
        }
    }
    public Mode GetMode(){
        return mode;
    }

    private bool IsMouseOverUI(){
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void EndGame(){
        if(gameStatus == GameStatus.WIN){
            Debug.Log("You WON!");
        }else {
            Debug.Log("You LOST!");
        }
    }

    public void GetKeyPress(){
        if(Input.GetKeyUp("1")){
            shop.SelectPiece("ShopPawn");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("2")){
            shop.SelectPiece("ShopKnight");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("3")){
            shop.SelectPiece("ShopBishop");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("4")){
            shop.SelectPiece("ShopRook");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("5")){
            shop.SelectPiece("ShopMiner");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("6")){
            shop.SelectPiece("ShopQueen");
            SetMode(Mode.PLACE);
        }

        if(Input.GetKeyUp("e")){
            if(selectedPiece){
                board.UnhighlightTiles(tileArray);
                if(selectedPiece.GetComponent<Miner>() != null){
                    SetMode(Mode.MINE);
                    shop.DeselectPiece(Mode.MINE);
                    GetActionTiles();
                }else if(selectedPiece.GetComponent<Bishop>()!=null){
                    SetMode(Mode.HEAL);
                    shop.DeselectPiece(Mode.HEAL);
                    GetActionTiles();
                }else{
                    SetMode(Mode.ATTACK);
                    shop.DeselectPiece(Mode.ATTACK);
                    GetActionTiles();
                }
            }else{
                SetMode(Mode.ATTACK);
                shop.DeselectPiece(Mode.ATTACK);
                GetActionTiles();
            }
        }

        if(Input.GetKeyUp("q")){
            if(selectedPiece){
                board.UnhighlightTiles(tileArray);
            }
            SetMode(Mode.MOVE);
            shop.DeselectPiece(Mode.MOVE);
            GetActionTiles();
        }

        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)){
            if(selectedPiece == null && shop.selectedEntry == null){
                //open menu
            }
            else{
                Deselect();
                shop.DeselectPiece(Mode.MOVE);
            }
        }


    }
}
