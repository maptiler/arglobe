using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugText : MonoBehaviour
{
    public TMPro.TMP_Text consoleText;
    private string consoleString;


    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        //if (type == LogType.Error)
        {
            //consoleString = consoleString + "\n" + message;
            //consoleText.text = consoleString;
            consoleText.text = message + "\n" + stackTrace;
        }
    }
}