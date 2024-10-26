using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Tower : NetworkBehaviour
{
    private const float maxShield = 5; //Escudo total
    private const float maxLife = 10; //Vida total 
    private const int shootingTime = 7;
    private const int minionsAttack = 5;
    public NetworkVariable<float> actualLife = new NetworkVariable<float>();
    public NetworkVariable<float> shield = new NetworkVariable<float>(); //PowerUp de escudo
    private bool _isDefending = false;
    [SerializeField] private GameObject turrets; //Objeto torretas que disparan

    void Start()
    {
        
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            actualLife.Value = maxLife;
            shield.Value = maxShield;
        }
        turrets.SetActive(false);
    }
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Minions")) return; //De momento le pongo player para que vaya yendo
        //if (collision.gameObject.GetComponent<Player>().GetTeamTower().tag == this.tag) return; //No puede atacar su propia torre. Lo desactivo para probar los powerups bien pero con esta línea de código va
        //Debug.Log("Colision aceptada");
        if (_isDefending) DamageShields();
        else DamageTower();
    }
    public void DamageTower()
    {
        int damageTower = 1; //daño que haga la colisión, se saca desde fuera
        Debug.Log("HiriendoTorrre");
        DamageTowerRpc(damageTower);
        Debug.Log("Mi vida es de " + actualLife.Value);
    }

    [Rpc(SendTo.Server)]
    private void DamageTowerRpc(int damage)
    {
        actualLife.Value -= damage;
        if (actualLife.Value <= 0)
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
            _isDefending = false;
            shield.Value = maxShield;
        }
    }
    
    public void HealTower()
    {
        Debug.Log("Curando Torre");
        HealTowerRpc(maxLife);
        Debug.Log("Mi vida es de " + actualLife.Value);

    }

    [Rpc(SendTo.Server)]
    private void HealTowerRpc(float maxLife)
    {
        if (actualLife.Value == maxLife) return; //Si está curada no hace nada
        actualLife.Value += Random.Range(2, 6); //Se le suma un número random entre 2 y 5
        if (actualLife.Value > maxLife) actualLife.Value = maxLife; //Si se pasa de la vida máxima queda con la vida máxima

    }

    public void SetDefending(bool defending)
    {
        _isDefending = defending;
    }

    public void ActivateTurrets()
    {
        Debug.Log("Activando torretas");
        StartCoroutine(ShootTurrets());
    }

    private IEnumerator ShootTurrets()
    {
        turrets.SetActive(true);
        //Lógica de disparo de las torretas
        yield return new WaitForSeconds(shootingTime);
        turrets.SetActive(false);

    }

    public void MinionsAttack()
    {
        StartCoroutine(MinionsAttackCoroutine());
    }

    private IEnumerator MinionsAttackCoroutine()
    {
        yield return new WaitForSeconds(minionsAttack);
    }


}
