using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;

    [SerializeField]
    private TMPro.TextMeshProUGUI inputPlayerName;

    public string playerName = "anonymous";

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

    }
    public void PlayerNameChange ()
    {
        playerName = inputPlayerName.text;
        Debug.Log("playerName ==================== " + playerName);
    }
    public void JoinTheGame()
    {
        playerName = inputPlayerName.text;
        SceneManager.LoadScene("Game");
    }

}
