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
    [SerializeField] private TMP_Text playerNameText;
    private NetString playerName;
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
        Debug.Log(PlayerPrefs.GetString("PlayerName"));
        
        
    ServerButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartServer();
            
    });
    ClientButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartClient();
       HostButton.gameObject.SetActive(false);
    });
    HostButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartHost();
        ClientButton.gameObject.SetActive(false);
        
    });
    }
    private void setName(){
        playerNameText.text = playerName.username.Value.ToString();

    
    }
    private void Update(){
        playerCount.text = "Players: "+ playerNum.Value.ToString();
        if(!IsOwner)return;
        playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        
    }
}
