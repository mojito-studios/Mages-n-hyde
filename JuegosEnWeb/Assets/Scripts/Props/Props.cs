using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PropsBehaviour : NetworkBehaviour 
{
   private Player _player;
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
            _player.SwapInputAction();
        }
    }

    protected void OnMouseExit()
    {
        _player = GetLocalPlayer();
        if (_player != null)
        {
            Debug.Log(_player + "activando atacar");
            _player.SwapInputAction();
            _player.pBehaviour = null;
        }
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
