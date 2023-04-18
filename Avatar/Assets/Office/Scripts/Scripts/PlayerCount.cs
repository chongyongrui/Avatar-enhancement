using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCount : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerCount;
      private NetworkVariable<int> playerNum = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);
      private void Start(){
        if(!IsClient){
          playerCount.gameObject.SetActive(false);
        }
      }
      private void Update(){
        playerCount.text = "Players: "+ playerNum.Value.ToString();
        if(!IsOwner)return;
        playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        
    }
   
}
