using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using TouchPhase = UnityEngine.TouchPhase;

public class Player : NetworkBehaviour
{
    //player
    private Camera _camera;
    public int teamAssign;
    private Tower teamTower;
    private const int maxLife = 100;
    public int attack { get; private set; }
    private NetworkVariable<int> health = new NetworkVariable<int> ();
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider towerHealth;
    private NetworkVariable<int> ultiAttack = new NetworkVariable<int>();
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _towerHealth;

    //move
    private float speed = 4f;
    private bool _moving = false;
    public bool button = false;
    private Vector3 targetPosition = Vector3.zero;

    //hide
    private bool _hiding = false;
    [HideInInspector] public PropsBehaviour pBehaviour;
    private Dictionary<ulong, GameObject> playerHidingObjects = new Dictionary<ulong, GameObject>();
    [SerializeField] private Sprite[] allSprites;

    //attack
    private Button _spell;
    private Button _ultimateAttack;
    private Vector3 _spawnPosition;
    private int _respawnTime = 5;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellTransform;

    void Start()
    {
        if (!IsOwner) return;
        _camera = GetComponentInChildren<Camera>();
        Debug.Log(_camera);
        allSprites[0] = GetComponent<SpriteRenderer>().sprite;
        Button[] buttonList = GetComponentsInChildren<Button>();
        foreach (var button in buttonList)
        {
            if (button.CompareTag("AttackButton")) { _spell = button; }
            else if(button.CompareTag("UltiButton")) { _ultimateAttack = button; }
        }
        _ultimateAttack.interactable = false;

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health.Value = maxLife;
        SetPlayer();
        AssignTower();
        ultiAttack.OnValueChanged += interactableButton;

    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (!button)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    targetPosition = _camera.ScreenToWorldPoint(Input.GetTouch(i).position);
                    targetPosition.z = transform.position.z;
                    _moving = true;
                }
            }
        }
        if (_moving)
            MovePlayer(); 
        updateHealthRpc();
    }

    void SetPlayer()
    {
        PlayerInput playerInput = GameObject.Find("@PlayerInput").GetComponent<PlayerInput>();

        playerInput.actionEvents[0].AddListener(this.OnMovement);
        playerInput.actionEvents[1].AddListener(this.OnHide);
    }

    private void AssignTower()
    {
        if (teamAssign == 0)
        {
            teamTower = GameObject.FindGameObjectWithTag("Team1Tower").GetComponent<Tower>();
            
        }
        else
        {
            teamTower = GameObject.FindGameObjectWithTag("Team2Tower").GetComponent<Tower>();
            
        }

    }

    public void getHit()
    {
        getHitRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void getHitRpc()
    {
        health.Value -= 10;
        if (health.Value <= 0)
        {
            health.Value = maxLife;
            RespawnPlayerRpc(_spawnPosition);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void updateHealthRpc()
    {
        _health.text = "Health: " + health.Value;
        healthBar.value = health.Value;
        _towerHealth.text = "TowerHealth: " + teamTower.currentLife.Value;
        towerHealth.value = teamTower.currentLife.Value;
    }

    [Rpc(SendTo.Server)]
    private void RespawnPlayerRpc(Vector3 position)
    {
        RespawnPlayer(position);
    }

    public void RespawnPlayer(Vector3 position)
    {
        SetActiveStateRpc(false);
        _spawnPosition = position;
        Invoke("SpawnObject", 5);
    }
   
    private void SpawnObject()
    {
        _moving = false;
        transform.position = _spawnPosition; 
        SetActiveStateRpc(true);

    }

    [Rpc(SendTo.Everyone)]
    private void SetActiveStateRpc(bool isActive) 
    {
        gameObject.SetActive(isActive);
    }
    void MovePlayer()
    { 
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed); 
        if(targetPosition!= transform.position)
        {
            Vector3 targetDirection = targetPosition - transform.position; 
            transform.up = targetDirection;
        }
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _moving = false;
        }
    }

    public void OnMovement(InputAction.CallbackContext context) 
    {
        if (!IsOwner) return;
        Vector3 position = Mouse.current.position.ReadValue();
        targetPosition = _camera.ScreenToWorldPoint(position);
        targetPosition.z = transform.position.z;
        _moving = true;
    }


    
    public void OnHide(InputAction.CallbackContext context) //Se activa al hacer click derecho cuando estás encima de un prop
    {

        if (!IsOwner || _hiding) return;
        Debug.Log(Vector3.Distance(transform.position, pBehaviour.transform.position));
        if (Vector3.Distance(transform.position, pBehaviour.transform.position) < 11f) //¿Por qué ahora la distancia es tanta?
        {
            
            OnHideRpc(pBehaviour.spriteNumber, pBehaviour.NetworkObjectId, pBehaviour.timeHiding);
            pBehaviour = null;
        }
    }

    [Rpc(SendTo.Server)]
    private void OnHideRpc(int spriteN, ulong NID, float time)
    {
        _hiding = true;
        ChangeSpriteRpc(spriteN, NID);
        StartCoroutine(HideCoroutine(time, NID)); 

    }

    [Rpc(SendTo.Everyone)]
    private void ChangeSpriteRpc(int spriteNumber, ulong NID)
    {
        GetComponent<SpriteRenderer>().sprite = allSprites[spriteNumber]; 
        var hideGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[NID].gameObject;
        hideGO.gameObject.SetActive(!hideGO.gameObject.activeSelf);

    }
    private IEnumerator HideCoroutine(float time, ulong NID)
    {
        yield return new WaitForSeconds(time);
        _hiding = false;
        ChangeSpriteRpc(0, NID);
       
    }

    public void OnAttack()
    {
        if (!IsOwner) return;
        spellServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void spellServerRpc()
    {
        GameObject spell = Instantiate(spellPrefab, spellTransform.position, spellTransform.rotation);
        spell.GetComponent<NetworkObject>().Spawn();
        spell.GetComponent<MoveSpell>().caster = this;
    }

    public ulong GetTeamTower()
    {
        return teamTower.NetworkObjectId;
    }

    public void SetSpawnPositionValue(Vector3 position)
    {
        SetSpawnPositionValueRpc(position);
    }

    [Rpc(SendTo.Server)]

    private void SetSpawnPositionValueRpc(Vector3 position)
    {
        _spawnPosition = position;
    }

    public void SetTeamAssing(int team)
    {
        SetTeamAssingRpc(team);
    }

    [Rpc(SendTo.Server)]

    private void SetTeamAssingRpc (int team)
    {
        teamAssign = team;
    }
    public void SetUltiValue(int value)
    {
        SetUltiValueRpc(value);
    }

    [Rpc(SendTo.Server)]

    private void SetUltiValueRpc(int value)
    {
        if (value == 0)
            ultiAttack.Value = value;
        else if (ultiAttack.Value == 15) ultiAttack.Value = 15; //Cambiar luego 15 por maxValue 
        else ultiAttack.Value += value;
    }

    private void interactableButton(int oldValue, int newValue)
    {
        if(newValue == 0) _ultimateAttack.interactable = false;
        if(newValue == 15) _ultimateAttack.interactable = true;
    }
    public void OnClickButtonTest()
    {
         SetUltiValue(0);
        

    }
}