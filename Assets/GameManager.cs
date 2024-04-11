using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject Pacman;

    public GameObject leftWarpNode;
    public GameObject rightWarpNode;


    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;
    public AudioSource powerPelletSound;
    public AudioSource respawningSound;
    public AudioSource ghostEatenSound;

    public bool isPowerPelletRunning = false;
    public float currentPowerPelletTime = 0;
    public float powerPelletTimer = 8f;
    public int powerPelletMultiplyer = 1;

    public int currentMunch = 0;

    public int score;
    public Text scoreText;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;

    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;

    public int totalPellets;
    public int pelletLeft;
    public int pellectedCollectedOnThisLife;

    public bool hadDeathOnThisLevel = false;

    public bool gameIsRunning;

    public List<NodeController> nodeControllers = new List<NodeController>();

    public bool newGame;
    public bool clearedLevel;

    public enum GhostMode
    {
        chase, scatter
    }

    public GhostMode currentGhostMode;

    public AudioSource startGameAudio;
    public AudioSource death;

    public int lives;
    public int currentLevel;

    public Image blackBackground;
    public Text gameOverText;

    public int[] ghostModeTimers = new int[] {7, 20, 7, 20, 5, 20, 5 };
    public int ghostModeTimerIndex;
    public float ghostModeTimer = 0;
    public bool runningTimer;
    public bool completedTimer;

    

    // Start is called before the first frame update
    void Awake()
    {
        newGame = true;
        clearedLevel = false;
        blackBackground.enabled = false;
        

        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();
        
        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode= true;
        Pacman = GameObject.Find("Player");


        StartCoroutine(Setup());
        gameIsRunning = true;

    }

    public IEnumerator PauseGame(int timeToPause)
    {
        gameIsRunning = false;
        yield return new WaitForSeconds(timeToPause);
        gameIsRunning = true;
    }

    public void GhostEaten()
    {
        ghostEatenSound.Play();
        AddToScore(400 * powerPelletMultiplyer);
        powerPelletMultiplyer++;
        StartCoroutine(PauseGame(1));
    }

    public IEnumerator Setup()
    {

        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        completedTimer = false;
        runningTimer = true;
        gameOverText.enabled = false;
        //if pacman cleared level, a background will appear covering level, the game will pause
        if(clearedLevel)
        {
            blackBackground.enabled = true;
            //active background
            yield return new WaitForSeconds(0.1f);
        }

        blackBackground.enabled = false;

        pellectedCollectedOnThisLife = 0;
        currentGhostMode = GhostMode.scatter;
        gameIsRunning = false;
        currentMunch = 0;
        float waitTimer = 1f;

        

        if(clearedLevel || newGame)
        {
            waitTimer = 4f;
            //pellet will respawn when pacman clear level or starts a new game
            for (int i = 0; i < nodeControllers.Count; i++)
            {
                nodeControllers[i].RespawnPellet();
            }
        }
        
        if(newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.text = "Score: " + score.ToString();
            lives = 3;
            currentLevel = 1;
        }

        Pacman.GetComponent<PlayerController>().Setup();

        redGhostController.Setup();
        pinkGhostController.Setup();
        blueGhostController.Setup();
        orangeGhostController.Setup();

        newGame = false;
        clearedLevel = false;

        yield return new WaitForSeconds(waitTimer);

        StartGame();

    }

    void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    void StopGame()
    {
        gameIsRunning = false;
        siren.Stop();
        powerPelletSound.Stop();
        Pacman.GetComponent<PlayerController>().Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameIsRunning)
        {
            return;
        }

        if(redGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || pinkGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || blueGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || orangeGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning)
        {
            if(!respawningSound.isPlaying)
            {
                respawningSound.Play();
            }
        }
        else
        {
            if (respawningSound.isPlaying)
            {
                respawningSound.Stop();
            }
        }

        if(!completedTimer && runningTimer)
        {
            ghostModeTimer += Time.deltaTime;
            if(ghostModeTimer >= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if(currentGhostMode == GhostMode.chase)
                {
                    currentGhostMode = GhostMode.scatter;
                }
                else
                {
                    currentGhostMode = GhostMode.chase;
                }

                if(ghostModeTimerIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostMode.chase;
                }
            }
        }

        if(isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if(currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;
                powerPelletSound.Stop();
                siren.Play();
                powerPelletMultiplyer = 1;
            }
        }
    }

    public void gotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletLeft++;

    }

    public void AddToScore(int amout)
    {
        score+= amout;
        scoreText.text = "Score: " + score.ToString();
    }

    public IEnumerator CollectecPallet(NodeController nodeController)
    {
        if (currentMunch == 0)
        {
            munch1.Play();
            currentMunch = 1;
        }
        else if (currentMunch == 1)
        {
            munch2.Play();
            currentMunch = 0;
        }

        pelletLeft--;
        pellectedCollectedOnThisLife++;

        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if (hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        {
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if (pellectedCollectedOnThisLife >= requiredBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            blueGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

        if (pellectedCollectedOnThisLife >= requiredOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            orangeGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

        AddToScore(10);

        //Add to score

        //Check if there are many pallets left
        if(pelletLeft == 0)
        {
            currentLevel++;
            clearedLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(Setup());
        }

        //check how many pallet were eaten

        //Is that a power pallet
        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletSound.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTime = 0;
            
            
            redGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);


        }
    }

    public IEnumerator PlayerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);

        Pacman.GetComponent<PlayerController>().Death();
        death.Play();
        yield return new WaitForSeconds(3);

        lives--;
        if(lives <= 0)
        {
            newGame = true;
            gameOverText.enabled = true;
            yield return new WaitForSeconds(3);  
        }

        StartCoroutine(Setup());
    }
}
