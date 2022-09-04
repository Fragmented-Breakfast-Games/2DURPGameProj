using UnityEngine;

namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        // Start is called before the first frame update

        public float speed;
        private Rigidbody2D rb;
        private Vector2 moveVelocity;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            // 2D movement and jump physics, makes the player move and jump, and sprint
            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveVelocity = moveInput.normalized * speed;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = 10;
            }
            else
            {
                speed = 5;
            }

        }

        private void FixedUpdate()
        {
            rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
            
            
        }
        
        // resets the player's position if they fall off the map
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Death"))
            {
                transform.position = new Vector3(0, 0, 0);
            }
        }
        
        // returns player to the ground from the air
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                var transform1 = transform;
                transform1.position = new Vector3(transform1.position.x, -3.5f, 0);
            }
        }
    }
}
