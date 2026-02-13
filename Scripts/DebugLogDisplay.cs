using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DebugLogDisplay : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    public int maxLines = 10;
    private Queue<string> _logQueue = new Queue<string>();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string color = "white";
        if (type == LogType.Error || type == LogType.Exception) color = "red";
        else if (type == LogType.Warning) color = "yellow";

        _logQueue.Enqueue($"<color={color}>[{System.DateTime.Now:HH:mm:ss}] {logString}</color>");

        if (_logQueue.Count > maxLines)
            _logQueue.Dequeue();

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (debugText != null)
        {
            debugText.text = string.Join("\n", _logQueue.ToArray());
        }
    }
}
