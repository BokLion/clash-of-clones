﻿using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simply prints a big message to the center of the screen for some seconds.
/// </summary>
public class MessageUI : MonoBehaviour 
{
    [SerializeField] private Text _text;
    [SerializeField] private GameObject _gameOverMenu;

    /// <summary>
    /// The number of seconds to display the message.
    /// </summary>
    [SerializeField] private float _displaySeconds;

    public void PrintMessage(string message)
    {
        // EARLY OUT! //
        if (_text == null)
        {
            Debug.LogWarning("Can't print a message without text.");
            return;
        }

        _text.enabled = true;
        _text.text = message;

        this.Invoke(HideMessage, _displaySeconds);
    }

    public void HideMessage()
    {
        this.CancelInvoke();
        
        // EARLY OUT! //
        if(_text == null) return;

        _text.enabled = false;
    }

    public void ShowGameOverUI()
    {
        if(_text != null)
        {
            _text.enabled = false;
        }

        if(_gameOverMenu != null)
        {
            _gameOverMenu.SetActive(true);
        }
    }

    void OnDestroy()
    {
        this.CancelInvoke();
    }
}