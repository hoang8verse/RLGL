using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 PlayerMovementInput;
    private Vector2 PlayerMouseInput;
    private float xRot;

    [SerializeField]
    private Transform TargetEnd;
    [SerializeField]
    private Animator anim;
    private bool isWalking;
    private bool isMoving;
    private bool isDying;
    private bool isInDeathZone;

    [SerializeField]
    private LayerMask FloorMask;

    [SerializeField]
    private Transform FeetTransform;

    [SerializeField]
    private Transform PlayerCamera;

    [SerializeField]
    private Rigidbody PlayerBody;

    [Space]

    [SerializeField]
    private float Speed;

    [SerializeField]
    private float Sensitivity;

    [SerializeField]
    private float Jumpforce;

    [SerializeField]
    AudioSource feetSteps;
    [SerializeField]
    AudioSource shoot;
    [SerializeField]
    AudioSource die;
    [SerializeField]
    AudioSource bg_Win;
    [SerializeField]
    AudioSource bg_Die;

    public Transform deathZone;

    private bool isPlayerWon =  false;
    public bool isReadyStartGame = false;
    public bool isHost = false;

    [SerializeField]
    private TMPro.TextMeshProUGUI GameTimer;
    [SerializeField]
    private TMPro.TextMeshProUGUI PlayerName;
    [SerializeField]
    private GameObject ReadyScreen;
    [SerializeField]
    private GameObject WatingHost;
    [SerializeField]
    private GameObject HostStartGame;

    [SerializeField]
    private GameObject EndGameScreen;
    [SerializeField]
    private Transform ResultTransfrom;

    void Start()
    {
        
        ReadyScreen.SetActive(true);
        EndGameScreen.SetActive(false);
        GameTimer.text = "";
        GameManager.instance.DollTransfrom.position = new Vector3(
            transform.position.x,
            GameManager.instance.DollTransfrom.position.y,
            GameManager.instance.DollTransfrom.position.z
            );
        GameManager.instance.TreeTransfrom.position = new Vector3(
            transform.position.x,
            GameManager.instance.TreeTransfrom.position.y,
            GameManager.instance.TreeTransfrom.position.z
        );
        TargetEnd = GameManager.instance.TargetEnd.transform;
    }
    // Update is called once per frame
    void Update()
    {
        if (isDying || isPlayerWon || !isReadyStartGame)
            return;

        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);

        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        Debug.Log(" ======================== GetMouseButtonDown ===========  ");
        //        SocketClient.instance.OnMoving();
        //    }
        //    else if (touch.phase == TouchPhase.Moved)
        //    {
        //        PlayerMovementInput = new Vector3(0, 0f, 1);
        //        isWalking = true;
        //    }
        //    else if (touch.phase == TouchPhase.Ended)
        //    {
        //        PlayerMovementInput = Vector3.zero;
        //        isWalking = false;
        //        SocketClient.instance.OnStopMove();
        //        Debug.Log(" ======================== GetMouseButtonUp dasdadadadad ===========  ");
        //    }
        //}

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Input.GetMouseButtonDown(0)) // 0 : left , 1 : right, 2 : wheel
        {
            Debug.Log(" ======================== GetMouseButtonDown ===========  ");
            SocketClient.instance.OnMoving();
            PlayerMovementInput = new Vector3(0, 0f, 1);
            isWalking = true;
        }
        if (Input.GetMouseButton(0)) // 0 : left , 1 : right, 2 : wheel
        {
            //anim.Play("Walk");

            Debug.Log(" ======================== GetMouseButton movingggggggggggggggggggggggggg ===========  ");
        } 
        else if (Input.GetMouseButtonUp(0))
        {
            anim.Play("Idle");
            PlayerMovementInput = Vector3.zero;
            isWalking = false;
            SocketClient.instance.OnStopMove();
            Debug.Log(" ======================== GetMouseButtonUp dasdadadadad ===========  ");
        }
        Debug.Log("  isWalking :::   " + isWalking);


        PlayerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        //Debug.Log(" PlayerMouseInput :::::  Horizontal h  " + h + "     Vertical v     " + v);

        //CheckJumping();
        CheckMoving();
        MovePlayer();

        CheckDeathTime();
    }

    private void MovePlayer()
    {
        if (!isWalking) return;
        //Vector3 MoveVector = transform.TransformDirection(PlayerMovementInput) * Speed;
        //PlayerBody.velocity = new Vector3(MoveVector.x, PlayerBody.velocity.y, MoveVector.z);

        float step = Speed * Time.deltaTime;
        //transform.position = Vector3.MoveTowards(transform.position, TargetEnd.position, step);
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, 0, TargetEnd.position.z), step);
    }
    private void PlayAnimationSmoothly(string animationName, float delayTime)
    {
        if (animationPlaying == animationName)
        {
            Debug.LogWarning(animationName + " is playing");
            return;
        }
        animationPlaying = animationName;        
        anim.CrossFadeInFixedTime(animationName, delayTime);        
    }    

    private void CheckMoving()
    {
        isMoving = isWalking;
    }

    private void CheckDeathTime()
    {
        if (GameManager.instance.headTurnComplete && isMoving && isInDeathZone || GameManager.instance.headTimeFinish)
        {
            //Debug.Log("GameManager.headTurnComplete ---------------------------------- " + GameManager.instance.headTurnComplete);
            //Debug.Log("isMoving ---------------------------------- " + isMoving);
            //Debug.Log("isInDeathZone ---------------------------------- " + isInDeathZone);
            //Debug.Log("GameManager.headTimeFinish ---------------------------------- " + GameManager.instance.headTimeFinish);

            if (isPlayerWon)
                return;

            SocketClient.instance.OnPlayerDie();

            isDying = true;
            anim.SetBool("isDying", true);
            //feetSteps.Stop();
            shoot.Play(0);
            die.PlayDelayed(.2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isInDeathZone = other.transform == deathZone;

        if(other.transform == TargetEnd)
        {
            Debug.Log(" player reach to target end line to win");
            isPlayerWon = true;
            SocketClient.instance.OnPlayerWin();

            anim.SetBool("isWalking", false);
            //feetSteps.Stop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == deathZone)
            isInDeathZone = false;
    }
    public void CheckHostStatus()
    {
        if (isHost)
        {
            WatingHost.SetActive(false);
            HostStartGame.SetActive(true);
        } 
        else
        {
            WatingHost.SetActive(true);
            HostStartGame.SetActive(false);
        }
        
    }
    public void RequestStartGame()
    {
        SocketClient.instance.OnStartGame();
        HostStartGame.GetComponent<Button>().interactable = false;
    }
    public void StartGame()
    {
        isReadyStartGame = true;
        ReadyScreen.SetActive(false);
        GameManager.instance.ReadyToPlay();
    }
    public void SetTextGameTimer(string timer)
    {
        GameTimer.text = timer;
    }
    public void SetPlayerName(string name)
    {
        PlayerName.text = name;
    }
    
    public void PlayAgain()
    {
        if (isPlayerWon)
            bg_Win.Stop();
        else
            bg_Die.Stop();
        isReadyStartGame = false;
        SceneManager.LoadScene("MainMenu");
    }
    public void EnableEndGameScreen()
    {
        if (isPlayerWon)
            bg_Win.Play();
        else
            bg_Die.Play();

        EndGameScreen.SetActive(true);
        PlayerName.gameObject.SetActive(false);
        SocketClient.instance.OnCloseConnectSocket();
    }

    public void AddPlayerResult(string playerName, string playerStatus, int index)
    {
        Debug.Log(" index ================   " + index);
        Vector3 pos = new Vector3(0,- 100 * index + 200, 0);
        TMPro.TextMeshProUGUI user = ResultTransfrom.gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        string _text = playerName + " is " + playerStatus;
        
        user.text = _text;
        user.fontSize = 50;
        if(playerStatus == "die")
        {
            user.color = Color.black;
        }
        else
        {
            user.color = Color.green;
        }
        user.gameObject.SetActive(true);
        //user.gameObject.transform.position = pos;
        Debug.Log(" pos ================   " + pos);
        GameObject resultPlayer = Instantiate(user.gameObject, EndGameScreen.transform);
        resultPlayer.transform.localPosition = pos;
        ResultTransfrom.gameObject.SetActive(false);
    }
}
