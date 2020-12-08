using UnityEngine;
using Mirror;
using Cinemachine;

public class CameraController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Vector2 maxFollowOffset = new Vector2(-1f, 6f);
    [SerializeField] private Vector2 cameraVelocity = new Vector2(4f, 0.25f);
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
    private float minFov = 15f;
    private float maxFov = 90f;
    private float sensitivity = 10f;
    private int speed = 12;
    private int rotationSpeed = 50;

    // private Controls controls;
    // private Controls Controls{
    //     get
    //     {
    //         if (controls != null) { return controls; }
    //         return controls = new Controls();
    //     }
    // }

    private CinemachineTransposer transposer;

    public override void OnStartAuthority(){
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        virtualCamera.gameObject.SetActive(true);

        enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("d"))
            playerTransform.Translate(new Vector3(speed * Time.deltaTime,0,0));
        if(Input.GetKey("a"))
            playerTransform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
        if(Input.GetKey("s"))
            playerTransform.Translate(new Vector3(0,-speed * Time.deltaTime,0));
        if(Input.GetKey("w"))
            playerTransform.Translate(new Vector3(0,speed * Time.deltaTime,0));
        if(Input.GetKey("up"))
            playerTransform.Rotate(new Vector3(rotationSpeed * Time.deltaTime,0,0));
        if(Input.GetKey("down"))
            playerTransform.Rotate(new Vector3(-rotationSpeed * Time.deltaTime,0,0));

        float fov = virtualCamera.m_Lens.FieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        virtualCamera.m_Lens.FieldOfView = fov;
    }
}

