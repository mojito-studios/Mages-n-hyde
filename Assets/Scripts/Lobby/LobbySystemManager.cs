using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbySystemManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject characterStats;
    [SerializeField] private List<GameObject> charactersPrefabs;
    [SerializeField] Button t1;
    [SerializeField] Button t2;
    private SpriteRenderer spriteToShow;
    private int prefabIndex;


    private void Awake()
    {
        
    }
    void Start()
    {
        spriteToShow = playerPrefab.GetComponent<SpriteRenderer>();
        prefabIndex = 0;
       // spriteToShow.sprite = charactersPrefabs[0].GetComponent<SpriteRenderer>().sprite;
         spriteToShow.color = charactersPrefabs[0].GetComponent<SpriteRenderer>().color; //Como de momento solo cambia el color lo dejo así
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeSprite()
    {
        prefabIndex++;
        if(prefabIndex >= charactersPrefabs.Count)
        {
            prefabIndex = 0;
        }
        //spriteToShow.sprite = charactersPrefabs[prefabIndex].GetComponent<SpriteRenderer>().sprite;
        spriteToShow.color = charactersPrefabs[prefabIndex].GetComponent<SpriteRenderer>().color;
        OptionsChosen.Instance.ChangePlayerPrefab(prefabIndex);
    }

   public void ChangeTeam(int team)
    {
        if (team == 0)
        {
            t1.interactable = false;
            t2.interactable = true;
        }
        if (team == 1)
        {
            t2.interactable = false;
            t1.interactable = true;
        }
        OptionsChosen.Instance.ChangePlayerTeam(team);
    }
   
}
