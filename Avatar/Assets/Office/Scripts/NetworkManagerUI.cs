using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button ServerButton;
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;
    [SerializeField] private TextMeshProUGUI playerCount;
    private bool isServerStarted =false;
    // ServerButton.onClick.AddListener(() =>
    //     {
    //         if (!isServerStarted)
    //         {
    //             NetworkManager.Singleton.StartServer();
    //             isServerStarted = true;
    //         }
    //         else
    //         {
    //             NetworkManager.Singleton.StopAllCoroutines();
    //             isServerStarted = false;
    //         }
    //     });
    private NetworkVariable<int> playerNum = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);
    // Start is called before the first frame update
    private void Awake(){
    ServerButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartServer();
            
    });
    ClientButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartClient();
    });
    HostButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartHost();
    });
    }
    private void Update(){
        playerCount.text = "Players: "+playerNum.Value.ToString();
        if(!IsOwner)return;
        playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        
    }
}
