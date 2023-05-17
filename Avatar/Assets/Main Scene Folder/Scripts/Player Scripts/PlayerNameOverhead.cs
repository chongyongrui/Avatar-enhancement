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
        public string Playername => displayName.Value.ToString();

        [ServerRpc(RequireOwnership =false)]
        public void UpdateplayernameServerRPC(string newPLayername){
            displayName.Value = new FixedString32Bytes(newPLayername);
            onUpdatePLayerNameClientRpc(displayName.Value.ToString());

        }
        [ClientRpc]
        public void onUpdatePLayerNameClientRpc(string newPLayername){
            Debug.Log(newPLayername);
        }
        private void Awake(){
            displayName.OnValueChanged += OnPlayerNameChanged;
        }
        private void OnPlayerNameChanged(FixedString32Bytes oldName,FixedString32Bytes newName){

        }

        // private void OnEnable()
        // {
        //     displayName.OnValueChanged += HandleDisplayNameChanged;
        // }

        // private void OnDisable()
        // {
        //     displayName.OnValueChanged -= HandleDisplayNameChanged;
        // }

        // private void HandleDisplayNameChanged(FixedString32Bytes oldDisplayName, FixedString32Bytes newDisplayName)
        // {
        //     displayNameText.text = newDisplayName.ToString();
        // }
}

