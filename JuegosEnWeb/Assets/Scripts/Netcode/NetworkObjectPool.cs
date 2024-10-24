using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

    public class NetworkObjectPool : NetworkBehaviour //Script de NetworkObjectPool propio de unity
    {
        public static NetworkObjectPool Singleton { get; private set; }

        [SerializeField]
        List<PoolConfigObject> PooledPrefabsList;

        HashSet<GameObject> m_Prefabs = new HashSet<GameObject>();

        Dictionary<GameObject, ObjectPool<NetworkObject>> m_PooledObjects = new Dictionary<GameObject, ObjectPool<NetworkObject>>();

         Dictionary<GameObject, int> nonPooledObjects = new Dictionary<GameObject, int>();


    public void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Singleton = this;
            }
        }

        public override void OnNetworkSpawn()
        {
            foreach (var configObject in PooledPrefabsList)
            {
                nonPooledObjects[configObject.Prefab] = 0;
                RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
            }
        }

        public override void OnNetworkDespawn()
        {
            foreach (var prefab in m_Prefabs)
            {
                NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
                m_PooledObjects[prefab].Clear();
            }
            m_PooledObjects.Clear();
            m_Prefabs.Clear();
        }

        public int GetCurrentPrefabCount(GameObject prefab)
        {
            return nonPooledObjects[prefab];

        }

        public void OnValidate()
        {
            for (var i = 0; i < PooledPrefabsList.Count; i++)
            {
                var prefab = PooledPrefabsList[i].Prefab;
                if (prefab != null)
                {
                    Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
                }
            }
        }
        public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var networkObject = m_PooledObjects[prefab].Get();

            nonPooledObjects[prefab]++;

            var noTransform = networkObject.transform;
            noTransform.position = position;
            noTransform.rotation = rotation;

            return networkObject;
        }

        /// <summary>
        /// Return an object to the pool (reset objects before returning).
        /// </summary>
        public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
        {
            m_PooledObjects[prefab].Release(networkObject);
            nonPooledObjects[prefab]--;
    }

        /// <summary>
        /// Builds up the cache for a prefab.
        /// </summary>
        void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
        {
            NetworkObject CreateFunc()
            {
                return Instantiate(prefab).GetComponent<NetworkObject>();
            }

            void ActionOnGet(NetworkObject networkObject)
            {
                networkObject.gameObject.SetActive(true);
            }

            void ActionOnRelease(NetworkObject networkObject)
            {
                networkObject.gameObject.SetActive(false);
            }

            void ActionOnDestroy(NetworkObject networkObject)
            {
                Destroy(networkObject.gameObject);
            }

            m_Prefabs.Add(prefab);

            // Create the pool
            m_PooledObjects[prefab] = new ObjectPool<NetworkObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, defaultCapacity: prewarmCount);

            // Populate the pool
            var prewarmNetworkObjects = new List<NetworkObject>();
            for (var i = 0; i < prewarmCount; i++)
            {
                prewarmNetworkObjects.Add(m_PooledObjects[prefab].Get());
            }
            foreach (var networkObject in prewarmNetworkObjects)
            {
                m_PooledObjects[prefab].Release(networkObject);
            }

            // Register Netcode Spawn handlers
            NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler(prefab, this));
        }
    }

    [Serializable]
    struct PoolConfigObject
    {
        public GameObject Prefab;
        public int PrewarmCount;
    }

    class PooledPrefabInstanceHandler : INetworkPrefabInstanceHandler
    {
        GameObject m_Prefab;
        NetworkObjectPool m_Pool;

        public PooledPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
        {
            m_Prefab = prefab;
            m_Pool = pool;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return m_Pool.GetNetworkObject(m_Prefab, position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
        {
            m_Pool.ReturnNetworkObject(networkObject, m_Prefab);
        }
    }

