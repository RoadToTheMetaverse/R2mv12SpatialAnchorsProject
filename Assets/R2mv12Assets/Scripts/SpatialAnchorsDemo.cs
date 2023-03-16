using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(SpatialAnchorManager))]
public class SpatialAnchorsDemo : MonoBehaviour
{
    private const string PlayerPrefsKey = "SpatialAnchorsHistory";

    public enum DemoState
    {
        Start,
        PlacingAnchor,
        CreatingSpatialAnchor,
        LocatingAnchor
    }

    public DemoState state = DemoState.Start;
    
    #region Editor Managed Variables

    public GameObject anchorPrefab;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI creatingAnchorStatusText;

    public GameObject startUIView;
    public GameObject placeUIView;
    public GameObject locateUIView;
    public GameObject createAnchorUIView;

    #endregion

    #region Private Variables

    static List<ARRaycastHit> _hits = new List<ARRaycastHit>();
    private SpatialAnchorManager _spatialAnchorManager;
    private ARRaycastManager _raycastManager;

    private GameObject _placedAnchor;
    private List<GameObject> _foundOrCreatedAnchors = new List<GameObject>();
    
    #endregion

    #region GameObject Lifecycle

    // Start is called before the first frame update
    void Start()
    {
        _raycastManager = FindAnyObjectByType<ARRaycastManager>();
        InitializeSpatialAnchorManager();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                Debug.Log("SpatialAnchorsDemo - Over UI!");
        }

