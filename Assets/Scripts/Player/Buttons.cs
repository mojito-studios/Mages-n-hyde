using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    private Player player;

    private void Start()
    {
        player = GetComponentInParent<Player>();
    }
    public void OnHover()
    {
        GameObject.Find("@PlayerInput").GetComponent<PlayerInput>().actionEvents[0].RemoveListener(player.OnMovement);
        GameObject.Find("@PlayerInput").GetComponent<PlayerInput>().actionEvents[1].RemoveListener(player.OnHide);
        player.button = true;
    }

    public void OnExit()
    {
        GameObject.Find("@PlayerInput").GetComponent<PlayerInput>().actionEvents[0].AddListener(player.OnMovement);
        GameObject.Find("@PlayerInput").GetComponent<PlayerInput>().actionEvents[1].AddListener(player.OnHide);
        player.button = false;
    }
}