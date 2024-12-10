using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Prop")]
public class PropSO : ScriptableObject 
{
    public GameObject prefab;
    public int spriteNumber;
    public float timeHiding;
    public float speed;
}
