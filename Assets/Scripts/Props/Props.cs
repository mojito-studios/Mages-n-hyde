using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PropsBehaviour : NetworkBehaviour 
{
   private Player _player;
   public PropSO propSO;
   public int spriteNumber;
   public float timeHiding;
   protected void Start()
    {
        spriteNumber = propSO.spriteNumber;
        timeHiding = propSO.timeHiding;
        
    }

    protected void Update()
    {
    }

    protected void OnMouseEnter()
    {
        _player = GetLocalPlayer(); 
        _player.pBehaviour = this;
    }

    protected void OnMouseExit()
    {
        if (_player != null)
        {
           _player = null;
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
