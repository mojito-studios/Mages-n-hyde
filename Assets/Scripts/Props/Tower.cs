using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Tower : NetworkBehaviour
{
    private const float maxShield = 100; //Escudo total
    private const float maxLife = 800; //Vida total 
    private const int shootingTime = 2;
    private const float minionsDamage = 50f;
    private const float minionsTime = 1.5f;
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private UnityEngine.UI.Slider shieldBar;
    [SerializeField] private GameObject healthEffect;
    [SerializeField] private GameObject shieldEffect;

    public NetworkVariable<float> currentLife = new NetworkVariable<float>();
    public NetworkVariable<float> shield = new NetworkVariable<float>(); //PowerUp de escudo
    public NetworkVariable<bool> _isDefending { get; private set; } = new NetworkVariable<bool>(false);
    [SerializeField] private GameObject minions;
    [SerializeField] private GameObject arrows;
    Animator minionsAnim;
    public Player caster;
    private Vector3 minionsOgPos;
    private Tilemap tilemap;
    private Bounds bounds;



    void Start()
    {
        minionsOgPos = minions.transform.position;
        minionsAnim = minions.GetComponent<Animator>();
        tilemap = GameObject.Find("Suelo").GetComponent<Tilemap>();
        bounds = tilemap.localBounds;
    }
    
    private void Awake()
    {

    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentLife.Value = maxLife;
            shield.Value = maxShield;
        }
    }

    void Update()
    {
        updateStatsRpc();
        if (currentLife.Value <= 0)
        {
            GameManager.Instance.EndGameRpc(this.tag);
        }
        
    }

    [Rpc(SendTo.Everyone)]
    private void updateStatsRpc()
    {
        healthBar.value = currentLife.Value;
        if (shieldBar != null)
        {
            shieldBar.value = shield.Value;
        }
    }

    public void DamageTower(float damageTower)
    {
        Debug.Log("HiriendoTorrre");
        DamageTowerRpc(damageTower);
        Debug.Log("Mi vida es de " + currentLife.Value);
    }

    public bool GetIsDefending()
    {
        return _isDefending.Value;
    }
    [Rpc(SendTo.Everyone)]
    private void DamageTowerRpc(float damage)
    {
        currentLife.Value -= damage*2;
    }
    public void DamageShields(float damage)
    {
        Debug.Log("HiriendoEscudo");
        ; // Da�o que haga la colisi�n, se le pasa desde fuera
        DamageShieldRpc(damage, maxShield);
        
    }

    [Rpc(SendTo.Server)]
    private void DamageShieldRpc(float damage, float maxShield)
    {
        shield.Value -= damage*5;
        if (shield.Value <= 0)
        {
            SetDefending(false);
            shield.Value = maxShield;
        }
    }
    
    public void HealTower()
    {
        HealEffectRpc();
        Debug.Log("Curando Torre");
        HealTowerRpc(maxLife);
        Debug.Log("Mi vida es de " + currentLife.Value);
       

    }

    [Rpc(SendTo.Server)]
    private void HealTowerRpc(float maxLife)
    {
        if (currentLife.Value == maxLife) return; //Si est� curada no hace nada
        currentLife.Value += Random.Range(2, 6); //Se le suma un n�mero random entre 2 y 5
        if (currentLife.Value > maxLife) currentLife.Value = maxLife; //Si se pasa de la vida m�xima queda con la vida m�xima

    }

    [Rpc(SendTo.Everyone)]
    private void HealEffectRpc()
    {
        healthEffect.gameObject.SetActive(true);
        healthEffect.GetComponent<Animator>().enabled = true;
    }

    public void SetDefending(bool defending)
    {
        SetDefendingRpc(defending);
        
    }

    [Rpc(SendTo.Server)]
    private void SetDefendingRpc(bool defending)
    {
        _isDefending.Value = defending;
        SetShieldRpc(defending);
    }

    [Rpc(SendTo.Everyone)]
    private void SetShieldRpc(bool defending)
    {
        if (defending) 
        {
            shieldBar.gameObject.SetActive(true);
            shieldEffect.gameObject.SetActive(true);
            shieldEffect.GetComponent<Animator>().enabled = true;
        }
        else
        {
            shieldBar.gameObject.SetActive(false);
            shieldEffect.GetComponent<Animator>().enabled = true;
            shieldEffect.gameObject.SetActive(false);
        }
    }

    public void ArrowRain()
    {
        Debug.Log("Activando flechas");
        int arrowNumber = Random.Range(15, 20);
        ArrowRainRpc(arrowNumber, shootingTime);
    }
    [Rpc(SendTo.Server)]
    private void ArrowRainRpc(int number, int shootingTime)
    {
        StartCoroutine(FallingObjects(number,  shootingTime));
    }


    private IEnumerator FallingObjects(int number, int shootingTime)
    {
        for(int i = 0; i < number; i++)
        {
            Debug.Log(bounds.min.y);
            GameObject arrow = Instantiate(arrows, new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.max.y, 0), arrows.transform.rotation);
            arrow.GetComponent<NetworkObject>().Spawn();
            arrow.GetComponent<Arrow>().bounds = bounds.min.y;
            arrow.GetComponent<Arrow>().casterTower = this;
            arrow.GetComponent<Arrow>().caster = caster;
            yield return new WaitForSeconds(shootingTime);
        }
        if(caster != null)
        {
            caster.SetPlayerPURpc(0);
            caster = null;
        }
        
        

    }

    public void SpawnMinions(Player player)
    {
        caster = player;
        Tower targetTower = FindOtherTower();
        ulong targetId = targetTower.NetworkObjectId;
        SpawnMinionsRpc(targetTower.transform.position, minionsOgPos, targetId);
       
    }

   
    private Tower FindOtherTower()
    {
        Tower[] allTowers =GameObject.FindObjectsOfType<Tower>();

        foreach(var tower in allTowers)
        {
            if(tower.tag != gameObject.tag) return tower;
        }
        return null;
    }
    [Rpc(SendTo.Everyone)]
    private void SpawnMinionsRpc(Vector3 enemytower, Vector3 originalPosition, ulong t)
    {
        StartCoroutine(MinionsAct(enemytower, originalPosition, t));
    }
    private IEnumerator MinionsAct(Vector3 enemyTower, Vector3 originalPosition, ulong t)
    {
        WalkingMinionsRpc(true);
        yield return new WaitForSeconds(1f);
        while(Vector3.Distance(minions.transform.position, enemyTower) > 2f)
        {
            minions.transform.position = Vector3.MoveTowards(minions.transform.position, enemyTower, Time.deltaTime*3);            
            yield return null;
        }
        WalkingMinionsRpc(false);
        AttackingMinionsRpc(true);
        StartCoroutine(AttackingMinions(t, originalPosition));
        


    }

    private IEnumerator AttackingMinions(ulong t, Vector3 originalPosition)
    {
        
        var tower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[t].GetComponent<Tower>();

        var i = 0;
        while (i < minionsTime)
        {
            if (tower._isDefending.Value) tower.DamageShields(minionsDamage);
            else tower.DamageTower(minionsDamage);
            i++;
            yield return new WaitForSeconds(1f);

        }
        AttackingMinionsRpc(false);
        minions.transform.position = originalPosition;
        if (caster != null)
        {
            caster.SetPlayerPURpc(0);
            caster = null;
        }

    }

    [Rpc(SendTo.Everyone)]
    void WalkingMinionsRpc(bool walk)
    {
        minionsAnim.SetBool("isActive", walk);
    }

    [Rpc(SendTo.Everyone)]
    void AttackingMinionsRpc(bool attack)
    {
        minionsAnim.SetBool("IsAttacking", attack);
    }

   

}
