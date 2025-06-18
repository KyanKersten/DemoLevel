using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7.5f;
    [SerializeField] float rollForce = 6.0f;
    [SerializeField] Collider2D attackHitbox; 
    [SerializeField] float attackHitboxActiveTime = 0.2f;
    [SerializeField] SpriteRenderer hitboxVisual;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool grounded = false;
    private float delayToIdle = 0f;
    private float inputX = 0f;
    private bool jumpPressed = false;
    private int currentAttack = 0;
    private float timeSinceAttack = 0f;
    private bool rolling = false;
    private int facingDirection = 1;
    private float rollDuration = 8.0f / 14.0f;
    private float rollCurrentTime = 0f;
    private float hitboxOriginalLocalX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        
        if (attackHitbox != null)
            attackHitbox.enabled = false;
        hitboxOriginalLocalX = attackHitbox.transform.localPosition.x;
    }

    void Update()
    {
        timeSinceAttack += Time.deltaTime;
        if (rolling)
            rollCurrentTime += Time.deltaTime;
        if (rollCurrentTime > rollDuration)
            rolling = false;

        inputX = Input.GetAxisRaw("Horizontal");

        // Flip sprite
        // After flipping the sprite
        if (inputX > 0)
        {
            spriteRenderer.flipX = false;
            facingDirection = 1;
        }
        else if (inputX < 0)
        {
            spriteRenderer.flipX = true;
            facingDirection = -1;
        }
        
        if (attackHitbox != null)
        {
            attackHitbox.transform.localPosition = new Vector3(
                Mathf.Abs(hitboxOriginalLocalX) * facingDirection,
                attackHitbox.transform.localPosition.y,
                attackHitbox.transform.localPosition.z
            );
        }

        // Attack combo
        if (Input.GetMouseButtonDown(0) && timeSinceAttack > 0.25f && !rolling)
        {
            currentAttack++;
            if (currentAttack > 3)
                currentAttack = 1;
            if (timeSinceAttack > 1.0f)
                currentAttack = 1;
            animator.SetTrigger("Attack" + currentAttack);
            timeSinceAttack = 0f;
            
            if (attackHitbox != null)
                StartCoroutine(EnableHitboxTemporarily());
        }

        // Block
        if (Input.GetMouseButtonDown(1) && !rolling)
        {
            animator.SetTrigger("Block");
            animator.SetBool("IdleBlock", true);
        }
        if (Input.GetMouseButtonUp(1))
        {
            animator.SetBool("IdleBlock", false);
        }

        // Roll
        if (Input.GetKeyDown(KeyCode.LeftShift) && !rolling)
        {
            rolling = true;
            rollCurrentTime = 0f;
            animator.SetTrigger("Roll");
            rb.linearVelocity = new Vector2(facingDirection * rollForce, rb.linearVelocity.y);
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
            jumpPressed = true;

        // Animations
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            delayToIdle = 0.05f;
            animator.SetInteger("AnimState", 1);
        }
        else
        {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle < 0)
                animator.SetInteger("AnimState", 0);
        }
    }

    void FixedUpdate()
    {
        if (!rolling)
            rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);

        if (jumpPressed && grounded && !rolling)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            grounded = false;
            animator.SetBool("Grounded", false);
            animator.SetTrigger("Jump");
        }

        animator.SetFloat("AirSpeedY", rb.linearVelocity.y);
        jumpPressed = false;
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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hit: " + other.name);
    }
}