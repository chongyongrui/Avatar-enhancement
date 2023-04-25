using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Netcode;
public class testingNetworkManager : NetworkBehaviour
{  [SerializeField]private GameObject holder;
     public void Client(){
        NetworkManager.Singleton.StartClient();
        holder.SetActive(false);
     }
     public void Host(){
        NetworkManager.Singleton.StartHost();
        holder.SetActive(false);
     }
}
