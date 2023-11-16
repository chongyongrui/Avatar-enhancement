using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkStatusIconScript : MonoBehaviour
{

    [SerializeField] TMP_Text IPTextField;
    string connectedIPAddress;
    // Start is called before the first frame update
    void Start()
    {
        TestConnection();
        InvokeRepeating("TestConnection", 10.0f, 7.0f);
    }
    
    // Update is called once per frame
    public void TestConnection()
    {
        try
        {
            connectedIPAddress = SQLConnection.instance.IPAddress;
            IPTextField.text = "Server IP: " + connectedIPAddress + "   Player ID: " + userdatapersist.Instance.verifiedUser.GetHashCode();
            RawImage image = this.GetComponent<RawImage>();
            if (SQLConnection.instance.SQLServerConnected)
            {
                image.color = Color.green;
            }
            else
            {
                image.color = Color.red;
            }
        }catch (Exception ex)
        {
            //Debug.LogException(ex);
        }
        
    }
}
