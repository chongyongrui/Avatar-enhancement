using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class InputManagerScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private  Button enterButton;
    [SerializeField] private  Button clearButton;

    private void Start()
    {
        enterButton.onClick.AddListener(SaveName);
        clearButton.onClick.AddListener(ClearName);
        LoadName();
    }

    private void SaveName()
    {
        string playerName = nameInputField.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        ClearName();
        LoadNextScene();
    }

    private void ClearName()
    {
        nameInputField.text = "";
        PlayerPrefs.DeleteKey("PlayerName");
    }

    private void LoadName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string playerName = PlayerPrefs.GetString("PlayerName");
            nameInputField.text = playerName;
        }
    }

    private void LoadNextScene()
    {
        string nextSceneName = "Main";
        SceneManager.LoadScene(nextSceneName);
    }

}
