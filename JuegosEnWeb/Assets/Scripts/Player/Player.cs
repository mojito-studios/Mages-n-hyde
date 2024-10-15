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
    private bool _hiding = false;
    [SerializeField] private Sprite[] allSprites;
    private Vector3 targetPosition = Vector3.zero;
    private InputActionMap _actionMap;
    [HideInInspector] public PropsBehaviour pBehaviour;
    void Start()
    {
        _camera = Camera.main;
        Debug.Log(_camera);
        allSprites[0] = GetComponent<SpriteRenderer>().sprite;
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
       
      MovePlayer(); //De esta manera funciona solo el movimiento en el cliente


      /*  if (!IsOwner) return; De esta manera no funciona pero detecta todas las funciones y todo los cambios 
         if (IsServer) MovePlayer();
         else if (IsClient)
         {
                Debug.Log("entra");
                MovePlayerServerRpc();
         
        */
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
        if (_moving)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed); //Todavía no lo probé con el rigidbody 2D
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _moving = false;
        }
    }

    [ServerRpc]
    void MovePlayerServerRpc()
    {
        MovePlayer();
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
        OnMovementServerRpc(mousePosition);

    }

    [ServerRpc]

    public void OnMovementServerRpc(Vector3 movement)
    {
        targetPosition = _camera.ScreenToWorldPoint(movement);
        targetPosition.z = transform.position.z;
        _moving = true;
    }

    
    public void OnHide(InputAction.CallbackContext context) //Se activa al hacer click derecho cuando estás encima de un prop
    {
        if (!IsOwner || _hiding) return;
        Debug.Log("Distanciaa " + Vector3.Distance(transform.position, pBehaviour.transform.position));
        if (Vector3.Distance(transform.position, pBehaviour.transform.position) < 3f)
        {
            
            OnHideServerRpc();
        }
    }

    [ServerRpc]
    private void OnHideServerRpc()
    {
        Debug.Log("hiding");
        _hiding = true;
        ChangeSpriteClientRpc(pBehaviour.spriteNumber);
        StartCoroutine(HideCoroutine(10)); //Cambiar el hardcode por el tiempo que vaya a durar el objeto según su SO

    }

    [ClientRpc]
    private void ChangeSpriteClientRpc(int spriteNumber)
    {
        GetComponent<SpriteRenderer>().sprite = allSprites[spriteNumber]; //Voy a ignorar el tema del color porque en principio solo se cambia ahora por ser placeholders 
        pBehaviour.gameObject.SetActive(!pBehaviour.gameObject.activeSelf);

    }
    private IEnumerator HideCoroutine(int time)
    {
        yield return new WaitForSeconds(time);
        _hiding = false;
        ChangeSpriteClientRpc(0);
        pBehaviour = null;
       
    }
    public void OnAttack(InputAction.CallbackContext context) //Se activa al hacer click izquierdo si no estás encima de un prop
    {
        if (!IsOwner) return;
        Debug.Log("attacking");
        // currentState = PlayerState.Attacking;

    }


}