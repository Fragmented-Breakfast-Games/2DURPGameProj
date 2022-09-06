using System;
using System.Collections;
using UnityEngine;

namespace Controllers
{
    
    /// <summary>
    /// Simple WASD controller for the player character
    /// The camera is attached to the player and follows along with it.
    /// IDK what that is supposed to be called.
    /// Double jump has kinda sorta been added, it's kinda OP.
    /// </summary>
    
    
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]

    public class PlayerController : MonoBehaviour
    {
        // Move player in 2D space
        public float maxSpeed = 5.5f;
        public float jumpHeight = 15.5f;
        public float gravityScale = 2.5f;
        public Camera mainCamera;
        
        
        private float coyoteTime = 0.2f;
        private float coyoteTimeCounter;
        
        private float jumpBufferTime= 0.2f;
        private float jumpBufferCounter;
        private bool isJumping;
        
        private int jumpCount; //added this to keep track of jumps
        public int maxJumps = 2; //added this to keep track of jumps

        public bool canThrust = true; // if the player can thrust
        public bool isThrusting; //added this to keep track of thrusting
        

        bool facingRight = true;
        float moveDirection = 0;
        bool isGrounded = false;
        Vector3 cameraPos;
        Rigidbody2D r2d;
        CapsuleCollider2D mainCollider;
        Transform t;
        private IEnumerator m_Enumerator;

        // Use this for initialization
        void Start()
        {
            m_Enumerator = ThrustCooldown();
            t = transform;
            r2d = GetComponent<Rigidbody2D>();
            mainCollider = GetComponent<CapsuleCollider2D>();
            r2d.freezeRotation = true;
            r2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            r2d.gravityScale = gravityScale;
            facingRight = t.localScale.x > 0;

            if (mainCamera)
            {
                cameraPos = mainCamera.transform.position;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Movement controls
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                moveDirection = Input.GetKey(KeyCode.A) ? -1 : 1;
            }
            else
            {
                if (isGrounded || r2d.velocity.magnitude < 0.01f)
                {
                    moveDirection = 0;
                }

            }
            
            
            // Change facing direction 
            if (moveDirection != 0)
            {
                if (moveDirection > 0 && !facingRight)
                {
                    facingRight = true;
                    t.localScale = new Vector3(Mathf.Abs(t.localScale.x), t.localScale.y, transform.localScale.z);
                }
                if (moveDirection < 0 && facingRight)
                {
                    facingRight = false;
                    t.localScale = new Vector3(-Mathf.Abs(t.localScale.x), t.localScale.y, t.localScale.z);
                }
            }
            
            
            // Jump controls for various things
            // such as coyote time, bufferedaaas  jump, and double jump
            
            // Jumping
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Jumping");
                this.Jump();
                //should be a double jump
            }
            //coyote time and jump buffer time
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
                
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
            {
                r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
                coyoteTimeCounter = 0f;
                StartCoroutine(JumpCooldown());
            }
            if (Input.GetKeyUp(KeyCode.Space) && r2d.velocity.y > 0f)
            {
                r2d.velocity = new Vector2(r2d.velocity.x, r2d.velocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
            
            
            //thrust player in the direction they are facing
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                //thrust once then cooldown
                if (canThrust)
                {
                    isThrusting = true;
                    StartCoroutine(ThrustCooldown());
                }

            }


            // Camera follow
            if (mainCamera)
            {
                mainCamera.transform.position = new Vector3(t.position.x, cameraPos.y, cameraPos.z);
            }
        }

        void FixedUpdate()
        {
            Bounds colliderBounds = mainCollider.bounds;
            float colliderRadius = mainCollider.size.x * 0.4f * Mathf.Abs(transform.localScale.x);
            Vector3 groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, colliderRadius * 0.9f, 0);
            // Check if player is grounded
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckPos, colliderRadius);
            //Check if any of the overlapping colliders are not player collider, if so, set isGrounded to true
            isGrounded = false;
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != mainCollider)
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }

            // Apply movement velocity
            r2d.velocity = new Vector2((moveDirection) * maxSpeed, r2d.velocity.y);

            // Simple debug
            Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(0, colliderRadius, 0), isGrounded ? Color.green : Color.red);
            Debug.DrawLine(groundCheckPos, groundCheckPos - new Vector3(colliderRadius, 0, 0), isGrounded ? Color.green : Color.red);
        }
        
        private void Jump()
        {
            r2d.velocity = new Vector2(r2d.velocity.x, jumpHeight);
            if(jumpCount > 0)
            {
                gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
                isGrounded = false;
                jumpCount = jumpCount - 1;
            }
            if(jumpCount== 0)
            {
                return;
            }
            StartCoroutine(JumpCooldown());
        }

        private void OnCollisionEnter2D(Collision2D collider)
        {
            if(collider.gameObject.CompareTag("Ground"))
            {
                jumpCount = maxJumps;
                isGrounded = true;
                maxSpeed = 5.5f;
            }
            
        }
        // cool down coroutine
        private IEnumerator JumpCooldown()
        {
            isJumping = true;
            yield return new WaitForSeconds(0.5f);
            isJumping = false;
        }
        
        private IEnumerator ThrustCooldown()
        {
            //thrust once then cooldown
            if (canThrust)
            {
                isThrusting = true;
                r2d.AddForce(new Vector2(5000f*moveDirection, 0f));
                canThrust= false;
                Debug.Log("Thrusting");
                // blink red for 5 seconds then can thrust again
                StartCoroutine(BlinkRed());
                yield return new WaitForSeconds(5.0f);
                canThrust = true;
                
            }
        }

        private IEnumerator BlinkRed()
        {
            // blink red for 5 seconds
            for (int i = 0; i < 5; i++)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                yield return new WaitForSeconds(0.10f);
                gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                yield return new WaitForSeconds(0.10f);
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                yield return new WaitForSeconds(0.10f);
                gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                yield return new WaitForSeconds(0.10f);
            }
        }
        
    }
}