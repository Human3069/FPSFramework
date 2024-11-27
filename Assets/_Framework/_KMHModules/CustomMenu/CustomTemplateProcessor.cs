using UnityEngine;

#if UNITY_EDITOR
public class CustomTemplateProcessor : UnityEditor.AssetModificationProcessor
{
    // call on create new asset
    private static void OnWillCreateAsset(string assetName)
    {
        assetName = assetName.Replace(".meta", "");

        if (System.IO.Path.GetExtension(assetName) != ".cs")
        {
            return;
        }

        int index = Application.dataPath.LastIndexOf("Assets");

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(Application.dataPath.Substring(0, index)).Append(assetName);

        string path = sb.ToString();

        if (System.IO.File.Exists(path) == false)
        {
            return;
        }

        string fileContent = System.IO.File.ReadAllText(path);
        fileContent = fileContent.Replace("#DATE#", System.DateTime.Now.ToString("yyyy.MM.dd tt hh:mm", System.Globalization.CultureInfo.CreateSpecificCulture("ko-KR")));

        System.IO.File.WriteAllText(path, fileContent);
        UnityEditor.AssetDatabase.Refresh();
    }
}
#endif