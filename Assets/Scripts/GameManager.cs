using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private int minutes;
    public float timeValue;
    private float lastTimeToHead;

    [SerializeField]
    private Transform Head;
    [SerializeField]
    private int headTimer;

    [SerializeField]
    private Text timeText;

    [SerializeField]
    private AudioSource dollSing;
    [SerializeField]
    private AudioSource dollHeadOff;
    [SerializeField]
    private AudioSource dollHeadOn;
    [SerializeField]
    private AudioSource dollScan;
    [SerializeField]
    private AudioSource bg_Music;

    public bool headTime;
    public bool headTimeFinish;
    public bool headTurnComplete;
    public float speedHeadTurn = 3;

    [SerializeField]
    private int totalBots;

    [SerializeField]
    GameObject Bot;


    public Transform DeathZone;
    public Transform SpawnArea;
    public Transform TargetEnd;

    public Transform DollTransfrom;
    public Transform TreeTransfrom;

    static bool isReadyStartGame =  false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {        
        //SpawnBots();
        headTime = false;
        headTurnComplete = false;
        timeValue = minutes * 60;

        SocketClient.instance.OnJoinRoom();
        bg_Music.Play();
    }

    public int GetGameTime()
    {
        return minutes * 60;
    }

    public IEnumerator CheckReadyToStart()
    {
        yield return new WaitForSeconds(0.5f);

        if (SocketClient.instance.player.GetComponent<PlayerMovement>().isReadyStartGame)
        {
            isReadyStartGame = true;
            dollSing.Play();
        }
            
        Debug.Log("CheckReadyToStart ---------------------------------- " + isReadyStartGame);
    }

    public void ReadyToPlay()
    {
        StartCoroutine(CheckReadyToStart());
    }
    
    // Update is called once per frame
    void Update()
    {
        if(isReadyStartGame)
            CountDown();
    }

    private void SpawnBots()
    {
        for (int i = 0; i < totalBots; i++)
            Instantiate(Bot, RandomPosition(), SpawnArea.rotation);
    }

    private Vector3 RandomPosition()
    {
        Vector3 origin = SpawnArea.position;
        Vector3 range = SpawnArea.localScale / 2.0f;
        Vector3 randomRange = new Vector3(Random.Range(-range.x, range.x),
                                          Random.Range(-range.y, range.y),
                                          Random.Range(-range.z, range.z));
        Vector3 randomCoordinate = origin + randomRange;
        return randomCoordinate;
    }

    private void CountDown()
    {
        if (timeValue > 0)
        {
            timeValue -= Time.deltaTime;
        }
        else
        {
            timeValue = 0;
        }

        DisplayTime(timeValue);
    }

    private void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0)
            timeToDisplay = 0;

        float mins = Mathf.FloorToInt(timeToDisplay / 60);
        float secs = Mathf.FloorToInt(timeToDisplay % 60);
        HeadTime(secs);

        //timeText.text = string.Format("{0:00}:{1:00}", mins, secs);
        SocketClient.instance.player.GetComponent<PlayerMovement>().SetTextGameTimer(string.Format("{0:00}:{1:00}", mins, secs));
    }

    private void HeadTime(float secs)
    {
        if (timeValue <= 0)
        {
            headTimeFinish = true;
            RotHead(180);
            isReadyStartGame = false;
            bg_Music.Stop();
            dollSing.Stop();
            // set end game
            SocketClient.instance.OnEndGame();
            return;
        }

        //Debug.Log(" secs ==============  " + secs + "  lastTimeToHead ===  " + lastTimeToHead);
        if (secs % headTimer == 0 && secs != lastTimeToHead)
        {
            Debug.Log(" secs % headTimer ==============  " + secs % headTimer + "  lastTimeToHead ===  " + (secs != lastTimeToHead));
            lastTimeToHead = secs;
            headTime = !headTime;
            //SocketClient.instance.OnHeadTurn();
            if (headTime)
            {
                dollHeadOn.Play(0);
                dollScan.PlayDelayed(.2f);
            }
            else
            {
                if (!dollSing.isPlaying)
                    dollHeadOff.Play(0);

                if (!dollSing.isPlaying)
                    dollSing.PlayDelayed(1);
            }
        }

        if (headTime)
            RotHead(180);
        else
            RotHead(0);
    }

    public void DoSingAndHeadTurn()
    {
        if (headTime)
        {
            dollHeadOn.Play(0);
            dollScan.PlayDelayed(1);
        }
        else
        {
            if (!dollSing.isPlaying)
            {
                dollHeadOff.Play(0);
            }

            if (!dollSing.isPlaying)
                dollSing.PlayDelayed(1);
        }

        //if (headTime)
        //    RotHead(180);
        //else
        //    RotHead(0);
    }

    private void RotHead(int deg)
    {
        Vector3 direction = new Vector3(Head.rotation.eulerAngles.x, deg, Head.rotation.eulerAngles.z);
        Quaternion targetRotation = Quaternion.Euler(direction);
        Head.rotation = Quaternion.Lerp(Head.rotation, targetRotation, Time.deltaTime * speedHeadTurn);
        Vector3 rot = new Vector3(Head.rotation.eulerAngles.x, Head.rotation.eulerAngles.y, Head.rotation.eulerAngles.z);
        //Debug.Log("Head.rotation ============= " + (int)Head.rotation.eulerAngles.y);
        if((int)Head.rotation.eulerAngles.y >= 180 && (int)Head.rotation.eulerAngles.y <= 190)
        {
            headTurnComplete = true;
        } else
        {
            headTurnComplete = false;
        }
    }
}
