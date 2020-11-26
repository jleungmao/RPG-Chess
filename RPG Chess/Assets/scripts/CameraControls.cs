using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    // Start is called before the first frame update
    private float minFov = 15f;
    private float maxFov = 90f;
    private float sensitivity = 10f;
    private int speed = 10;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("d"))
            transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
        if(Input.GetKey("a"))
            transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
        if(Input.GetKey("s"))
            transform.Translate(new Vector3(0,-speed * Time.deltaTime,0));
        if(Input.GetKey("w"))
            transform.Translate(new Vector3(0,speed * Time.deltaTime,0));

        float fov = Camera.main.fieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        Camera.main.fieldOfView = fov;
    }
}
