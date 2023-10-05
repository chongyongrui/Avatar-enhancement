using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class AESMessager : MonoBehaviour
{
    public static AESMessager instance;
    public string username;
    public static byte[] userPublicKey;
    [SerializeField] public TMP_InputField ReceiverNameInputField;
    [SerializeField] public TMP_InputField MessageString;
    public string ReceiverName;
    public string IPAddress;
    public string ledgerUrl;
    [SerializeField] GameObject popupWindow;
    [SerializeField] TMP_Text windowMessage;
    private static readonly HttpClient client = new HttpClient();
    public string message;
    public string hashedReceiverUserName;
    private void Start()
    {
        IPAddress = userdatapersist.Instance.IPAdd;
        ledgerUrl = "http://" + IPAddress + ":9000";
        instance = this;
        try
        {
            username = userdatapersist.Instance.verifiedUser;
        }
        catch (System.Exception e)
        {
            Debug.Log("Unable to get username from Login page.");
        }


    }


    public void EncryptAndSend()
    {
        message = MessageString.text;
        hashedReceiverUserName = ReceiverNameInputField.text.ToString();

        // Your AES key value as a string
        string keyValue = "7603311629598998022494766896591742076967314215044656048467376034985166383551714368654391774233093974027930697111222901957035206393486823514084136093174272";

        // Convert the key value from a hexadecimal string to a byte array
        byte[] keyBytes = HexStringToByteArray(keyValue);

        // Ensure the key is 32 bytes long (AES-256)
        if (keyBytes.Length != 32)
        {
            // Pad or truncate the key to 32 bytes
            Array.Resize(ref keyBytes, 32);
        }

        // Your message to be encrypted
        try
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.Key = keyBytes;
                
                // Generate a random IV for encryption
                myAes.GenerateIV();

                // Encrypt the string to an array of bytes
                byte[] encrypted = EncryptStringToBytes_Aes(message, myAes.Key, myAes.IV);

                // Decrypt the bytes to a string
                string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);
                
                // Display the original data and the decrypted data
                Debug.Log("Original:  " + message);
                Debug.Log("Encrypted: " + BitConverter.ToString(encrypted).Replace("-", ""));
                Debug.Log("Decrypted: " + roundtrip);
            }
        }catch(Exception e)
        {
            Debug.Log("Failed at message encryption process" + e.Message);
        }

        
    }

    // Function to convert a hexadecimal string to a byte array
    static byte[] HexStringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    // Start is called before the first frame update
    static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }
}
