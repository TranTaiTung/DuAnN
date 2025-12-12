using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("HP Settings")]
    public float maxHP = 100f;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("MP Settings")]
    public float maxMP = 100f;
    public Slider mpSlider;
    public TextMeshProUGUI mpText;

    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy = 0f;
    public Slider energySlider;
    private bool isBuffActive = false;
    private float buffDuration = 15f;
    private Coroutine buffCoroutine;

    [Header("Combat Settings")]
    public float normalAttackDamage = 10f;
    public float skill1Damage = 15f;
    public float skill2Damage = 20f;
    [Tooltip("Layer(s) chứa Enemy để OverlapCircle chỉ quét mục tiêu hợp lệ")]
    public LayerMask enemyLayer;

    [Header("Skill Cooldowns")]
    public float skill1Cooldown = 12f;
    public float skill2Cooldown = 15f;

    private float skill1Timer = 0f;
    private float skill2Timer = 0f;
    public float attackRange = 1.5f;

    // Tham chiếu tới UI button và text
    public Button skill1Button;
    public TextMeshProUGUI skill1Text;

    public Button skill2Button;
    public TextMeshProUGUI skill2Text;

    //UI Lose
    public GameObject loseImage;

    private float currentHP;
    private float currentMP;
    private float mpRegenInterval = 2f;
    private float mpRegenTimer = 0f;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = true;
    private bool isUsingSkill = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currentHP = maxHP;
        currentMP = maxMP;
        UpdateHPUI();
        UpdateMPUI();
        UpdateEnergyUI();
    }

    void Update()
    {
        // Không cho phép điều khiển nếu đã chết
        if (isDead) return;

        // Di chuyển
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        if (move > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (move < 0) transform.localScale = new Vector3(-1, 1, 1);

        anim.SetFloat("Speed", Mathf.Abs(move));

        // Nhảy
        if (Input.GetButtonDown("Jump") && isGrounded && !isUsingSkill)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            anim.SetBool("isJumping", true);
        }

        // Attack thường
        if (Input.GetMouseButtonDown(0) && !isUsingSkill)
        {
            anim.SetTrigger("Attack");
        }

        // Giảm timer theo thời gian
        if (skill1Timer > 0)
        {
            skill1Timer -= Time.deltaTime;
            skill1Text.text = Mathf.Ceil(skill1Timer).ToString(); // hiện số giây còn lại
            skill1Button.interactable = false;
        }
        else
        {
            skill1Text.text = ""; // hết cooldown thì xóa text
            skill1Button.interactable = true;
        }

        if (skill2Timer > 0)
        {
            skill2Timer -= Time.deltaTime;
            skill2Text.text = Mathf.Ceil(skill2Timer).ToString();
            skill2Button.interactable = false;
        }
        else
        {
            skill2Text.text = "";
            skill2Button.interactable = true;
        }

        // Skill 1
        if (Input.GetKeyDown(KeyCode.E) && !isUsingSkill && currentMP >= 20f && skill1Timer <= 0)
        {
            UseMP(20f);
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 1);
            anim.SetTrigger("UseSkill");

            TryDealDamageToEnemies(skill1Damage);

            skill1Timer = skill1Cooldown; // bắt đầu hồi chiêu
        }

        // Skill 2
        if (Input.GetKeyDown(KeyCode.Q) && !isUsingSkill && currentMP >= 20f && skill2Timer <= 0)
        {
            UseMP(20f);
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 2);
            anim.SetTrigger("UseSkill");

            TryDealDamageToEnemies(skill2Damage);

            skill2Timer = skill2Cooldown; // bắt đầu hồi chiêu
        }

        // Hồi MP
        mpRegenTimer += Time.deltaTime;
        if (mpRegenTimer >= mpRegenInterval)
        {
            mpRegenTimer = 0f;
            RegenerateMP(5f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("isJumping", false);
        }
    }

    // Gọi từ Animation Event khi Attack animation chạm
    public void DealAttackDamage()
    {
        TryDealDamageToEnemies(normalAttackDamage);
    }

    public void EndSkill()
    {
        anim.ResetTrigger("UseSkill");
        anim.SetInteger("skillIndex", 0);
        isUsingSkill = false;
    }

    // Gây sát thương lên các enemy trong phạm vi tấn công (hỗ trợ nhiều loại enemy)
    void TryDealDamageToEnemies(float damage)
    {
        int mask = enemyLayer.value;
        if (mask == 0) mask = Physics2D.DefaultRaycastLayers; // tránh bỏ sót nếu chưa cấu hình layer

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, mask);
        if (hits.Length == 0) return;

        // Tránh double-hit cùng một GameObject nếu có nhiều collider
        HashSet<GameObject> processed = new HashSet<GameObject>();

        foreach (Collider2D hit in hits)
        {
            GameObject target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
            if (target == gameObject || processed.Contains(target)) continue;
            processed.Add(target);

            if (target.TryGetComponent(out SkeletonAI skeleton))
            {
                skeleton.TakeDamage(damage);
                continue;
            }

            if (target.TryGetComponent(out FlyingEnemy flying))
            {
                flying.TakeHit(Mathf.RoundToInt(damage));
            }
        }
    }

    public void TakeDamage(float damage)
    {
        // Không nhận sát thương nếu đã chết
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        // Ngăn chặn gọi Die() nhiều lần
        if (isDead) return;
        
        isDead = true;
        
        // Dừng di chuyển
        rb.linearVelocity = Vector2.zero;
        
        // Chạy animation Death (sử dụng bool thay vì trigger để tránh lặp lại)
        anim.SetBool("isDead", true);
        anim.SetTrigger("Death");
    }

    public void OnDeathAnimationEnd()
    {
        // Hiện YOU LOSE và giữ player lại (không destroy để UI không bị phá nếu là con)
        if (loseImage != null)
        {
            loseImage.SetActive(true);
            Time.timeScale = 0f; // dừng game
        }
    }


    IEnumerator ShowLoseImage(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (loseImage != null)
        {
            loseImage.SetActive(true);
            Time.timeScale = 0f; // dừng toàn bộ game
        }
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f; // đảm bảo thời gian trở lại bình thường khi về menu
        SceneManager.LoadScene("Menu Screen");
    }

    public void AgainGame()
    {
        Time.timeScale = 1f; // đảm bảo thời gian trở lại bình thường khi chơi lại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Heal(float amount)
    {
        // Không hồi máu nếu đã chết
        if (isDead) return;
        
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();
    }

    void UpdateHPUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        if (hpText != null) hpText.text = $"{currentHP:F0}/{maxHP:F0}";
    }

    void UpdateMPUI()
    {
        if (mpSlider != null)
        {
            mpSlider.maxValue = maxMP;
            mpSlider.value = currentMP;
        }
        if (mpText != null) mpText.text = $"{currentMP:F0}/{maxMP:F0}";
    }

    void UpdateEnergyUI()
    {
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
        }
    }

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

    public void AddEnergy(float amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        UpdateEnergyUI();

        if (currentEnergy >= maxEnergy && !isBuffActive)
        {
            buffCoroutine = StartCoroutine(ApplyBuff());
        }
    }

    IEnumerator ApplyBuff()
    {
        isBuffActive = true;

        float originalMaxHP = maxHP;
        float originalMaxMP = maxMP;
        float originalMoveSpeed = moveSpeed;
        float originalNormalAttackDamage = normalAttackDamage;
        float originalSkill1Damage = skill1Damage;
        float originalSkill2Damage = skill2Damage;

        // Buff 20%
        maxHP *= 1.2f;
        currentHP = maxHP;
        maxMP *= 1.2f;
        currentMP = maxMP;
        moveSpeed *= 1.2f;
        normalAttackDamage *= 1.2f;
        skill1Damage *= 1.2f;
        skill2Damage *= 1.2f;

        UpdateHPUI();
        UpdateMPUI();

        Debug.Log("Buff 20% ATK, HP, MP trong 15s!");

        yield return new WaitForSeconds(buffDuration);

        // Reset về giá trị gốc
        maxHP = originalMaxHP;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        maxMP = originalMaxMP;
        currentMP = Mathf.Clamp(currentMP, 0, maxMP);
        moveSpeed = originalMoveSpeed;
        normalAttackDamage = originalNormalAttackDamage;
        skill1Damage = originalSkill1Damage;
        skill2Damage = originalSkill2Damage;

        UpdateHPUI();
        UpdateMPUI();

        currentEnergy = 0f;
        UpdateEnergyUI();

        isBuffActive = false;
        Debug.Log("Buff kết thúc, chỉ số quay về bình thường. Năng lượng reset về 0.");
    }
    public void ApplyBleed(int damagePerSecond, int duration)
    {
        StartCoroutine(BleedCoroutine(damagePerSecond, duration));
    }

    IEnumerator BleedCoroutine(int dps, int duration)
    {
        for (int i = 0; i < duration; i++)
        {
            if (isDead) yield break; // Dừng nếu đã chết
            TakeDamage(dps); // hoặc gọi hàm giảm máu của bạn
            yield return new WaitForSeconds(1f);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Vẽ một hình tròn tại vị trí Player với bán kính là attackRange.
        // Điều này chỉ hiển thị khi bạn chọn Player GameObject.
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
