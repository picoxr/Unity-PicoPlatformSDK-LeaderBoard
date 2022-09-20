using TMPro;
using UnityEditor;

public class SyncBtnNameEditor
{
    [MenuItem("GameObject/同步名称", false, 10)]
    static void ButtonReName_1()
    {
        var obj = Selection.activeGameObject;
        var name = obj.name;
        var text = obj.GetComponentInChildren<TextMeshProUGUI>();
        text.text = name;
        EditorUtility.SetDirty(text);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("GameObject/同步名称", true)]
    static bool ButtonReName_VF()
    {
        var obj = Selection.activeGameObject;
        var text = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (null != obj && null != text)
        {
            return true;
        }

        return false;
    }
}