using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    
    [SerializeField] private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(string.Empty,NetworkVariableReadPermission.Everyone);

    
    public override void OnNetworkSpawn()
    {   
            // Set the player name to the one entered by the player
             playerName = PlayerPrefs.GetString("PlayerName");
            playerNameText.text = playerName.ToString();
        
        // Update the player name text
        
    }
}
