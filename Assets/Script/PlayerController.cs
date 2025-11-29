using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("HP Settings")]
    public float maxHP = 100f;
    public Slider hpSlider;
    public TextMeshProUGUI hpText; // Optional: để hiển thị số HP dạng text

    [Header("MP Settings")]
    public float maxMP = 100f;
    public Slider mpSlider;
    public TextMeshProUGUI mpText;

    private float currentHP;
    private float currentMP;
    private float mpRegenInterval = 2f; // Thời gian hồi MP (giây)
    private float mpRegenTimer = 0f;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = true;
    private bool isUsingSkill = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Khởi tạo HP,MP
        currentHP = maxHP;
        currentMP = maxMP;
        UpdateHPUI();
    }

    void Update()
    {
        // Di chuyển trái/phải
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Flip hướng nhân vật
        if (move > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (move < 0) transform.localScale = new Vector3(-1, 1, 1);

        // Animation di chuyển
        anim.SetFloat("Speed", Mathf.Abs(move));

        // Nhảy
        if (Input.GetButtonDown("Jump") && isGrounded && !isUsingSkill)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            anim.SetBool("isJumping", true);
        }

        // Tấn công thường
        if (Input.GetKeyDown(KeyCode.Z) && !isUsingSkill)
        {
            anim.SetBool("isAttacking", true);
        }

        // Skill 1
        if (Input.GetKeyDown(KeyCode.E) && !isUsingSkill && currentMP >= 20f)
        {
            UseMP(20f);
            Debug.Log("chieu 1 duoc kich hoat");
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 1);
            anim.SetTrigger("UseSkill");
        }

        // Skill 2
        if (Input.GetKeyDown(KeyCode.Q) && !isUsingSkill && currentMP >= 20f)
        {
            UseMP(20f);
            Debug.Log("chieu 2 duoc kich hoat");
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 2);
            anim.SetTrigger("UseSkill");
        }

        // Hồi MP theo thời gian
        mpRegenTimer += Time.deltaTime;
        if (mpRegenTimer >= mpRegenInterval)
        {
            mpRegenTimer = 0f;
            currentMP += 5f; // Hồi 5 MP mỗi lần
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);
            if (mpSlider != null)
            {
                mpSlider.maxValue = maxMP;
                mpSlider.value = currentMP;
            }
            if (mpText != null)
            {
                mpText.text = $"{currentMP:F0}/{maxMP:F0}";
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check nếu chạm đất
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("isJumping", false);
        }
    }

    // Reset attack animation
    public void EndAttack()
    {
        anim.SetBool("isAttacking", false);
    }

    // Hàm này có thể được gọi từ Animation Event khi animation skill kết thúc
    public void EndSkill()
    {
        anim.ResetTrigger("UseSkill");
        anim.SetInteger("skillIndex", 0);
        isUsingSkill = false;
    }

    // Hàm nhận sát thương
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP); // Đảm bảo HP không âm và không vượt quá maxHP
        UpdateHPUI();

        // Kiểm tra nếu HP = 0
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Hàm hồi HP
    public void Heal(float amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();
    }

    // Cập nhật UI thanh HP và số HP
    void UpdateHPUI()
    {
        // Cập nhật slider
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // Cập nhật text hiển thị số HP (nếu có)
        if (hpText != null)
        {
            hpText.text = $"{currentHP:F0}/{maxHP:F0}";
        }
    }

    // Hàm xử lý khi chết
    void Die()
    {
        Debug.Log("Player đã chết!");
        // Thêm logic xử lý khi chết ở đây (ví dụ: animation chết, respawn, game over, v.v.)
    }

    // Getter để lấy HP hiện tại
    public float GetCurrentHP()
    {
        return currentHP;
    }

    // Getter để lấy HP tối đa
    public float GetMaxHP()
    {
        return maxHP;
    }

    // Hàm sử dụng MP
    public void UseMP(float amount)
    {
        currentMP -= amount;
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        UpdateMPUI();
    }

    public void RegenerateMP(float amount)
    {
        currentMP += amount;
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        UpdateMPUI();
    }

    // Cập nhật UI thanh MP và số MP
    void UpdateMPUI()
    {
        // Cập nhật slider
        if (mpSlider != null)
        {
            mpSlider.maxValue = maxMP;
            mpSlider.value = currentMP;
        }
        // Cập nhật text hiển thị số MP (nếu có)
        if (mpText != null)
        {
            mpText.text = $"{currentMP:F0}/{maxMP:F0}";
        }
    }
    public void CastSkill1()
    {
        if (!isUsingSkill && currentMP >= 20f)
        {
            UseMP(20f);
            Debug.Log("Chiêu 1 được kích hoạt qua UI");
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 1);
            anim.SetTrigger("UseSkill");
        }
    }

    public void CastSkill2()
    {
        if (!isUsingSkill && currentMP >= 20f)
        {
            UseMP(20f);
            Debug.Log("Chiêu 2 được kích hoạt qua UI");
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 2);
            anim.SetTrigger("UseSkill");
        }
    }

}
