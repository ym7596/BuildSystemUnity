using dduR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadManager : MonoBehaviour
{

    private LoadType _loadType = LoadType.Standalone;

    private string _loadPath;
    private string _assetName;
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        _loadType = LoadType.Standalone;
#elif UNITY_WEBGL
    _loadType = LoadType.Web;
#endif
        LoadAsset();
    }

    public void LoadAsset()
    {
        GameObject test = Resources.Load<GameObject>("Armchair");
        Instantiate(test);
    }
}
