using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Net.WebSockets;
using Newtonsoft.Json.Linq;

// Use plugin namespace
using NativeWebSocket;
using Random = UnityEngine.Random;
using System.Globalization;

public class SocketClient : MonoBehaviour
{

    public static SocketClient instance;

    public delegate void ReceiveAction(string message);
    public event ReceiveAction OnReceived;

    //public ClientWebSocket webSocket = null;
    public WebSocket webSocket;

    [SerializeField]
    private string url = "";
    static string baseUrl = "ws://192.168.1.39";
    static string HOST = "8081";

    //static string baseUrl = "ws://34.87.31.157";
    //static string HOST = "8081";

    public string ROOM = "";
    public string clientId = "";

    public string playerJoinName = "";
    public int currentPlayerJoined = 0;

    public bool isHost = false;

    //[SerializeField]
    //Transform SpawnArea;
    //[SerializeField]
    //Transform deathZone;

    [SerializeField]
    private GameObject playerMenPrefab;
    [SerializeField]
    private GameObject playerWomenPrefab;
    public GameObject player = null;
    [SerializeField]
    private GameObject otherPlayerMenPrefab;
    [SerializeField]
    private GameObject otherPlayerWomenPrefab;
    public JArray players;

    private Dictionary<string,GameObject> otherPlayers;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    }
    void Start()
    {        
        //OnConnectWebsocket();

        otherPlayers = new Dictionary<string, GameObject>();
    }

    void Update()
    {
       
#if !UNITY_WEBGL || UNITY_EDITOR
        if(webSocket!=null)
            webSocket.DispatchMessageQueue();
#endif
    }

    async void OnDestroy()
    {
        if (webSocket != null)
        {
            //webSocket.Dispose();
           await webSocket.Close();
        }

        Debug.Log("WebSocket closed.");
    }

    private Vector3 RandomPosition()
    {
        Vector3 center = GameManager.instance.SpawnArea.position;
        Vector3 scale = GameManager.instance.SpawnArea.localScale;
        Vector3 randomPoint = new Vector3(Random.Range(-0.5f, 0.5f),
                                          2,
                                          0
                                          );
        randomPoint = Vector3.Scale(randomPoint, scale);
        randomPoint += center;
        return randomPoint;
    }

    public void OnConnectWebsocket()
    {
        url = baseUrl + ":" + HOST;
        Connect(url);
        //OnReceived = ReceiveSocket;
    }
    async void Connect(string uri)
    {
        try
        {
            webSocket = new WebSocket(uri);

            Debug.Log(" webSocket connect ===========================================  " + webSocket.State);
            webSocket.OnOpen += () =>
            {
                Debug.Log("WS OnOpen  ");
                OnRequestRoom();
            };
            webSocket.OnMessage += (bytes) =>
            {
                // Reading a plain text message
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
                ReceiveSocket(message);
            };

            //webSocket.OnMessage += (bytes) =>
            //{

            //    Debug.Log("WS received message:  " + Encoding.UTF8.GetString(bytes));
            //    string message = Encoding.UTF8.GetString(bytes);

            //    Debug.Log("session response : " + message);
            //    ReceiveSocket(message);
            //    //if (OnReceived != null) OnReceived(message);

            //};
            // Add OnError event listener
            webSocket.OnError += (string errMsg) =>
            {
                Debug.Log("WS error: " + errMsg);
            };

            // Add OnClose event listener
            webSocket.OnClose += (WebSocketCloseCode code) =>
            {
                Debug.Log("WS closed with code: " + code.ToString());
                
            };
            // Keep sending messages at every 0.3s
            //InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);
            //Receive();
            await webSocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }

    }

    private async void Send(string message)
    {
        var encoded = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);


        //await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        if (webSocket.State == WebSocketState.Open)
        {
            // Sending bytes
            await webSocket.Send(encoded);

            // Sending plain text
            //await webSocket.SendText(message);
        }
    }
    private void Receive()
    {
        while (webSocket.State== WebSocketState.Open)
        {
            webSocket.OnMessage += (byte[] msg) =>
            {

                Debug.Log("WS received message:  " + System.Text.Encoding.UTF8.GetString(msg));
                string message = Encoding.UTF8.GetString(msg);

                Debug.Log("session response : " + message);
                if (OnReceived != null) OnReceived(message);

            };

            // Add OnError event listener
            webSocket.OnError += (string errMsg) =>
            {
                Debug.Log("WS error: " + errMsg);
            };

            // Add OnClose event listener
            webSocket.OnClose += (WebSocketCloseCode code) =>
            {
                Debug.Log("WS closed with code: " + code.ToString());
            };

        }
    }

    //private async Task Receive()
    //{

    //    ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);

    //    while (webSocket.State == WebSocketState.Open)
    //    {

    //        WebSocketReceiveResult result = null;

    //        using (var ms = new MemoryStream())
    //        {
    //            do
    //            {
    //                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
    //                ms.Write(buffer.Array, buffer.Offset, result.Count);
    //            }
    //            while (!result.EndOfMessage);

    //            ms.Seek(0, SeekOrigin.Begin);

    //            if (result.MessageType == WebSocketMessageType.Text)
    //            {
    //                using (var reader = new StreamReader(ms, Encoding.UTF8))
    //                {
    //                    string message = reader.ReadToEnd();

    //                    Debug.Log("session response : " + message);
    //                    if (OnReceived != null) OnReceived(message);

    //                }
    //            }
    //            else if (result.MessageType == WebSocketMessageType.Close)
    //            {
    //                Debug.Log("session onclose : " + WebSocketMessageType.Close);
    //                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    //            }
    //        }
    //    }


    //}

    void ReceiveSocket(string message)
    {
        JObject data = JObject.Parse(message);

        switch (data["event"].ToString())
        {
            case "roomDetected":
                ROOM = data["room"].ToString();
                clientId = data["clientId"].ToString();
                //OnJoinRoom();
                OnJoinLobbyRoom();

                break;
            case "failJoinRoom":
                Debug.Log("  failJoinRoom =================  " + data);
                MainMenu.instance.ShowFailScreen(data["message"].ToString());
                break;
            case "joinLobbyRoom":
                MainMenu.instance.ShowLobby();
                players = JArray.Parse(data["players"].ToString());

                currentPlayerJoined = players.Count;
                Debug.Log(" joinLobbyRoom   " + players);

                // for new player
                if (data["clientId"].ToString() == clientId && player == null)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        //MainMenu.instance.AddPlayerJoinRoom(players[i]["id"].ToString(),players[i]["playerName"].ToString(), i);
                        StartCoroutine(LoadAvatarImage(players[i]["avatar"].ToString(), players[i]["id"].ToString(), i));


                    }
                } 
                // for old player
                else
                {
                    //MainMenu.instance.AddPlayerJoinRoom(data["clientId"].ToString(), data["playerName"].ToString(), players.Count - 1);
                    StartCoroutine(LoadAvatarImage(data["avatar"].ToString(), data["clientId"].ToString(), players.Count - 1));
                }

                break;
            case "gotoGame":
                MainMenu.instance.GotoGame();
                break;
            case "joinRoom":
                players = JArray.Parse(data["players"].ToString());

                currentPlayerJoined = players.Count;
                Debug.Log(" joinRoom playersssssssss  " + players);

                foreach (var _player in players)
                {
                    string _clientId = _player["id"].ToString();
                    playerJoinName = _player["playerName"].ToString();
                    Debug.Log(" clientId =================  " + clientId + "   ---   _clientId ==  " + _clientId);

                    Vector3 pos = new Vector3(float.Parse(_player["position"][0].ToString()), 
                        float.Parse(_player["position"][1].ToString()), 
                        float.Parse(_player["position"][2].ToString()));
                    if (_clientId == clientId && player == null)
                    {
                        Debug.Log("  ===========  player =================  " );
                        //  player
                        
                        if(_player["gender"].ToString() == "0")
                        {
                            player = Instantiate(playerMenPrefab, pos, GameManager.instance.SpawnArea.rotation);
                        }
                        else
                        {
                            player = Instantiate(playerWomenPrefab, pos, GameManager.instance.SpawnArea.rotation);
                        }
                        player.name = "Player-" + playerJoinName;
                        player.GetComponent<PlayerMovement>().deathZone = GameManager.instance.DeathZone;
                        isHost = _player["isHost"].ToString() == "1";
                        player.GetComponent<PlayerMovement>().isHost = isHost;
                        player.GetComponent<PlayerMovement>().CheckHostStatus();
                        player.GetComponent<PlayerMovement>().SetPlayerName(playerJoinName);
                        player.SetActive(true);

                    } 
                    else if (_clientId != clientId )
                    {
                        Debug.Log("  ===========  other player =================  ");
                        if (!otherPlayers.ContainsKey(_clientId))
                        {
                            // other player
                            if (_player["gender"].ToString() == "0")
                            {
                                otherPlayers[_clientId] = Instantiate(otherPlayerMenPrefab, pos, GameManager.instance.SpawnArea.rotation);
                            }
                            else
                            {
                                otherPlayers[_clientId] = Instantiate(otherPlayerWomenPrefab, pos, GameManager.instance.SpawnArea.rotation);
                            }

                            otherPlayers[_clientId].name = "otherplayer-" + playerJoinName;
                            otherPlayers[_clientId].SetActive(true);
                        }

                    }

                }

                break;
            case "startGame":
                Debug.Log("  startGame =================  " + data);
                player.GetComponent<PlayerMovement>().StartGame();
                break;

            case "moving":

                if (otherPlayers.ContainsKey(data["clientId"].ToString()))
                    otherPlayers[data["clientId"].ToString()].GetComponent<OtherPlayer>().StartWalking();
                break;
            case "stopMove":
                if (otherPlayers.ContainsKey(data["clientId"].ToString()))
                    otherPlayers[data["clientId"].ToString()].GetComponent<OtherPlayer>().StopWalking();
                break;

            case "headTurn":
                Debug.Log("  headTurn data ==========  " + data);
                GameManager.instance.speedHeadTurn = float.Parse(data["speedHeadTurn"].ToString());
                GameManager.instance.DoSingAndHeadTurn();
                break;
            case "playerDie":
                Debug.Log("  playerDie data ==========  " + data);
                if (otherPlayers.ContainsKey(data["clientId"].ToString()))
                    otherPlayers[data["clientId"].ToString()].GetComponent<OtherPlayer>().SetDeadPlayer();
                break;
            case "playerWin":

                break;
            case "endGame":
                Debug.Log("  endGame data ==========  " + data);
                players = JArray.Parse(data["players"].ToString());
                for (int i = 0; i < players.Count; i++)
                {
                    Texture2D avatar = MainMenu.instance.listPlayerAvatars[players[i]["id"].ToString()];
                    player.GetComponent<PlayerMovement>().AddPlayerResult(avatar, players[i]["playerName"].ToString(), players[i]["playerStatus"].ToString(), i);
                    //player.GetComponent<PlayerMovement>().AddPlayerResult(players[i]["playerName"].ToString(), players[i]["playerStatus"].ToString(), i);
                }
                player.GetComponent<PlayerMovement>().EnableEndGameScreen();
                break;
            case "playerLeaveRoom":
                string playerLeaveId = data["clientId"].ToString();

                //MainMenu.instance.listPlayers[playerLeaveId].SetActive(false);
                //Destroy(MainMenu.instance.listPlayers[playerLeaveId]);

                for (int i = 0; i < players.Count; i++)
                {
                    Debug.Log(" players player leave ==   " + players[i].ToString());
                    if (playerLeaveId == players[i]["id"].ToString())
                    {
                        players.RemoveAt(i);
                        Debug.Log(" players playerLeaveRoom 222222222222222  " + playerLeaveId);

                    }
                }

                Debug.Log(" data player leave ==   " + data.ToString());
                // check new host 
                string checkNewHost = data["newHost"].ToString();
                
                if (checkNewHost != "" && checkNewHost == clientId)
                {
                    isHost = true;

                    if (player != null)
                    {
                        player.GetComponent<PlayerMovement>().isHost = true;
                    } 
                    else
                    {
                        MainMenu.instance.isHost = "1";
                        MainMenu.instance.CheckTheHost();
                    }
                    
                }
                break;

            default:
                break;
        }
    }
    public void OnRequestRoom()
    {
        string room = MainMenu.instance.roomId;
        JObject jsData = new JObject();
        jsData.Add("meta", "requestRoom");
        jsData.Add("playerLen", 30);
        jsData.Add("room", room);
        jsData.Add("host", MainMenu.instance.isHost);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData).ToString());
    }
    public void OnJoinLobbyRoom()
    {
        string playerName = MainMenu.instance.playerName;

        if (playerName.Length <= 1)
        {
            playerName = "anonymous";
        }

        JObject jsData = new JObject();
        jsData.Add("meta", "joinLobby");
        jsData.Add("room", ROOM);
        jsData.Add("isHost", MainMenu.instance.isHost);
        jsData.Add("playerName", playerName);
        jsData.Add("userAppId", MainMenu.instance.userAppId);
        jsData.Add("avatar", MainMenu.instance.userAvatar);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }
    public void OnJoinRoom()
    {
        string playerName = MainMenu.instance.playerName;

        if (playerName.Length <= 1 )
        {
            playerName = "anonymous";
        }

        Vector3 ranPos = RandomPosition();

        JObject jsData = new JObject();
        jsData.Add("meta", "join");
        jsData.Add("room", ROOM);
        jsData.Add("isHost", MainMenu.instance.isHost);
        jsData.Add("playerName", playerName);
        jsData.Add("pos", ranPos.ToString());
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }
    public void OnGotoGame()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "gotoGame");
        jsData.Add("room", ROOM);

        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData).ToString());
    }
    public void OnStartGame()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "startGame");
        jsData.Add("room", ROOM);

        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData).ToString());
    }
    public void OnHeadTurn()
    {
        if (!player.GetComponent<PlayerMovement>().isHost) return;
        JObject jsData = new JObject();
        jsData.Add("meta", "headTurn");
        jsData.Add("clientId", clientId);
        jsData.Add("room", ROOM);
        jsData.Add("maxTime", GameManager.instance.GetGameTime());
        jsData.Add("currentTime", GameManager.instance.timeValue);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }
    public void OnMoving()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "moving");
        jsData.Add("clientId", clientId);
        jsData.Add("room", ROOM);
        jsData.Add("isMoving", true);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }

    public void OnStopMove()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "stopMove");
        jsData.Add("clientId", clientId);
        jsData.Add("room", ROOM);
        jsData.Add("isMoving", false);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }
    public void OnPlayerDie()
    {
        Debug.Log(" ======================== OnPlayerDie() ======================================");
        JObject jsData = new JObject();
        jsData.Add("meta", "playerDie");
        jsData.Add("clientId", clientId);
        jsData.Add("room", ROOM);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }

    public void OnPlayerWin()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "playerWin");
        jsData.Add("clientId", clientId);
        jsData.Add("room", ROOM);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }

    public void OnEndGame()
    {
        if (!player.GetComponent<PlayerMovement>().isHost) return;
        JObject jsData = new JObject();
        jsData.Add("meta", "endGame");
        jsData.Add("clientId", clientId);
        jsData.Add("room", ROOM);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }

    public void OnLeaveRoom()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "leave");
        jsData.Add("clientId", clientId);
        jsData.Add("room", ROOM);
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }

    public async void OnCloseConnectSocket()
    {
        await webSocket.Close();
    }

    IEnumerator LoadAvatarImage(string imageUrl, string playerID, int index)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            Texture2D textureImageUrl = null;
            MainMenu.instance.AddPlayerJoinRoomByAvatar(textureImageUrl, playerID, index);
        }
        else
        {
            Texture2D textureImageUrl = ((DownloadHandlerTexture)request.downloadHandler).texture;
            // use the texture here
            MainMenu.instance.AddPlayerJoinRoomByAvatar(textureImageUrl, playerID, index);
        }
    }

}
