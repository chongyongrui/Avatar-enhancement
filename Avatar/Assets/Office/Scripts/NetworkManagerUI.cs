using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button ServerButton;
    [SerializeField] private Button HostButton;
    [SerializeField] private Button ClientButton;
    // Start is called before the first frame update
    private void Awake(){
    ServerButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartServer();
        Debug.Log("serverbtn");
    });
    ClientButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartClient();
    });
    HostButton.onClick.AddListener(()=>{
        NetworkManager.Singleton.StartHost();
    });
    }
}
