using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using TouchPhase = UnityEngine.TouchPhase;

public class Player : NetworkBehaviour
{
    private Camera _camera;
    private float speed = 4f;
    private bool _moving = false;
    private Dictionary<ulong, GameObject> playerHidingObjects = new Dictionary<ulong, GameObject>();
    [SerializeField] private Sprite[] allSprites;
    private Vector3 targetPosition = Vector3.zero;
    private InputActionMap _actionMap;
    private Rigidbody2D _rb;
    [HideInInspector] public PropsBehaviour pBehaviour;
    private Button _spell;


    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        Debug.Log(_camera);
        allSprites[0] = GetComponent<SpriteRenderer>().sprite;
        _rb = GetComponent<Rigidbody2D>();
        Button[] buttonList = GetComponentsInChildren<Button>();
        foreach (var button in buttonList)
        {
            if (button.CompareTag("AttackButton")) { _spell = GetComponentInChildren<Button>(); }
        }
        //SetPlayer();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetPlayer();

    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (_moving)
            MovePlayer(); //Ahora se mueve por tener una transform autoritativa de parte del cliente para mejor responsividad a los jugadores
    }

    void SetPlayer()
    {
        PlayerInput playerInput = GameObject.Find("@PlayerInput").GetComponent<PlayerInput>();

        _actionMap = playerInput.actions.FindActionMap("PlayerInGame");


        playerInput.actionEvents[0].AddListener(this.OnMovement);
        playerInput.actionEvents[1].AddListener(this.OnHide);

    }

  
    void MovePlayer()
    {
       
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed); //Todavía no lo probé con el rigidbody 2D
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _moving = false;
        }
    }

    

    //En el host funcionan todas las cosas sin problema y se ven los cambios reflejados en el otro cliente
    public void OnMovement(InputAction.CallbackContext context) //Se activa al hacer click izquierdo
    {
        if (!IsOwner) return;
        Debug.Log("Moving");

        Vector3 position = Vector3.zero;
        if(SystemInfo.deviceType == DeviceType.Desktop) position = Mouse.current.position.ReadValue();
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began) position = Touchscreen.current.position.ReadValue();
            }
        }
        targetPosition = _camera.ScreenToWorldPoint(position);
        targetPosition.z = transform.position.z;
        _moving = true;
    }


    
    public void OnHide(InputAction.CallbackContext context) //Se activa al hacer click derecho cuando estás encima de un prop
    {
        if (!IsOwner) return;
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Ray raycast = _camera.ScreenPointToRay(Input.GetTouch(i).position);
                    RaycastHit raycastHit;
                    if (Physics.Raycast(raycast, out raycastHit))
                    {
                        if (raycastHit.collider.CompareTag("Props"))
                        {
                            pBehaviour = raycastHit.transform.gameObject.GetComponent<PropsBehaviour>();
                        }
                    }
                }
            }
        }
        Debug.Log(GetComponent<SpriteRenderer>().sprite);
        Debug.Log("Distanciaa " + Vector3.Distance(transform.position, pBehaviour.transform.position));
        if (Vector3.Distance(transform.position, pBehaviour.transform.position) < 3f)
        {
            
            OnHideRpc(pBehaviour.spriteNumber, pBehaviour.NetworkObjectId);
            pBehaviour = null;
        }
    }

    [Rpc(SendTo.Server)]
    private void OnHideRpc(int spriteN, ulong NID)
    {
        
        ChangeSpriteRpc(spriteN, NID);
        StartCoroutine(HideCoroutine(10, NID)); //Cambiar el hardcode por el tiempo que vaya a durar el objeto según su SO

    }

    [Rpc(SendTo.Everyone)]
    private void ChangeSpriteRpc(int spriteNumber, ulong NID)
    {
        Debug.Log("Cambio de sprite");
        GetComponent<SpriteRenderer>().sprite = allSprites[spriteNumber]; //Voy a ignorar el tema del color porque en principio solo se cambia ahora por ser placeholders 
        var hideGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[NID].gameObject;
        hideGO.gameObject.SetActive(!hideGO.gameObject.activeSelf);

    }
    private IEnumerator HideCoroutine(int time, ulong NID)
    {
        yield return new WaitForSeconds(time);
        ChangeSpriteRpc(0, NID);
       
    }
    public void OnAttack(InputAction.CallbackContext context) //Se activa al hacer click izquierdo si no estás encima de un prop
    {
        if (!IsOwner) return;
        Debug.Log("attacking");
        

    }


}