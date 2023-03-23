using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ZXing;
using ZXing.QrCode;

namespace UIElements
{
    public class LobbyScreen : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_roomID;
        [SerializeField] RawImage m_qrImage;
        [SerializeField] Texture2D m_avatarDefault;
        [SerializeField] Dictionary<string, int> m_playerAvatarsList;
        public List<PlayerAvatar> avatarsLists;
        int[] listAvatarAvailable;

        public string RoomID => m_roomID.text;
        public RawImage QRImage => m_qrImage;
        
        // Start is called before the first frame update
        void Start()
        {
            m_playerAvatarsList = new Dictionary<string, int>();
            listAvatarAvailable = new int[avatarsLists.Count];
            for (int i = 0; i < listAvatarAvailable.Length; i++)
            {
                listAvatarAvailable[i] = 0;
            }
            m_roomID.text = MainMenu.instance.roomId;

            string qrCoreGen = MainMenu.deepLinkZaloApp + "?roomId="+ MainMenu.instance.roomId;
            m_qrImage.texture = GetQRCodeTexture(qrCoreGen, 256, 256);
        }
        
        int GetIndexAvatarValid()
        {
            for (int i = 0; i < listAvatarAvailable.Length; i++)
            {
                if( listAvatarAvailable[i] == 0)
                {
                    listAvatarAvailable[i] = 1;
                    return i;
                }
            }
            return -1;
        }

        public void SetAvatarForPlayer(Texture2D avatarImage, string playerId)
        {
            int index = GetIndexAvatarValid();
            if (index == -1) return;
            m_playerAvatarsList[playerId] = index;
            avatarsLists[index].gameObject.GetComponent<RawImage>().texture = avatarImage;
        }
        public void RemoveAvatarForPlayer(string playerId)
        {
            int index = m_playerAvatarsList[playerId];
            listAvatarAvailable[index] = 0;
            avatarsLists[index].gameObject.GetComponent<RawImage>().texture = m_avatarDefault;
        }

        public void ResetAvatarList()
        {
            for (int i = 0; i < avatarsLists.Count; i++)
            {
                listAvatarAvailable[i] = 0;
                avatarsLists[i].gameObject.GetComponent<RawImage>().texture = m_avatarDefault;
            }

        }
        public void SetRoomID(string roomID)
        {
            m_roomID.text = roomID;
        }
        public void OnCopyRoomID()
        {
            
        }
        public void OnStartGame()
        {
            MainMenu.instance.JoinTheGame();
        }
        public void OnExitScreen()
        {
             MainMenu.instance.BackToMainMenu();
        }

        private Texture2D GetQRCodeTexture(string text, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width
                }
            };
            var result = writer.Write(text);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(result);
            texture.Apply();
            return texture;
        }
    }
}