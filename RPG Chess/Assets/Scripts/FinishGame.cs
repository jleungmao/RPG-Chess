using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishGame : MonoBehaviour
{
    public void ResetGame(){
        Destroy(GameObject.Find("NetworkManager"));
        SceneManager.LoadScene("MenuScene");
    }
}
