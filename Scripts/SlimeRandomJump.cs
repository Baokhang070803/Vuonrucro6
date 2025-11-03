using System.Collections;
using UnityEngine;

public class SlimeRandomJump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 300f;          // Lực nhảy
    public float minJumpDelay = 0.8f;       // Thời gian chờ tối thiểu giữa các lần nhảy
    public float maxJumpDelay = 1.2f;       // Thời gian chờ tối đa giữa các lần nhảy
    
    [Header("Movement Range")]
    public float moveRadius = 5f;           // Phạm vi di chuyển từ vị trí ban đầu
    public Transform centerPoint;           // Điểm trung tâm (nếu không set thì dùng vị trí ban đầu)
    
    [Header("Ground Check")]
    public LayerMask groundLayerMask = 1;   // Layer của ground
    public float groundCheckDistance = 0.1f; // Khoảng cách check ground
    
    private Rigidbody2D rb;
    private Vector3 initialPosition;        // Vị trí ban đầu
    private Vector3 centerPosition;         // Vị trí trung tâm để tính phạm vi
    private bool isGrounded = true;
    private Animator animator;
    private bool hasIsGroundedParam = false; // Cache parameter check
    private bool parameterChecked = false;   // Đã kiểm tra parameter chưa
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Lưu vị trí ban đầu
        initialPosition = transform.position;
        centerPosition = centerPoint != null ? centerPoint.position : initialPosition;
        
        // Bắt đầu coroutine nhảy ngẫu nhiên
        StartCoroutine(RandomJumpRoutine());
    }
    
    void Update()
    {
        CheckGrounded();
    }
    
    void CheckGrounded()
    {
        // Kiểm tra xem slime có đang trên mặt đất không
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayerMask);
        isGrounded = hit.collider != null;
        
        // Cập nhật animation nếu có và parameter tồn tại
        if (animator != null)
        {
            // Chỉ kiểm tra parameter 1 lần duy nhất
            if (!parameterChecked)
            {
                parameterChecked = true;
                for (int i = 0; i < animator.parameterCount; i++)
                {
                    if (animator.GetParameter(i).name == "IsGrounded")
                    {
                        hasIsGroundedParam = true;
                        break;
                    }
                }
                
                if (!hasIsGroundedParam)
                {
                    Debug.LogWarning($"Parameter 'IsGrounded' not found in Animator Controller for {gameObject.name}. Animation will not be updated.");
                }
            }
            
            // Chỉ set parameter nếu nó tồn tại
            if (hasIsGroundedParam)
            {
                animator.SetBool("IsGrounded", isGrounded);
            }
        }
    }
    
    IEnumerator RandomJumpRoutine()
    {
        while (true)
        {
            // Chờ thời gian ngẫu nhiên
            float waitTime = Random.Range(minJumpDelay, maxJumpDelay);
            yield return new WaitForSeconds(waitTime);
            
            // Chỉ nhảy khi đang trên mặt đất
            if (isGrounded)
            {
                Jump();
            }
        }
    }
    
    void Jump()
    {
        if (rb == null) return;
        
        // Tạo điểm đích ngẫu nhiên trong phạm vi
        Vector2 randomDirection = GetRandomDirectionInRange();
        
        // Reset velocity trước khi nhảy
        rb.linearVelocity = Vector2.zero;
        
        // Áp dụng lực nhảy
        Vector2 jumpVector = new Vector2(randomDirection.x, 1f).normalized * jumpForce;
        rb.AddForce(jumpVector);
        
        // Trigger animation nhảy nếu có
        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
        
        Debug.Log($"Slime nhảy về hướng: {randomDirection}");
    }
    
    Vector2 GetRandomDirectionInRange()
    {
        Vector3 currentPos = transform.position;
        Vector2 randomDirection;
        
        // Thử tối đa 10 lần để tìm hướng hợp lệ
        for (int i = 0; i < 10; i++)
        {
            // Tạo hướng ngẫu nhiên
            randomDirection = Random.insideUnitCircle.normalized;
            
            // Tính vị trí đích dự kiến
            Vector3 targetPos = currentPos + new Vector3(randomDirection.x, 0, 0) * 2f;
            
            // Kiểm tra xem có nằm trong phạm vi không
            float distanceFromCenter = Vector3.Distance(targetPos, centerPosition);
            
            if (distanceFromCenter <= moveRadius)
            {
                return randomDirection;
            }
        }
        
        // Nếu không tìm được hướng hợp lệ, nhảy về phía trung tâm
        Vector3 directionToCenter = (centerPosition - currentPos).normalized;
        return new Vector2(directionToCenter.x, 0);
    }
    
    // Vẽ gizmo để hiển thị phạm vi di chuyển trong Scene view
    void OnDrawGizmosSelected()
    {
        Vector3 center = centerPoint != null ? centerPoint.position : 
                        (Application.isPlaying ? centerPosition : transform.position);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, moveRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, 0.2f);
    }
    
    // Phương thức public để gọi nhảy từ bên ngoài
    public void ForceJump()
    {
        if (isGrounded)
        {
            Jump();
        }
    }
    
    // Phương thức để thay đổi phạm vi di chuyển
    public void SetMoveRadius(float newRadius)
    {
        moveRadius = newRadius;
    }
    
    // Phương thức để thay đổi điểm trung tâm
    public void SetCenterPoint(Transform newCenter)
    {
        centerPoint = newCenter;
        centerPosition = newCenter != null ? newCenter.position : initialPosition;
    }
}