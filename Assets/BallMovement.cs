using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float speed = 8f;
    public float jumpForce = 7f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        rb.linearVelocity = new Vector3(moveX * speed, rb.linearVelocity.y, moveZ * speed);

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            isGrounded = true;
        }
    }
}