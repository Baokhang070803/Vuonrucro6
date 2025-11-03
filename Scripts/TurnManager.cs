using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public TextMeshProUGUI turnText;  // Text hiển thị chữ
    public Image turnIcon;            // Ảnh icon nhân vật

    public Sprite tiNiIcon;           // ảnh của Tí Nị
    public Sprite slimeIcon;          // ảnh của Slime

    private string currentTurn = "Tí Nị";

    void Start()
    {
        UpdateTurnUI();
    }

    public void SetTurn(string turnOwner)
    {
        currentTurn = turnOwner;
        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        if (turnText != null)
            turnText.text = "Lượt: " + currentTurn;

        if (turnIcon != null)
        {
            if (currentTurn == "Tí Nị")
                turnIcon.sprite = tiNiIcon;
            else if (currentTurn == "Slime")
                turnIcon.sprite = slimeIcon;
        }
    }

    public string GetCurrentTurn()
    {
        return currentTurn;
    }
}
