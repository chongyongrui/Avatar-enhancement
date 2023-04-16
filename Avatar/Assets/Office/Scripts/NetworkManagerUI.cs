using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using TMPro;



public class NetworkManagerUI : NetworkBehaviour
{   
    [SerializeField]private TMP_InputField passwordInputField;
   [SerializeField] private TMP_InputField nameInputField;
    
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;
    [SerializeField] private Button LeaveButton;

    [SerializeField] GameObject Holder;
    [SerializeField] private TextMeshProUGUI playerCount;
    private static Dictionary<ulong,PlayerData> clientData;//Dictionary to store 
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
    private void Start(){
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }
    private void Destroy(){
        if (NetworkManager.Singleton == null) { return; }
          NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    private void Awake(){
       // Debug.Log(PlayerPrefs.GetString("PlayerName"));
        
    LeaveButton.onClick.AddListener(()=>{
        if(IsClient) NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        else NetworkManager.Singleton.Shutdown();
        if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }
        Holder.SetActive(false);
   
            
    });

    ClientButton.onClick.AddListener(()=>{
        //Send data to server when client connects;Serialize data into string(Easy transmission across network);
        //Transmission in binary data thus conversion to byte array.
       var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                
                NetworkPlayerName = passwordInputField.text,
                password = passwordInputField.text
            });

            byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData=  payloadBytes;
            NetworkManager.Singleton.StartClient();
       HostButton.gameObject.SetActive(false);
    });

    HostButton.onClick.AddListener(()=>{
        clientData = new Dictionary<ulong, PlayerData>();
        clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(PlayerPrefs.GetString("PlayerName"));
         NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();

        
    });
    }

    public static PlayerData? GetPlayerData(ulong clientId){
        if (clientData.TryGetValue(clientId, out PlayerData playerData))
            {
                return playerData;
            }

            return null;
    }
    private void Update(){
        playerCount.text = "Players: "+ playerNum.Value.ToString();
        if(!IsOwner)return;
        playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        
    }
  private void HandleClientDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                clientData.Remove(clientId);
            }

            // Are we the client that is disconnecting?
            
        }
private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
{
    // Get the password sent by the client
     
    byte[] payloadBytes = request.Payload;
    var payloadString = Encoding.ASCII.GetString(payloadBytes);
    var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payloadString);
    var clientPassword = connectionPayload.password;

    // Check if the password is correct
    bool approved = clientPassword == passwordInputField.text;

    if (approved)
    {   
        // If the client is approved, create their player object
        response.CreatePlayerObject = true;
       
        // Store their player data in the clientData dictionary
        ulong clientId = request.ClientNetworkId;
        PlayerData playerData = new PlayerData(connectionPayload.NetworkPlayerName);
        clientData[clientId] = playerData;
    }
    else
    {
        // If the client is not approved, reject the connection and provide a reason
        response.Approved = false;
        response.Reason = "Incorrect password.";
    }
}

}
