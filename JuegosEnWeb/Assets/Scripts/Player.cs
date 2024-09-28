using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    private Camera _camera ;
    private float speed = 4f;
    private bool _moving = false;
    private Vector3 targetPosition = Vector3.zero;
    void Start()
    {
        _camera = Camera.main;
        Debug.Log(_camera);
    
            SetPlayer();
    }


    // Update is called once per frame
    void Update()
    {
        if(_moving)
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _moving = false;
        }

    }

    void SetPlayer()
    {
        PlayerInput playerInput = GameObject.Find("@PlayerInput").GetComponent<PlayerInput>();

        playerInput.actionEvents[0].AddListener(this.OnMovement); //En mi cabeza tiene sentido esto es como lo de entornos pero en vez d hacer un input controller lo asigno directamente desde el player que creo que tiene sentido y se puede hacer.
        //Si no he dejao un script de input controller para ponerlo ahí a ver qué pasa.
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        targetPosition = _camera.ScreenToWorldPoint(mousePosition); //Esto luego se va cuando se hagan las movidas de serverrpc
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
     
     

}
