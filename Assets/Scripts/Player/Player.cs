using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations;
using UnityEngine.Playables;

enum AnimLayers
{
    Spawn,
    Movement,
    Despawn
}


public class Player : MonoBehaviour, IDamageable
{
    [Header("Components")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;

    [Header("Stats")]
    [SerializeField] float moveSpeed;
    [SerializeField] float maxHealth;
    float currHealth;

    [SerializeField] float respawnTime;
    Vector3 originalPosition;

    [Header("Jump")]
    [SerializeField] float jumpHeight;
    [SerializeField] float gravity;
    [SerializeField] float coyoteTime;
    [SerializeField] float jumpBufferTime;
    float coyoteTimeCounter;
    float jumpBufferCounter;
    bool isJumping;
    bool isFalling;
    float horizontal;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    bool isFacingRight = true;

    [Header("Animation")]
    [SerializeField] AnimationClip spawnAnim;
    [SerializeField] AnimationClip despawnAnim;
    PlayableGraph playGraph;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = horizontal * moveSpeed;
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        anim.SetFloat("Speed", Mathf.Abs(speed));
        CoyoteCheck();
        CheckDirection(horizontal);
        CheckYVelocity();

    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0.0f && coyoteTimeCounter > 0.0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            jumpBufferCounter = 0.0f;
        }

        if (context.canceled && rb.linearVelocity.y > 0.0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            coyoteTimeCounter = 0.0f;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1.0f;
        transform.localScale = localScale;
    }

    private void CheckDirection(float horizontalValue)
    {
        if (!isFacingRight && horizontalValue > 0.0f)
        {
            Flip();
        }
        else if (isFacingRight && horizontalValue < 0.0f)
        {
            Flip();
        }
    }

    private void CoyoteCheck()
    {
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void CheckYVelocity()
    {
        if (IsGrounded())
        {
            isJumping = false;
            isFalling = false;
        }
        else if (rb.linearVelocityY > 0.1f)
        {
            isJumping = true;
            isFalling = false;

        }
        else if (rb.linearVelocityY < 0.1f)
        {
            isFalling = true;
            isJumping = false;
        }

        anim.SetBool("IsJumping", isJumping);
        anim.SetBool("IsFalling", isFalling);
    }

    public void OnSpawnAnimFinish()
    {
        anim.SetFloat("Speed", 0.0f);
        //anim.enabled = true;
        rb.gravityScale = gravity;
       
    }

    public void OnDespawnAnimFinish()
    {
        spriteRenderer.enabled = false;       
        StartCoroutine(Respawn(respawnTime));
    }

    public void TakeDamage(float damage)
    {
        currHealth -= damage;
        if (currHealth <= 0.0f)
        {
            Die();
        }
    }

    void Die()
    {
        PlayAnimationOneShot(despawnAnim, OnDespawnAnimFinish);
    }

    void Spawn()
    {
        spriteRenderer.enabled = true;
        
        transform.position = originalPosition;
        rb.gravityScale = 0.0f;
        currHealth = maxHealth;
        PlayAnimationOneShot(spawnAnim, OnSpawnAnimFinish);
    }

    IEnumerator Respawn(float spawnDelay)
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
    }

    void PlayAnimationOneShot(AnimationClip clip, System.Action onComplete)
    {
        if (playGraph.IsValid())
                playGraph.Destroy();

        playGraph = PlayableGraph.Create("OneShotAnimation");
        var playableOutput = AnimationPlayableOutput.Create(playGraph, "Animation", anim);

        var clipPlayable = AnimationClipPlayable.Create(playGraph, clip);
        playableOutput.SetSourcePlayable(clipPlayable);

        playGraph.Play();

        StartCoroutine(AnimationCallback(clip.length, onComplete));
    }

    IEnumerator AnimationCallback(float delay, System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);

        if(playGraph.IsValid())
        {
            playGraph.Destroy();
        }

        onComplete?.Invoke();
    }
}
