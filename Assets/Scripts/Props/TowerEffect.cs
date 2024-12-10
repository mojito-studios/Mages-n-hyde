using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TowerEffect : MonoBehaviour
{
    private float timeAnim = 5;
    private float timeOn = 0;

    void Awake()
    {
    }

    void Update()
    {
        timeOn += Time.deltaTime;

        if (timeOn > timeAnim)
        {
            timeOn = 0;
            gameObject.SetActive(false);
        }
    }
}