        if (Input.touchCount > 0 && state == DemoState.PlacingAnchor)
        {
            Touch touch = Input.GetTouch(0);

            // Check if we tapped on a UI element
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            OnTouchInteraction(touch);
        }
    }

    #endregion


    private void OnTouchInteraction(Touch touch)
    {
        if (touch.phase == TouchPhase.Ended)
        {
            if (IsPointOverAnyObject(touch.position)) return;

            if (_raycastManager.Raycast(touch.position, _hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = _hits[0].pose;
                if (_placedAnchor == null)
                {
                    _placedAnchor = Instantiate(anchorPrefab, hitPose.position, hitPose.rotation);
                }
                else
                {
                    _placedAnchor.transform.position = hitPose.position;
                }
            }
        }
    }

    #region Public API

    public void Restart()
    {
        if (state == DemoState.Start) return;
        
        StopSession();
        
        state = DemoState.Start;

        UpdateDemoState();
    }

    public void PlaceAnchor()
    {
        if (state == DemoState.PlacingAnchor) return;

        state = DemoState.PlacingAnchor;

        UpdateDemoState();
    }

    public void SaveAnchor()
    {
        if (state == DemoState.CreatingSpatialAnchor) return;

        state = DemoState.CreatingSpatialAnchor;

        UpdateDemoState();
    }


    public void LocateAnchor()
    {
        if (state == DemoState.LocatingAnchor) return;

        state = DemoState.LocatingAnchor;

        UpdateDemoState();
    }

    public void ClearHistory()
    {
        var h = new SpatialAnchorHistoryCollection();
        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(h));
        
        Debug.Log($"{nameof(SpatialAnchorsDemo)} - Cleared History.");
    }

    #endregion

    #region Azure Session Management

    private async void StartSession()
    {
        debugText.text = "Attempting to start Sessions";
        await _spatialAnchorManager.StartSessionAsync();
    }

    public async void StartLocateSession()
    {
        debugText.text = "Attempting to start Locate Sessions";
        await _spatialAnchorManager.StartSessionAsync();

        var history = LoadSpatialAnchorHistory();

        Debug.Log($"ASA - Loaded {history.Collection.Count} Anchors from history.");

        if (history.Collection.Count > 0)
        {
            AnchorLocateCriteria criteria = new AnchorLocateCriteria();
            var ids = new string[history.Collection.Count];
            
            for (int i = 0; i < history.Collection.Count; i++)
            {
                ids[i] = history.Collection[i].Id;
                Debug.Log($"ASA - Added ID {ids[i]}");
            }

            criteria.Identifiers = ids;
            _spatialAnchorManager.Session.CreateWatcher(criteria);

            Debug.Log($"ASA - Watcher created!");
        }
    }


    private void StopSession()
    {
        debugText.text += "\nAttempting to stop Sessions";
        _spatialAnchorManager.DestroySession();

        // Remove placed object
        if (_placedAnchor != null)
        {
            Destroy(_placedAnchor.gameObject);
            _placedAnchor = null;
        }
        
            
        // Remove created and found objects
        foreach (var anchor in _foundOrCreatedAnchors)
        {
            Debug.Log($"ASA - Destroying {anchor.name}");
            Destroy(anchor);
        }
    
        _foundOrCreatedAnchors.Clear();
    }

    #endregion


    #region Demo Lifecycle

    private void InitializeSpatialAnchorManager()
    {
        _spatialAnchorManager = GetComponent<SpatialAnchorManager>();
        _spatialAnchorManager.Error += CloudManagerOnError;
        _spatialAnchorManager.LogDebug += CloudManagerLogDebug;
        _spatialAnchorManager.SessionUpdated += CloudManagerOnSessionUpdated;
        _spatialAnchorManager.AnchorLocated += CloudManagerOnAnchorLocated;
        _spatialAnchorManager.LocateAnchorsCompleted += CloudManagerOnLocateAnchorsCompleted;
    }


    private async void UpdateDemoState()
    {
        UpdateUI();

        switch (state)
        {
            case DemoState.Start:
                StopSession();
                break;
            case DemoState.PlacingAnchor:
                StartSession();
                break;
            case DemoState.CreatingSpatialAnchor:
                await CreateSpatialAnchor();
                break;
            case DemoState.LocatingAnchor:
                StartLocateSession();
                break;
        }
    }

    private void UpdateUI()
    {
        startUIView.SetActive(state == DemoState.Start);
        placeUIView.SetActive(state == DemoState.PlacingAnchor);
        createAnchorUIView.SetActive(state == DemoState.CreatingSpatialAnchor);
        locateUIView.SetActive(state == DemoState.LocatingAnchor);
    }

    private async Task CreateSpatialAnchor()
    {
        CloudNativeAnchor cloudNativeAnchor = _placedAnchor.GetComponent<CloudNativeAnchor>();
        await cloudNativeAnchor.NativeToCloud();

        CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;
        cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(30);

        //Collect Environment Data
        while (!_spatialAnchorManager.IsReadyForCreate)
        {
            await Task.Delay(330);
            float createProgress = _spatialAnchorManager.SessionStatus.RecommendedForCreateProgress;

            Debug.Log($"ASA - Move your device to capture more environment data: {createProgress:0%}");
            creatingAnchorStatusText.text =
                $"ASA - Move your device to capture more environment data: {createProgress:0%}";
        }

        Debug.Log($"ASA - Saving cloud anchor... ");
        creatingAnchorStatusText.text = "ASA - Saving cloud anchor... ";

        try
        {
            // Actually save
            await _spatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);
            
            
            // Success?
            if (cloudSpatialAnchor != null)
            {
                // Await override, which may perform additional tasks
                // such as storing the key in the AnchorExchanger
            }
            else
            {
                Debug.LogError("ASA - Failed to save, but no exception was thrown.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("ASA - " + ex.Message);
        }

        Debug.Log($"ASA - Created a cloud anchor with ID={cloudSpatialAnchor.Identifier}");
        creatingAnchorStatusText.text = $"Created a cloud anchor with ID={cloudSpatialAnchor.Identifier}";
        debugText.text += $"\nCreated a cloud anchor with ID={cloudSpatialAnchor.Identifier}";

        SaveSpatialAnchorHistory(CreateSpatialAnchor(cloudSpatialAnchor));
    }

    private void AddFoundCloudSpatialAnchor(CloudSpatialAnchor anchor)
    {
        
        var go = Instantiate(anchorPrefab);
        CloudNativeAnchor cloudNativeAnchor = go.GetComponent<CloudNativeAnchor>();
        cloudNativeAnchor.CloudToNative(anchor);
        
        _foundOrCreatedAnchors.Add(go);
        
        Debug.Log($"{nameof(SpatialAnchorsDemo)} - found a new cloud anchor with ID={anchor.Identifier}. Adding it to the scene.");
    }
    
    #endregion

    #region Spatial Anchor Event Handlers

    private void CloudManagerLogDebug(object sender, OnLogDebugEventArgs args)
    {
        Debug.LogErrorFormat("{0} -- {1}", nameof(SpatialAnchorsDemo), args.Message);
        UnityDispatcher.InvokeOnAppThread(() => debugText.text += "\n" + args.Message);
    }

    private void CloudManagerOnLocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
        Debug.LogFormat("{0} -- {1}", nameof(SpatialAnchorsDemo), "CloudManagerOnLocateAnchorsCompleted");
    }

    private void CloudManagerOnAnchorLocated(object sender, AnchorLocatedEventArgs args)
    {

        switch (args.Status)
        {
            case LocateAnchorStatus.AlreadyTracked:
                Debug.LogFormat("{0} - CloudManagerOnAnchorLocated - {1}", nameof(SpatialAnchorsDemo), "Already Tracked");
                break;
            case LocateAnchorStatus.Located:
                //Creating and adjusting GameObjects have to run on the main thread. We are using the UnityDispatcher to make sure this happens.
                Debug.LogFormat("{0} - CloudManagerOnAnchorLocated - {1}", nameof(SpatialAnchorsDemo), "Located");
                UnityDispatcher.InvokeOnAppThread(() => { AddFoundCloudSpatialAnchor(args.Anchor); });
                break;
            case LocateAnchorStatus.NotLocated:
                Debug.LogFormat("{0} - CloudManagerOnAnchorLocated - {1}", nameof(SpatialAnchorsDemo), "Not Located");
                break;
            case LocateAnchorStatus.NotLocatedAnchorDoesNotExist:
                Debug.LogFormat("{0} - CloudManagerOnAnchorLocated - {1}", nameof(SpatialAnchorsDemo), "Not Located Anchor Does Not Exist");
                break;
        }
    }


    private void CloudManagerOnSessionUpdated(object sender, SessionUpdatedEventArgs args)
    {
        var status = args.Status;
        if (status.UserFeedback == SessionUserFeedback.None) return;
        var message = $"Feedback: {Enum.GetName(typeof(SessionUserFeedback), status.UserFeedback)} -" +
                      $" Recommend Create={status.RecommendedForCreateProgress: 0.#%}";

        Debug.LogFormat("{0} -- {1}", nameof(SpatialAnchorsDemo), message);
        UnityDispatcher.InvokeOnAppThread(() => debugText.text += "\n" + message);

        if (state == DemoState.CreatingSpatialAnchor)
            UnityDispatcher.InvokeOnAppThread(() => creatingAnchorStatusText.text = message);
    }

    private void CloudManagerOnError(object sender, SessionErrorEventArgs args)
    {
        Debug.LogErrorFormat("{0} -- {1}", nameof(SpatialAnchorsDemo), args.ErrorMessage);
        UnityDispatcher.InvokeOnAppThread(() => debugText.text += "\n" + args.ErrorMessage);
    }

    #endregion

    #region Utility Functions

    // Trying to get AR Foundation AR Rays to ignore Touch events over UI Elements
    // Costly, should investigate a better way
    bool IsPointOverAnyObject(Vector2 pos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(pos.x, pos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private SpatialAnchorHistory CreateSpatialAnchor(CloudSpatialAnchor anchor)
    {
        var history = LoadSpatialAnchorHistory();
        
        return new SpatialAnchorHistory(
            "SpatialAnchor" + history.Collection.Count,
            anchor.Identifier
        );
    }
    
    private SpatialAnchorHistoryCollection LoadSpatialAnchorHistory()
    {
        
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            var history = JsonUtility.FromJson<SpatialAnchorHistoryCollection>(PlayerPrefs.GetString(PlayerPrefsKey));
            return history;
        }

        return new SpatialAnchorHistoryCollection();
    }

    private void SaveSpatialAnchorHistory(SpatialAnchorHistory data)
    {
        var history = LoadSpatialAnchorHistory();
        
        history.Collection.Add(data);
        history.Collection.Sort((left, right) => right.CreatedTime.CompareTo(left.CreatedTime));
        
        PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(history));
        
        Debug.Log($"ASA - Saved {history.Collection.Count} items to history.");
    }
    
    #endregion
}

