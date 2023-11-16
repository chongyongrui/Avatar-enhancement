using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ImageSender : MonoBehaviour
{
    public static ImageSender Instance;
    [SerializeField]  TMP_InputField ReceiverNameInputField;
    [SerializeField]  GameObject popupWindow; 
    [SerializeField]  TMP_Text windowMessage;

    void Start()
    {
        Instance = this;
    }

   public void EncryptAndSendImage()
    {
      
        byte[] imageBytes = TextureToByteArray(FileMangerOpener.instance.uwrTexture);
        string hexString = BitConverter.ToString(imageBytes).Replace("-", "");



        string hashedReceiverUserName = ReceiverNameInputField.text.ToString();
        //check if such a connection exists
        Dictionary<string, int> connections = new Dictionary<string, int>();
        DHMessager.instance.GetEstablishedConnections(connections);
        if (connections.ContainsKey(hashedReceiverUserName))
        {

            string keyValue = AESMessager.instance.GetAESKey(hashedReceiverUserName);

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(keyValue);
            // Ensure the key is 32 bytes long (AES-256)
            if (keyBytes.Length != 32)
            {
                // Pad or truncate the key to 32 bytes
                Array.Resize(ref keyBytes, 32);
            }
            // Your message to be encrypted
            AESMessager.instance.EncryptAndPost(keyBytes, hexString, 6);

        }
        else
        {
            popupWindow.SetActive(true);
            windowMessage.text = "Connection does not exist!";
        }



        hexString = hexString.Length % 2 == 0 ? hexString : "0" + hexString; // Ensure an even number of characters
        byte[] byteArray = new byte[hexString.Length / 2];
        for (int i = 0; i < byteArray.Length; i++)
        {
            byteArray[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }
        var decryptedImage = ByteArrayToTexture(byteArray);
        
    }

    static byte[] TextureToByteArray(Texture2D texture)
    {
        byte[] byteArray = null;

        if (texture != null)
        {
            // Encode the texture to a byte array using the specified format (e.g., "PNG" or "JPG").
            byteArray = texture.EncodeToPNG(); // or texture.EncodeToJPG() for JPEG

            if (byteArray != null)
            {
                // Optionally, you can save the byte array to a file for debugging or other purposes.
                // File.WriteAllBytes("texture.png", byteArray);
            }
        }

        return byteArray;
    }

    static Texture2D ByteArrayToTexture(byte[] byteArray)
    {
        if (byteArray != null && byteArray.Length > 0)
        {
            Texture2D texture = new Texture2D(2, 2); // Create a new Texture2D object.

            // Load the image data from the byte array into the texture.
            if (texture.LoadImage(byteArray))
            {
                return texture; // Return the loaded texture.
            }
        }

        return null; // Return null if the conversion failed.
    }
}
