using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AnotherFileBrowser.Windows;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System;
using System.Diagnostics.Metrics;
using System.Text;
using Org.BouncyCastle.Utilities;

public class FileMangerOpener : MonoBehaviour
{
    public GameObject imagePreviewer;
    public GameObject messageBox;
    public RawImage rawImage;
    public bool pictureMode = false;
    public static FileMangerOpener instance;
    public Texture2D uwrTexture;


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }


    public void OpenFileBrowser()
    {
        pictureMode = true;
        imagePreviewer.SetActive(true);
        messageBox.SetActive(false);
        var bp = new BrowserProperties();
        bp.filter = "Image files | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            StartCoroutine(LoadImage(path));

        });


    }

    IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                uwrTexture = DownloadHandlerTexture.GetContent(uwr);
                rawImage.texture = uwrTexture;
                
     

                


            }
        }
        
        
    }
}
