using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Mirror;
using NameSpaces;

namespace NameSpaces
{
    public enum Mode {MOVE, ATTACK, PLACE}

    public enum GameStatus {WIN, LOSE, ONGOING}

}

public class GamePlayer : NetworkBehaviour
{

    [SyncVar] private string displayName;
    [SerializeField][SyncVar] private int playerNumber;
    [SerializeField] private GameObject ui = null;
    
    private ChessNetworkManager room;
    private ChessNetworkManager Room{
        get{
            if(room != null){
                return room;
            }
            return room = NetworkManager.singleton as ChessNetworkManager;
        }
    }

    #region GameVars

    public Mode mode = Mode.MOVE;
    public Board board;
    public Piece selectedPiece;
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
    private bool ready = false;

    #endregion






    public override void OnStartClient(){
        DontDestroyOnLoad(gameObject);
        
        Room.GamePlayers.Add(this);
    }

    

    public void Setup(){
        board = GameObject.Find("Board(Clone)").GetComponent<Board>();
        if(hasAuthority){
            Debug.Log("Setting up");
            ui.SetActive(true);
            enabled = true;
        }
    }

    public override void OnStopClient(){
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string name){
        this.displayName = name;
    }

    [Server]
    public void SetPlayerNumber(int number){
        this.playerNumber = number;
    }


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
    public void GameSetup(){
        if(!hasAuthority)
            return;
        GenerateShopTiles();


        pieces = new GameObject[] {pawn, knight, bishop, rook, miner, queen, king};
        if(playerNumber == 1){ //Player 2
            playerNumber = 0;
            for(int i = 0; i < Board.rows; i++){
                PlacePiece(pawn, new Vector3(i, 0, 2));
            }
            PlacePiece(king, new Vector3(0, 0, 0));

            playerNumber = 1;
            for(int i = 0; i < Board.rows; i++){
                PlacePiece(pawn, new Vector3(i, 0, Board.columns - 3));
            }
            PlacePiece(king, new Vector3(Board.rows-1,0,Board.columns- 1));
        }

        ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(ready && hasAuthority){
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
    }

    public void PlacePiece(GameObject piece, Vector3 position){
        Quaternion rot = piece.transform.rotation;
        if(playerNumber == 1)
            rot *= Quaternion.Euler(0,180,0);

        CmdSpawnPiece(piece.name, position, rot, playerNumber);
    }

    [Command]
    public void CmdSpawnPiece(String pieceName, Vector3 position, Quaternion rotation, int playerNumber){
        foreach(GameObject prefab in Room.spawnPrefabs){
            if(prefab.name == pieceName){
                GameObject pieceToSpawn = prefab;
                GameObject obj = Instantiate(pieceToSpawn, position, rotation);
                obj.GetComponent<Piece>().SetPlayer(playerNumber);
                Room.SpawnPiece(obj, gameObject);
                board.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
                board.RpcPlacePiece(obj, position);
                board.GetComponent<NetworkIdentity>().RemoveClientAuthority();
                RpcChangeIncome(obj.GetComponent<Piece>().GetIncome());
            }
        }
    }


    [ClientRpc]
    public void RpcChangeIncome(int amount){
        shop.ChangeIncome(amount);
    } 

    [ClientRpc]
    public void RpcChangeGold(int amount){
        shop.ChangeGold(amount);
    }

    public void GetActionTiles(){
        board.UnhighlightTiles(shopTiles);
        if(tileArray != null)
            board.UnhighlightTiles(tileArray);

        if(selectedPiece && mode != Mode.PLACE){
            tileArray = board.MarkTiles(selectedPiece, mode, null);
            if((!selectedPiece.CanAttack() && (mode == Mode.ATTACK) || (!selectedPiece.CanMove() && mode == Mode.MOVE))){
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
            if(mode == Mode.ATTACK){
                if(selectedPiece.GetComponent<Miner>()!=null){
                    CmdSelectMine(objectPosition, tileArray, mode);
                }else if(selectedPiece.GetComponent<Bishop>()!=null){
                    CmdSelectHeal(selectedPiece.gameObject, tileArray, mode, objectPosition);
                }else{
                    CmdSelectAttack(selectedPiece.gameObject, tileArray, mode, objectPosition);
                }
            }else{
                shop.DeselectPiece(Mode.MOVE);
            }
        }else{
            if(mode == Mode.PLACE){
                SelectPlace(objectPosition);
            }else if(mode == Mode.MOVE && selectedPiece != null){
                shop.DeselectPiece(Mode.MOVE);
                CmdSelectMove(selectedPiece.gameObject, tileArray, objectPosition);
            }
        }
    }

    private void SelectPiece(Piece piece){ //PICKING A NEW PIECE
        if(piece.GetPlayer() == playerNumber){ 
            if(mode == Mode.ATTACK){
                if(piece.GetComponent<Miner>()!= null && piece.GetPosition() == selectedPiece.GetPosition() && selectedPiece.CanAttack()){
                    return;
                }else if(selectedPiece && selectedPiece.GetComponent<Bishop>()!=null && selectedPiece.CanAttack() 
                && Array.Exists(tileArray, attack => attack == piece.GetPosition())){
                    return;
                }else{
                    if(selectedPiece && selectedPiece.GetComponent<Miner>() != null && piece.GetComponent<Miner>() == null){
                        mode = Mode.MOVE;
                    }
                    Deselect();
                    selectedPiece = piece;
                    selectedPiece.Highlight();
                }
            }else{
                Deselect();
                if(mode == Mode.PLACE){
                    shop.DeselectPiece(Mode.MOVE);
                }
                if(piece.GetComponent<Miner>() != null){
                    mode = Mode.ATTACK;
                }
                selectedPiece = piece;
                selectedPiece.Highlight();
            }
        }
    }

    [Command]
    private void CmdSelectMine(Vector3 tilePosition, Vector3[] tileArray, Mode mode){
        Piece clickedPiece = board.GetPiece(tilePosition);
        if((clickedPiece.GetComponent<Miner>() != null) && (clickedPiece.GetPlayer() == playerNumber) && clickedPiece.CanAttack()){
            if(tileArray != null && Array.Exists(tileArray, attack => attack == tilePosition)){
                int valueChanges = board.AttackSquare(clickedPiece, mode, tilePosition);
                RpcChangeGold(valueChanges);
            }
        }
    }

    [Command]
    private void CmdSelectHeal(GameObject pieceObject, Vector3[] tileArray, Mode mode,  Vector3 tilePosition){
        Piece clickedPiece = board.GetPiece(tilePosition);
        Piece selected= pieceObject.GetComponent<Piece>();
        if((selected.GetComponent<Bishop>() != null) && (clickedPiece.GetPlayer() == playerNumber) && selected.CanAttack()){
            if(tileArray != null && Array.Exists(tileArray, attack => attack == tilePosition)){
                board.AttackSquare(selected, mode, tilePosition);
            }
        }
    }

    [Command]
    private void CmdSelectMove(GameObject pieceObject, Vector3[] tileArray, Vector3 tilePosition){
        Piece piece = pieceObject.GetComponent<Piece>();
        if(tileArray != null && Array.Exists(tileArray, move => move == tilePosition)){
            if(piece.CanMove()){
                if(board.IsFree(tilePosition)){
                    Vector3 oldPosition = piece.GetPosition();
                    board.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
                    board.RpcRemovePiece(oldPosition);
                    board.RpcPlacePiece(pieceObject, tilePosition);
                    board.GetComponent<NetworkIdentity>().RemoveClientAuthority();
                    piece.StartMovementCooldown();
                }
            }
        }
    }

    [Command]
    private void CmdSelectAttack(GameObject pieceObject, Vector3[] tileArray, Mode mode,  Vector3 tilePosition){
        Piece clickedPiece = board.GetPiece(tilePosition);
        Piece selected = pieceObject.GetComponent<Piece>();
        if(clickedPiece && clickedPiece.GetPlayer() != playerNumber && selected.CanAttack()){
            if(tileArray != null && Array.Exists(tileArray, attack => attack == tilePosition)){
                board.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
                int value = board.AttackSquare(selected, mode, tilePosition);
                board.GetComponent<NetworkIdentity>().RemoveClientAuthority();
                if(value == -1){
                    CmdWinGame(playerNumber);
                }else{
                    RpcChangeGold(value);
                }
            }
        }
    }

    [Command]
    public void CmdWinGame(int playerNumber){   
        Room.GamePlayers[1-playerNumber].gameStatus = GameStatus.LOSE;
        Room.GamePlayers[playerNumber].gameStatus = GameStatus.WIN;
    }
    
    private void SelectPlace(Vector3 tilePosition){
        if(Array.Exists(tileArray, place => place == tilePosition)){
            shop.BuyPiece(tilePosition);
            SetMode(Mode.MOVE);
            Deselect();
        }
    }

    public void Deselect(){
        if(selectedPiece){
            selectedPiece.Unhighlight();
            selectedPiece = null;
        }
        if(tileArray != null)
            board.UnhighlightTiles(tileArray);
        tileArray=null;
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
                return hit.transform;
            }
        }

        return null;
    }

