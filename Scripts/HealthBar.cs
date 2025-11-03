using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image fill;       // ảnh màu đỏ bên trong
    public TextMeshProUGUI text; // chữ % hiển thị
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        // Khi bắt đầu thì máu full
        currentHealth = maxHealth;
        UpdateUI();
    }

    void UpdateUI()
    {
        float percent = (float)currentHealth / maxHealth;
        fill.fillAmount = percent;
        text.text = (percent * 100).ToString("F0") + "%";
    }

    // Hàm này gọi khi bị trừ máu
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateUI();
    }

    // Hàm này gọi khi hồi máu
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

        public bool IsDead()
    {
        return currentHealth <= 0;
    }

}
