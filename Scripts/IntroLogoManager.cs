using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroLogoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string loginSceneName = "Login"; // Đặt đúng tên scene đăng nhập của bạn

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(loginSceneName);
    }
}
