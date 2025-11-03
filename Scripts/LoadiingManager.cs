using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class LoadingManager : MonoBehaviour
{
    public static string NEXT_SCENE;

    public GameObject progressBar;
    public Text textPercent;
    private float fixedLoadingTime = 3f;

    private void Start()
    {
        StartCoroutine(LoadSceneFixedTime(NEXT_SCENE));
    }

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.GetComponent<Image>().fillAmount = progress;
            textPercent.text = (progress * 100).ToString("0") + "%";

            yield return null;
        }
    }

    public IEnumerator LoadSceneFixedTime(string sceneName)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fixedLoadingTime)
        {
            float progress = Mathf.Clamp01(elapsedTime / fixedLoadingTime);
            progressBar.GetComponent<Image>().fillAmount = progress;
            textPercent.text = (progress * 100).ToString("0") + "%";

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

}

