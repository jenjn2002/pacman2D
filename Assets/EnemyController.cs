using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum GhostNodeStatesEnum
    {
        respawning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingInNodes
    }

    public GhostNodeStatesEnum ghostNodeState;
    public GhostNodeStatesEnum startGhostNodeState;
    public GhostNodeStatesEnum respawnState;

    public enum GhostType
    {
        red,
        blue,
        pink,
        orange
    }

    public GhostType ghostType;
    

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public MovementController movementController;

    public GameObject startingNode;

    public bool readyToLeaveHome = false;

    public GameManager gameManager;

    public bool testRespawn = false;
    public bool isFrightened = false;

    public GameObject[] scatterNodes;
    public int scatterNodeIndex;

    public bool leftHomeBefore = false;

    public bool isVisible = true;

    public SpriteRenderer ghostSprite;
    public SpriteRenderer eyesSprite;

    public Animator animator;

    public Color color;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        ghostSprite = GetComponent<SpriteRenderer>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();
        if(ghostType == GhostType.red)
        {
            startGhostNodeState = GhostNodeStatesEnum.startNode;
            respawnState = GhostNodeStatesEnum.startNode;
            startingNode = ghostNodeStart; 
            
        }
        else if(ghostType == GhostType.pink)
        {
            startGhostNodeState = GhostNodeStatesEnum.centerNode;
            respawnState = GhostNodeStatesEnum.centerNode;
            startingNode = ghostNodeCenter;
            
        }
        else if (ghostType == GhostType.blue)
        {
            startGhostNodeState = GhostNodeStatesEnum.leftNode;
            respawnState = GhostNodeStatesEnum.leftNode; 
            startingNode = ghostNodeLeft;
        }
        else if (ghostType == GhostType.orange)
        {
            startGhostNodeState = GhostNodeStatesEnum.rightNode;
            respawnState = GhostNodeStatesEnum.rightNode;

            startingNode = ghostNodeRight;
        }
        

    }

    public void Setup()
    {
        ghostNodeState = startGhostNodeState;
        readyToLeaveHome = false;

        //reset all ghosts back to their home posion

        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;

        movementController.direction = "";
        movementController.lastMovingDirection = "";


        //Set their scatter node index back to 0
        scatterNodeIndex = 0;

        //Set isFrightened
        isFrightened = false;
        leftHomeBefore= false;

        //Set ready to leave home to false if they are blue or pink
        if(ghostType == GhostType.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }
        else if(ghostType == GhostType.pink)
        {
            readyToLeaveHome = true;
        }
        SetVisible(true);    
    }

    // Update is called once per frame
    void Update()
    {
        if(ghostNodeState != GhostNodeStatesEnum.movingInNodes || !gameManager.isPowerPelletRunning)
        {
            isFrightened = false;
        }

        if(isVisible)
        {
            if(ghostNodeState != GhostNodeStatesEnum.respawning)
            {
                ghostSprite.enabled = true;
            }
            else
            {
                ghostSprite.enabled = false;
            }
            eyesSprite.enabled = true;
        }
        else
        {
            ghostSprite.enabled = false;
            eyesSprite.enabled = false;
        }

        if (isFrightened)
        {

            animator.SetBool("frighten", true);
            eyesSprite.enabled = false;
            ghostSprite.color = new Color(255, 255, 255, 255);
        }
        else
        {
            animator.SetBool("frighten", false);
            ghostSprite.color = color;

        }

        if (!gameManager.gameIsRunning)
        {
            return;
        }

        
        animator.SetBool("moving", true);
         
        if (testRespawn == true)
        {
            readyToLeaveHome = false;
            ghostNodeState = GhostNodeStatesEnum.respawning;
            testRespawn = false;
        }

        if (movementController.currentNode.GetComponent<NodeController>().isSideNode)
        {
            movementController.SetSpeed(1);
        }
        else movementController.SetSpeed(1);
    }

    public void SetFrightened(bool newIsFrightened)
    {
        isFrightened = newIsFrightened;
    }

    public void ReachedCenterOfNode(NodeController nodeController)
    {
        if(ghostNodeState == GhostNodeStatesEnum.movingInNodes)
        {
            leftHomeBefore = true;
            //Scatter Mode
            if (gameManager.currentGhostMode == GameManager.GhostMode.scatter)
            {
                DetermineGhostScatterModeDirection();               
            }
            //Frightened Mode
            else if (isFrightened)
            {
                string direction = GetRandomDirection();
                movementController.SetDirection(direction);
            }
            //Chase Mode
            else
            {
                //Determine next game node direction to go to
                if (ghostType == GhostType.red)
                {
                    DetermineRedGhostDirection();
                }
                else if(ghostType == GhostType.pink)
                {
                    DeterminePinkGhostDirection();
                }
                else if(ghostType == GhostType.blue)
                {
                    DetermineBlueGhostDirection();
                }
                else if(ghostType == GhostType.orange)
                {
                    DetermineOrangeGhostDirection(); 
                }
            }        
        }
        else if(ghostNodeState == GhostNodeStatesEnum.respawning)
        {
            string direction = "";

            //we have reached start node, move to the center node
            if (transform.position.x == ghostNodeStart.transform.position.x && transform.position.y == ghostNodeStart.transform.position.y)
            {
                direction = "down";
                
            }
            //we have reachec center node
            else if (transform.position.x == ghostNodeCenter.transform.position.x && transform.position.y == ghostNodeCenter.transform.position.y)
            {
                if (respawnState == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeState = respawnState;
                }
                else if (respawnState == GhostNodeStatesEnum.leftNode)
                {
                    direction = "left";
                }
                else if (respawnState == GhostNodeStatesEnum.rightNode)
                {
                    direction = "right";
                }
            }
            //If respawn state is either the left or right node, and we got to that node, leave home again
            else if (
                (transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y)
                || (transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y)
                )
            {
                ghostNodeState = respawnState;
            }
            //Locate start node
            else
            {
                //Determine direction to home
                direction = GetClosestDirection(ghostNodeStart.transform.position);
            }
            movementController.SetDirection(direction); 
        }
        else
        {
            //Ready to leave home
            if(readyToLeaveHome)
            {
                //If we are in the left or right, move to the center
                if(ghostNodeState == GhostNodeStatesEnum.leftNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("right");
                }
                else if(ghostNodeState == GhostNodeStatesEnum.rightNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("left");
                }
                //If we are in the center, move to start
                else if(ghostNodeState == GhostNodeStatesEnum.centerNode)
                { 
                    ghostNodeState = GhostNodeStatesEnum.startNode;
                    movementController.SetDirection("up");
                }
                //If we are in the start node, start moving around
                else if(ghostNodeState == GhostNodeStatesEnum.startNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.movingInNodes;
                    movementController.SetDirection("left");
                }
            }
        }
    }

    void DetermineGhostScatterModeDirection()
    {
        //if we reached the scatter node, add one to our scatter node index
        if (transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;

            if (scatterNodeIndex == scatterNodes.Length - 1)
            {
                scatterNodeIndex = 0;
            }
        }

        string direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);

        movementController.SetDirection(direction);
    }

    string GetRandomDirection()
    {
        List<string> possibleDirections = new List<string>();
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        if(nodeController.canMoveDown && movementController.lastMovingDirection != "up")
        {
            possibleDirections.Add("down");
        }
        else if(nodeController.canMoveUp && movementController.lastMovingDirection != "down")
        {
            possibleDirections.Add("up");
        }
        else if(nodeController.canMoveRight && movementController.lastMovingDirection != "left")
        {
            possibleDirections.Add("right");
        }
        else if(nodeController.canMoveLeft && movementController.lastMovingDirection != "right")
        {
            possibleDirections.Add("left");
        }

        string direction = "";
        int randomDirectionIndex = Random.Range(0, possibleDirections.Count - 1);
        direction = possibleDirections[randomDirectionIndex];
        return direction;
    }

    void DetermineRedGhostDirection()
    {
        string direction = GetClosestDirection(gameManager.Pacman.transform.position);
        movementController.SetDirection(direction);
    }

    void DeterminePinkGhostDirection()
    {
        string pacmanDirection = gameManager.Pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.Pacman.transform.position;
        if(pacmanDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if(pacmanDirection=="right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if(pacmanDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if(pacmanDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        string direction = GetClosestDirection(target);
        movementController.SetDirection(direction);
    }

    void DetermineBlueGhostDirection()
    {
        string pacmanDirection = gameManager.Pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.Pacman.transform.position;
        if (pacmanDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        GameObject redGhost = gameManager.redGhost;
        float xDistance = target.x - redGhost.transform.position.x;
        float yDistance = target.y - redGhost.transform.position.y;

        Vector2 blueTarger = new Vector2(target.x + xDistance,target.y + yDistance);

        string distance = GetClosestDirection(blueTarger);
        movementController.SetDirection(distance);
    }

    void DetermineOrangeGhostDirection()
    {
        float distance = Vector2.Distance(gameManager.Pacman.transform.position, transform.position);
        float distanceBetweenNodes = 0.35f;

        if (distance < 0)
        {
            distance *= -1;

        }
        // if we are within 8 nodes of pacman, chase him using red's logic
        if(distance <= distanceBetweenNodes * 8)
        {
            DetermineRedGhostDirection();
        }
        else
        {
            //scatter node
            DetermineGhostScatterModeDirection(); 
        }
    }

    string GetClosestDirection(Vector2 target)
    {
        float shortestDistance = 0;
        string lastMovingDirection = movementController.lastMovingDirection;
        string newDirection = "";

        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        //If we can move up and we are not reversing
        if(nodeController.canMoveUp && lastMovingDirection != "down")
        {
            GameObject nodeUp = nodeController.nodeUp;
            //get the distance between top node and pacman
            float distance = Vector2.Distance(nodeUp.transform.position, target);
            
            //If this is the shortest distance so far, set our direction
            if(distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            GameObject nodeDown = nodeController.nodeDown;
            //get the distance between top node and pacman
            float distance = Vector2.Distance(nodeDown.transform.position, target);

            //If this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }

        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            GameObject nodeLeft = nodeController.nodeLeft;
            //get the distance between top node and pacman
            float distance = Vector2.Distance(nodeLeft.transform.position, target);

            //If this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }

        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            GameObject nodeRight = nodeController.nodeRight;
            //get the distance between top node and pacman
            float distance = Vector2.Distance(nodeRight.transform.position, target);

            //If this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }

        return newDirection;
    }

    public void SetVisible(bool newIsVisible)
    {
        isVisible = newIsVisible;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && ghostNodeState != GhostNodeStatesEnum.respawning) 
        {
            if(isFrightened)
            {
                gameManager.GhostEaten();
                ghostNodeState = GhostNodeStatesEnum.respawning;
            }
            else
            {
                StartCoroutine(gameManager.PlayerEaten());
            }
        }
    }

}
