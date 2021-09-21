
//1、云锚点使用
//---------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
#if WINDOWS_UWP
using Windows.Storage;
#endif
public class AzureSpatialAnchorController : MonoBehaviour
{
    [HideInInspector]
    public string currentAzureAnchorID = "";  //云锚点ID
    private SpatialAnchorManager cloudManager;
    private CloudSpatialAnchor currentCloudAnchor;
    private AnchorLocateCriteria anchorLocateCriteria;
    private CloudSpatialAnchorWatcher currentWatcher;

    private readonly Queue<Action> dispatchQueue = new Queue<Action>();
    void Start()
    {
        cloudManager = GetComponent<SpatialAnchorManager>();
        cloudManager.AnchorLocated += CloudManager_AnchorLocated;
        anchorLocateCriteria = new AnchorLocateCriteria();
    }

    void Update()
    {
        lock (dispatchQueue)
        {
            if (dispatchQueue.Count > 0)
            {
                dispatchQueue.Dequeue()();
            }
        }
    }
    void OnDestroy()
    {
        if (cloudManager != null && cloudManager.Session != null)
        {
            cloudManager.DestroySession();
        }

        if (currentWatcher != null)
        {
            currentWatcher.Stop();
            currentWatcher = null;
        }
    }
    #region Azure云锚点处理方法
    public async void StartAzureSession()
    {
        if (cloudManager.Session == null)
        {
            await cloudManager.CreateSessionAsync();
        }
        await cloudManager.StartSessionAsync();
        Debug.Log("开启云锚Session成功！");
    }

    public async void StopAzureSession()
    {
        cloudManager.StopSession();
        await cloudManager.ResetSessionAsync();
        Debug.Log("关闭云锚Session成功！");
    }

    public async void CreateAzureAnchor(GameObject theObject)
    {
        theObject.CreateNativeAnchor();
        CloudSpatialAnchor localCloudAnchor = new CloudSpatialAnchor();
        localCloudAnchor.LocalAnchor = theObject.FindNativeAnchor().GetPointer();
        if (localCloudAnchor.LocalAnchor == IntPtr.Zero)
        {
            Debug.Log("无法创建本地锚点");
            return;
        }
        localCloudAnchor.Expiration = DateTimeOffset.Now.AddDays(7);
        while (!cloudManager.IsReadyForCreate)
        {
            await Task.Delay(330);
            float createProgress = cloudManager.SessionStatus.RecommendedForCreateProgress;
            QueueOnUpdate(new Action(() => Debug.Log($"请缓慢移动以采集更多环境信息，当前进度： {createProgress:0%}")));
        }

        bool success;
        try
        {
            await cloudManager.CreateAnchorAsync(localCloudAnchor);
            currentCloudAnchor = localCloudAnchor;
            localCloudAnchor = null;
            success = currentCloudAnchor != null;

            if (success)
            {
                Debug.Log($"云锚点ID： '{currentCloudAnchor.Identifier}' 创建成功");
                currentAzureAnchorID = currentCloudAnchor.Identifier;
            }
            else
            {
                Debug.Log($"创建云锚点ID： '{currentAzureAnchorID}' 失败");
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    public void RemoveLocalAnchor(GameObject theObject)
    {
        theObject.DeleteNativeAnchor();
        if (theObject.FindNativeAnchor() == null)
        {
            Debug.Log("本地锚点移除成功");
        }
        else
        {
            Debug.Log("本地锚点移除失败");
        }
    }

    public void FindAzureAnchor(string id = "")
    {
        if (id != "")
        {
            currentAzureAnchorID = id;
        }
        List<string> anchorsToFind = new List<string>();
        if (currentAzureAnchorID != "")
        {
            anchorsToFind.Add(currentAzureAnchorID);
        }
        else
        {
            Debug.Log("无需要查找的云锚点ID");
            return;
        }

        anchorLocateCriteria.Identifiers = anchorsToFind.ToArray();
        if ((cloudManager != null) && (cloudManager.Session != null))
        {
            currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);
            Debug.Log("开始查找云锚点");
        }
        else
        {
            currentWatcher = null;
        }
    }

    public async void DeleteAzureAnchor()
    {
        await cloudManager.DeleteAnchorAsync(currentCloudAnchor);
        currentCloudAnchor = null;
        Debug.Log("云锚点移除成功");
    }

    public void SaveAzureAnchorIdToDisk()
    {
        string filename = "SavedAzureAnchorID.txt";
        string path = Application.persistentDataPath;
#if WINDOWS_UWP
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        path = storageFolder.Path.Replace('\\', '/') + "/";
#endif
        string filePath = Path.Combine(path, filename);
        File.WriteAllText(filePath, currentAzureAnchorID);
        Debug.Log($"保存文件成功！");
    }

    public void GetAzureAnchorIdFromDisk()
    {
        string filename = "SavedAzureAnchorID.txt";
        string path = Application.persistentDataPath;
#if WINDOWS_UWP
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        path = storageFolder.Path.Replace('\\', '/') + "/";
#endif
        string filePath = Path.Combine(path, filename);
        currentAzureAnchorID = File.ReadAllText(filePath);
    }

    public void ShareAzureAnchorIdToNetwork()
    {
        //通过网络传输云锚点
    }

    public void GetAzureAnchorIdFromNetwork()
    {
        //通过网络接收云锚点
    }
    #endregion

    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        if (args.Status == LocateAnchorStatus.Located || args.Status == LocateAnchorStatus.AlreadyTracked)
        {
            currentCloudAnchor = args.Anchor;

            QueueOnUpdate(() =>
            {
                Debug.Log($"云锚点定位成功");
                gameObject.CreateNativeAnchor();
                if (currentCloudAnchor != null)
                {
                    //利用锚点信息变换游戏对象位姿
                    gameObject.GetComponent<UnityEngine.XR.WSA.WorldAnchor>().SetNativeSpatialAnchorPtr(currentCloudAnchor.LocalAnchor);
                }
            });
        }
        else
        {
            QueueOnUpdate(new Action(() => Debug.Log($"锚点ID： '{args.Identifier}' 定位失败, 定位状态为： '{args.Status}'")));
        }
    }
    private void QueueOnUpdate(Action updateAction)
    {
        lock (dispatchQueue)
        {
            dispatchQueue.Enqueue(updateAction);
        }
    }
}


