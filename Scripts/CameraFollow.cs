using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Python.Runtime;
using System;
using System.IO;

namespace Cainos.PixelArtTopDown_Basic
{
    //let camera follow target using Python.NET or C# fallback
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float lerpSpeed = 1.0f;

        private static PyObject cameraFollowModule;
        private bool usePython = false; // Chuyển sang false nếu Python không khả dụng

        private void Start()
        {
            if (target == null) return;

            TryInitializePython();
        }

        private void TryInitializePython()
        {
            try
            {
                var pythonManager = PythonManager.Instance;
                
                if (!PythonManager.IsPythonInitialized())
                {
                    Debug.LogWarning("Python không khả dụng cho camera - sử dụng C# fallback");
                    usePython = false;
                    return;
                }

                cameraFollowModule = PythonManager.ImportModule("camera_follow");
                if (cameraFollowModule == null)
                {
                    Debug.LogWarning("Không load được camera_follow module - sử dụng C# fallback");
                    usePython = false;
                    return;
                }

                using (Py.GIL())
                {
                    PyObject result = cameraFollowModule.InvokeMethod("initialize_camera_follow",
                        new PyObject[] {
                            new PyFloat(transform.position.x),
                            new PyFloat(transform.position.y), 
                            new PyFloat(transform.position.z),
                            new PyFloat(target.position.x),
                            new PyFloat(target.position.y),
                            new PyFloat(target.position.z)
                        });
                    usePython = result.As<bool>();
                    
                    if (usePython)
                    {
                        Debug.Log("✅ Sử dụng Python cho camera follow");
                    }
                    else
                    {
                        Debug.LogWarning("Python camera init failed - sử dụng C# fallback");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Lỗi khởi tạo Python camera: {e.Message} - sử dụng C# fallback");
                usePython = false;
            }
        }

        private void Update()
        {
            if (target == null) return;

            if (usePython && cameraFollowModule != null)
            {
                UpdateWithPython();
            }
            else
            {
                UpdateWithCSharp(); // ✅ FALLBACK C#
            }
        }

        private void UpdateWithPython()
        {
            try
            {
                using (Py.GIL())
                {
                    PyObject result = cameraFollowModule.InvokeMethod("update_camera_follow",
                        new PyObject[] {
                            new PyFloat(transform.position.x),
                            new PyFloat(transform.position.y),
                            new PyFloat(transform.position.z),
                            new PyFloat(target.position.x),
                            new PyFloat(target.position.y),
                            new PyFloat(target.position.z),
                            new PyFloat(lerpSpeed),
                            new PyFloat(Time.deltaTime)
                        });
                    
                    var list = result.As<float[]>();
                    transform.position = new Vector3(list[0], list[1], list[2]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Lỗi Python camera update: {e.Message} - Chuyển sang C#");
                usePython = false;
            }
        }

        // ✅ C# FALLBACK LOGIC
        private void UpdateWithCSharp()
        {
            if (target == null) return;

            // Tính toán vị trí camera mới (giống logic Python)
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            
            // Lerp smooth để camera theo dõi mượt mà
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            
            transform.position = newPosition;
        }

    }
}
