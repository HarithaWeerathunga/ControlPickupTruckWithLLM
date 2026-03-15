using UnityEngine;

public class JeepController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float turnSpeed = 120f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        float moveInput = 0f;
        float turnInput = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            moveInput = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            moveInput = -1f;

        if (Input.GetKey(KeyCode.LeftArrow))
            turnInput = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            turnInput = 1f;

        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        Quaternion turn = Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }
}