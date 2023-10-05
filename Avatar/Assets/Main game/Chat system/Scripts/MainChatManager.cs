using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;


public class MainChatManager : NetworkBehaviour
{
    public static MainChatManager Singleton;

    [SerializeField] ChatMessage chatMessagePrefab;
    [SerializeField] CanvasGroup chatContent;
    [SerializeField] TMP_InputField chatInput;
    [SerializeField] GameObject holder;

    public PlayerInput interactingPlayerController;
    public Button chatButton;
    public GameObject player;
    bool chatStatus = false;
    bool playerspawned = false;




    public void Start()
    {

    }
    public override void OnNetworkSpawn()
    {try{
        base.OnNetworkSpawn();
        chatButton.gameObject.SetActive(true);
        player = GameObject.FindGameObjectWithTag("Player");
        interactingPlayerController = player.GetComponent<PlayerInput>();
        string message = "Player " + NetworkManager.Singleton.LocalClientId + " has joined.";
        SendChatMessageServerRpc(message, NetworkManager.Singleton.LocalClientId);
        playerspawned = true;}
 catch(Exception e){
            Debug.Log("hi22");
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        string message = "Player " + NetworkManager.Singleton.LocalClientId + " has left.";
        SendChatMessageServerRpc(message, NetworkManager.Singleton.LocalClientId);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(chatInput.text);
            chatInput.text = "";
        }
        if (playerspawned)
        {
            if (chatStatus)
            {
                interactingPlayerController.enabled = false;

            }
            else
            {
                interactingPlayerController.enabled = true;

            }
        }

    }

    public void SendChatMessage(string _message)
    {
        if (string.IsNullOrWhiteSpace(_message)) return;

        //string S = "Player " + NetworkManager.Singleton.LocalClientId + " > " +  _message;
        SendChatMessageServerRpc(_message, NetworkManager.Singleton.LocalClientId);
    }
    public void Chatpop()
    {
        if (holder.activeSelf)
        {
            chatStatus = false;
            holder.SetActive(false);




        }
        else
        {
            holder.SetActive(true);
            chatStatus = true;
        }
        //   interactingPlayerController.enabled = false;

    }
    void AddMessage(string msg, ulong senderID)
    {
        ChatMessage CM = Instantiate(chatMessagePrefab, chatContent.transform);
        CM.SetMessage(senderID, msg);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatMessageServerRpc(string message, ulong senderID)
    {
        ReceiveChatMessageClientRpc(message, senderID);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string message, ulong senderID)
    {
        //ChatManager.Singleton.AddMessage(message,senderID);
        AddMessage(message, senderID);
    }
}
