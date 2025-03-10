using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MovementController movementController;

    public SpriteRenderer sprite;
    public Animator animator;

    public GameObject startNode;

    public Vector2 startPos;

    public GameManager gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        startPos = new Vector2(-0.178f, -0.6279998f);

        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        movementController = GetComponent<MovementController>();
        startNode = movementController.currentNode;
    }

    public void Setup()
    {
        animator.SetBool("dead", false);
        animator.SetBool("moving", false);

        movementController.currentNode = startNode;
        movementController.lastMovingDirection = "left";
        sprite.flipX = false;
        transform.position = startPos;
        animator.speed = 1; 
        animator.SetBool("moving", false);
    }

    public void Stop()
    {
        animator.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (!gameManager.gameIsRunning)
        {
            return;
        }

        animator.SetBool("moving", true);
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movementController.SetDirection("left");
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            movementController.SetDirection("right");
        }
        if(Input.GetKey(KeyCode.UpArrow))
        {
            movementController.SetDirection("up");
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            movementController.SetDirection("down");
        }

        bool flipX = false;
        bool flipY = false;
        if(movementController.lastMovingDirection == "left") 
        {
            animator.SetInteger("direction", 0);    
        }
        else if(movementController.lastMovingDirection=="right")
        {
            animator.SetInteger("direction", 0);
            flipX = true;
        }
        else if(movementController.lastMovingDirection =="up")
        {
            animator.SetInteger("direction", 1);
        }
        else if(movementController.lastMovingDirection == "down")
        {
            animator.SetInteger("direction", 1);
            flipY = true;
        }

        sprite.flipY= flipY;
        sprite.flipX= flipX;
    }

    public void Death()
    {
        animator.SetBool("moving", false);
        animator.speed= 1;
        animator.SetBool("dead", true);
    }
}
