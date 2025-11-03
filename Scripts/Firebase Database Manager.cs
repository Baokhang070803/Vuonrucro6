using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseDatabaseManager : MonoBehaviour
{
    private DatabaseReference reference;

    private void Awake()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

   

    public void WriteDatabase(string path, string message)
    {
        reference.Child(path).SetValueAsync(message).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Ghi dữ liệu thành công!");
            }
            else
            {
                Debug.Log("Ghi dữ liệu thất bại: " + task.Exception);
            }
        });
    }

    public void ReadDatabase(string id)
    {
        reference.Child("Users").Child(id).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("Dữ liệu đọc được: " + snapshot.Value.ToString());
            }
            else
            {
                Debug.Log("Đọc dữ liệu thất bại: " + task.Exception);
            }
        });
    }
}