/// <summary>
/// A serializable struct that stores the basic information of a persistent spatial anchor.
/// </summary>
[Serializable]
public struct SpatialAnchorHistory
{
    /// <summary>
    /// An informative name given by the user.
    /// </summary>
    public string Name;

    /// <summary>
    /// The Spatial Anchor Id which is used for resolving.
    /// </summary>
    public string Id;

    /// <summary>
    /// The created time of this Spatial Anchor.
    /// </summary>
    public string SerializedTime;

    /// <summary>
    /// Construct a Spatial Anchor history.
    /// </summary>
    /// <param name="name">An informative name given by the user.</param>
    /// <param name="id">The Spatial Anchor Id which is used for resolving.</param>
    /// <param name="time">The time this spatial Anchor was created.</param>
    public SpatialAnchorHistory(string name, string id, DateTime time)
    {
        Name = name;
        Id = id;
        SerializedTime = time.ToString();
    }

    /// <summary>
    /// Construct a spatial Anchor history.
    /// </summary>
    /// <param name="name">An informative name given by the user.</param>
    /// <param name="id">The spatial Anchor Id which is used for resolving.</param>
    public SpatialAnchorHistory(string name, string id) : this(name, id, DateTime.Now)
    {
    }

    /// <summary>
    /// Gets created time in DataTime format.
    /// </summary>
    public DateTime CreatedTime
    {
        get { return Convert.ToDateTime(SerializedTime); }
    }

    /// <summary>
    /// Overrides ToString() method.
    /// </summary>
    /// <returns>Return the json string of this object.</returns>
    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

/// <summary>
/// A wrapper class for serializing a collection of <see cref="SpatialAnchorHistory"/>.
/// </summary>
[Serializable]
public class SpatialAnchorHistoryCollection
{
    /// <summary>
    /// A list of Spatial Anchor History Data.
    /// </summary>
    public List<SpatialAnchorHistory> Collection = new List<SpatialAnchorHistory>();
}