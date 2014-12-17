#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
 
 
public static class ScriptableObjectUtility
{
    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static void CreateAsset<T> () where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T> ();
 
        string path = AssetDatabase.GetAssetPath (Selection.activeObject);
        if (path == "") 
        {
            path = "Assets";
        } 
        else if (Path.GetExtension (path) != "") 
        {
            path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
        }
 
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/" + typeof(T).ToString() + ".asset");
 
        AssetDatabase.CreateAsset (asset, assetPathAndName);
 
        AssetDatabase.SaveAssets ();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow ();
        Selection.activeObject = asset;
    }
}

public class EditorCommand
{
    [MenuItem("Assets/DeferredShading/Create")]
    public static void CreateAsset ()
    {
        //ScriptableObjectUtility.CreateAsset<Hoge>();
    }
}
#endif
