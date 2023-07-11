using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCore.JSON;

public class PROTOCOL
{
    //gs://myportfoliounityasset.appspot.com/Web/armchair
    public static string FIRESTORAGEURL = "gs://myportfoliounityasset.appspot.com";
}
[Serializable]
public class BundleInfo
{
    public string id;
    public string categoryId;
    public string filename;
    public string thumbnail;
    public string bundleKey;
    public ulong upAt;

    
    public int type;

    public List<string> assetKeys;

    public bool isSingleAsset => assetKeys.Count == 1;

    public BundleInfo() { }

    public BundleInfo(JSONObject json)
    {
        id = json.GetString(nameof(id));
        categoryId = json.GetString(nameof(categoryId));
        filename = json.GetString(nameof(filename));
        thumbnail = json.GetString(nameof(thumbnail));
        type = json.GetInt(nameof(type));
        bundleKey = json.GetString(nameof(bundleKey));

        assetKeys = new List<string>();
        foreach (var jArr in json.GetArray(nameof(assetKeys)))
        {
            assetKeys.Add(jArr.Str);
        }
    }

    public JSONObject ToJson()
    {
        return JSONObject.Parse(JsonUtility.ToJson(this));
    }

    public string GetAssetKey(int index = 0)
    {
        if (assetKeys == null || assetKeys.Count <= index)
            return string.Empty;

        return assetKeys[index];
    }
}