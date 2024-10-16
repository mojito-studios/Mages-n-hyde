using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class Player : NetworkBehaviour
{
    private Camera _camera;
    private float speed = 4f;
    private bool _moving = false;
    private NetworkVariable<bool> _hiding = new NetworkVariable<bool>(false);
    private Dictionary<ulong, GameObject> playerHidingObjects = new Dictionary<ulong, GameObject>();
    [SerializeField] private Sprite[] allSprites;
    private Vector3 targetPosition = Vector3.zero;
    private InputActionMap _actionMap;
    private Rigidbody2D _rb;
    [HideInInspector] public PropsBehaviour pBehaviour;
    void Start()
    {
        _camera = Camera.main;
        Debug.Log(_camera);
        allSprites[0] = GetComponent<SpriteRenderer>().sprite;
        _rb = GetComponent<Rigidbody2D>();
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
        playerInput.actionEvents[2].AddListener(this.OnAttack);

        _actionMap.FindAction("Hide").Disable(); //Como atacar y esconderse tienen el mismo botón, quito esconderse hasta que seleccione un objeto
    }

    public void SwapInputAction()
    {
        if (_actionMap.FindAction("Attack").enabled)
        {
            _actionMap.FindAction("Attack").Disable();
            _actionMap.FindAction("Hide").Enable();
        }
        else if (!_actionMap.FindAction("Attack").enabled)
        {
            _actionMap.FindAction("Hide").Disable();
            _actionMap.FindAction("Attack").Enable();
        }
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

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        targetPosition = _camera.ScreenToWorldPoint(mousePosition); 
        targetPosition.z = transform.position.z;
        _moving = true;

    }


    
    public void OnHide(InputAction.CallbackContext context) //Se activa al hacer click derecho cuando estás encima de un prop
    {
        Debug.Log("HIDING VALUE "+ _hiding.Value);
        if (!IsOwner || _hiding.Value) return;
        Debug.Log("Distanciaa " + Vector3.Distance(transform.position, pBehaviour.transform.position));
        if (Vector3.Distance(transform.position, pBehaviour.transform.position) < 3f)
        {
            
            OnHideServerRpc();
        }
    }

    [ServerRpc]
    private void OnHideServerRpc()
    {
        
        _hiding.Value = true;
        SwapInputAction();
        ChangeSpriteClientRpc(pBehaviour.spriteNumber, pBehaviour.NetworkObjectId);
        StartCoroutine(HideCoroutine(10)); //Cambiar el hardcode por el tiempo que vaya a durar el objeto según su SO

    }

    [ClientRpc]
    private void ChangeSpriteClientRpc(int spriteNumber, ulong NID)
    {
        Debug.Log("Cambio de sprite");
        GetComponent<SpriteRenderer>().sprite = allSprites[spriteNumber]; //Voy a ignorar el tema del color porque en principio solo se cambia ahora por ser placeholders 
        var hideGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[NID].gameObject;
        hideGO.gameObject.SetActive(!hideGO.gameObject.activeSelf);

    }
    private IEnumerator HideCoroutine(int time)
    {
        yield return new WaitForSeconds(time);
        _hiding.Value = false;
        ChangeSpriteClientRpc(0, pBehaviour.NetworkObjectId);
        pBehaviour = null;
       
    }
    public void OnAttack(InputAction.CallbackContext context) //Se activa al hacer click izquierdo si no estás encima de un prop
    {
        if (!IsOwner) return;
        Debug.Log("attacking");
        

    }


}