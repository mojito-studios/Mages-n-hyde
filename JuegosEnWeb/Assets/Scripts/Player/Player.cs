using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    private Camera _camera;
    private float speed = 4f;
    private enum PlayerState {Idle, Moving, Hiding, Attacking}
    private PlayerState currentState;
    [SerializeField] private Sprite[] allSprites;
    private Vector3 targetPosition = Vector3.zero;
    private InputActionMap _actionMap;
    [HideInInspector] public PropsBehaviour pBehaviour;
    
    void Start()
    {
        _camera = Camera.main;
        Debug.Log(_camera);
        SetState(PlayerState.Idle);
        allSprites[0] = GetComponent<SpriteRenderer>().sprite;
    }

    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            SetPlayer();
        }
    }

        // Update is called once per frame
        void Update()
        {
            if (IsOwner)
            {
                switch (currentState)
                {
                    case PlayerState.Idle:
                        Debug.Log("idle");
                        break;
                    case PlayerState.Moving:
                        Debug.Log("Moving");
                        if (IsServer) MovePlayer();
                        else if (IsClient)
                        {
                            Debug.Log("entra");
                            OnMovementServerRpc();
                        }
                        break;
                    case PlayerState.Attacking:
                        break;
                    case PlayerState.Hiding: 
                        break;

                }
            }
    }

    void SetPlayer()
    {
        PlayerInput playerInput = GameObject.Find("@PlayerInput").GetComponent<PlayerInput>();
        _actionMap = playerInput.actions.FindActionMap("PlayerInGame");


        playerInput.actionEvents[0].AddListener(this.OnMovement);
        playerInput.actionEvents[1].AddListener(this.OnHide);
        playerInput.actionEvents[2].AddListener(this.OnAttack);

       _actionMap.FindAction("Hide").Disable(); //Como atacar y esconderse tienen el mismo bot�n, quito esconderse hasta que seleccione un objeto
        
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

    private void SetState(PlayerState state)
    {
        currentState = state;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        targetPosition = _camera.ScreenToWorldPoint(mousePosition);
        targetPosition.z = transform.position.z;
        SetState(PlayerState.Moving);
        
    }

    [ServerRpc]
    private void OnMovementServerRpc()
    {
        MovePlayer();
    }

    public void MovePlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
        Debug.Log("Moving to: " + targetPosition);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetState(PlayerState.Idle);
        }
    }


    public void OnHide(InputAction.CallbackContext context)
    {
        if (!IsOwner || currentState == PlayerState.Hiding) return;
        if (Vector3.Distance(targetPosition, pBehaviour.transform.position) < 1.5f)
        {
            
            OnHideServerRpc();
        }
    }

    [ServerRpc]
    private void OnHideServerRpc()
    {
        Debug.Log("hiding");
        currentState = PlayerState.Hiding;
        ChangeSpriteClientRpc(pBehaviour.spriteNumber);
        StartCoroutine(HideCoroutine(10));
     
    }

    [ClientRpc]
    private void ChangeSpriteClientRpc(int spriteNumber)
    {
        GetComponent<SpriteRenderer>().sprite = allSprites[spriteNumber]; //Voy a ignorar el tema del color porque en principio solo da problemas ahora por ser placeholders
        pBehaviour.gameObject.SetActive(!pBehaviour.gameObject.activeSelf);

    }
    private IEnumerator HideCoroutine(int time)
    {
        yield return new WaitForSeconds(time);
        ChangeSpriteClientRpc(0);
        pBehaviour = null;
        currentState = PlayerState.Idle;
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        Debug.Log("attacking");
       // currentState = PlayerState.Attacking;

    }

}
