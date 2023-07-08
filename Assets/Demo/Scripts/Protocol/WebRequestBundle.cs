using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class WebRequestBundle : MonoBehaviour
{
    FirebaseStorage storage;
    StorageReference storageRef;
    StorageReference gsReference;

    private void Start()
    {
        storage = FirebaseStorage.DefaultInstance;

        storageRef = storage.GetReferenceFromUrl(PROTOCOL.FIRESTORAGEURL);

        gsReference =
            storage.GetReferenceFromUrl("gs://myportfoliounityasset.appspot.com/Win/armchair");//Path.Combine(PROTOCOL.FIRESTORAGEURL,"Win\\armchair"));

        gsReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Download URL: " + task.Result);
            }
            else if(task.IsCompletedSuccessfully) { Debug.Log("sucsses"); }
        });
    }
    
}
