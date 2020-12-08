using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceUI : MonoBehaviour
{
    // Start is called before the first frame update
    protected Piece piece;
    protected Slider healthBar;
    protected Image moveCooldown;
    protected Image attackCooldown;
    protected Camera cameraMain;

    void Start()
    {
        healthBar = transform.GetChild(0).Find("HealthBar").gameObject.GetComponent<Slider>();
        moveCooldown = transform.GetChild(0).Find("Cooldown.move").GetChild(0).gameObject.GetComponent<Image>();
        attackCooldown = transform.GetChild(0).Find("Cooldown.attack").GetChild(0).gameObject.GetComponent<Image>();
        piece = transform.parent.gameObject.GetComponent<Piece>();
        cameraMain = (Camera) FindObjectOfType(typeof(Camera));
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealthBar();
        updateCooldowns();
        //follows camera rotation
        transform.LookAt(transform.position + cameraMain.transform.rotation * Vector3.forward, cameraMain.transform.rotation * Vector3.up);
    }

    void UpdateHealthBar(){
        healthBar.value = piece.GetHealthPercentage();
    }

    void updateCooldowns(){
        moveCooldown.fillAmount = piece.GetMoveCooldownPercentage();
        attackCooldown.fillAmount = piece.GetAttackCooldownPercentage();

        if(moveCooldown.fillAmount == 1){
            transform.GetChild(0).Find("Cooldown.move").gameObject.SetActive(false);
        }else{
            transform.GetChild(0).Find("Cooldown.move").gameObject.SetActive(true);
        }

        if(attackCooldown.fillAmount == 1){
            transform.GetChild(0).Find("Cooldown.attack").gameObject.SetActive(false);
        }else{
            transform.GetChild(0).Find("Cooldown.attack").gameObject.SetActive(true);
        }
    }

}
