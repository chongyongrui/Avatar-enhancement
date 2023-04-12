using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

public struct NetString :  INetworkSerializable
{
    private FixedString32Bytes info;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
       
    }
 
    public override string ToString()
    {
        return info.Value.ToString();
    }
 
    public static implicit operator string(NetString s) => s.ToString();
    public static implicit operator NetString(string s) => new NetString() { info = new FixedString32Bytes(s) };
}
