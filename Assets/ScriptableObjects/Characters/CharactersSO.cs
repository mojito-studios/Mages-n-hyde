using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character")]
public class CharactersSO : ScriptableObject
{
    public Sprite characterSprite;
    public string characterName;
    public float characterRange;
    public float characterHealth;
    public float characterSpeed;
    public float characterAttack;
    public string ultiName;
    public string ultiDescription;

}


