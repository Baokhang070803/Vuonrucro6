using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [Header("Tham chiếu")]
    public ShopUI shopUI;
    public GameObject shopIndicator; // Hiển thị khi có thể mở shop
    
    [Header("Cài đặt")]
    public float interactionRange = 2f;
    public KeyCode interactionKey = KeyCode.E;
    
    private bool isPlayerInRange = false;
    private bool isShopOpen = false;
    
    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleShop();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowShopIndicator(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ShowShopIndicator(false);
            
            if (isShopOpen)
            {
                CloseShop();
            }
        }
    }
    
    private void ToggleShop()
    {
        if (isShopOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }
    
    private void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.OpenShop();
            isShopOpen = true;
            Debug.Log("Mở shop");
        }
    }
    
    private void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.CloseShop();
            isShopOpen = false;
            Debug.Log("Đóng shop");
        }
    }
    
    private void ShowShopIndicator(bool show)
    {
        if (shopIndicator != null)
        {
            shopIndicator.SetActive(show);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Vẽ phạm vi tương tác trong Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
