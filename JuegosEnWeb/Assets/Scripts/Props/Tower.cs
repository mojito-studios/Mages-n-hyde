using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Tower : NetworkBehaviour
{
    private const float maxShield = 5; //Escudo total
    private const float maxLife = 10; //Vida total 
    private const int shootingTime = 7;
    public NetworkVariable<float> actualLife = new NetworkVariable<float>();
    public NetworkVariable<float> shield = new NetworkVariable<float>(); //PowerUp de escudo
    private bool _isDefending = false;
    [SerializeField] private GameObject turrets; //Objeto torretas que disparan

    void Start()
    {
        actualLife.Value = maxLife;
        shield.Value = 0;
        turrets.SetActive(false);
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player") || !collision.collider.CompareTag("Minions")) return; //De momento le pongo player para que vaya yendo
        Debug.Log("Colision aceptada");
       // if (_isDefending) DamageShields();
       // DamageTower();
    }
    public void DamageTower()
    {
        Debug.Log("Hiriendo torre");

    }

    public void DamageShields()
    {
        Debug.Log("HiriendoEscudo");
        int damage = 0; // Daño que haga la colisión, se le pasa desde fuera
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
        if (actualLife.Value == maxLife) return; //Si está curada no hace nada
        actualLife.Value += Random.Range(2, 6); //Se le suma un número random entre 2 y 5
        if(actualLife.Value > maxLife) actualLife.Value = maxLife; //Si se pasa de la vida máxima queda con la vida máxima
        Debug.Log("Curando, ahora tengo un " + actualLife.Value + " de vida");
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


}
