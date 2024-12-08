    using System.Collections;
    using System.Collections.Generic;
    using Unity.Netcode;
    using UnityEngine;

    public class PowerUpBehaviour : NetworkBehaviour
    {
        private const int MAX_TIME_PLAYER = 5;
        private bool _isTriggered = false;
        private float _currentTime = 0f;
        private int _ultimateValue = 15; //Luego ajustarde torre 4 curar torre
        [SerializeField] private GameObject _puEffectPrefab;
        private NetworkVariable<int> _puType = new NetworkVariable<int>();
        Player player;
        private Animator _animator;
        private SpriteRenderer _sp;
        public void Start()
        {
            _animator = GetComponent<Animator>();
            _sp = GetComponent<SpriteRenderer>();
            GetPlayers();
            AnimateEnterRpc();



        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Props")) DestroyPU();
            if (!collision.CompareTag("Player")) return;
            player = collision.GetComponent<Player>();
            if (player.PUValue.Value != _puType.Value) _isTriggered = true;
            else DestroyPU();


        }

        private void OnTriggerExit2D(Collider2D collision) //Tanto si se sale como si el jugador se desactiva (por el tema del respawn)
        {
            _isTriggered = false;

        }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {

             _puType.Value = Random.Range(1, 5);
            //_puType.Value = 2; //Para probar los minions


        }


    }

    [Rpc(SendTo.Everyone)]

        void AnimateEnterRpc()
        {

            switch (_puType.Value)
            {
                case 1:
                    _animator.SetBool("isMinionA", true);
                   
                    break;
                case 2:
                    _animator.SetBool("isArrowsA", true);
                    break;
                case 3:
                    _animator.SetBool("isShieldA", true);
                    break;
                case 4:
                    _animator.SetBool("isLifeA", true);
                    break;
            }
        }

        [Rpc(SendTo.Everyone)]
        void AnimateExitRpc()
        {
        // Cambiar el estado de la animación según el tipo de power-up
        switch (_puType.Value)
        {
            case 1:
                _animator.SetBool("isMinionA", false);
                break;
            case 2:
                _animator.SetBool("isArrowsA", false);
                break;
            case 3:
                _animator.SetBool("isShieldA", false);
                break;
            case 4:
                _animator.SetBool("isLifeA", false);
                break;
        }


    }


    private IEnumerator WaitToDestroy()
    {
        

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            while (stateInfo.normalizedTime > 0f)
            {
                yield return null; 
                stateInfo = _animator.GetCurrentAnimatorStateInfo(0);  
            }

        DestroyPU();

        }



    private void GetPlayers()
        {
            foreach (var player in FindObjectsOfType<Player>())
            {
                if (player.PUValue.Value == _puType.Value)
                {
                    ChangeColorRpc(player.OwnerClientId, true);
                }
            }
        }

    public void ChangeColorUpdate(int newValue, ulong player)
    {
        if(newValue == this._puType.Value)
        {
            ChangeColorRpc(player, true);
        } else ChangeColorRpc(player, false);
        

    }
        [Rpc(SendTo.Everyone)]

        private void ChangeColorRpc(ulong clientId, bool gray)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
                return;

           if(gray) _sp.color = Color.gray;
           else _sp.color = Color.white;
        }

     

        [Rpc(SendTo.Server)]
        public void SetTypeRpc(int value)
        {
            _puType.Value = value;
        }
        public void PULogic()
        {
            ExecutePowerUp();
            AnimateExitRpc();
        //StartCoroutine(WaitToDestroy());
        EffectRpc(player.NetworkObjectId);
        DestroyPU();



    }


    [Rpc(SendTo.Server)]
    private void EffectRpc(ulong clientid)
    {
        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[clientid].transform;

        var eff = Instantiate(_puEffectPrefab, obj.position, Quaternion.identity);
        eff.GetComponent<NetworkObject>().Spawn();
    }
    private void DestroyPU()
        {
            NetworkObject.Despawn();
            GameManager.Instance.puInScene--;
        }

        // Update is called once per frame
        void Update()
        {
    
        }

        void FixedUpdate()
        {
            if (_isTriggered)
            {
                _currentTime += Time.fixedDeltaTime;

                if (_currentTime >= MAX_TIME_PLAYER)
                {
                    PULogic();
                    _currentTime = 0f;
                }
            }
            else
            {
                _currentTime = 0f;
            }
        }

     

    
            public void ExecutePowerUp() //Función que según el powerUp hace una cosa u otra;
            {

            var tower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[player.GetTeamTower()].GetComponent<Tower>();
            player.SetPlayerPURpc(_puType.Value);


            switch (_puType.Value)
            {
                default:
                    break;
                case 1:  
                    tower.SpawnMinions(player);
                    break;
                case 2:
                    tower.caster = player;
                    tower.ArrowRain();
                    break;
                case 3:
                    tower.SetDefending(true, player);
                    break;
                case 4:
                    tower.HealTower(player);
                    break;
        

            }
            player.SetUltiValue(_ultimateValue);

        }

   
    }
