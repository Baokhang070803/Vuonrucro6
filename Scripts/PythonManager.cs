using UnityEngine;
using Python.Runtime;
using System;
using System.IO;

public class PythonManager : MonoBehaviour
{
    private static PythonManager instance;
    private static bool pythonInitialized = false;
    
    public static PythonManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PythonManager");
                instance = go.AddComponent<PythonManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePython();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static bool IsPythonInitialized()
    {
        return pythonInitialized;
    }

    public static PyObject ImportModule(string moduleName)
    {
        if (!pythonInitialized)
        {
            Debug.LogError("Python is not initialized!");
            return null;
        }

        try
        {
            using (Py.GIL())
            {
                return Py.Import(moduleName);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to import Python module '{moduleName}': {e.Message}");
            return null;
        }
    }

    private void InitializePython()
    {
        if (pythonInitialized) return;

        try
        {
            // Set Python DLL path for Python 3.11.0 BEFORE initializing
            if (!PythonEngine.IsInitialized)
            {
                string pythonDll = @"C:\Users\ACER\AppData\Local\Programs\Python\Python311\python311.dll";
                
                // Check if the DLL exists
                if (!File.Exists(pythonDll))
                {
                    Debug.LogError($"Could not find python311.dll at: {pythonDll}");
                    return;
                }

                // Set the Python DLL path BEFORE initialization
                Runtime.PythonDLL = pythonDll;
                Debug.Log($"Using Python DLL: {pythonDll}");

                // Initialize Python.NET
                PythonEngine.Initialize();
                Debug.Log("Python.NET initialized successfully");
            }
            
            pythonInitialized = true;

            // Add Scripts directory to Python path
            using (Py.GIL())
            {
                string scriptsPath = Path.Combine(Application.dataPath, "Scripts");
                scriptsPath = scriptsPath.Replace("\\", "/");
                
                dynamic sys = Py.Import("sys");
                sys.path.append(scriptsPath);
                
                Debug.Log($"Added Scripts path to Python: {scriptsPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Python: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        if (pythonInitialized && PythonEngine.IsInitialized)
        {
            try
            {
                PythonEngine.Shutdown();
                pythonInitialized = false;
                Debug.Log("Python.NET shutdown successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error shutting down Python: {e.Message}");
            }
        }
    }
}