    public void SetMode(Mode mode){
        this.mode = mode;
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

    

    public void SetNumber(int number){
        playerNumber = number;
    }

    public void GetKeyPress(){
        if(Input.GetKeyUp("1") && shop.gold > pawn.GetComponent<Piece>().GetCost()){
            shop.SelectPiece("ShopPawn");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("2")&& shop.gold > knight.GetComponent<Piece>().GetCost()){
            shop.SelectPiece("ShopKnight");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("3")&& shop.gold > bishop.GetComponent<Piece>().GetCost()){
            shop.SelectPiece("ShopBishop");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("4")&& shop.gold > rook.GetComponent<Piece>().GetCost()){
            shop.SelectPiece("ShopRook");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("5")&& shop.gold > queen.GetComponent<Piece>().GetCost()){
            shop.SelectPiece("ShopQueen");
            SetMode(Mode.PLACE);
        }
        if(Input.GetKeyUp("6")&& shop.gold > miner.GetComponent<Piece>().GetCost()){
            shop.SelectPiece("ShopMiner");
            SetMode(Mode.PLACE);
        }

        if(Input.GetKeyUp("e")){
            if(selectedPiece){
                board.UnhighlightTiles(tileArray);
            }
            SetMode(Mode.ATTACK);
            shop.DeselectPiece(Mode.ATTACK);
            GetActionTiles();
        }

        if(Input.GetKeyUp("q")){
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
