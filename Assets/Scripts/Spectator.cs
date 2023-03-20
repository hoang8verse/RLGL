using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spectator : MonoBehaviour
{
    float OFFSET_MOVE = 0.1f;
    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private TextMeshProUGUI GameTimer;
    [SerializeField]
    private GameObject EndGameScreen;
    [SerializeField]
    AudioSource clicked;
    private Transform ResultTransfrom;

    // Start is called before the first frame update
    void Start()
    {
        EndGameScreen.SetActive(false);
        GameTimer.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) == true || Input.GetKey(KeyCode.A) == true)
        {
            if (camera.transform.position.x > -120f)
                camera.transform.position = new Vector3(camera.transform.position.x - OFFSET_MOVE, camera.transform.position.y, camera.transform.position.z);
        }
        if (Input.GetKey(KeyCode.RightArrow) == true || Input.GetKey(KeyCode.D) == true)
        {
            if (camera.transform.position.x < 120f)
                camera.transform.position = new Vector3(camera.transform.position.x + OFFSET_MOVE, camera.transform.position.y, camera.transform.position.z);
        }
        if (Input.GetKey(KeyCode.UpArrow) == true || Input.GetKey(KeyCode.W) == true)
        {
            camera.fieldOfView -= OFFSET_MOVE;
        }
        if (Input.GetKey(KeyCode.DownArrow) == true || Input.GetKey(KeyCode.S) == true)
        {
            camera.fieldOfView += OFFSET_MOVE;
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            camera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * 6;
        }

        if(camera.fieldOfView < 60f)
        {
            camera.fieldOfView = 60f;
        } 
        if(camera.fieldOfView > 120f)
        {
            camera.fieldOfView = 120f;
        }
    }

    public void StartGame()
    {

        //ReadyScreen.SetActive(false);
        GameManager.instance.ReadyToPlay();
    }
    public void SetTextGameTimer(string timer)
    {
        GameTimer.text = timer;
    }

    public void PlayAgain()
    {

        OnClickVFX();
        SceneManager.LoadScene("MainMenu");
    }
    public void EnableEndGameScreen()
    {
        EndGameScreen.SetActive(true);
        SocketClient.instance.OnCloseConnectSocket();
    }

    public void AddPlayerResult(string playerName, string playerStatus, int index)
    {
        Debug.Log(" index ================   " + index);
        Vector3 pos = new Vector3(0, -100 * index + 200, 0);
        TMPro.TextMeshProUGUI user = ResultTransfrom.gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        string _text = playerName + " is " + playerStatus;

        user.text = _text;
        user.fontSize = 50;
        if (playerStatus == "die")
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
    public void AddPlayerResult(Texture2D playerAvatar, string playerName, string playerStatus, int index)
    {
        EndGameScreen.GetComponent<UIElements.EndGameScreen>().AddPlayerResult(playerAvatar, playerName, playerStatus, index);
    }
    public void OnClickVFX()
    {
        clicked.Play();
    }

}
