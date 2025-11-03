using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class IntroVideoPlayer : MonoBehaviour
{
    public RawImage rawImage;
    public VideoPlayer videoPlayer;
    public Canvas videoCanvas; // Canvas chứa RawImage (dùng Canvas để điều khiển sorting)

    // NEW: render texture options
    public RenderTexture renderTexture;    // optional: assign in inspector
    public bool createRenderTextureIfNull = true;
    public int renderTextureWidth = 1920;
    public int renderTextureHeight = 1080;
    public bool playOnStart = true;
    public bool allowSkipWithInput = true;

    // NEW: canvas ordering
    public int canvasSortingOrder = 9999;
    public bool forceScreenSpaceOverlay = true;

    private bool finished = false;
    private RenderTexture createdRT;

    // NEW: store canvases disabled while video plays
    private List<Canvas> disabledCanvases = new List<Canvas>();

    void Start()
    {
        // safety
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }

        // ensure render mode uses RenderTexture
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // create or use provided RenderTexture
        if (renderTexture == null && createRenderTextureIfNull)
        {
            createdRT = new RenderTexture(renderTextureWidth, renderTextureHeight, 0);
            createdRT.Create();
            videoPlayer.targetTexture = createdRT;
            if (rawImage != null) rawImage.texture = createdRT;
        }
        else if (renderTexture != null)
        {
            videoPlayer.targetTexture = renderTexture;
            if (rawImage != null) rawImage.texture = renderTexture;
        }
        else
        {
            // no RT and not creating one: if rawImage assigned, use its texture if it's a RenderTexture
            if (rawImage != null && rawImage.texture is RenderTexture rt)
            {
                videoPlayer.targetTexture = rt;
            }
        }

        // --- NEW: ensure we have a Canvas reference ---
        if (videoCanvas == null && rawImage != null)
        {
            videoCanvas = rawImage.canvas;
        }

        if (videoCanvas != null)
        {
            // force overlay and top sorting
            if (forceScreenSpaceOverlay)
            {
                videoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            videoCanvas.overrideSorting = true;
            videoCanvas.sortingOrder = canvasSortingOrder;

            // make sure RawImage is last sibling to be visually on top of other elements in same Canvas
            if (rawImage != null)
            {
                rawImage.rectTransform.SetAsLastSibling();
            }

            // disable other canvases so video is highest priority
            Canvas[] all = FindObjectsOfType<Canvas>();
            foreach (var c in all)
            {
                if (c == videoCanvas) continue;
                if (c.enabled)
                {
                    c.enabled = false;
                    disabledCanvases.Add(c);
                }
            }

            // ensure the canvas GameObject is active
            videoCanvas.gameObject.SetActive(true);
        }

        // subscribe end event
        videoPlayer.loopPointReached += OnVideoEnd;

        if (playOnStart)
        {
            videoPlayer.Play();
        }
    }

    void Update()
    {
        if (finished) return;

        if (!allowSkipWithInput) return;

        bool skip = false;

        // Nếu project đang dùng Input System mới và Legacy Input tắt -> dùng Keyboard.current
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var kb = Keyboard.current;
        if (kb != null && kb.anyKey != null)
        {
            // phát hiện phím mới nhấn trong frame
            skip = kb.anyKey.wasPressedThisFrame;
        }
#else
        // fallback sang Legacy Input Manager
        skip = UnityEngine.Input.anyKeyDown;
#endif

        if (skip)
        {
            // stop and call end handler
            if (videoPlayer.isPlaying) videoPlayer.Stop();
            OnVideoEnd(videoPlayer);
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (finished) return;
        finished = true;

        if (videoCanvas != null)
        {
            videoCanvas.gameObject.SetActive(false);
        }
        else if (rawImage != null)
        {
            rawImage.enabled = false;
        }

        // restore previously disabled canvases
        if (disabledCanvases != null)
        {
            foreach (var c in disabledCanvases)
            {
                if (c != null) c.enabled = true;
            }
            disabledCanvases.Clear();
        }

        // release created RT to free memory
        if (createdRT != null)
        {
            vp.targetTexture = null;
            if (rawImage != null) rawImage.texture = null;
            createdRT.Release();
            Destroy(createdRT);
            createdRT = null;
        }
    }
}
