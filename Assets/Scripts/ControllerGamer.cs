using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Cinemachine;
using System.Drawing;
using UnityEngine.UI;

public class ControllerGamer : NetworkBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravityForce = 9.81f;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private Transform groundChecker;

    private bool isGround = true;
    private Vector3 velocity;


    [Header("Player Controller Settings")]
    [SerializeField] private GameObject targetLookUp;
    [SerializeField, Min(1f)] private float mouseSensitivity;
    [SerializeField] private float lookAngle;
    [SerializeField] private float speed;
    private float verticalMove;
    private float horizontalMove;
    private float verticalLook;
    private float horizontalLook;
    private float yRotation = 0f;
    private bool escPush;
    private CinemachineVirtualCamera playerVirtualCamera;


    [SerializeField] private TextMesh nickName;
    [SerializeField] private Text id;
    [SyncVar(hook = nameof(OnNameChanged))] private string playerName;
    [SerializeField] private GameObject nickInfo;
    private Animator playerAnimator;
    private CharacterController playerController;

    [Header("Dash Settings")]
    [SerializeField] protected KeyCode dashButton = KeyCode.Mouse0;
    [SerializeField] private float dashSpeedChanger = 150f;//change speed dash
    [SerializeField] private float maxDashTime = 2f;//расстояние рывка


    [SerializeField] private GameObject rayCastDot;//dot spawn ray
    [SerializeField] private float rayDistance = 10f;
    public int dashForWin = 3;//кол-во попаданий для победы


    private LayerMask maskPlayer;//mask for hit ray
    private InteractionDash changeColor;//call method change color
    protected Renderer[] material;

    public int dashHit = 0;//кол-во попаданий по противнику
    private float dashSpeed;//speed
    private float dashTime;// таймер рывка
    [SerializeField] private TrailRenderer dashEffect;


    [Header("Events")]
    public static UnityEvent winCall = new UnityEvent();
    public void OnNameChanged(string _Old, string _New)
    {
        id.text = playerName;
        nickName.text = playerName;
    }
    public override void OnStartLocalPlayer()
    {
        string name = "Player" + Random.Range(0, 999);
        CmdSetupPlayer(name);
    }

    [Command] private void CmdId()
    {
        RpcId();
    }
    [ClientRpc] void RpcId()
    {
        Timer();
    }
    IEnumerator Timer()
    {
        id.enabled = true;
        yield return new WaitForSeconds(3f);
        id.enabled = false;
    }
    [Command]
    public void CmdSetupPlayer(string _name)
    { 
        playerName = _name;
    }
    private void Start()
    {

        if (isLocalPlayer)
        {
            playerVirtualCamera = CinemachineVirtualCamera.FindObjectOfType<CinemachineVirtualCamera>();
            playerVirtualCamera.LookAt = targetLookUp.transform;
            playerVirtualCamera.Follow = targetLookUp.transform;
            maskPlayer = LayerMask.GetMask("Players");
            dashTime = 3f;
            playerAnimator = NetworkClient.localPlayer.GetComponent<Animator>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerController = NetworkClient.localPlayer.GetComponent<CharacterController>();
        }
    }
    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(escPush == false)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                    escPush = true;
                }
                else if(escPush == true)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    escPush = false;
                }
            }
            if (dashHit >= dashForWin)
            {
                CmdId();
                winCall.Invoke();
                dashHit = 0;
            } 
            Debug.DrawRay(rayCastDot.transform.position, rayCastDot.transform.forward * rayDistance);
            if (Input.GetKeyDown(dashButton))
            {
                dashTime = 0;
            }
            DashLogic();
            Controller();
            GravityPhysic();
        }
    }
    private void DashLogic()
    {
        if (maxDashTime >= dashTime)
        {
            Ray ray = new Ray(rayCastDot.transform.position, rayCastDot.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance, maskPlayer))
            {
                changeColor = hit.transform.GetComponent<InteractionDash>();
                changeColor.CmdChangeColorPlayer();
                dashHit += 1;
            }
            Physics.IgnoreLayerCollision(7, 7, true);
            dashTime += Time.deltaTime;
            dashEffect.enabled = true;
            dashSpeed = dashSpeedChanger;

        }
        else
        {
            dashSpeed = 0f;
            dashEffect.enabled = false;
            Physics.IgnoreLayerCollision(7, 7, false);
        }
    }
    private void Controller()
    {
        verticalMove = Input.GetAxis("Vertical");
        horizontalMove = Input.GetAxis("Horizontal");
        horizontalLook = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        verticalLook = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        yRotation -= verticalLook;
        yRotation = Mathf.Clamp(yRotation, -lookAngle, lookAngle);
        targetLookUp.transform.localRotation = Quaternion.Euler(yRotation, 0, 0);
        transform.Rotate(0, horizontalLook, 0);
        Vector3 move = transform.right * horizontalMove + transform.forward * (verticalMove + dashSpeed);
        playerController.Move(move * Time.deltaTime * speed);
        if (move.x != 0 || move.z != 0)
        {
            playerAnimator.SetFloat("WalkingBlend", verticalMove);
            playerAnimator.SetFloat("StraifBlend", horizontalMove);
        }
    }
    private void GravityPhysic()
    {
        isGround = Physics.CheckSphere(groundChecker.position, groundDistance);
        if (isGround && velocity.y < 0)
            velocity.y = 0f;
        velocity.y -= gravityForce;
        playerController.Move(velocity * Time.deltaTime);
    }
}
