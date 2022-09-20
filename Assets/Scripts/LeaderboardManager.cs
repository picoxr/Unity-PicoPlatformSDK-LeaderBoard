using System.Linq;
using UnityEngine;
using Pico.Platform;
using UnityEngine.UI;

public static class PicoDemoConfig
{
    public const string AppID = "4559425f4f2417f8ee0e929d8e4c3a6b";
    public const string LeaderBoardName = "v5v8";
}

public class LeaderboardManager : MonoBehaviour
{
    private static string LeaderBoardName => PicoDemoConfig.LeaderBoardName;

    private const int MaxCount = 10;

    [SerializeField] private Transform container;

    [SerializeField] private Toggle globalToggle;

    private LeaderboardFilterType _filterType = LeaderboardFilterType.None;

    void AddLog(string log)
    {
        SceneLog.AddLog(log);
        Debug.Log(log);
    }

    private void Start()
    {
        if (globalToggle.isOn == false)
        {
            globalToggle.isOn = true;
        }

        _filterType = LeaderboardFilterType.None;

        for (int i = 0; i < container.childCount; i++)
        {
            var item = container.GetChild(i);
            item.gameObject.SetActive(false);
        }

        AddLog($"CoreService.Initialize({PicoDemoConfig.AppID})");
        CoreService.Initialize(PicoDemoConfig.AppID);
        AddLog($"CoreService.IsInitialized(){CoreService.IsInitialized()}");
        if (!CoreService.IsInitialized())
        {
            return;
        }
        else
        {
            Debug.Log("CoreService.Initialize success!");
        }

        AddLog($"UserService.GetAccessToken()");
        UserService.GetAccessToken().OnComplete(delegate(Message<string> message)
        {
            if (message.IsError)
            {
                var err = message.GetError();
                AddLog($"Got access token error {err.Message} code={err.Code}");
                return;
            }

            string accessToken = message.Data;
            AddLog($"Got accessToken {accessToken}, GameInitialize begin");

            CoreService.GameUninitialize();
            var request = CoreService.GameInitialize(accessToken);
            AddLog($"GameInitialize request.TaskId {request.TaskId}");
            if (request.TaskId != 0)
            {
                request.OnComplete(msg =>
                {
                    if (msg.IsError)
                    {
                        AddLog($"GameInitialize is error ,code:{msg.Error.Code} msg:{msg.Error.Message}");
                        return;
                    }

                    AddLog($"The GameInitializeResult is {msg.Data}");
                });
            }
            else
            {
                Debug.Log($"The requestID of Core.GameInitialize is 0! Repeated initialization or network error");
            }

            StartRefreshLeaderBoard();
        });
    }


    public void StartRefreshLeaderBoard()
    {
        AddLog($"LeaderboardService.Get {LeaderBoardName}");
        LeaderboardService.Get(LeaderBoardName).OnComplete(msg =>
        {
            if (msg.IsError)
            {
                AddLog($"[LeaderboardService.Get] is error ,code: {msg.Error.Code}  msg:{msg.Error.Message}");
                return;
            }

            var lis = msg.Data;
            lis.ForEach(data =>
            {
                AddLog(
                    $"{data.ID} \n {data.ApiName} \n{data.DestinationOptional?.ApiName} \n{data.DestinationOptional?.DisplayName} \n{data.DestinationOptional?.DeeplinkMessage}");
            });
            if (lis.Any(data => data.ApiName == LeaderBoardName))
            {
                InvokeRepeating(nameof(RefreshLeaderBoard), 0, 10);
            }
            else
            {
                AddLog($"[LeaderboardService.Get({LeaderBoardName})], {LeaderBoardName} is not exit");
            }
        });
    }


    public void SetFilterType(int t)
    {
        var type = (LeaderboardFilterType)t;
        SetFilterType(type);
    }

    void SetFilterType(LeaderboardFilterType t)
    {
        _filterType = t;
        CancelInvoke(nameof(RefreshLeaderBoard));
        InvokeRepeating(nameof(RefreshLeaderBoard), 0, 10);
    }

    public void RefreshLeaderBoard()
    {
        AddLog($" LeaderboardService.GetEntries {LeaderBoardName}");
        var cb = LeaderboardService.GetEntries(LeaderBoardName, MaxCount, 0, _filterType,
            LeaderboardStartAt.CenteredOnViewer);
        if (null != cb)
        {
            cb.OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    AddLog($"[LeaderboardService.Get] {msg.Error.Code}  {msg.Error.Message}");
                    return;
                }

                AddLog($"[LeaderboardService.Get] msg.Data.Count {msg.Data.Count}");
                var leaderboardEntryList = msg.Data;
                if (null != leaderboardEntryList)
                {
                    container.DisableChild();
                    for (var i = 0; i < leaderboardEntryList.Count; i++)
                    {
                        var data = leaderboardEntryList[i];

                        if (container.childCount < i)
                        {
                            var first = container.GetChild(0);
                            Instantiate(first, container);
                        }

                        var item = container.GetChild(i);
                        item.gameObject.SetActive(true);
                        item.Find("Position").GetComponent<Text>().text = data.Rank.ToString();
                        item.Find("Player").GetComponent<Text>().text = data.User.DisplayName;
                        item.Find("Score").GetComponent<Text>().text = data.Score.ToString();
                    }
                }
            });
        }
        else
        {
            AddLog(CoreService.UninitializedError);
        }
    }
}

public static class ExFunc
{
    public static void DisableChild(this Transform tran)
    {
        for (int i = 0; i < tran.childCount; i++)
        {
            var item = tran.GetChild(i);
            item.gameObject.SetActive(false);
        }
    }
}