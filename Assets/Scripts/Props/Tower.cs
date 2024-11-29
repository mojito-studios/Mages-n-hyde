using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tower : NetworkBehaviour
{
    private const float maxShield = 5; //Escudo total
    private const float maxLife = 800; //Vida total 
    private const int shootingTime = 2;
    private const float minionsDamage = 0.5f;
    private const float minionsTime = 1.5f;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider shieldBar;
    public NetworkVariable<float> currentLife = new NetworkVariable<float>();
    public NetworkVariable<float> shield = new NetworkVariable<float>(); //PowerUp de escudo
    public NetworkVariable<bool> _isDefending { get; private set; } = new NetworkVariable<bool>(false);
    [SerializeField] private GameObject minions;
    [SerializeField] private GameObject arrows;
    Animator minionsAnim;
    public Player caster;
    private Vector3 minionsOgPos;

    
    void Start()
    {
        minionsOgPos = minions.transform.position;
        minionsAnim = minions.GetComponent<Animator>();
        
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
        //Aqu� se le a�aden las actualizaciones del resto de cosas de la ui so tiene para el escudo etc si no pues no
    }

    public void DamageTower(float damageTower)
    {
        //int damageTower = 1; //da�o que haga la colisi�n, se saca desde fuera
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
        currentLife.Value -= damage*5;
    }
    public void DamageShields()
    {
        Debug.Log("HiriendoEscudo");
        float damage = 0.5f; // Da�o que haga la colisi�n, se le pasa desde fuera
        DamageShieldRpc(damage, maxShield);
        
    }

    [Rpc(SendTo.Server)]
    private void DamageShieldRpc(float damage, float maxShield)
    {
        shield.Value -= damage;
        if (shield.Value <= 0)
        {
            SetDefending(false);
            shield.Value = maxShield;
        }
    }
    
    public void HealTower()
    {
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
        if (defending) shieldBar.gameObject.SetActive(true); else shieldBar.gameObject.SetActive(false);
    }

    public void ArrowRain()
    {
        Debug.Log("Activando flechas");
        int arrowNumber = Random.Range(4, 6);
        ArrowRainRpc(arrowNumber, shootingTime);
    }
    [Rpc(SendTo.Server)]
    private void ArrowRainRpc(int number, int puta)
    {
        StartCoroutine(FallingObjects(number,  shootingTime));
    }


    private IEnumerator FallingObjects(int number, int shootingTime)
    {
        for(int i = 0; i < number; i++)
        {
            GameObject arrow = Instantiate(arrows, new Vector3(0,0), new Quaternion(0,0,90, 0));
            arrow.GetComponent<NetworkObject>().Spawn();
            arrow.GetComponent<Arrow>().casterTower = this;
            arrow.GetComponent<Arrow>().caster = caster;
            yield return new WaitForSeconds(shootingTime);
        }
        

    }

    public void SpawnMinions()
    {
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
        while(Vector3.Distance(minions.transform.position, enemyTower) > 1f)
        {
            minions.transform.position = Vector3.MoveTowards(minions.transform.position, enemyTower, Time.deltaTime);
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
            Debug.Log("ATACO");
            if (tower._isDefending.Value) tower.DamageShields();
            else tower.DamageTower(minionsDamage);
            i++;
            yield return new WaitForSeconds(1f);

        }
        AttackingMinionsRpc(false);
        minions.transform.position = originalPosition;

    }

    [Rpc(SendTo.Everyone)]
    void WalkingMinionsRpc(bool walk)
    {
        minionsAnim.SetBool("IsWalking", walk);
    }

    [Rpc(SendTo.Everyone)]
    void AttackingMinionsRpc(bool attack)
    {
        minionsAnim.SetBool("IsAttacking", attack);
    }

   

}
