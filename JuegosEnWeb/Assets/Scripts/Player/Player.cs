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
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>(PlayerState.Idle);
    private Vector3 targetPosition = Vector3.zero;
    private InputActionMap _actionMap;
    [HideInInspector] public PropsBehaviour pBehaviour;
    
    void Start()
    {
        _camera = Camera.main;
        Debug.Log(_camera);
        
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
        switch (networkPlayerState.Value)
        {
          
            case PlayerState.Idle:
                    Debug.Log("idle");
                break;
            case PlayerState.Moving:
                Debug.Log("Moving");
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
                    Debug.Log("Moving to: " + targetPosition);
                    if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                        OnMovementCompleteServerRpc();
                }
                break;
            case PlayerState.Attacking:
                break;
            case PlayerState.Hiding: //Mientras se está escondiendo no puede hacer nada (ni de fisicas ni de golpes ni nada) asi que le meto un return para que salga del switch en cada iteración que esté escondido
                return;
                

        }
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

    //Si veo que el código de Player se queda muy largo cambio todo esto al InputController
    public void OnMovement(InputAction.CallbackContext context)
    {
        if(!IsOwner || networkPlayerState.Value == PlayerState.Hiding) return;
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        targetPosition = _camera.ScreenToWorldPoint(mousePosition);
        targetPosition.z = transform.position.z;
        OnMovementServerRpc();

    }

    [ServerRpc]

    private void OnMovementServerRpc()
    {
      
        networkPlayerState.Value = PlayerState.Moving;

    }

    [ServerRpc]
    private void OnMovementCompleteServerRpc()
    {
        networkPlayerState.Value = PlayerState.Idle;
    }


    public void OnHide(InputAction.CallbackContext context)
    {
        if (!IsOwner || networkPlayerState.Value == PlayerState.Hiding) return;
        if (Vector3.Distance(targetPosition, pBehaviour.transform.position) < 1.5f)
        {
            
            OnHideServerRpc();
        }
    }

    [ServerRpc]
    private void OnHideServerRpc()
    {
        Debug.Log("hiding");
        networkPlayerState.Value = PlayerState.Hiding;
        HideClientRpc(false);
        StartCoroutine(HideCoroutine(10));
     
    }

    [ClientRpc]
    private void HideClientRpc(bool isVisible)
    {
        GetComponent<SpriteRenderer>().enabled = isVisible;
    }
    private IEnumerator HideCoroutine(int time)
    {
        yield return new WaitForSeconds(time); 
        HideClientRpc(true);
        networkPlayerState.Value = PlayerState.Idle;
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner || networkPlayerState.Value == PlayerState.Hiding) return;
        Debug.Log("attacking");

    }

}
