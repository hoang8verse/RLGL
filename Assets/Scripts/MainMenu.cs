using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [SerializeField]
    private GameObject homeScreen;
    [SerializeField]
    private GameObject createRoomScreen;
    [SerializeField]
    private GameObject joinRoomScreen;
    [SerializeField]
    private GameObject failJoinRoomScreen;
    [SerializeField]
    private TMPro.TextMeshProUGUI failMessage;
    [SerializeField]
    private GameObject hostButtonJoinGame;
    [SerializeField]
    private Transform transformPlayers;
    [SerializeField]
    private TMPro.TextMeshProUGUI playerPrefab;

    [SerializeField]
    private TMPro.TextMeshProUGUI RoomId;

    public string playerName = "anonymous";
    public string roomId = "";
    public string isHost = "0";
    public Dictionary<string, GameObject> listPlayers;

    //private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const string CHARS = "abcdefghijklmnopqrstuvwxyz";
    public int length = 6;

    TouchScreenKeyboard myKeyboard;
    private TMPro.TextMeshProUGUI currentInput;
    public string Generate()
    {
        string result = "";
        System.Random rand = new System.Random();
        while (result.Length < 6)
        {
            result += CHARS[rand.Next(0, CHARS.Length)];
        }
        return result;
        //if (IsUnique(result))
        //{
        //    return result;
        //}
        //else
        //{
        //    return Generate();
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        homeScreen.SetActive(true);
        listPlayers = new Dictionary<string, GameObject>();
    }
    private void Update()
    {
        if (myKeyboard != null && myKeyboard.status == TouchScreenKeyboard.Status.Done)
        {
            Debug.Log("Input: " + myKeyboard.text);
            currentInput.text = myKeyboard.text;
            myKeyboard = null;
        }
    }
    public void PlayerNameChange (TMPro.TextMeshProUGUI inputPlayerName)
    {
        playerName = inputPlayerName.text;
    }
    public void InputRoomId(TMPro.TextMeshProUGUI inputRoomId)
    {
        roomId = inputRoomId.text;
    }
    public void OnSelectedInput(TMPro.TextMeshProUGUI _currentInput)
    {
        currentInput = _currentInput;
        //TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        if (myKeyboard == null)
        {
            myKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        }


    }
    public void CheckTheHost()
    {
        if(isHost == "1")
        {
            hostButtonJoinGame.SetActive(true);
        }
        else
        {
            hostButtonJoinGame.SetActive(false);
        }
    }
    public void JoinRoom()
    {
        RoomId.text = roomId;

        if (playerName.Length <= 1)
        {
            playerName = "anonymous";
        }
        SocketClient.instance.OnConnectWebsocket();
        //createRoomScreen.SetActive(true);
        //homeScreen.SetActive(false);
        //joinRoomScreen.SetActive(false);
        //CheckTheHost();
    }
    public void ShowLobby()
    {
        createRoomScreen.SetActive(true);
        homeScreen.SetActive(false);
        joinRoomScreen.SetActive(false);
        CheckTheHost();
    }
    public void JoinTheGame()
    {
        //SceneManager.LoadScene("Game");
        SocketClient.instance.OnGotoGame();
    }
    public void GotoGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void HostCreateNewRoom()
    {
        roomId = Generate();
        //RoomId.text = "Room ID : " +  roomId;
        isHost = "1";
        JoinRoom();
    }
    public void UserJoinRoom()
    {
        joinRoomScreen.SetActive(true);
        homeScreen.SetActive(false);
        isHost = "0";
    }
    public void ShowFailScreen(string message)
    {
        failMessage.text = message;
        homeScreen.SetActive(false);
        joinRoomScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        failJoinRoomScreen.SetActive(true);
    }

    public void FailToJoinRoom()
    {
        homeScreen.SetActive(true);
        joinRoomScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        failJoinRoomScreen.SetActive(false);
    }

    public void AddPlayerJoinRoom(string _clientId, string _playerName, int index)
    {
        Debug.Log(" index ================   " + index);
        Vector3 pos = new Vector3(0, -100 * index + 160, 0);
        TMPro.TextMeshProUGUI user = playerPrefab;
        string _text = _playerName + " is join the game." ;

        user.text = _text;
        if (_clientId != SocketClient.instance.clientId)
        {
            user.color = Color.white;
        }
        else
        {
            user.color = Color.green;
        }
        user.gameObject.SetActive(true);
        Debug.Log(" pos ================   " + pos);
        listPlayers[_clientId] = Instantiate(user.gameObject, transformPlayers);
        listPlayers[_clientId].transform.localPosition = pos;
        playerPrefab.gameObject.SetActive(false);
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = roomId;
    }

}
