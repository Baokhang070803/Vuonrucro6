using UnityEngine;
using UnityEngine.UI;
using TMPro; // support TextMeshPro

// Simple singleton to manage background music and provide a UI toggle.
public class MusicManager : MonoBehaviour
{
    public static MusicManager I;

    // Assign an AudioSource (looping background music) on this GameObject or via Inspector
    public AudioSource musicSource;

    // Optional UI button and text to control music
    public Button toggleButton;
    // (text fields removed — icon-only mode)

    // New: icon support
    public UnityEngine.UI.Image toggleButtonIcon;
    public Sprite iconOn;
    public Sprite iconOff;

    private bool muted = false;
    private const string PREF_KEY = "MusicMuted";

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        muted = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        ApplyMute();

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleMusic);

            // If the button contains legacy Text or TMP child objects, destroy them so only icon remains
            var legacyTexts = toggleButton.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var t in legacyTexts)
            {
                if (t != null && t.gameObject != null)
                    Destroy(t.gameObject);
            }

            var tmpTexts = toggleButton.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var t in tmpTexts)
            {
                if (t != null && t.gameObject != null)
                    Destroy(t.gameObject);
            }
        }

        UpdateButtonText();
    }

    public void ToggleMusic()
    {
        muted = !muted;
        ApplyMute();
        PlayerPrefs.SetInt(PREF_KEY, muted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateButtonText();
    }

    public void SetMusicOn(bool on)
    {
        muted = !on;
        ApplyMute();
        PlayerPrefs.SetInt(PREF_KEY, muted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateButtonText();
    }

    private void ApplyMute()
    {
        if (musicSource != null)
            musicSource.mute = muted;
    }

    private void UpdateButtonText()
    {
        // Icon-only: update icon if available, otherwise do nothing
        UnityEngine.UI.Image icon = toggleButtonIcon != null ? toggleButtonIcon : (toggleButton != null ? toggleButton.image : null);
        if (icon != null && iconOn != null && iconOff != null)
        {
            icon.sprite = muted ? iconOff : iconOn;
        }
        // no text handling — icon only
    }
}
