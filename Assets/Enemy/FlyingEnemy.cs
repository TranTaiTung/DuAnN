using UnityEngine;
using UnityEngine.UI;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    private int currentHealth;
    public float moveSpeed = 3f;
    public float patrolRange = 5f;
    public float attackRange = 1.5f;
    public float detectRange = 7f;
    public float attackCooldown = 2f;

    [Header("Damage Stats")]
    public int baseDamage = 10;
    public int bleedDamage = 2;
    public int bleedDuration = 5;

    [Header("References")]
    public Transform player;
    public Slider hpSlider;   // Slider thanh máu gắn trực tiếp
    public Animator anim;

    private Vector3 startPoint;
    private Vector3 patrolPoint;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        startPoint = transform.position;
        GetNewPatrolPoint();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < detectRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
            else
            {
                MoveTowards(player.position);
            }
        }
        else
        {
            Patrol();
        }

        FlipSprite(player.position);
    }

    void Patrol()
    {
        MoveTowards(patrolPoint);

        if (Vector3.Distance(transform.position, patrolPoint) < 0.2f)
        {
            GetNewPatrolPoint();
        }
    }

    void GetNewPatrolPoint()
    {
        Vector2 randomPoint = Random.insideUnitCircle * patrolRange;
        patrolPoint = startPoint + new Vector3(randomPoint.x, randomPoint.y, 0);
    }

    void MoveTowards(Vector3 targetPos)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        // Animation bay (Flight) có thể set bằng Blend Tree hoặc bool
    }

    void AttackPlayer()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("Attack"); // Trigger Attack
            lastAttackTime = Time.time;
        }
    }

    // Hàm này gọi từ Animation Event trong clip Attack
    public void DealDamage()
    {
        if (isDead) return;

        PlayerController playerHealth = player.GetComponent<PlayerController>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage((float)baseDamage);
            playerHealth.ApplyBleed(bleedDamage, bleedDuration);
        }
    }

    public void TakeHit(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (hpSlider != null)
            hpSlider.value = currentHealth;

        anim.SetTrigger("TakeHit"); // Trigger TakeHit

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Death"); // Trigger Death
        Destroy(gameObject, 0.5f); // Hủy sau 2s để animation chạy xong
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.AddEnergy(25f); // cộng năng lượng cho Player
        }

        // Thông báo Enemy đã chết cho KillCounter
        if (EnemyKillCounter.Instance != null)
        {
            EnemyKillCounter.Instance.AddKill();
        }
    }

    void FlipSprite(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPoint == Vector3.zero ? transform.position : startPoint, patrolRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
