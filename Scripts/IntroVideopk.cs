using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem; // thêm dòng này

public class IntroVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    
    private bool hasPlayed = false; // Flag để đảm bảo video chỉ phát 1 lần
    private float skipDelay = 1f; // Thời gian chờ trước khi cho phép skip
    private float startTime;

    void Start()
    {
        Debug.Log("IntroVideoController Start");
        
        // Đảm bảo video không lặp lại
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false; // Đảm bảo không tự phát
        
        // Đăng ký sự kiện kết thúc video trước khi play
        videoPlayer.loopPointReached += OnVideoEnd;

        if (videoPlayer.targetTexture != null)
        {
            rawImage.texture = videoPlayer.targetTexture;
        }

        // Chỉ phát video nếu chưa phát lần nào
        if (!hasPlayed)
        {
            Debug.Log("Preparing video...");
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared, starting playback...");
        if (!hasPlayed)
        {
            hasPlayed = true; // Đánh dấu đã phát
            startTime = Time.time; // Ghi nhận thời điểm bắt đầu
            videoPlayer.Play();
            Debug.Log($"Video started at time: {startTime}");
        }
    }

    void Update()
    {
        // Nếu video chưa được chuẩn bị hoặc chưa phát thì không làm gì
        if (!hasPlayed || !videoPlayer.isPlaying)
            return;

        // Kiểm tra thời gian chờ - phải đợi ít nhất skipDelay giây
        if (Time.time - startTime < skipDelay)
            return;

        // Kiểm tra phím ESC để skip video
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESC pressed - Skipping video");
            SkipVideo();
            return;
        }

        // Kiểm tra Space hoặc Enter để skip
        if (Keyboard.current != null && 
            (Keyboard.current.spaceKey.wasPressedThisFrame || 
             Keyboard.current.enterKey.wasPressedThisFrame))
        {
            Debug.Log("Space/Enter pressed - Skipping video");
            SkipVideo();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video ended naturally");
        // Dừng video hoàn toàn và ẩn video player
        videoPlayer.Stop();
        FinishVideo();
    }

    void SkipVideo()
    {
        Debug.Log("Video skipped by user");
        // Dừng video ngay lập tức
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        FinishVideo();
    }

    void FinishVideo()
    {
        Debug.Log("Finishing video and cleaning up");
        // Hủy đăng ký các event
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.prepareCompleted -= OnVideoPrepared;
            
        // Ẩn video player và raw image
        if (rawImage != null)
            rawImage.gameObject.SetActive(false);
        if (videoPlayer != null)
            videoPlayer.gameObject.SetActive(false);
            
        Debug.Log("Video cleanup completed");
        // Có thể thêm logic khác ở đây như hiện UI chính của game
    }
}
