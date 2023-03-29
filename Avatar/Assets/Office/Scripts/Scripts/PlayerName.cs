using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerNameText;

    private NetworkVariable<string> playerName = new NetworkVariable<string>(string.Empty,NetworkVariableReadPermission.Everyone);

    public override void OnNetworkSpawn()
    { base.OnNetworkSpawn();

        if (IsClient&&IsOwner)
        {
            // Set the player name to the one entered by the player
            string name = PlayerPrefs.GetString("PlayerName");
            playerName.Value = name;
        }
        
        // Update the player name text
        playerNameText.text = playerName.Value;
    }
}
