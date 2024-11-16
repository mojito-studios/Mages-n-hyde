using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable 
{
    public ulong ClientId; 
    public int prefabId;
    public int team; //valor de nombre del jugador

    public bool Equals(PlayerData other) //compara si un objeto es igual a otro, se añade para poder implementar la interfaz
    {
        return ClientId == other.ClientId && prefabId == other.prefabId && team == other.team;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter //se serializan sus atributos
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref prefabId);
        serializer.SerializeValue(ref team);
    }
}