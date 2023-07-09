using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SadMovement2D : MonoBehaviour {
    private Rigidbody2D rb;
    private float direction = -1f;

    [SerializeField] private float speed = 0.2f;
    [SerializeField] private bool horizontal = false;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Update() {
        
    }

    private void FixedUpdate() {
        if (horizontal) {
            rb.velocity = new Vector2(speed * direction, rb.velocity.y);
        } else {
            rb.velocity = new Vector2(rb.velocity.x, speed * direction);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Wall")) {
            direction = -direction;
        }
    }
}
