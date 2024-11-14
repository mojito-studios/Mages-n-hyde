using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tower : NetworkBehaviour
{
    private const float maxShield = 5; //Escudo total
    private const float maxLife = 10; //Vida total 
    private const int shootingTime = 2;
    private const float minionsDamage = 0.5f;
    private const float minionsTime = 1.5f;
    [SerializeField] private Slider healthBar;
    public NetworkVariable<float> currentLife = new NetworkVariable<float>();
    public NetworkVariable<float> shield = new NetworkVariable<float>(); //PowerUp de escudo
    private NetworkVariable<bool> _isDefending = new NetworkVariable<bool>(false);
    [SerializeField] private GameObject minions;
    [SerializeField] private GameObject arrows;


    void Start()
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
        
    }

  
    public void DamageTower()
    {
        int damageTower = 1; //daño que haga la colisión, se saca desde fuera
        Debug.Log("HiriendoTorrre");
        DamageTowerRpc(damageTower);
        Debug.Log("Mi vida es de " + currentLife.Value);
    }

    public bool GetIsDefending()
    {
        return _isDefending.Value;
    }
    [Rpc(SendTo.Server)]
    private void DamageTowerRpc(int damage)
    {
        currentLife.Value -= damage;
        healthBar.value = currentLife.Value;
        if (currentLife.Value <= 0)
        {
         GameManager.Instance.EndGame(this.tag);
        }
    }
    public void DamageShields()
    {
        Debug.Log("HiriendoEscudo");
        float damage = 0.5f; // Daño que haga la colisión, se le pasa desde fuera
        DamageShieldRpc(damage, maxShield);
        
    }

    [Rpc(SendTo.Server)]
    private void DamageShieldRpc(float damage, float maxShield)
    {
        shield.Value -= damage;
        if (shield.Value <= 0)
        {
            _isDefending.Value = false;
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
        if (currentLife.Value == maxLife) return; //Si está curada no hace nada
        currentLife.Value += Random.Range(2, 6); //Se le suma un número random entre 2 y 5
        if (currentLife.Value > maxLife) currentLife.Value = maxLife; //Si se pasa de la vida máxima queda con la vida máxima

    }

    public void SetDefending(bool defending)
    {
        SetDefendingRpc(defending);
    }

    [Rpc(SendTo.Server)]
    private void SetDefendingRpc(bool defending)
    {
        _isDefending.Value = defending;

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
            GameObject arrow = Instantiate(arrows, new Vector3(0,0), new Quaternion(0,0,180, 0));
            arrow.GetComponent<NetworkObject>().Spawn();
            arrow.GetComponent<Arrow>().casterTower = this;
            yield return new WaitForSeconds(shootingTime);
        }
        

    }

    public void SpawnMinions()
    {
        Tower targetTower = FindOtherTower();
        ulong targetId = targetTower.NetworkObjectId;
        SpawnMinionsRpc(targetTower.transform.position, minions.transform.position, targetId);
       
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
        StartCoroutine(MinionsAct(enemytower, minions.transform.position, t));
    }
    private IEnumerator MinionsAct(Vector3 enemyTower, Vector3 originalPosition, ulong t)
    {
        while(Vector3.Distance(minions.transform.position, enemyTower) > 1f)
        {
            minions.transform.position = Vector3.MoveTowards(minions.transform.position, enemyTower, Time.deltaTime);
            yield return null;
        }

       StartCoroutine(AttackingMinions(t, originalPosition));

    }

    private IEnumerator AttackingMinions(ulong t, Vector3 originalPosition)
    {
        var tower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[t].GetComponent<Tower>();

        var i = 0;
        while (i < minionsTime)
        {
            Debug.Log("ATACO");
            if (tower._isDefending.Value) DamageShields();
            else DamageTower();
            i++;
            yield return new WaitForSeconds(1f);

        }


        minions.transform.position = originalPosition;
    }


}
