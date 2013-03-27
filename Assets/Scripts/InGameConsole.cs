using UnityEngine;
using System.Collections;
 
public class InGameConsole : MonoBehaviour
{
    static string myLog = "";
    static Queue myLogQueue = new Queue();
    public string output = "";
    public string stack = "";
    private bool hidden = false;
    private Vector2 scrollPos;
    public int maxLines = 10;
 
    void OnEnable()
    {
        Application.RegisterLogCallback(HandleLog);
    }
 
    void OnDisable()
    {
        Application.RegisterLogCallback(null);
    }
 
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        string newString = "\n [" + type + "] : " + output;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
 
        while (myLogQueue.Count > maxLines)
        {
            myLogQueue.Dequeue();
        }
 
        myLog = string.Empty;
        foreach (string s in myLogQueue)
        {
            myLog += s;
        }
    }
 
    void OnGUI()
    {
        if (!hidden)
        {
            GUI.TextArea(new Rect(0, 0, Screen.width / 3, Screen.height / 2), myLog);
			
            if (GUI.Button(new Rect(Screen.width - 200, 20, 160, 40), "Clear"))
            {
                //hide(true);
				myLog = string.Empty;
            }
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width - 200, 20, 160, 40), "Show"))
            {
                hide(false);
            }
        }
    }
 
    public void hide(bool shouldHide)
    {
        hidden = shouldHide;
    }
}