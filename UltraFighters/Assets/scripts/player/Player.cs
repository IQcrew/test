using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask platformLayerMask;
    [SerializeField] private float WalkForce = 5f;
    [SerializeField] private float SprintForce = 8f;
    [SerializeField] private float JumpForce = 5f;
    [SerializeField] private float DoubleTapTime = 0.2f;



    // walking variables
    private float LastKeyRight = -5f;
    private float LastKeyLeft = -5f;
    private bool LastFreeKeys = false;

    //sprinting variables
    private bool sprinting = false;
    private bool LastFreeKeysSprint = false;
    private float LastSprintRight = -5f;
    private float LastSprintLeft = -5f;



    public KeyCode Right;          // Nastavovanie pre klavesy pomocou unity
    public KeyCode Left;
    public KeyCode Up;
    public KeyCode Down;
    public KeyCode Slot1;
    public KeyCode Slot2;
    public KeyCode Slot3;

    Rigidbody2D PlayerBody;
    BoxCollider2D PlayerHitBox;
    SpriteRenderer PlayerRender;

    // Start is called before the first frame update
    void Start()
    {
        PlayerBody = GetComponent<Rigidbody2D>();
        PlayerHitBox = GetComponent<BoxCollider2D>();
        PlayerRender = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
    }
    private void Move()
    {
       if (sprinting) { sprint(); }
       else { walk(); }
       
    }


    private void walk()
    {
        if (Input.GetKey(Right) && !Input.GetKey(Left))
        {
            PlayerRender.flipX = false;
            if (LastFreeKeys && Time.time-LastKeyRight < DoubleTapTime) {
                sprinting = true;
                sprint();
                return;
            }
            LastKeyRight = Time.time;  LastFreeKeys = false;
            PlayerBody.velocity = new Vector2(+WalkForce, PlayerBody.velocity.y);
        }
        else if (Input.GetKey(Left) && !Input.GetKey(Right))
        {
            PlayerRender.flipX = true;
            if (LastFreeKeys && Time.time - LastKeyLeft < DoubleTapTime) { 
                sprinting = true;
                sprint();
                return;}
            LastKeyLeft = Time.time; LastFreeKeys = false;
            PlayerBody.velocity = new Vector2(-WalkForce, PlayerBody.velocity.y);
        }
        else { PlayerBody.velocity = new Vector2(0, PlayerBody.velocity.y); LastFreeKeys = true; }
    }

    private void sprint()
    {
        if (Input.GetKey(Right) && !Input.GetKey(Left))
        {
            if (LastFreeKeysSprint && Time.time - LastSprintRight < DoubleTapTime){
                sprinting = false;
                walk();
                return;
            }
            PlayerBody.velocity = new Vector2(+SprintForce, PlayerBody.velocity.y);
            PlayerRender.flipX = false;
            LastSprintRight = Time.time;
            LastFreeKeysSprint = false;
        }
        else if (Input.GetKey(Left) && !Input.GetKey(Right))
        {
            if (LastFreeKeysSprint && Time.time-LastSprintLeft < DoubleTapTime){
                sprinting = false;
                walk();
                return;
            }
            PlayerBody.velocity = new Vector2(-SprintForce, PlayerBody.velocity.y);
            PlayerRender.flipX = true;
            LastSprintLeft = Time.time;
            LastFreeKeysSprint = false;
        }
        else{ 
            if (Time.time-LastSprintLeft > DoubleTapTime && Time.time - LastSprintRight > DoubleTapTime) {sprinting = false; }
            LastFreeKeysSprint = true;
            PlayerBody.velocity = new Vector2(0, PlayerBody.velocity.y); }
    }
    private void Jump()
    {
        if(Input.GetKey(Up) && isGrounded())
        {
            PlayerBody.velocity = Vector2.up * JumpForce;
        }
    }    
    private bool isGrounded()
    {
        RaycastHit2D rayCastHit = Physics2D.BoxCast(PlayerHitBox.bounds.center, PlayerHitBox.bounds.size, 0f, Vector2.down, 0.02f, platformLayerMask);
            return rayCastHit.collider != null;
    }    
}

