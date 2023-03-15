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
        [SerializeField] Dictionary<string, PlayerAvatar> m_playerAvatarsList;
        public List<PlayerAvatar> avatarsLists;

        public string RoomID => m_roomID.text;
        public RawImage QRImage => m_qrImage;
        
        // Start is called before the first frame update
        void Start()
        {
            m_roomID.text = MainMenu.instance.roomId;
            m_qrImage.texture = GetQRCodeTexture(m_roomID.text, 256, 256);
        }

        public void SetAvatarForPlayer(Texture2D avatarImage, int index)
        {
            //m_playerAvatarsList[playerID].SetAvatarImage(avatarImage);
            //avatarsLists[index].SetAvatarImage(ImageLoader.instance.textureImageUrl);

            avatarsLists[index].gameObject.GetComponent<RawImage>().texture = avatarImage;
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
            MainMenu.instance.GotoGame();
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