using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BundleInfoAsset", menuName = "BundleLoader/BundleInfoAsset")]
public class BundleInfos : ScriptableObject
{
    public TextAsset textAsset;

    public List<BundleInfo> bundleInfos = new List<BundleInfo>();
}