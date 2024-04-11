using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    public bool canMoveLeft = false;
    public bool canMoveRight = false;
    public bool canMoveUp = false;
    public bool canMoveDown = false;

    public GameObject nodeLeft;
    public GameObject nodeRight;
    public GameObject nodeUp;
    public GameObject nodeDown;

    public bool isWarpRightNode = false;
    public bool isWarpLeftNode = false;

    public bool isPalletNode = false;
    public bool hasPallet = false;
     
    public bool isGhostStartingNode = false;

    public SpriteRenderer palletSprite;

    public GameManager gameManager;

    public bool isSideNode = false;

    public bool isPowerPellet=false;
    public float powerPelletBlinkingTimer = 0;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if(transform.childCount > 0)
        {
            gameManager.gotPelletFromNodeController(this);
            hasPallet = true;
            isPalletNode = true;
            palletSprite = GetComponentInChildren<SpriteRenderer>();
        }

        RaycastHit2D[] hitsdown;
        //raycast line going down
        hitsdown = Physics2D.RaycastAll(transform.position, -Vector2.up);
        
        for(int i = 0; i < hitsdown.Length; i++)
        {
            float distance = Mathf.Abs(hitsdown[i].point.y - transform.position.y);
            if (distance < 0.4f && hitsdown[i].collider.tag == "Node")
            {
                canMoveDown = true;
                nodeDown = hitsdown[i].collider.gameObject;
            } 
        }

        RaycastHit2D[] hitsup;
        //raycast line going down
        hitsup = Physics2D.RaycastAll(transform.position, Vector2.up);

        for (int i = 0; i < hitsup.Length; i++)
        {
            float distance = Mathf.Abs(hitsup[i].point.y - transform.position.y);
            if (distance < 0.4f && hitsup[i].collider.tag == "Node")
            {
                canMoveUp = true;
                nodeUp = hitsup[i].collider.gameObject;
            }
        }

        RaycastHit2D[] hitsleft;
        //raycast line going down
        hitsleft = Physics2D.RaycastAll(transform.position, Vector2.left);

        for (int i = 0; i < hitsleft.Length; i++)
        {
            float distance = Mathf.Abs(hitsleft[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsleft[i].collider.tag == "Node")
            {
                canMoveLeft = true;
                nodeLeft = hitsleft[i].collider.gameObject;
            }
        }

        RaycastHit2D[] hitsright;
        //raycast line going down
        hitsright = Physics2D.RaycastAll(transform.position, Vector2.right);

        for (int i = 0; i < hitsright.Length; i++)
        {
            float distance = Mathf.Abs(hitsright[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsright[i].collider.tag == "Node")
            {
                canMoveRight = true;
                nodeRight = hitsright[i].collider.gameObject;
            }
        }

        if (isGhostStartingNode)
        {
            canMoveDown= true;
            nodeDown = gameManager.ghostNodeCenter; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }

        if (isPowerPellet && hasPallet)
        {
            powerPelletBlinkingTimer += Time.deltaTime;
            if(powerPelletBlinkingTimer >= 0.1f)
            {
                powerPelletBlinkingTimer = 0;
                palletSprite.enabled = !palletSprite.enabled;
            }
        }
    }

    public GameObject GetNodeFromDirection(string direction)
    {
        if (direction == "left" && canMoveLeft)
        {
            return nodeLeft;
        }
        else if (direction == "right" && canMoveRight)
        {
            return nodeRight;
        }
        else if (direction == "up" && canMoveUp)
        {
            return nodeUp;
        }
        else if (direction == "down" && canMoveDown)
        {
            return nodeDown;
        }
        else return null; 
    }

    public void RespawnPellet()
    {
        if(isPalletNode)
        {
            hasPallet = true;
            palletSprite.enabled= true;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && isPalletNode && hasPallet)
        {
            hasPallet = false;
            palletSprite.enabled= false;
            StartCoroutine(gameManager.CollectecPallet(this));
            
        }
    }
}
