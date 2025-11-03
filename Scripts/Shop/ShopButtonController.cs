using UnityEngine;
using UnityEngine.UI;

public class ShopButtonController : MonoBehaviour
{
    [Header("Tham chiếu")]
    public ShopUI shopUI;
    public Button shopButton;
    
    private void Start()
    {
        // Tự động tìm nút nếu chưa gán
        if (shopButton == null)
        {
            shopButton = GetComponent<Button>();
        }
        
        // Tự động tìm ShopUI nếu chưa gán
        if (shopUI == null)
        {
            shopUI = FindObjectOfType<ShopUI>();
        }
        
        // Gán sự kiện click
        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OpenShop);
        }
    }
    
    public void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.OpenShop();
            Debug.Log("Mở shop từ nút!");
        }
        else
        {
            Debug.LogError("ShopUI không được gán!");
        }
    }
    
    public void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.CloseShop();
            Debug.Log("Đóng shop từ nút!");
        }
    }
    
    private void OnDestroy()
    {
        if (shopButton != null)
        {
            shopButton.onClick.RemoveListener(OpenShop);
        }
    }
}
