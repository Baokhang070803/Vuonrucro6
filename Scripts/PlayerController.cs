using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Health Bars")]
    public HealthBar slimeHealthBar;
    public HealthBar playerHealthBar;

    [Header("Tí Nị Skill")]
    public VideoPlayer playerSkillVideo;
    public GameObject playerRawImage;

    [Header("Slime Skill 1")]
    public VideoPlayer slimeSkillVideo1;
    public GameObject slimeRawImage1;

    [Header("Slime Skill 2")]
    public VideoPlayer slimeSkillVideo2;
    public GameObject slimeRawImage2;

    [Header("Intro 2")]
    public VideoPlayer intro2Video;     // gán IntroVideoPlayer (Video intro2)
    public GameObject intro2RawImage;   // gán RawImage của intro2

    [Header("Turn Manager")]
    public TurnManager turnManager;

    private bool isPlayingSkill = false;
    private int playerAttackCount = 0;

    void Start()
    {
        if (playerRawImage != null) playerRawImage.SetActive(false);
        if (playerSkillVideo != null) playerSkillVideo.gameObject.SetActive(false);

        if (slimeRawImage1 != null) slimeRawImage1.SetActive(false);
        if (slimeSkillVideo1 != null) slimeSkillVideo1.gameObject.SetActive(false);

        if (slimeRawImage2 != null) slimeRawImage2.SetActive(false);
        if (slimeSkillVideo2 != null) slimeSkillVideo2.gameObject.SetActive(false);

        if (intro2RawImage != null) intro2RawImage.SetActive(false);
        if (intro2Video != null) intro2Video.gameObject.SetActive(false);

        if (turnManager != null)
            turnManager.SetTurn("Tí Nị");
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame && !isPlayingSkill)
        {
            if (turnManager != null && turnManager.GetCurrentTurn() == "Tí Nị")
            {
                PlayPlayerSkill();
            }
        }
    }

    // ================== TÍ NỊ ==================
    void PlayPlayerSkill()
    {
        isPlayingSkill = true;

        // set canvas video chieu1 lên trên cùng
        Canvas playerCanvas = playerRawImage.GetComponentInParent<Canvas>();
        if (playerCanvas != null) playerCanvas.sortingOrder = 9999;

        playerRawImage.SetActive(true);
        playerSkillVideo.gameObject.SetActive(true);
        playerSkillVideo.Play();

        playerSkillVideo.loopPointReached += OnPlayerSkillFinished;
    }

    void OnPlayerSkillFinished(VideoPlayer vp)
    {
        int damage = Mathf.RoundToInt(slimeHealthBar.maxHealth * 0.1f);
        slimeHealthBar.TakeDamage(damage);

        playerSkillVideo.loopPointReached -= OnPlayerSkillFinished;

        playerRawImage.SetActive(false);
        playerSkillVideo.gameObject.SetActive(false);

        isPlayingSkill = false;
        playerAttackCount++;

        if (turnManager != null)
            turnManager.SetTurn("Slime");

        if (playerAttackCount == 1)
            StartCoroutine(SlimeCounterAttack1());
        else if (playerAttackCount == 2)
            StartCoroutine(SlimeCounterAttack2());
    }

    // ================== SLIME SKILL 1 ==================
    IEnumerator SlimeCounterAttack1()
    {
        yield return new WaitForSeconds(3f);

        Canvas slimeCanvas1 = slimeRawImage1.GetComponentInParent<Canvas>();
        if (slimeCanvas1 != null) slimeCanvas1.sortingOrder = 9999;

        slimeRawImage1.SetActive(true);
        slimeSkillVideo1.gameObject.SetActive(true);
        slimeSkillVideo1.Play();

        slimeSkillVideo1.loopPointReached += OnSlimeSkill1Finished;
    }

    void OnSlimeSkill1Finished(VideoPlayer vp)
    {
        int damage = Mathf.RoundToInt(playerHealthBar.maxHealth * 35 / 100);
        playerHealthBar.TakeDamage(damage);

        slimeSkillVideo1.loopPointReached -= OnSlimeSkill1Finished;

        slimeRawImage1.SetActive(false);
        slimeSkillVideo1.gameObject.SetActive(false);

        if (turnManager != null)
            turnManager.SetTurn("Tí Nị");
    }

    // ================== SLIME SKILL 2 ==================
    IEnumerator SlimeCounterAttack2()
    {
        yield return new WaitForSeconds(3f);

        Canvas slimeCanvas2 = slimeRawImage2.GetComponentInParent<Canvas>();
        if (slimeCanvas2 != null) slimeCanvas2.sortingOrder = 9999;

        slimeRawImage2.SetActive(true);
        slimeSkillVideo2.gameObject.SetActive(true);
        slimeSkillVideo2.Play();

        slimeSkillVideo2.loopPointReached += OnSlimeSkill2Finished;
    }

    void OnSlimeSkill2Finished(VideoPlayer vp)
    {
        int damage = Mathf.RoundToInt(playerHealthBar.maxHealth * 50 / 100);
        playerHealthBar.TakeDamage(damage);

        slimeSkillVideo2.loopPointReached -= OnSlimeSkill2Finished;

        slimeRawImage2.SetActive(false);
        slimeSkillVideo2.gameObject.SetActive(false);

        // Sau khi Slime đánh xong → chờ 2s rồi chiếu intro2
        StartCoroutine(DelayPlayIntro2());
    }

    // ================== INTRO 2 ==================
    IEnumerator DelayPlayIntro2()
    {
        yield return new WaitForSeconds(2f);
        PlayIntro2();
    }

    void PlayIntro2()
    {
        Canvas introCanvas = intro2RawImage.GetComponentInParent<Canvas>();
        if (introCanvas != null) introCanvas.sortingOrder = 9999;

        intro2RawImage.SetActive(true);
        intro2Video.gameObject.SetActive(true);
        intro2Video.Play();

        intro2Video.loopPointReached += OnIntro2Finished;
    }

    void OnIntro2Finished(VideoPlayer vp)
    {
        intro2Video.loopPointReached -= OnIntro2Finished;

        intro2RawImage.SetActive(false);
        intro2Video.gameObject.SetActive(false);

        // Hoàn thành nhiệm vụ trận chiến cuối cùng
        QuestManager.CompleteCurrentQuest("Trận chiến cuối cùng");
        
        // Set flag để biết vừa hoàn thành combat
        PlayerPrefs.SetString("JustFinishedCombat", "true");
        PlayerPrefs.Save();

        // Chuyển qua Loading screen trước khi về map1
        LoadingManager.NEXT_SCENE = "map1";
        SceneManager.LoadScene("Loading");
    }
}
