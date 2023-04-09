using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;

public struct NetString :  INetworkSerializeByMemcpy
{
    private ForceNetworkSerializeByMemcpy<FixedString32Bytes> _info;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _info);
       
    }
 
    public override string ToString()
    {
        return _info.Value.ToString();
    }
 
    public static implicit operator string(NetString s) => s.ToString();
    public static implicit operator NetString(string s) => new NetString() { _info = new FixedString32Bytes(s) };
}
