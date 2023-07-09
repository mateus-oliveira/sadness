using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoviment2D : MonoBehaviour {
    // Constants
    private const string PLAYER_WALKING = "PlayerWalking";
    private const string PLAYER_JUMP_VERTICAL = "PlayerJumpVertical";
    private const string PLAYER_JUMP_DIAGONAL = "PlayerJumpDiagonal";
    private const string PLAYER_FALLING_DOWN = "PlayerFallingDown";

    // Atributes
    private float move; // Horizontal move
    private bool isGrounded;
    private bool canUpdate = true;
    [SerializeField] private bool jumping = false;
    [SerializeField] private float ghostJump;
    [SerializeField] private float moveSpeed = 0f;
    [SerializeField] private float jumpSpeed = 7.0f;
    [SerializeField] private float sizeRadius = 0.156f;

    // Unity Components
    public Transform feetPosition;
    public LayerMask whatIsGround;
    private AudioSource backgroundSound;
    private Rigidbody2D rigidBody;
    private SpriteRenderer sprite;
    private Animator animator;
    [SerializeField] private GameObject gameWinSound;
    [SerializeField] private GameObject gameOverSound;
    [SerializeField] private GameObject playerPositive;
    [SerializeField] private GameObject happySprite;
    [SerializeField] private GameObject positiveGrid;
    [SerializeField] private GameObject negativeGrid;
    [SerializeField] private GameObject youWin;
    [SerializeField] private GameObject youLost;


    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        backgroundSound = Camera.main.GetComponent<AudioSource>();
    }


    void Update() {
        // Checks Ground
        isGrounded = Physics2D.OverlapCircle(feetPosition.position, sizeRadius, whatIsGround);

        // Player Jump
        if (Input.GetKeyDown(KeyCode.DownArrow) && ghostJump > 0) {
            jumping = true;
        }

        // Moviment 2D Input
        move = -Input.GetAxisRaw("Horizontal");

        if (move != 0) {
            moveSpeed += 15f * Time.deltaTime;
            if (moveSpeed >= 5f) {
                moveSpeed = 5f;
            }
        } else {
            moveSpeed = 0f;
        }


        // Invert flip position
        this.Flip();


        // Animations 
        this.Animate();
    }


    // Physics udpates
    void FixedUpdate(){
        rigidBody.velocity = new Vector2(move * moveSpeed, rigidBody.velocity.y);

        if (jumping) {
            rigidBody.velocity = Vector2.up * jumpSpeed;
            jumping = false;
            isGrounded = false;
        }
    }


    // Displays the Imaginary Circle
    void OnDrawGizmosSelected(){
        Gizmos.color = new Color(0, 0, 0, 0.5f);
        Gizmos.DrawSphere(feetPosition.position, sizeRadius);
    }

    // Invert sprites
    private void Flip() {
        if (move != 0) {
            sprite.flipX = move < 0;
        }
    }


    // Switch in sprite animations
    private void Animate(){
        if (isGrounded) {
            ghostJump = 0.1f;

            animator.SetBool(PLAYER_JUMP_VERTICAL, false);
            animator.SetBool(PLAYER_JUMP_DIAGONAL, false);
            animator.SetBool(PLAYER_FALLING_DOWN, false);
            if (rigidBody.velocity.x != 0 && move != 0) {
                // PLAYER Walking
                animator.SetBool(PLAYER_WALKING, true);
            } else {
                // PLAYER Idle
                animator.SetBool(PLAYER_WALKING, false);
            }
        } else {
            ghostJump -= Time.deltaTime;

            if (ghostJump <= 0) {
                ghostJump = 0f;
            }

            animator.SetBool(PLAYER_WALKING, false);
            if (rigidBody.velocity.x == 0){
                if (rigidBody.velocity.y > 0) {
                    // PLAYER Jump Vertical
                    animator.SetBool(PLAYER_JUMP_VERTICAL, true);
                    animator.SetBool(PLAYER_JUMP_DIAGONAL, false);
                    animator.SetBool(PLAYER_FALLING_DOWN, false);
                } else if (rigidBody.velocity.y < 0){
                    // PLAYER Falling Down
                    animator.SetBool(PLAYER_JUMP_VERTICAL, false);
                    animator.SetBool(PLAYER_JUMP_DIAGONAL, false);
                    animator.SetBool(PLAYER_FALLING_DOWN, true);
                }
            } else {
                if (rigidBody.velocity.y > 0) {
                    // PLAYER Jump Diagonal
                    animator.SetBool(PLAYER_JUMP_VERTICAL, false);
                    animator.SetBool(PLAYER_JUMP_DIAGONAL, true);
                    animator.SetBool(PLAYER_FALLING_DOWN, false);
                } else if (rigidBody.velocity.y < 0){
                    // PLAYER Falling Down
                    animator.SetBool(PLAYER_JUMP_VERTICAL, false);
                    animator.SetBool(PLAYER_JUMP_DIAGONAL, false);
                    animator.SetBool(PLAYER_FALLING_DOWN, true);
                }
            }
        }
    }



    // When collides with Emoji Sprites
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "HappySprite" && canUpdate){
            canUpdate = false;
            Time.timeScale = 0f;

            float x = other.gameObject.transform.position.x;
            float y = other.gameObject.transform.position.y;

            rigidBody.constraints = RigidbodyConstraints2D.FreezePosition;
            Destroy(other.gameObject);
            Destroy(gameObject);
            Destroy(negativeGrid);
            Instantiate(positiveGrid);
            Instantiate(playerPositive, new Vector3(x, y, 0f), Quaternion.identity);
            
            ShowYouWinMessage(new Vector3(x, y+2f, 0f));

            GameObject[] sprites = GameObject.FindGameObjectsWithTag("SadSprite");
            foreach (GameObject sprite in sprites) {
                Destroy(sprite);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "SadSprite" && canUpdate) {
            Time.timeScale = 0f;
            canUpdate = false;
            ShowYouLostMessage();
        }
    }


    private void ShowYouLostMessage() {
        print("You Lost");
        float x = gameObject.transform.position.x;
        float y = gameObject.transform.position.y;
        rigidBody.constraints = RigidbodyConstraints2D.FreezePosition;

        Instantiate(youLost, new Vector3(x, y+2f, 0f), Quaternion.identity);
        StopAudio();
        Instantiate(gameOverSound, new Vector3(x, y, 0f), Quaternion.identity);
    }


    private void ShowYouWinMessage(Vector3 position) {
        print("You Win");
        rigidBody.constraints = RigidbodyConstraints2D.FreezePosition;
        Instantiate(youWin, position, Quaternion.identity);
        StopAudio();
        Instantiate(gameWinSound, position, Quaternion.identity);
    }

    private void StopAudio() {
        if (backgroundSound != null && backgroundSound.isPlaying) {
            backgroundSound.Stop();
        }
    }
}
