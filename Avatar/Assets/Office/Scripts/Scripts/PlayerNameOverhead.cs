using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Text;
using TMPro;
public class PlayerNameOverhead : NetworkBehaviour
{
    
        [SerializeField] private TextMeshProUGUI displayNameText;

        private NetworkVariable<FixedString32Bytes> displayName = new NetworkVariable<FixedString32Bytes>();

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) { return; }

            PlayerData? playerData = NetworkManagerUI.GetPlayerData(OwnerClientId);

            if (playerData.HasValue)
            {  
                displayName.Value = playerData.Value.PlayerName;
            }
        }

        private void OnEnable()
        {
            displayName.OnValueChanged += HandleDisplayNameChanged;
        }

        private void OnDisable()
        {
            displayName.OnValueChanged -= HandleDisplayNameChanged;
        }

        private void HandleDisplayNameChanged(FixedString32Bytes oldDisplayName, FixedString32Bytes newDisplayName)
        {
            displayNameText.text = newDisplayName.ToString();
        }
}

