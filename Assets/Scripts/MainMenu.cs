using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    private TMPro.TextMeshProUGUI RoomId;

    public string playerName = "anonymous";
    public string roomId = "";
    public string isHost = "0";

    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public int length = 8;

    TouchScreenKeyboard myKeyboard;
    private TMPro.TextMeshProUGUI currentInput;
    public string Generate()
    {
        string result = "";
        System.Random rand = new System.Random();
        while (result.Length < length)
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
    public void JoinTheGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void HostCreateNewRoom()
    {
        roomId = Generate();
        RoomId.text = "Room ID : " +  roomId;
        createRoomScreen.SetActive(true);
        homeScreen.SetActive(false);
        isHost = "1";
    }
    public void UserJoinRoom()
    {
        joinRoomScreen.SetActive(true);
        homeScreen.SetActive(false);
        isHost = "0";
    }

}
