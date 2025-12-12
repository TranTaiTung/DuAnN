using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkeletonAI : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public float moveSpeed = 2f;
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Điểm tuần tra")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    private Transform currentPatrolTarget;

    [Header("Phạm vi")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;

    [Header("Tấn công")]
    public int damage = 15;
    public float attackCooldown = 2f;
    private bool isAttacking = false;

    [Header("Tham chiếu")]
    public Transform player;
    public Slider hpSlider; // thanh máu
    private Animator anim;
    private Rigidbody2D rb;

    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentPatrolTarget = patrolPointA;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // Nếu đang attack thì không làm gì khác
        if (isAttacking) return;

        if (distToPlayer <= attackRange)
        {
            // Chỉ attack nếu Enemy đã đứng gần Player (không còn di chuyển chase)
            rb.linearVelocity = Vector2.zero; // dừng di chuyển
            anim.SetBool("isWalking", false);
            StartCoroutine(AttackSequence());
        }
        else if (distToPlayer <= detectionRange)
        {
            // Chase Player nhưng KHÔNG gọi attack
            ChasePlayer();
        }
        else
        {
            PatrolLogic();
        }

    }

    void PatrolLogic()
    {
        anim.SetBool("isWalking", true);
        transform.position = Vector2.MoveTowards(transform.position, currentPatrolTarget.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentPatrolTarget.position) < 0.2f)
        {
            currentPatrolTarget = (currentPatrolTarget == patrolPointA) ? patrolPointB : patrolPointA;
        }

        Flip(currentPatrolTarget.position.x);
    }

    void ChasePlayer()
    {
        anim.SetBool("isWalking", true);

        // Enemy di chuyển về phía Player
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        Flip(player.position.x);

        // Đảm bảo không bật animation Attack khi chase
        anim.ResetTrigger("TriggerAttack1");
        anim.ResetTrigger("TriggerAttack2");

    }

    void Flip(float targetX)
    {
        if (targetX > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        anim.SetBool("isWalking", false);

        // ATK1
        anim.SetTrigger("TriggerAttack1");
        yield return new WaitForSeconds(0.5f);
        if (!isDead) DealDamageToPlayer();

        if (isDead) { isAttacking = false; yield break; }
        yield return new WaitForSeconds(attackCooldown);

        // ATK2
        anim.SetTrigger("TriggerAttack2");
        yield return new WaitForSeconds(0.5f);
        if (!isDead) DealDamageToPlayer();

        if (isDead) { isAttacking = false; yield break; }
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    void DealDamageToPlayer()
    {
        if (isDead) return;
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(damage);
                Debug.Log("Enemy gây " + damage + " sát thương lên Player!");
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        anim.SetTrigger("TriggerHurt");

        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

    }

    void Die()
    {
        isDead = true;
        isAttacking = false;
        StopAllCoroutines(); // dừng các chuỗi tấn công đang chạy
        anim.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;      // giữ nguyên vị trí, không rơi khỏi map
        rb.simulated = false;      // tắt physics để enemy đứng yên khi chết
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        if (hpSlider != null)
        {
            Destroy(hpSlider.gameObject); // thanh máu biến mất cùng Enemy
        }

        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.AddEnergy(20f); // cộng năng lượng cho Player
        }

        // Thông báo Enemy đã chết cho KillCounter
        if (EnemyKillCounter.Instance != null)
        {
            EnemyKillCounter.Instance.AddKill();
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void TakePercentDamage(float percent)
    {
        float damageAmount = maxHealth * percent;
        TakeDamage(damageAmount);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
