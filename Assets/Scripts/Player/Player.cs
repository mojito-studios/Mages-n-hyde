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
    private const float maxLife = 100;
    public float attack = 1;
    public NetworkVariable<float> health { get; private set; } = new NetworkVariable<float> ();
    private NetworkVariable<int> killCount = new NetworkVariable<int>();
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
    private NetworkVariable<bool> _hiding = new NetworkVariable<bool>();
    Sprite _sprite;
    private NetworkVariable<int> spriteIndex = new NetworkVariable<int>();
    [HideInInspector] NetworkVariable<ulong> _pBehaviour = new NetworkVariable<ulong>();

    //attack
    private Button _spell;
    private Button _ultimateAttack;
    private Vector3 _spawnPosition;
    private int _respawnTime = 5;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellTransform;

    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>().sprite;

    }
    void Start()
    {
        if (!IsOwner) return;
        _camera = GetComponentInChildren<Camera>();
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
        if (IsServer)
        {
            health.Value = maxLife;
            _hiding.Value = false;
            spriteIndex.Value = GameManager.Instance.GetPrefabIndex(_sprite);

        }

        SetPlayer();
        AssignTower();
        ultiAttack.OnValueChanged += interactableButton;
        _hiding.OnValueChanged = ChangeSprite;

    }

   

    public void SetPBehaviour(ulong pBehaviourID)
    {
        SetPBehaviourRpc(pBehaviourID);
    }

    [Rpc(SendTo.Server)]

    public void SetPBehaviourRpc(ulong pBehaviourID)
    {
        _pBehaviour.Value = pBehaviourID;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        SetMovingRpc(false);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        SetMovingRpc(false);
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

    public void getHit(float damage)
    {
        getHitRpc(damage);
    }

    [Rpc(SendTo.Everyone)]
    private void getHitRpc(float damage)
    {
        health.Value -= damage*10;
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

    public void kill()
    {
        killCount.Value++;
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

    [Rpc(SendTo.Server)]
    private void SetMovingRpc(bool isMoving)
    {
        _moving = isMoving;
    }

    [Rpc(SendTo.Server)]
    private void OnHideRpc(bool hide)
    {
        _hiding.Value = hide;
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

        if (!IsOwner || _hiding.Value) return;

        Debug.Log(_pBehaviour.Value);
        PropsBehaviour pBehaviour = NetworkManager.Singleton.SpawnManager.SpawnedObjects[this._pBehaviour.Value].GetComponent<PropsBehaviour>();
        if (Vector3.Distance(transform.position, pBehaviour.transform.position) < 11f) //¿Por qué ahora la distancia es tanta?
        {

            OnHideRpc(true);
            
        }
    }

    void ChangeSprite(bool oldValue, bool newValue)
    {
        if(newValue)
        {
            PropsBehaviour pBehaviourHide = NetworkManager.Singleton.SpawnManager.SpawnedObjects[this._pBehaviour.Value].GetComponent<PropsBehaviour>();
            Sprite spriteToChange = GameManager.Instance.props[pBehaviourHide.spriteNumber].GetComponent<SpriteRenderer>().sprite;
            GetComponent<SpriteRenderer>().sprite = spriteToChange;
            var hideGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[this._pBehaviour.Value].gameObject;
            hideGO.gameObject.SetActive(!hideGO.gameObject.activeSelf);
            StartCoroutine(HideCoroutine(pBehaviourHide.timeHiding));

        }
        else
        {
            Sprite oldSprite = GameManager.Instance.prefabs[spriteIndex.Value].GetComponent<SpriteRenderer>().sprite;
            GetComponent<SpriteRenderer>().sprite = oldSprite;
            var hideGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_pBehaviour.Value].gameObject;
            hideGO.gameObject.SetActive(!hideGO.gameObject.activeSelf);
            SetPBehaviour(0);
        }
    }
    private IEnumerator HideCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        OnHideRpc(false);
       
       
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