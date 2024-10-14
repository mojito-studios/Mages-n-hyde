using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PropsBehaviour : NetworkBehaviour 
{
   private Player _player;
   public int spriteNumber = 1;
   protected void Start()
    {
        
        
    }

    protected void Update()
    {
    }

    protected void OnMouseEnter()
    {
        _player = GetLocalPlayer(); 
        _player.pBehaviour = this;
        if (_player != null)
        {
            Debug.Log(_player + "activando esconderse");

            _player.SwapInputAction();
        }
    }

    protected void OnMouseExit()
    {
        _player = GetLocalPlayer();
        Debug.Log(_player + "activando atacar");
        _player.SwapInputAction();
        
    }

    protected Player GetLocalPlayer()
    {
        foreach(var player in FindObjectsOfType<Player>())
        {
            if (player.IsLocalPlayer)
            {
                return player;
            }
        }
        return null;
    }


}
