using UnityEngine;

public class LoggingTest : MonoBehaviour
{
    void Start() => DemoLogging();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Logger.Log("Space key pressed!", true, LogType.Log);
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            Logger.Log("Warning test message", true, LogType.Warning);
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Logger.Log("Error test message", true, LogType.Error);
        }
    }
    
    private void DemoLogging()
    {
        // Configure enhanced logging
        Logger.EnableFileLogging = true;
        Logger.EnableScreenLogging = true;
        Logger.MaxScreenLogLines = 20;
        Logger.ScreenLogDisplayTime = 10.0f;
        
        // Test different log types
        Logger.Log("This is a normal log message", true, LogType.Log);
        Logger.Log("This is a warning message", true, LogType.Warning);
        Logger.Log("This is an error message", true, LogType.Error);
        
        // Test with exception
        try
        {
            throw new System.Exception("Test exception");
        }
        catch (System.Exception ex)
        {
            Logger.Log("Exception occurred", true, LogType.Exception, ex);
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 200));
        
        if (GUILayout.Button("Test Log"))
        {
            Logger.Log($"GUI Test Log {Time.time:F2}", true, LogType.Log);
        }
        
        if (GUILayout.Button("Test Warning"))
        {
            Logger.Log($"GUI Test Warning {Time.time:F2}", true, LogType.Warning);
        }
        
        if (GUILayout.Button("Test Error"))
        {
            Logger.Log($"GUI Test Error {Time.time:F2}", true, LogType.Error);
        }

        if (GUILayout.Button("Test Exception"))
        {
            try
            {
                throw new System.Exception("Test exception from GUI");
            }
            catch (System.Exception ex)
            {
                Logger.Log("Exception from GUI", true, LogType.Exception, ex);
            }
        }

        if (GUILayout.Button("Test Assert"))
        {
            Logger.Log($"GUI Test Assert {Time.time:F2}", true, LogType.Assert);
        }

        //if (GUILayout.Button("Clear Screen Logs"))
        //{
        //    Logger.ClearScreenLogs();
        //}

        if (GUILayout.Button("Toggle Screen Log"))
        {
            Logger.ToggleScreenLogging();
        }
        
        GUILayout.EndArea();
    }
}

