using System.Collections;
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

    private float currentHP;
    private float currentMP;
    private float mpRegenInterval = 2f;
    private float mpRegenTimer = 0f;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = true;
    private bool isUsingSkill = false;

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

        // Skill 1
        if (Input.GetKeyDown(KeyCode.E) && !isUsingSkill && currentMP >= 20f)
        {
            UseMP(20f);
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 1);
            anim.SetTrigger("UseSkill");

            SkeletonAI enemy = FindAnyObjectByType<SkeletonAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(skill1Damage); // gây 15 damage
            }
        }

        // Skill 2
        if (Input.GetKeyDown(KeyCode.Q) && !isUsingSkill && currentMP >= 20f)
        {
            UseMP(20f);
            isUsingSkill = true;
            anim.SetInteger("skillIndex", 2);
            anim.SetTrigger("UseSkill");

            SkeletonAI enemy = FindAnyObjectByType<SkeletonAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(skill2Damage); // gây 20 damage
            }
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
        SkeletonAI enemy = FindAnyObjectByType<SkeletonAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(normalAttackDamage); // gây 10 damage
        }
    }

    public void EndSkill()
    {
        anim.ResetTrigger("UseSkill");
        anim.SetInteger("skillIndex", 0);
        isUsingSkill = false;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        UpdateHPUI();
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Player đã chết!");
    }

    public void Heal(float amount)
    {
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

}
