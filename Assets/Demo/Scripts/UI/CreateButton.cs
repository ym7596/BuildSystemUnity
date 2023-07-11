using dduR;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CreateButton : MonoBehaviour
{
    public string id = "0";
    public Image imgbg;
    public UICanvas uiCanvas;

    private BundleInfos _bundleInfos;

    // Start is called before the first frame update
    private void Start()
    {
        if(imgbg == null)
            imgbg = GetComponent<Image>();
/*
        _bundleInfos = uiCanvas.bundleInfos;
        BundleInfo bundle = _bundleInfos.bundleInfos.Find( x => x.id == id);
        var ThumObj = Resources.Load<GameObject>(bundle.filename);

        yield return new WaitUntil(() => ThumObj == true);

        Texture2D thumb = GetThumnail(ThumObj);*/

       // yield return new WaitUntil(() => thumb != null);

      //  SetBG(thumb);

    }



    public void OnButtonClick()
    {
        _bundleInfos = uiCanvas.bundleInfos;
        BundleInfo bundle = _bundleInfos.bundleInfos.Find(x => x.id == id);
      StartCoroutine(RoomManager.instance.RequestCreateObject(bundle));
      
    }
}
