using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using WebSocketSharp;

[RequireComponent(typeof(UIDocument))]
public class UsernameInputFieldUI : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button saveButton;
    private TextField textField;
    // Start is called before the first frame update
    private void Awake() 
    {
        uiDocument = GetComponent<UIDocument>();
        textField = uiDocument.rootVisualElement.Q<TextField>("UserNameField");
        saveButton = uiDocument.rootVisualElement.Q<Button>("Save");

        saveButton.clicked += Save;    
        string savedUsername = PlayerPrefs.GetString("userName");
        if (savedUsername.IsNullOrEmpty()) 
        {
            return;
        }

        textField.value = savedUsername;
    }

    private void Save() 
    {
        if (textField.value.IsNullOrEmpty()) {
            return;
        }
        //DO like regexchecks or checks for banned words
        if (!textField.value.Any(ch => !char.IsLetterOrDigit(ch)) && textField.value.Length > 1) 
        {
            PlayerPrefs.SetString("userName",textField.value);
            SceneManager.LoadScene("S_firstScene");
        }
    }
}
