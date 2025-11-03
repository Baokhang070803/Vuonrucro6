using UnityEngine;
using Python.Runtime;
using System;
using System.Collections.Generic;

/// <summary>
/// Test script để kiểm tra Python farming integration
/// </summary>
public class FarmingPythonTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTestOnStart = false;
    public bool enableDebugLogs = true;
    
    private static PyObject farmingControllerModule;
    private bool usePython = false;
    
    void Start()
    {
        if (runTestOnStart)
        {
            TestPythonFarming();
        }
    }
    
    [ContextMenu("Test Python Farming")]
    public void TestPythonFarming()
    {
        Debug.Log("=== PYTHON FARMING TEST ===");
        
        try
        {
            // Kiểm tra Python availability
            if (!PythonManager.IsPythonInitialized())
            {
                Debug.LogError("❌ Python không khả dụng!");
                return;
            }
            
            // Import module
            farmingControllerModule = PythonManager.ImportModule("playerfarmcontroller");
            if (farmingControllerModule == null)
            {
                Debug.LogError("❌ Không thể import playerfarmcontroller module!");
                return;
            }
            
            Debug.Log("✅ Python module imported thành công!");
            
            // Test initialize
            using (Py.GIL())
            {
                PyObject result = farmingControllerModule.InvokeMethod("initialize_farming_controller",
                    new PyObject[] {
                        new PyInt(1), // quest_manager_available
                        new PyInt(0)  // farming_quest_completed
                    });
                
                bool success = result.As<bool>();
                if (success)
                {
                    Debug.Log("✅ Python farming controller initialized!");
                }
                else
                {
                    Debug.LogError("❌ Python farming controller init failed!");
                    return;
                }
            }
            
            // Test plant seed
            TestPlantSeed();
            
            // Test harvest
            TestHarvest();
            
            // Test farming stats
            TestFarmingStats();
            
            Debug.Log("=== PYTHON FARMING TEST COMPLETED ===");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Python farming test failed: {e.Message}");
        }
    }
    
    void TestPlantSeed()
    {
        try
        {
            using (Py.GIL())
            {
                PyObject result = farmingControllerModule.InvokeMethod("plant_seed",
                    new PyObject[] {
                        new PyInt(0), // cell_pos_x
                        new PyInt(0), // cell_pos_y
                        new PyString("sunflower"), // seed_type
                        new PyFloat(Time.time) // current_time
                    });
                
                var resultDict = result.As<Dictionary<string, object>>();
                bool success = (bool)resultDict["success"];
                string message = (string)resultDict["message"];
                
                if (success)
                {
                    Debug.Log($"✅ Plant seed test: {message}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Plant seed test: {message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Plant seed test failed: {e.Message}");
        }
    }
    
    void TestHarvest()
    {
        try
        {
            using (Py.GIL())
            {
                PyObject result = farmingControllerModule.InvokeMethod("harvest_plant",
                    new PyObject[] {
                        new PyInt(0), // cell_pos_x
                        new PyInt(0)  // cell_pos_y
                    });
                
                var resultDict = result.As<Dictionary<string, object>>();
                bool success = (bool)resultDict["success"];
                string message = (string)resultDict["message"];
                
                if (success)
                {
                    Debug.Log($"✅ Harvest test: {message}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Harvest test: {message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Harvest test failed: {e.Message}");
        }
    }
    
    void TestFarmingStats()
    {
        try
        {
            using (Py.GIL())
            {
                PyObject result = farmingControllerModule.InvokeMethod("get_farming_stats");
                
                var resultDict = result.As<Dictionary<string, object>>();
                int totalPlants = (int)resultDict["total_plants"];
                int harvestedPlants = (int)resultDict["harvested_plants"];
                int growingPlants = (int)resultDict["growing_plants"];
                int harvestedCount = (int)resultDict["harvested_count"];
                
                Debug.Log($"📊 Farming Stats:");
                Debug.Log($"  - Total Plants: {totalPlants}");
                Debug.Log($"  - Harvested Plants: {harvestedPlants}");
                Debug.Log($"  - Growing Plants: {growingPlants}");
                Debug.Log($"  - Harvested Count: {harvestedCount}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Farming stats test failed: {e.Message}");
        }
    }
    
    [ContextMenu("Reset Farming Data")]
    public void ResetFarmingData()
    {
        try
        {
            using (Py.GIL())
            {
                PyObject result = farmingControllerModule.InvokeMethod("reset_farming_data");
                var resultDict = result.As<Dictionary<string, object>>();
                bool success = (bool)resultDict["success"];
                string message = (string)resultDict["message"];
                
                if (success)
                {
                    Debug.Log($"✅ Reset farming data: {message}");
                }
                else
                {
                    Debug.LogError($"❌ Reset farming data failed: {message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Reset farming data failed: {e.Message}");
        }
    }
}
