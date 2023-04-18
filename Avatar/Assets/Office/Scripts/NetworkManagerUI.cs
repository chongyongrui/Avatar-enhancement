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
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;

    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;
    [SerializeField] private Button LeaveButton;

    [SerializeField] private GameObject Holder;

    private static Dictionary<ulong, PlayerData> clientData;//Dictionary to store 
    private bool isServerStarted = false;


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

    // Start is called before the first frame update
    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
        
        LeaveButton.gameObject.SetActive(false);
    }
    private void Destroy()
    {
        if (NetworkManager.Singleton == null) { return; }
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    public void Leave()
    {
        if (IsClient) NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        else NetworkManager.Singleton.Shutdown();
        if (NetworkManager.Singleton.IsServer)
        {   
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
    }
    public void Client()
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {

            NetworkPlayerName = nameInputField.text,
            password = passwordInputField.text
        });

        byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
        HostButton.gameObject.SetActive(false);
    }
    public void Host()
    {
        clientData = new Dictionary<ulong, PlayerData>();
        clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(nameInputField.text);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        
    }
   

    public static PlayerData? GetPlayerData(ulong clientId)
    {
        if (clientData.TryGetValue(clientId, out PlayerData playerData))
        {
            return playerData;
        }

        return null;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            clientData.Remove(clientId);
        }

        // Are we the client that is disconnecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Holder.SetActive(true);
            LeaveButton.gameObject.SetActive(false);


        }
    }
    private void HandleClientConnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Holder.SetActive(false);
            LeaveButton.gameObject.SetActive(true);

        }
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


            // Store their player data in the clientData dictionary
            ulong clientId = request.ClientNetworkId;
            PlayerData playerData = new PlayerData(connectionPayload.NetworkPlayerName);
            clientData[clientId] = playerData;
            response.Approved = true;
        }
        else
        {
            // If the client is not approved, reject the connection and provide a reason
            response.Approved = false;
            response.Reason = "Incorrect password.";
        }
    }

}
