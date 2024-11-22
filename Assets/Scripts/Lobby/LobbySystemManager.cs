using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbySystemManager : MonoBehaviour
{
    public static LobbySystemManager Instance { get; private set; }
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject characterStats;
    [SerializeField] private List<GameObject> charactersPrefabs;
    [SerializeField] Button t1;
    [SerializeField] Button t2;
    [SerializeField] Button ready;
    [SerializeField] Button next;
    private SpriteRenderer spriteToShow;
    private int prefabIndex;
    private int provisionalTeam;
    private const int MAX_PLAYERS_TEAM = 2;


    private void Awake()
    {
        provisionalTeam = -1;
        Instance = this;
    }
    void Start()
    {
        
        spriteToShow = playerPrefab.GetComponent<SpriteRenderer>();
        prefabIndex = 0;
        // spriteToShow.sprite = charactersPrefabs[0].GetComponent<SpriteRenderer>().sprite;
         spriteToShow.color = charactersPrefabs[0].GetComponentInChildren<SpriteRenderer>().color; //Como de momento solo cambia el color lo dejo así
      

    }

    // Update is called once per frame
    void Update()
    {
        
    }

  public  void EnableButtons()
    {
        t1.interactable = true;
        t2.interactable = true;
        ready.interactable = true;  
        next.interactable = true;
    }
    void ChangeSprite()
    {
        prefabIndex++;
        if(prefabIndex >= charactersPrefabs.Count)
        {
            prefabIndex = 0;
        }
        spriteToShow.sprite = charactersPrefabs[prefabIndex].GetComponentInChildren<SpriteRenderer>().sprite;
        spriteToShow.color = charactersPrefabs[prefabIndex].GetComponentInChildren<SpriteRenderer>().color;
        OptionsChosen.Instance.ChangePlayerPrefab(prefabIndex);
    }

   public void ChangeTeam(int team)
    {
        if (team == 0 && OptionsChosen.Instance.actualPlayersT1.Value < MAX_PLAYERS_TEAM)
        {
            t1.interactable = false;
            t2.interactable = true;
            if (provisionalTeam == 1)
            {
                OptionsChosen.Instance.AddDelete(provisionalTeam, false);
            }
            OptionsChosen.Instance.AddDelete(team, true);
            provisionalTeam = 0;
        }
        if (team == 1 && OptionsChosen.Instance.actualPlayersT2.Value < MAX_PLAYERS_TEAM)
        {
            t2.interactable = false;
            t1.interactable = true;
            if (provisionalTeam == 0)
            {
                OptionsChosen.Instance.AddDelete(provisionalTeam, false);

            }
            OptionsChosen.Instance.AddDelete(team, true);
            provisionalTeam = 1;
        }
        OptionsChosen.Instance.ChangePlayerTeam(team);
    }
   

}
