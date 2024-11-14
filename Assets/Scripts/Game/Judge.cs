using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class Judge : MonoBehaviour
{
    public FixedString128Bytes winningTeam;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
