using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private int minutes;
    private float timeValue;
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

    public bool headTime;
    public bool headTimeFinish;

    public bool headTurnComplete;

    [SerializeField]
    private int totalBots;

    [SerializeField]
    GameObject Bot;

    [SerializeField]
    Transform SpawnArea;

    public Transform DollTransfrom;
    public Transform TreeTransfrom;

    static bool isReadyStartGame =  false;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        //SpawnBots();
        headTime = false;
        headTurnComplete = false;
        timeValue = minutes * 60;
    }

    public IEnumerator CheckReadyToStart()
    {
        yield return new WaitForSeconds(1f);
        
        if (SocketClient.instance.player.GetComponent<PlayerMovement>().isReadyStartGame)
        {
            isReadyStartGame = true;
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
            // set end game
            SocketClient.instance.OnEndGame();
            return;
        }

        if (secs % headTimer == 0 && secs != lastTimeToHead)
        {
            lastTimeToHead = secs;
            headTime = !headTime;

            if (headTime)
            {
                dollHeadOn.Play(0);
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

    private void RotHead(int deg)
    {
        Vector3 direction = new Vector3(Head.rotation.eulerAngles.x, deg, Head.rotation.eulerAngles.z);
        Quaternion targetRotation = Quaternion.Euler(direction);
        Head.rotation = Quaternion.Lerp(Head.rotation, targetRotation, Time.deltaTime * 3);
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
