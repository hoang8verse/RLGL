using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResult : MonoBehaviour
{
    [Header("Player Avatar")]
    [SerializeField] RawImage m_playerAvatar;
    [SerializeField] Image m_avatarHolder;

    [Header("Player Name")]
    [SerializeField] TextMeshProUGUI m_playerName;

    [Header("Result Symbols")]
    [SerializeField] bool m_isWin;
    [SerializeField] Image m_resultPanel;
    [SerializeField] GameObject m_victoryMark;

    public bool IsWin => m_isWin;
    private Color VICTORY_COLOR = new Color32(0x16, 0x55, 0x1E, 0xFF);
    private Color DEFEAT_COLOR = new Color(226f / 255f, 226f / 255f, 226f / 255f);

    // Start is called before the first frame update
    void Start()
    {
        if (m_isWin)
        {
            m_victoryMark.SetActive(true);
            m_resultPanel.color = VICTORY_COLOR;
            m_avatarHolder.color = VICTORY_COLOR;
            m_playerName.color = Color.white;
        }
        else
        {
            m_victoryMark.SetActive(false);
            m_resultPanel.color = DEFEAT_COLOR;
            m_avatarHolder.color = DEFEAT_COLOR;
            m_playerName.color = new Color32(0x16, 0x55, 0x1E, 0xFF);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerResult(bool isWin)
    {
        m_isWin = isWin;
    }
    public void SetPlayerName(string playerName)
    {

        m_playerName.text = playerName;
    }
    public void SetPlayerAvatar(Texture2D avatar)
    {
        m_playerAvatar.texture = avatar;
    }    
}
