using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager I; // Singleton đơn giản
    public static bool IsDialogueOpen => I != null && I._isOpen;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public Button closeButton;

    [Header("Typing")]
    public float typeSpeed = 0.02f;

    private List<string> _lines;
    private int _index;
    private bool _isTyping;
    private bool _isOpen;

    void Awake()
    {
        if (I == null) I = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePanel == null)
        {
            var panel = GameObject.Find("DialoguePanel");
            if (panel) dialoguePanel = panel;
        }
        if (dialogueText == null)
        {
            var text = GameObject.Find("DialogueText");
            if (text) dialogueText = text.GetComponent<TextMeshProUGUI>();
        }
        if (nextButton == null)
        {
            var btn = GameObject.Find("NextButton");
            if (btn) nextButton = btn.GetComponent<Button>();
        }
        if (closeButton == null)
        {
            var btn = GameObject.Find("CloseButton");
            if (btn) closeButton = btn.GetComponent<Button>();
        }

        // Ensure dialogue is rendered on a Screen Space Overlay canvas for Game view
        if (dialoguePanel != null)
        {
            Canvas parentCanvas = dialoguePanel.GetComponentInParent<Canvas>();
            bool needsOverlay = parentCanvas == null || parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay;
            if (needsOverlay)
            {
                Canvas overlay = FindObjectOfType<Canvas>(includeInactive: true);
                if (overlay == null || overlay.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    var canvasGO = new GameObject("DialogueCanvasRuntime");
                    overlay = canvasGO.AddComponent<Canvas>();
                    overlay.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                // Normalize overlay canvas transform
                overlay.transform.localScale = Vector3.one;
                overlay.transform.localPosition = Vector3.zero;

                // Ensure CanvasScaler sensible defaults
                var scaler = overlay.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }

                dialoguePanel.transform.SetParent(overlay.transform, false);

                var rt = dialoguePanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.offsetMin = new Vector2(40f, 40f);
                    rt.offsetMax = new Vector2(-40f, -120f);
                    rt.localScale = Vector3.one;
                }
            }
            else
            {
                // Already overlay → normalize scale/pos to avoid gigantic values (like 81x)
                parentCanvas.transform.localScale = Vector3.one;
                parentCanvas.transform.localPosition = Vector3.zero;

                // Also normalize CanvasScaler
                var scaler = parentCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
        }

        if (dialoguePanel) dialoguePanel.SetActive(false);
        _isOpen = false;
        if (nextButton) nextButton.onClick.AddListener(Next);
        if (closeButton) closeButton.onClick.AddListener(Close);
    }

    public void Show(List<string> lines)
    {
        // Guard invalid input
        if (lines == null || lines.Count == 0)
        {
            Close();
            return;
        }
        // Re-resolve references if needed
        if (dialoguePanel == null)
        {
            var panel = GameObject.Find("DialoguePanel");
            if (panel) dialoguePanel = panel;
        }
        if (dialogueText == null && dialoguePanel != null)
        {
            var t = dialoguePanel.transform.Find("DialogueText");
            if (t) dialogueText = t.GetComponent<TextMeshProUGUI>();
        }

        // Ensure overlay canvas at the moment of showing
        if (dialoguePanel != null)
        {
            Canvas parentCanvas = dialoguePanel.GetComponentInParent<Canvas>();
            if (parentCanvas == null || parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Canvas overlay = Object.FindObjectOfType<Canvas>(includeInactive: true);
                if (overlay == null || overlay.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    var canvasGO = new GameObject("DialogueCanvasRuntime");
                    overlay = canvasGO.AddComponent<Canvas>();
                    overlay.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                overlay.transform.localScale = Vector3.one;
                overlay.transform.localPosition = Vector3.zero;

                var scaler = overlay.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }
                dialoguePanel.transform.SetParent(overlay.transform, false);
                var rt = dialoguePanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.offsetMin = new Vector2(40f, 40f);
                    rt.offsetMax = new Vector2(-40f, -120f);
                    rt.localScale = Vector3.one;
                }
            }
            else
            {
                parentCanvas.transform.localScale = Vector3.one;
                parentCanvas.transform.localPosition = Vector3.zero;

                var scaler = parentCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }
            }
        }

        _lines = lines;
        _index = 0;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        _isOpen = true;
        Debug.Log("Dialogue Show() → active: " + (dialoguePanel != null && dialoguePanel.activeInHierarchy));
        Next();
    }

    public void Next()
    {
        if (_isTyping) { // bấm khi đang gõ → hiện ngay full dòng hiện tại
            StopAllCoroutines();
            if (_lines != null && _lines.Count > 0 && dialogueText != null)
            {
                int currentIndex = Mathf.Clamp(_index - 1, 0, _lines.Count - 1);
                dialogueText.text = _lines[currentIndex];
            }
            _isTyping = false;
            return;
        }

        if (_lines == null || _index >= _lines.Count)
        {
            Close();
            return;
        }

        DialogueManagerWithCallback.InvokeOnNext(); // Gọi callback mỗi lần Next

        StartCoroutine(TypeLine(_lines[_index]));
        _index++;
    }

    System.Collections.IEnumerator TypeLine(string line)
    {
        _isTyping = true;
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        _isTyping = false;
    }

    public void Close()
    {
        dialoguePanel.SetActive(false);
        _isOpen = false;
        _lines = null;
        DialogueManagerWithCallback.InvokeOnClose(); // Gọi callback khi đóng hội thoại
    }
}

public static class DialogueManagerWithCallback
{
    private static System.Action onCloseCallback;
    private static System.Action onNextCallback; // Thêm callback cho Next

    public static void RegisterOnClose(System.Action callback)
    {
        onCloseCallback = callback;
    }

    public static void RegisterOnNext(System.Action callback)
    {
        onNextCallback = callback;
    }

    public static void InvokeOnClose()
    {
        onCloseCallback?.Invoke();
        onCloseCallback = null;
    }

    public static void InvokeOnNext()
    {
        onNextCallback?.Invoke();
    }
}
