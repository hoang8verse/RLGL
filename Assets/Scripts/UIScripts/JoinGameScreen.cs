using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIElements
{
    public class JoinGameScreen : Screen
    {
        [SerializeField] TMP_InputField m_inputField;
        [SerializeField] TextMeshProUGUI m_notificationText;

        [SerializeField]
        private string m_roomIDEntered;
        public string RoomIDEntered => m_roomIDEntered;
        private TouchScreenKeyboard m_touchScreenKeyboard;
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnShowNotificationMessage(string notificationText = "Mã phòng không tồn tại*")
        {
            m_notificationText.text = notificationText;
            m_notificationText.gameObject.SetActive(true);
        }
        public void OnEnteredRoomID()
        {
            m_roomIDEntered = m_inputField.text;
        }
        public void OnInputFieldSelected()
        {
            m_touchScreenKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
        }
        public void OnJoinRoom()
        {
            MainMenu.instance.JoinRoom();
        }
        public void OnExitScreen()
        {
            MainMenu.instance.FailToJoinRoom();
        }
    }
}