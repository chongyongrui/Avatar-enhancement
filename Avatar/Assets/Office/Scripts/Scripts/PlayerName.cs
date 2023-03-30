using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    
    private NetworkVariable<string> playerName = new NetworkVariable<string>(string.Empty,NetworkVariableReadPermission.Everyone);

    public override void OnNetworkSpawn()
    { base.OnNetworkSpawn();

        if (IsClient&&IsOwner)
        {
            // Set the player name to the one entered by the player
            string playerName = PlayerPrefs.GetString("PlayerName");
            playerNameText.text = playerName;
        }
        
        // Update the player name text
        
    }
}
