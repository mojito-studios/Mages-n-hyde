using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbySystemManager : MonoBehaviour
{
    public static LobbySystemManager Instance { get; private set; }
    [SerializeField] GameObject playerPrefab;
    [SerializeField] List<CharactersSO> characters;
    [SerializeField] GameObject characterStats;
    [SerializeField] GameObject textContainer;
    [SerializeField] GameObject transition;
    [SerializeField] Button t1;
    [SerializeField] Button t2;
    [SerializeField] Button ready;
    [SerializeField] Button next;

    private Image spriteToShow;
    private TMP_Text[] texts;
    private Slider[] stats;
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
      
         texts = textContainer.GetComponentsInChildren<TMP_Text>();
         stats = characterStats.GetComponentsInChildren<Slider>();
         spriteToShow = playerPrefab.GetComponent<Image>();
         prefabIndex = 0;
         OptionsChosen.Instance.ChangePlayerPrefab(prefabIndex);
         OptionsChosen.Instance.OnReadyDisable += DeactivateReady;
         Initiate();
        
         
      

    }

   

   
    // Update is called once per frame
    void Update()
    {
        
    }

    void Initiate()
    {
        spriteToShow.sprite = characters[0].characterSprite;
        texts[0].text = characters[0].characterName;
        texts[1].text = characters[0].ultiName;
        texts[2].text = characters[0].ultiDescription;
        stats[0].value = characters[0].characterHealth;
        stats[1].value = characters[0].characterAttack;
        stats[2].value = characters[0].characterSpeed;
        stats[3].value = characters[0].characterRange;

     


    }

    public void EnableButtons()
    {
        t1.interactable = true;
        t2.interactable = true;
        ready.interactable = true;
        next.interactable = true;
    }

   public void ChangeSprite()
    {
        prefabIndex++;
        if(prefabIndex >= characters.Count)
        {
            prefabIndex = 0;
        }
        spriteToShow.sprite = characters[prefabIndex].characterSprite;
        OptionsChosen.Instance.ChangePlayerPrefab(prefabIndex);
    }

    public void ChangeText()
    {
        texts[0].text = characters[prefabIndex].characterName;
        texts[1].text = characters[prefabIndex].ultiName;
        texts[2].text = characters[prefabIndex].ultiDescription;

    }

    void DeactivateReady()
    {
        ready.interactable = false;

    }

    private void OnDestroy()
    {
        OptionsChosen.Instance.OnReadyDisable -= DeactivateReady;

    }

    public void ChangeStats()
    {
        stats[0].value = characters[prefabIndex].characterHealth;
        stats[1].value = characters[prefabIndex].characterAttack;
        stats[2].value = characters[prefabIndex].characterSpeed;
        stats[3].value = characters[prefabIndex].characterRange;

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
            OptionsChosen.Instance.ChangePlayerTeam(team);
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
            OptionsChosen.Instance.ChangePlayerTeam(team);

            provisionalTeam = 1;
        }
    }
   

}
