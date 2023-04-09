using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    
    [SerializeField] private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(string.Empty,NetworkVariableReadPermission.Everyone);

    
    public override void OnNetworkSpawn()
    {   
            // Set the player name to the one entered by the player
              playerName.Value = new FixedString32Bytes(PlayerPrefs.GetString("PlayerName"));
            playerNameText.text = playerName.Value.ToString();
        
        // Update the player name text
        
    }
}
