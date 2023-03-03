﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    public string ROOM = "";
    public string clientId = "";

    public string playerJoinName = "";
    public int currentPlayerJoined = 0;
    
    [SerializeField]
    Transform SpawnArea;
    [SerializeField]
    Transform deathZone;

    [SerializeField]
    private GameObject playerPrefab;
    public GameObject player = null;
    [SerializeField]
    private GameObject otherPlayerPrefab;
    public JArray players;

    private Dictionary<string,GameObject> otherPlayers;

    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);

        OnConnectWebsocket();

        otherPlayers = new Dictionary<string, GameObject>();
    }

    void Update()
    {
       
#if !UNITY_WEBGL || UNITY_EDITOR
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
        Vector3 origin = SpawnArea.position;
        Vector3 range = SpawnArea.localScale / 2.0f;
        Vector3 randomRange = new Vector3(Random.Range(-range.x, range.x),
                                          Random.Range(-range.y, range.y),
                                          Random.Range(0, range.z));
        Vector3 randomCoordinate = origin + randomRange;
        return randomCoordinate;
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
                OnJoinRoom();

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
                        player = Instantiate(playerPrefab, pos, SpawnArea.rotation);
                        player.name = "Player-" + playerJoinName;
                        player.GetComponent<PlayerMovement>().deathZone = deathZone;
                        player.GetComponent<PlayerMovement>().isHost = _player["isHost"].ToString() == "1";
                        player.GetComponent<PlayerMovement>().CheckHostStatus();
                        player.GetComponent<PlayerMovement>().SetPlayerName(playerJoinName);
                        player.SetActive(true);

                    } 
                    else if (_clientId != clientId )
                    {
                        Debug.Log("  ===========  other player =================  ");
                        // other player
                        otherPlayers[_clientId] = Instantiate(otherPlayerPrefab, pos, SpawnArea.rotation);
                        otherPlayers[_clientId].name = "otherplayer-" + playerJoinName;
                        otherPlayers[_clientId].SetActive(true);
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
                    player.GetComponent<PlayerMovement>().AddPlayerResult(players[i]["playerName"].ToString(), players[i]["playerStatus"].ToString(), i);
                }
                player.GetComponent<PlayerMovement>().EnableEndGameScreen();
                break;
            case "playerLeaveRoom":
                string playerLeaveId = data["clientId"].ToString();

                for (int i = 0; i < players.Count; i++)
                {
                    Debug.Log(" players player ideeee   " + players[i]["id"].ToString());
                    if (playerLeaveId == players[i]["id"].ToString())
                    {
                        players.RemoveAt(i);
                        Debug.Log(" players playerLeaveRoom 222222222222222  " + playerLeaveId);

                    }
                }

                // win 
                if (players.Count == 1)
                {

                }
                break;

            default:
                break;
        }
    }
    public void OnRequestRoom()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "requestRoom");
        jsData.Add("playerLen", 30);

        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData).ToString());
    }
    public void OnJoinRoom()
    {
        
        Vector3 ranPos = RandomPosition();
        JObject jsData = new JObject();
        jsData.Add("meta", "join");
        jsData.Add("room", ROOM);
        jsData.Add("playerName", MainMenu.instance.playerName);
        jsData.Add("pos", ranPos.ToString());
        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData));
    }
    public void OnStartGame()
    {
        JObject jsData = new JObject();
        jsData.Add("meta", "startGame");
        jsData.Add("room", ROOM);

        Send(Newtonsoft.Json.JsonConvert.SerializeObject(jsData).ToString());
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

}
