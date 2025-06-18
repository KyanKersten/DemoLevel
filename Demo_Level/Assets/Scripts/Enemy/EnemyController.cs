using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float rollForce = 6f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float rollCooldown = 2f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float jumpCooldown = 2f;
    [SerializeField] Collider2D attackHitbox;
    [SerializeField] float attackHitboxActiveTime = 0.2f;
    [SerializeField] SpriteRenderer hitboxVisual;
    [SerializeField] Transform player;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool rolling = false;
    private float rollTimer = 0f;
    private float attackTimer = 0f;
    private float jumpTimer = 0f;
    private int facingDirection = 1;
    private bool grounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        rollTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        // Flip sprite and hitbox
        if (dir > 0)
        {
            spriteRenderer.flipX = false;
            facingDirection = 1;
            if (attackHitbox != null)
                attackHitbox.transform.localPosition = new Vector3(
                    Mathf.Abs(attackHitbox.transform.localPosition.x),
                    attackHitbox.transform.localPosition.y,
                    attackHitbox.transform.localPosition.z
                );
        }
        else
        {
            spriteRenderer.flipX = true;
            facingDirection = -1;
            if (attackHitbox != null)
                attackHitbox.transform.localPosition = new Vector3(
                    -Mathf.Abs(attackHitbox.transform.localPosition.x),
                    attackHitbox.transform.localPosition.y,
                    attackHitbox.transform.localPosition.z
                );
        }

        if (!rolling)
        {
            // Try to jump over the player if close and grounded
            if (distance < attackRange * 1.5f && grounded && jumpTimer <= 0f && Random.value > 0.7f)
            {
                float jumpDir = -facingDirection; // Jump over to the other side
                rb.linearVelocity = new Vector2(jumpDir * moveSpeed * 1.2f, jumpForce);
                animator.SetTrigger("Jump");
                grounded = false;
                jumpTimer = jumpCooldown;
            }
            else if (distance > attackRange)
            {
                rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
                animator.SetInteger("AnimState", 1);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetInteger("AnimState", 0);

                if (attackTimer <= 0f && Random.value > 0.5f)
                {
                    animator.SetTrigger("Attack1");
                    attackTimer = attackCooldown;
                    if (attackHitbox != null)
                        StartCoroutine(EnableHitboxTemporarily());
                }
                else if (rollTimer <= 0f)
                {
                    rolling = true;
                    animator.SetTrigger("Roll");
                    rb.linearVelocity = new Vector2(facingDirection * rollForce, rb.linearVelocity.y);
                    rollTimer = rollCooldown;
                    StartCoroutine(EndRoll());
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            grounded = true;
            animator.SetBool("Grounded", true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        grounded = false;
        animator.SetBool("Grounded", false);
    }

    IEnumerator EndRoll()
    {
        yield return new WaitForSeconds(0.5f);
        rolling = false;
    }

    IEnumerator EnableHitboxTemporarily()
    {
        attackHitbox.enabled = true;
        if (hitboxVisual != null)
            hitboxVisual.enabled = true;
        yield return new WaitForSeconds(attackHitboxActiveTime);
        attackHitbox.enabled = false;
        if (hitboxVisual != null)
            hitboxVisual.enabled = false;
    }
}