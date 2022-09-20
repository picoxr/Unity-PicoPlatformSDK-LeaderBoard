using TMPro;
using UnityEngine;

public class SceneLog : MonoBehaviour
{
    private static SceneLog instance;


    public TextMeshProUGUI text;

    public static void AddLog(string info)
    {
        if (null == instance)
        {
            return;
        }

        instance.text.text = $"{info}\n{instance.text.text}";
    }

    private void Awake()
    {
        instance = this;
    }
}