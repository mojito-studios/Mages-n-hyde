using System.Collections;
using System.Collections.Generic;
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
    }

    public void OnExit()
    {
        GameObject.Find("@PlayerInput").GetComponent<PlayerInput>().actionEvents[0].AddListener(player.OnMovement);
    }
}
