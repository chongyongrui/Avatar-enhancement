using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauchDockerNetwork : MonoBehaviour
{
    public void runDocker()
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>();
        if (LoginController.instance.isHost)
        {
            LoginController.instance.StartAcaPyInstanceAsync(arguments);
        }
        
    }
}
