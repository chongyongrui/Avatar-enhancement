using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;
public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button ServerButton;
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;
    [SerializeField] private Button LeaveButton;
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private TMP_Text playerNameText;
   
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
        
    LeaveButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
   
            
    });
    ServerButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartServer();
       HostButton.gameObject.SetActive(false);
       ClientButton.gameObject.SetActive(false);
       LeaveButton.gameObject.SetActive(false);
            
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
       // playerNameText.text = NetString.player.username.Value.ToString();

    
    }
    private void Update(){
        playerCount.text = "Players: "+ playerNum.Value.ToString();
        if(!IsOwner)return;
        playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        
    }
}
