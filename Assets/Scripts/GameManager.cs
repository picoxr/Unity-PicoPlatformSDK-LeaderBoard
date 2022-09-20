using System;
using UnityEngine;
using Pico.Platform;
using UnityEngine.UI;
using Pico.Platform.Models;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Arena Objects")] private GameObject playerTank;

    [Header("Game UI")] public GameObject loadingScreen;
    public GameObject gameOverScreen;
    public GameObject GameUIScreen;
    public string arenaName = "GameService Demo";

    public Text CurrentPlayerNameText;
    public Text TextCurrentScore;

    public string UserOpenId;
    public string UserName;

    [Space] private const int ADD_SCORE = 100;
    private bool _isPaused = false;

    public static int WeakWallCount = 0;

    private int m_CurrentScore = 0;

    void Start()
    {
        playerTank = GameObject.FindGameObjectWithTag("Player");

        loadingScreen.SetActive(true);
        GameUIScreen.SetActive(false);

        WeakWallCount = GameObject.FindGameObjectsWithTag("WeakWall").Length;

        // first login to Pico account
        //Unity.XR.PXR.LoginSDK.Login();
        try
        {
            CoreService.AsyncInitialize().OnComplete(m =>
            {
                if (m.IsError)
                {
                    Debug.LogError($"Async initialize failed: code={m.GetError().Code} message={m.GetError().Message}");
                    return;
                }

                if (m.Data != PlatformInitializeResult.Success && m.Data != PlatformInitializeResult.AlreadyInitialized)
                {
                    Debug.LogError($"Async initialize failed: result={m.Data}");
                    return;
                }

                Debug.Log("AsyncInitialize Successfully");
                GetLoggedInUser();
            });
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }
    }

    void GetLoggedInUser()
    {
        Debug.Log("Trying to get currently logged in user");
        UserService.GetLoggedInUser().OnComplete(msg =>
        {
            if (!msg.IsError)
            {
                Debug.Log("Received get user success");
                User user = msg.Data;
                Debug.Log($"User: {User2String(user)}");
                Debug.Log(JsonUtility.ToJson(user));
                LoginToGameService(user.ID, user.DisplayName);
                OnLoginSuccess();
            }
            else
            {
                Debug.LogError("Received get user error");
                Error error = msg.GetError();
                Debug.LogError("Error: " + error.Message);
            }
        });
    }

    string User2String(User user)
    {
        return $"name={user.DisplayName},ID={user.ID},headImage={user.ImageUrl},presenceStatus={user.PresenceStatus}";
    }

    public void LoginToGameService(string openid, string name)
    {
        this.UserOpenId = openid;
        this.UserName = name;
        CurrentPlayerNameText.text = this.UserName;
    }

    public void RestartLevel()
    {
        // SceneManager.LoadScene("MainScene");
        SceneManager.LoadScene(0);
        Time.timeScale = 1.0f;
        Rigidbody playerRB = playerTank.GetComponent<Rigidbody>();
        playerRB.isKinematic = true;
        playerRB.isKinematic = false;
    }


    private void OnLoginSuccess()
    {
        loadingScreen.SetActive(false);
        GameUIScreen.SetActive(true);
        ResetPlayerScore();
    }

    private void ResetPlayerScore()
    {
        m_CurrentScore = 0;
        TextCurrentScore.text = "Score:" + m_CurrentScore;
    }

    //add score for the current player
    public void AddScore()
    {
        WeakWallCount--;
        if (WeakWallCount == 0)
        {
            //m_GameService.UpdateLevelAchievement("Tank");
        }

        m_CurrentScore += ADD_SCORE;
        TextCurrentScore.text = "Score:" + m_CurrentScore;
        var curScore = m_CurrentScore;
        LeaderboardService.WriteEntry(PicoDemoConfig.LeaderBoardName, m_CurrentScore).OnComplete(msg =>
        {
            if (msg.IsError)
            {
                Debug.LogError($"WriteEntry Error {msg.Error.Code}  {msg.Error.Message}");
                return;
            }

            if (msg.Data)
            {
                Debug.Log($"WriteEntry  Success {curScore}");
            }
            else
            {
                Debug.Log($"WriteEntry  fail {curScore}");
            }
        });
        // m_GameService.UpdateUserScore(m_CurrentScore);
    }

    public int GetCurrentScore()
    {
        return m_CurrentScore;
    }
}