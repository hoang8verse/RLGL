using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UIElements;

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

    [SerializeField] TMPro.TextMeshProUGUI m_notificationText;

    [SerializeField]
    AudioSource bg_Music;
    [SerializeField]
    private AudioSource bg_Win;
    [SerializeField]
    private AudioSource bg_Die;
    [SerializeField]
    private AudioSource vfx_click;
    [SerializeField]
    private TMPro.TMP_InputField inputRoomId;
    [SerializeField]
    private TMPro.TMP_InputField inputPlayerName;

    public string userAppId = "";
    public string userAvatar = "https://h5.zdn.vn/static/images/avatar.png";
    public string playerName = "anonymous";
    public string roomId = "";
    public string isHost = "0";
    //public Dictionary<string, GameObject> listPlayers;
    public Dictionary<string, Texture2D> listPlayerAvatars;

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


        //listPlayers = new Dictionary<string, GameObject>();
        listPlayerAvatars = new Dictionary<string, Texture2D>();
        StartCoroutine(WaitingReceiver());
    }
    IEnumerator WaitingReceiver()
    {
        //roomId = "roomid";// test already have room id
        yield return new WaitForSeconds(0.5f);
        if (roomId != "")
        {
            homeScreen.SetActive(false);
            createRoomScreen.SetActive(false);
            joinRoomScreen.SetActive(true);
            failJoinRoomScreen.SetActive(false);

            inputRoomId.text = roomId;
            JoinRoom();
        }
        else
        {
            homeScreen.SetActive(true);
            createRoomScreen.SetActive(false);
            joinRoomScreen.SetActive(false);
            failJoinRoomScreen.SetActive(false);
        }

        bg_Music.Play(0);
    }
    private void Update()
    {
        if (myKeyboard != null && myKeyboard.status == TouchScreenKeyboard.Status.Done)
        {
            currentInput.text = myKeyboard.text;
            if (homeScreen.activeSelf)
            {
                inputPlayerName.text = myKeyboard.text;
                playerName = myKeyboard.text;
                Debug.Log("Input roomId: " + roomId);
            }
            if (joinRoomScreen.activeSelf)
            {
                inputRoomId.text = myKeyboard.text;
                roomId = myKeyboard.text;
                Debug.Log("Input roomId: " + roomId);
            }

            myKeyboard = null;

        }
        //if (TouchScreenKeyboard.visible && Input.touchCount == 0)
        //{
        //    currentInput.text = myKeyboard.text;
        //    Debug.Log("Key board value was pressed : " + myKeyboard.text);
        //}
            
        
        //if (Input.anyKeyDown)
        //{
        //    // Check if any key was pressed down
        //    foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
        //    {
        //        if (Input.GetKeyDown(code))
        //        {
        //            // The code variable will contain the value of the key that was pressed down
        //            Debug.Log("Key " + code + " was pressed");
        //        }
        //    }
        //}
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
        OnClickVfx();

        if (joinRoomScreen.activeSelf)
        {
            roomId = inputRoomId.text;
        }
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
        OnClickVfx();

        bg_Music.Stop();
        SceneManager.LoadScene("Game");
    }
    public void HostCreateNewRoom()
    {
        OnClickVfx();

        roomId = Generate();
        //RoomId.text = "Room ID : " +  roomId;
        isHost = "1";
        JoinRoom();
    }
    public void UserJoinRoom()
    {
        OnClickVfx();

        roomId = inputRoomId.text;
        joinRoomScreen.SetActive(true);
        homeScreen.SetActive(false);
        isHost = "0";
    }
    public void ShowFailScreen(string message)
    {
        //failMessage.text = message;
        //homeScreen.SetActive(false);
        //joinRoomScreen.SetActive(false);
        //createRoomScreen.SetActive(false);
        //failJoinRoomScreen.SetActive(true);

        //m_notificationText.text = message;
        m_notificationText.text = "Mã phòng " + RoomId.text + " không tồn tại";
        m_notificationText.gameObject.SetActive(true);

    }

    public void FailToJoinRoom()
    {
        OnClickVfx();

        homeScreen.SetActive(true);
        joinRoomScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        failJoinRoomScreen.SetActive(false);
    }

    //public void AddPlayerJoinRoom(string _clientId, string _playerName, int index)
    //{
    //    Debug.Log(" index ================   " + index);
    //    Vector3 pos = new Vector3(0, -100 * index + 160, 0);
    //    TMPro.TextMeshProUGUI user = playerPrefab;
    //    string _text = _playerName + " is join the game." ;

    //    user.text = _text;
    //    if (_clientId != SocketClient.instance.clientId)
    //    {
    //        user.color = Color.white;
    //    }
    //    else
    //    {
    //        user.color = Color.green;
    //    }
    //    user.gameObject.SetActive(true);
    //    Debug.Log(" pos ================   " + pos);
    //    listPlayers[_clientId] = Instantiate(user.gameObject, transformPlayers);
    //    listPlayers[_clientId].transform.localPosition = pos;
    //    playerPrefab.gameObject.SetActive(false);
    //}

    public void AddPlayerJoinRoomByAvatar(Texture2D avatar, string playerID, int index)
    {
        if(avatar != null && index < createRoomScreen.GetComponent<LobbyScreen>().avatarsLists.Count - 1)
        {
            listPlayerAvatars.Add(playerID, avatar);
            createRoomScreen.GetComponent<LobbyScreen>().SetAvatarForPlayer(avatar, index);
        }
    }

    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = roomId;
    }

    public void ShareLinkToInvite()
    {
        OnClickVfx();
        JavaScriptInjected.instance.SendRequestShareRoom();
    }

    public void OnClickVfx()
    {
        vfx_click.Play();
    }

}
