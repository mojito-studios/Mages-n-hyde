using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
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
    [SerializeField] public string character = "Player";
    private const int MAX_ULTI_VALUE = 45;
    private Camera _camera;
    public int teamAssign;
    public Tower teamTower;
    public float maxLife = 100;
    public float attack = 1;
    public float range = 1;
    public float ultiTime;
   public float ultidamage = 3;
    public NetworkVariable<float> health { get; private set; } = new NetworkVariable<float>();
    public NetworkVariable<int> killCount { get; private set; } = new NetworkVariable<int>(0);
    public NetworkVariable<int> deathCount { get; private set; } = new NetworkVariable<int>(0);
    public NetworkVariable<int> assistCount { get; private set; } = new NetworkVariable<int>(0);
    public NetworkVariable<bool> inmune { get; private set; } = new NetworkVariable<bool>(false);
    private List<Player> assistant = new List<Player>(0);
    [SerializeField] private AnimationController anim;
    public NetworkVariable<int> PUValue { get; private set; } = new NetworkVariable<int>(0);

    //gameover
    public NetworkVariable<FixedString128Bytes> winningTeam = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<bool> win = new NetworkVariable<bool>();

    //UI
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider towerHealth;
    [SerializeField] private Slider towerShield;
    [SerializeField] private TextMeshProUGUI _towerHealth;
    [SerializeField] private Canvas GameOver;
    [SerializeField]private CinemachineConfiner2D camerabounds;

    [SerializeField] private GameObject healthEffect;
    [SerializeField] private GameObject shieldEffect;

    private Animator healthAnimator;
    private Animator shieldAnimator;

    //move
    [SerializeField] private float speed = 4f;
    private bool _moving = false;
    public bool canMove = true;
    public bool button = false;
    private Vector3 targetPosition = Vector3.zero;
    private float stepTime = 0.3f;
    private float stepLeft = 0.3f;

    //hide
    private NetworkVariable<bool> _hiding = new NetworkVariable<bool>();
    Sprite _sprite;
    private NetworkVariable<int> spriteIndex = new NetworkVariable<int>();
    [HideInInspector] NetworkVariable<ulong> _pBehaviour = new NetworkVariable<ulong>();

    //attack
    public int maxSpells = 10;
    [SerializeField] private Button spellButton;
    public NetworkVariable<int> spellCount { get; private set; } = new NetworkVariable<int>();
    [SerializeField] private Button _ultimateAttack;
    private NetworkVariable<int> ultiAttack = new NetworkVariable<int>();
    private Vector3 _spawnPosition;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private GameObject dustPrefab;
    [SerializeField] private GameObject smokePrefab;
    [SerializeField] private GameObject ultiPrefab;
    [SerializeField] private Transform spellTransform;
    [SerializeField] private Transform[] ultiTransforms;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>().sprite;
        healthAnimator = healthEffect.GetComponent<Animator>();
        shieldAnimator = shieldEffect.GetComponent<Animator>();
    }
    void Start()
    {
        if (!IsOwner) return;

        _camera = GetComponentInChildren<Camera>();
        _ultimateAttack.interactable = false;
        anim = GetComponentInChildren<AnimationController>();
        teamAssignRpc();
    }

    [Rpc(SendTo.Server)]
    private void teamAssignRpc()
    {
        if (teamTower.tag == "Team2Tower") teamAssign2Rpc();
        teamTower._isDefending.OnValueChanged += SetShieldRpc;
    }

    [Rpc(SendTo.Owner)]
    private void teamAssign2Rpc()
    {
        topRight(towerHealth.gameObject);
        topRight(_towerHealth.gameObject);
        topRight(towerShield.gameObject);
        towerHealth.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-60, -116f, 0f);
        _towerHealth.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-531, -43f, 0f);
        towerShield.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-40, -247f, 0f);
    }

    void topRight(GameObject uiObject)
    {
        RectTransform uitransform = uiObject.GetComponent<RectTransform>();
        uitransform.anchorMin = new Vector2(1, 1);
        uitransform.anchorMax = new Vector2(1, 1);
        uitransform.pivot = new Vector2(1, 1);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            health.Value = maxLife;
            spellCount.Value = maxSpells;
            _hiding.Value = false;
            spriteIndex.Value = GameManager.Instance.GetPrefabIndex(_sprite);

        }
        SetPlayer();
        ultiAttack.OnValueChanged += interactableButton;
        _hiding.OnValueChanged += ChangeSprite;
        PUValue.OnValueChanged += NotifyPowerUps;
        camerabounds.m_BoundingShape2D = GameObject.FindWithTag("CameraBounds").GetComponent<Collider2D>();
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
                    anim.isWalkingRpc(_moving);
                }
            }
        }
        
        if (_moving)
            MovePlayer();
        stepLeft += Time.deltaTime;
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

    void NotifyPowerUps(int oldValue, int newValue)
    {
        var powerUPS = GameObject.FindObjectsByType<PowerUpBehaviour>(FindObjectsSortMode.None);
        foreach(PowerUpBehaviour powerup in powerUPS)
        {
            if(powerup != null)
            powerup.ChangeColorUpdate(newValue, this.OwnerClientId);
        }


    }
    public void getHit(float damage)
    {
        getHitRpc(damage);
    }

    [Rpc(SendTo.Everyone)]
    private void getHitRpc(float damage)
    {
        health.Value -= damage * 10;
        if (health.Value <= 0)
        {
            health.Value = maxLife;
            RespawnPlayerRpc(_spawnPosition);
        }
    }

    [Rpc(SendTo.Server)]
    private void updateHealthRpc()
    {
        updateHealthBarsRpc(teamTower.currentLife.Value, teamTower.shield.Value);
    }

    [Rpc(SendTo.Everyone)]
    private void updateHealthBarsRpc(float towerLife, float shield)
    {
        healthBar.value = health.Value;
        _towerHealth.text = "TowerHealth: " + towerLife;
        float prev_towerHealth = towerHealth.value;
        towerHealth.value = towerLife;
        towerShield.value = shield;
        if (prev_towerHealth < towerLife)
        {
            HealEffectRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void HealEffectRpc()
    {
        healthEffect.gameObject.SetActive(true);
        healthAnimator.enabled = true;
    }

    [Rpc(SendTo.Everyone)]
    private void SetShieldRpc(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            towerShield.gameObject.SetActive(true);
            shieldEffect.gameObject.SetActive(true);
            shieldAnimator.enabled = true;
        }
        else towerShield.gameObject.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
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
        anim.isWalkingRpc(_moving);
        transform.position = _spawnPosition;
        ReloadRpc();
        SetActiveStateRpc(true);

    }

    [Rpc(SendTo.Server)]

    public void SetPlayerPURpc(int value)
    {
        foreach(Player player in FindObjectsOfType<Player>())
        {
            if(player.teamAssign == this.teamAssign)
            player.PUValue.Value = value;
        }
    }
    public void kill()
    {
        killCount.Value++;
    }

    public void assist()
    {
        assistCount.Value++;
    }

    public void assistantAssign(Player caster)
    {
        if (!assistant.Contains(caster)) assistant.Add(caster);
    }

    public void die(Player caster)
    {
        deathCount.Value++;
        if (assistant.Count > 1)
        {
            assistant.Remove(caster);
            assistant[0].assist();
            assistant.Clear();
        }
    }

   
    [Rpc(SendTo.Everyone)]
    private void SetActiveStateRpc(bool isActive)
    {
        gameObject.SetActive(isActive);
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


    [Rpc(SendTo.Server)]
    private void SetMovingRpc(bool isMoving)
    {
        _moving = isMoving;
    }

    [Rpc(SendTo.Server)]
    private void OnHideRpc(bool hide)
    {
        EffectHidingRpc();
        _hiding.Value = hide;
    }

    [Rpc(SendTo.Owner)]
    private void EffectHidingRpc()
    {
        Instantiate(smokePrefab, transform.position, Quaternion.identity);

    }
    void MovePlayer()
    {
        if (targetPosition != transform.position)
        {
            Vector3 targetDirection = targetPosition - transform.position;
            transform.up = targetDirection;
            anim.AnimateMovementRpc(targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);
            if (stepLeft >= stepTime)
            {
                dustServerRpc();
                stepLeft = 0;
            }
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            _moving = false;
            anim.isWalkingRpc(_moving);
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canMove) return;
        Vector3 position = Mouse.current.position.ReadValue();
        targetPosition = _camera.ScreenToWorldPoint(position);
        targetPosition.z = transform.position.z;
        _moving = true;
        anim.isWalkingRpc(_moving);
    }



    public void OnHide(InputAction.CallbackContext context) //Se activa al hacer click derecho cuando est�s encima de un prop
    {

        if (!IsOwner || _hiding.Value) return;


        PropsBehaviour pBehaviour = NetworkManager.Singleton.SpawnManager.SpawnedObjects[this._pBehaviour.Value].GetComponent<PropsBehaviour>();
        if (Vector3.Distance(transform.position, pBehaviour.transform.position) < 11f) //�Por qu� ahora la distancia es tanta?
        {

            OnHideRpc(true);

        }
    }

    void ChangeSprite(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            GetComponentInChildren<Animator>().enabled = false;
            healthBar.gameObject.SetActive(IsOwner);
            PropsBehaviour pBehaviourHide = NetworkManager.Singleton.SpawnManager.SpawnedObjects[this._pBehaviour.Value].GetComponent<PropsBehaviour>();
            pBehaviourHide.canDespawn = false;
            Sprite spriteToChange = GameManager.Instance.props[pBehaviourHide.spriteNumber].GetComponent<SpriteRenderer>().sprite;
            GetComponentInChildren<SpriteRenderer>().sprite = spriteToChange;
            var hideGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[this._pBehaviour.Value].gameObject;
            hideGO.gameObject.SetActive(!hideGO.gameObject.activeSelf);
            StartCoroutine(HideCoroutine(pBehaviourHide.timeHiding));

        }
        else
        {
            GetComponentInChildren<Animator>().enabled = true;
            healthBar.gameObject.SetActive(true);
            Sprite oldSprite = GameManager.Instance.prefabs[spriteIndex.Value].GetComponentInChildren<SpriteRenderer>().sprite;
            GetComponentInChildren<SpriteRenderer>().sprite = oldSprite;
            var hideGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_pBehaviour.Value].gameObject;
            hideGO.gameObject.SetActive(!hideGO.gameObject.activeSelf);
            hideGO.GetComponent<PropsBehaviour>().canDespawn = true;
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
        if (spellCount.Value <= 0)
        {
            spellButton.interactable = false;
            StartCoroutine(spellCooldown());
        }
    }

    [Rpc(SendTo.Server)]
    private void spellServerRpc()
    {
        GameObject spell = Instantiate(spellPrefab, spellTransform.position, spellTransform.rotation);
        spell.GetComponent<MoveSpell>().caster = this;
        spell.GetComponent<NetworkObject>().Spawn();
        spellCount.Value--;
    }

    [Rpc(SendTo.Owner)]
    private void dustServerRpc()
    {
        GameObject spell = Instantiate(dustPrefab, transform.position, spellTransform.rotation);
    }

    private IEnumerator spellCooldown()
    {
        yield return new WaitForSeconds(10);
        spellLoadSRpc();
        spellButton.interactable = true;
    }
    [Rpc(SendTo.Server)]
    private void spellLoadSRpc()
    {
        spellCount.Value = maxSpells;
    }
    [Rpc(SendTo.Owner)]
    private void spellLoadCRpc()
    {
        spellButton.interactable = true;
    }

    public ulong GetTeamTower()
    {
        return teamTower.GetComponent<NetworkObject>().NetworkObjectId;
    }

    [Rpc(SendTo.Server)]
    private void ReloadRpc()
    {
        health.Value = maxLife;
        spellLoadSRpc();
        spellLoadCRpc();
        _pBehaviour.Value = 0;
        if (_hiding.Value)
        {
            _hiding.Value = false;
        }
        SetUltiValue(0);

    }

    #region ULTI
    public void SetUltiValue(int value)
    {
        SetUltiValueRpc(value, MAX_ULTI_VALUE);
    }

    [Rpc(SendTo.Server)]

    private void SetUltiValueRpc(int value, int maxValue)
    {
        if (value == 0)
            ultiAttack.Value = value;
        else if (ultiAttack.Value == maxValue) ultiAttack.Value = maxValue; //Cambiar luego 15 por maxValue 
        else ultiAttack.Value += value;
    }

    private void interactableButton(int oldValue, int newValue)
    {
        if (newValue == 0) _ultimateAttack.interactable = false;
        if (newValue == MAX_ULTI_VALUE) _ultimateAttack.interactable = true;
    }

    [Rpc(SendTo.Server)]
    public void Ulti1ARpc()
    {
        inmune.Value = true;
        Ulti1ButtonRpc();
        StartCoroutine(Ulti1C(ultiTime));
    }

    [Rpc(SendTo.Owner)]
    public void Ulti1ButtonRpc()
    {
        _ultimateAttack.interactable = false;
        SetUltiValue(0);
    }

    private IEnumerator Ulti1C(float time)
    {
        yield return new WaitForSeconds(time);
        Ulti1BRpc();
        anim.EndUltiRpc();
    }
    [Rpc(SendTo.Server)]
    private void Ulti1BRpc()
    {
        inmune.Value = false;
    }


    //ULTI 2
    private IEnumerator Ulti2C(float time)
    {
        Spells2Rpc(ultidamage);
        yield return new WaitForSeconds(time);
        anim.EndUltiRpc();

    }

    [Rpc(SendTo.Server)]
    private void Spells2Rpc(float damage)
    {
        foreach (Transform transform in ultiTransforms)
        {
            GameObject spell = Instantiate(ultiPrefab, transform.position, Quaternion.identity);
            spell.GetComponent<UltiGrimm>().caster = this;
            spell.GetComponent<NetworkObject>().Spawn();
        }
    }
    public void Ulti2ButtonRpc()
    {
        SetUltiValue(0);
        anim.AnimateUltiRpc();
        StartCoroutine(Ulti2C(ultiTime));

    }

    //ULTI 3

    public void Ulti3Button()
    {
        SetUltiValue(0);
        anim.AnimateUltiRpc();
        StartCoroutine(Ulti3A());

    }

    private IEnumerator Ulti3C(float time, float  originalSpeed)
    {
        speed += speed * 0.5f;
        yield return new WaitForSeconds(time);
        speed = originalSpeed;
        
    }
    public IEnumerator Ulti3A()
    {
        SearchPlayersU3Rpc(ultidamage);
        yield return new WaitForSeconds(ultiTime);
        anim.EndUltiRpc();
        StartCoroutine(Ulti3C(ultiTime, speed));
    }

    [Rpc(SendTo.Server)]
    private void SearchPlayersU3Rpc(float damage)
    {

        Player[] enemyPlayers = GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None).Where(obj => obj.teamAssign != this.teamAssign).ToArray();
        Debug.Log(enemyPlayers.Length);
        Player closestPlayer = null;
        foreach (Player player in enemyPlayers)
        {
            if (closestPlayer == null || Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, closestPlayer.transform.position)) closestPlayer = player;
            else if (Vector3.Distance(transform.position, closestPlayer.transform.position) > Vector3.Distance(transform.position, player.transform.position)) closestPlayer = player;
        }
        Debug.Log(closestPlayer);
        ChangePositionRpc(closestPlayer.transform.position);
        if (!(closestPlayer.health.Value - damage* 10 > 0)) { this.kill(); closestPlayer.die(this); }

        closestPlayer.getHit(damage);
    }
    [Rpc(SendTo.Everyone)]
    void ChangePositionRpc(Vector3 position)
    {
        transform.position = position;
    }

    //ULTI 4

    private IEnumerator Ulti4C(float time)
    {
        Spells4Rpc(ultidamage);
        yield return new WaitForSeconds(time);
        anim.EndUltiRpc();
        yield return new WaitForSeconds(range);
    }

    [Rpc(SendTo.Server)]
    private void Spells4Rpc(float damage)
    {
        foreach (Transform transform in ultiTransforms) {
            GameObject spell = Instantiate(ultiPrefab, transform.position, transform.rotation);
            spell.GetComponent<MoveUlti>().caster = this;
            spell.GetComponent<MoveUlti>().damage = damage;
            spell.GetComponent<NetworkObject>().Spawn();
        }
    }
    public void Ulti4Button()
    {
        SetUltiValue(0);
        anim.AnimateUltiRpc();
        StartCoroutine(Ulti4C(ultiTime));
    }

    #endregion
    public void EndGame()
    {
        bool win = teamTower.tag == winningTeam.Value.ToString();
        EndGameRpc(win);
    }
    [Rpc(SendTo.Everyone)]
    public void EndGameRpc(bool win)
    {
        GetComponent<GameOver>().win = !win;
        GetComponent<GameOver>().OnEnd();
        GameOver.gameObject.SetActive(true);
        
    }
}