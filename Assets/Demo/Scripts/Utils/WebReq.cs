using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Policy;
using UnityEngine.Networking;

public class WebReq : MonoBehaviour
{
    public string path;
    public string token;
    // Start is called before the first frame update
    void Start()
    {
        path = "gs://myportfoliounityasset.appspot.com/Win/armchair";
        token = "https://firebasestorage.googleapis.com/v0/b/myportfoliounityasset.appspot.com/o/Win%2Farmchair?alt=media&token=6afa0f25-1691-496f-b1b4-13e0c48c0538";

        StartCoroutine(SendRequest());
    }

    private IEnumerator SendRequest()
    {
        // 요청 데이터 생성
        Dictionary<string, string> requestData = new Dictionary<string, string>
        {
            { "token", token }
        };

        // 요청 객체 생성
        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> entry in requestData)
        {
            form.AddField(entry.Key, entry.Value);
        }

        // 웹 요청 보내기
        using (var request = UnityWebRequest.Post(path, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("WebRequest Error: " + request.error);
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);

                GameObject assetObject = bundle.LoadAsset<GameObject>("Armchair");

                GameObject ins = Instantiate(assetObject);

                bundle.Unload(false);
                // 요청 결과 받기
          
                // 결과를 원하는 방식으로 처리
                // 예시: JSON 파싱
                // YourJsonParsingMethod(responseText);
            }
        }
    }
}
