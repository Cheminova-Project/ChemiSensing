using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Animations;
using System;
using DG.Tweening;
using Unity.Netcode;

public class Ruler : ToolComponent
{
    [Header("Common Settings")]
    public GameObject distanceLabelPrefab;
    public Material lineMaterial;
    
    [Header("Line Settings")]
    [SerializeField] private float baseLineWidth = 0.005f;

    [Header("Highlight Ray Settings")]
    [SerializeField] protected float minAnimationDuration = 1f;
    [SerializeField] protected float maxAnimationDuration = 5f;
    [SerializeField] protected float timePerExtraSegment = 0.5f;
    [SerializeField] protected float idleTime = 2f;
    [SerializeField] protected float pointSpacing = 0.01f;

    [Header("Dynamic Label Scaling")]
    [SerializeField] private float minDistanceThreshold = 1f;
    [SerializeField] private float maxDistanceThreshold = 10f;
    [SerializeField] private float minSize = 0.005f;
    [SerializeField] private float maxSize = 0.025f;
    [SerializeField] private float labelScalingUpdateInterval = 0.1f;
    
    [Header("Network Sync")]
    protected NetworkedRulerSync rulerSync;
    
    protected Transform measuredObject;
    protected List<Vector3> localPoints = new();
    protected List<GameObject> labels = new();
    protected List<Vector2Int> savedSegments = new();
    protected List<Transform> pointMarkers = new();

    private string rulerTag = "RulerPoints";
    private GameObject _rulerPointsContainer;
    
    protected LineRenderer highlightLineRenderer;
    protected LineRenderer sharedSegmentLineRenderer;
    
    private Coroutine highlightCoroutine;
    private Coroutine sharedSegmentCoroutine;

    public static event Action OnPointAdded;
    public static event Action OnPointsReset;
    public static event Action OnSavedSegmentsChanged;
    public static event Action<int, int> OnSharedSegmentDeleted;

    private Camera playerCamera;
    private float labelScalingTimer = 0f;
    private float initialModelSize = 1f;

    protected override void OnDisable()
    {
        ShowMarkers(false);
        SaveSegmentsToSessionPrefs();
        StopAllTracingAnimations();
        ClearVisuals();
        base.OnDisable();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetupLineRenderer();
        
        if (measuredObject == null && InspectedObjectController.Instance != null)
        {
            var obj = InspectedObjectController.Instance.GetInspectedObject();
            if (obj != null)
                SetMeasuredObject(obj.transform);
        }
        
        if (measuredObject == null)
        {
            Debug.LogWarning("[Ruler] No se encontró el modelo 3D. Los puntos no se dibujarán hasta que exista.");
            return;
        }
        
        playerCamera = Camera.main;
        labelScalingTimer = 0f;
        
        Transform existingContainer = measuredObject.Find("RulerPoints");
        if (existingContainer != null)
        {
            _rulerPointsContainer = existingContainer.gameObject;
        }
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            GameObject userGameobject = LocalRegistry.Instance.GetPlayerGameObject(NetworkManager.Singleton.LocalClientId);
            if (userGameobject != null)
            {
                rulerSync = userGameobject.GetComponentInChildren<NetworkedRulerSync>();
                if (rulerSync != null)
                {
                    rulerSync.rulerComponent = this;
                    SyncFromNetwork(rulerSync.syncedData.Value);
                }
            }
        }
        else
        {
            if (_rulerPointsContainer != null)
            {
                pointMarkers.Clear();
                for (int i = 0; i < _rulerPointsContainer.transform.childCount; i++) 
                {
                    pointMarkers.Add(_rulerPointsContainer.transform.GetChild(i));
                }

                SyncLocalPointsFromMarkers();
                RecreatePointVisuals();
                ShowMarkers(true);
                LoadSegmentsFromSessionPrefs();
            }
        }
    }
    
    protected void StopAllTracingAnimations()
    {
        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }
        if (sharedSegmentCoroutine != null)
        {
            StopCoroutine(sharedSegmentCoroutine);
            sharedSegmentCoroutine = null;
        }

        if (highlightLineRenderer != null) highlightLineRenderer.positionCount = 0;
        if (sharedSegmentLineRenderer != null) sharedSegmentLineRenderer.positionCount = 0;
    }
    
    public void SyncFromNetwork(SyncedRulerData data)
    {
        if (data.Points == null || data.Points.Length == 0)
        {
            ResetVisualsLocalOnly();
            OnPointsReset?.Invoke();
            return;
        }
            

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(data.MeasuredObjectId, out NetworkObject netObj))
            SetMeasuredObject(netObj.transform);
        else if (measuredObject == null) 
            return;

        ResetVisualsLocalOnly();

        foreach (var pt in data.Points)
        {
            var marker = CreatePointMarker(pt);
            ShowLabel(Vector3.zero, pointMarkers.Count.ToString(), marker);
        }
        
        SyncLocalPointsFromMarkers();
        OnPointAdded?.Invoke();
    }

    protected virtual void Update()
    {
        labelScalingTimer -= Time.deltaTime;
        if (labelScalingTimer <= 0f)
        {
            UpdateLabelScaling();
            labelScalingTimer = labelScalingUpdateInterval;
        }
    }

    public void SetMeasuredObject(Transform obj)
    {
        measuredObject = obj;
        CalculateInitialModelSize();
    }
    
    private void CalculateInitialModelSize()
    {
        if (measuredObject == null) return;
        
        Renderer[] renderers = measuredObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float currentPhysicalSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        
        Vector3 parentLossyScale = measuredObject.lossyScale;
        float parentScaleFactor = Mathf.Max((parentLossyScale.x + parentLossyScale.y + parentLossyScale.z) / 3f, 0.001f);

        initialModelSize = currentPhysicalSize / parentScaleFactor;
    }

    protected void SetupLineRenderer()
    {
        if (highlightLineRenderer == null)
        {
            GameObject highlightObj = new GameObject("RulerHighlightRay");
            highlightObj.transform.SetParent(this.transform); 
            highlightLineRenderer = highlightObj.AddComponent<LineRenderer>();
            highlightLineRenderer.positionCount = 0;
            highlightLineRenderer.material = lineMaterial;
            highlightLineRenderer.widthMultiplier = 0.005f;
            highlightLineRenderer.startColor = highlightLineRenderer.endColor = new Color(0f, 0f, 0f, 0.5f);
            highlightLineRenderer.useWorldSpace = true;
        }

        if (sharedSegmentLineRenderer == null)
        {
            GameObject sharedObj = new GameObject("RulerSharedSegmentRay");
            sharedObj.transform.SetParent(this.transform);
            sharedSegmentLineRenderer = sharedObj.AddComponent<LineRenderer>();
            sharedSegmentLineRenderer.positionCount = 0;
            sharedSegmentLineRenderer.material = lineMaterial;
            sharedSegmentLineRenderer.widthMultiplier = 0.005f;
            sharedSegmentLineRenderer.startColor = sharedSegmentLineRenderer.endColor = new Color(0f, 0f, 0f, 0.5f);
            sharedSegmentLineRenderer.useWorldSpace = true;
        }
    }

    public void RequestAddPoint(Vector3 worldPoint)
    {
        if (measuredObject == null) return;
        
        Vector3 localPoint = measuredObject.InverseTransformPoint(worldPoint);
        NetworkObject netObj = measuredObject.GetComponentInParent<NetworkObject>();

        if (netObj != null && rulerSync != null && rulerSync.IsSpawned && NetworkManager.Singleton.IsConnectedClient)
        {
            rulerSync.RequestAddPoint(localPoint, netObj.NetworkObjectId);
        }
        else
        {
            AddPointNetworked(localPoint);
        }
    }

    public void AddPointNetworked(Vector3 localPoint)
    {
        var marker = CreatePointMarker(localPoint);
        ShowLabel(Vector3.zero, pointMarkers.Count.ToString(), marker);
        SyncLocalPointsFromMarkers();
        OnPointAdded?.Invoke();
    }

    protected void AddPoint(Vector3 worldPoint) => RequestAddPoint(worldPoint);

    private Transform GetOrCreateMarkerContainer()
    {
        if (measuredObject == null)
            return null;
        
        Transform existing = measuredObject.Find("RulerPoints");
        if (existing != null)
        {
            _rulerPointsContainer = existing.gameObject;
            return _rulerPointsContainer.transform;
        }
        
        _rulerPointsContainer = new GameObject("RulerPoints");
        _rulerPointsContainer.tag = rulerTag;
        _rulerPointsContainer.transform.SetParent(measuredObject, false);
        _rulerPointsContainer.transform.localPosition = Vector3.zero;
        _rulerPointsContainer.transform.localRotation = Quaternion.identity;
        
        return _rulerPointsContainer.transform;
    }

    private Transform CreatePointMarker(Vector3 localPosition)
    {
        var container = GetOrCreateMarkerContainer();
        if (container == null) return null;
        var marker = new GameObject($"RulerPoint_{pointMarkers.Count}");
        marker.transform.SetParent(container, false);
        marker.transform.localPosition = localPosition;
        marker.transform.localRotation = Quaternion.identity;
        pointMarkers.Add(marker.transform);
        return marker.transform;
    }

    private void SyncLocalPointsFromMarkers()
    {
        localPoints.Clear();
        for (int i = 0; i < pointMarkers.Count; i++)
        {
            localPoints.Add(pointMarkers[i].localPosition);
        }
    }

    public void RemovePointAtLocalOnly(int index)
    {
        if (index < 0 || index >= pointMarkers.Count)
            return;
        
        StopAllTracingAnimations();
        
        var marker = pointMarkers[index];
        if (marker != null)
        {
            DOTween.Kill(marker.GetInstanceID());
            Destroy(marker.gameObject);
        }
        
        pointMarkers.RemoveAt(index);

        if(_rulerPointsContainer != null)
        {
            var container = _rulerPointsContainer.transform;
            for (int i = 0; i < container.childCount; i++)
                container.GetChild(i).name = $"RulerPoint_{i}";
        }

        for (int i = savedSegments.Count - 1; i >= 0; i--)
        {
            var s = savedSegments[i];
            if (s.x == index || s.y == index) savedSegments.RemoveAt(i);
            else
            {
                int sx = s.x > index ? s.x - 1 : s.x;
                int sy = s.y > index ? s.y - 1 : s.y;
                savedSegments[i] = new Vector2Int(sx, sy);
            }
        }

        SyncLocalPointsFromMarkers();
        RecreatePointVisuals();
        OnPointsReset?.Invoke();
        OnSavedSegmentsChanged?.Invoke();
    }

    public void AddSavedSegment(int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex <= startIndex || startIndex >= localPoints.Count || endIndex > localPoints.Count) return;
        var v = new Vector2Int(startIndex, endIndex);
        if (!savedSegments.Contains(v))
        {
            savedSegments.Add(v);
            OnSavedSegmentsChanged?.Invoke();
        }
    }

    public void RemoveSavedSegment(int startIndex, int endIndex)
    {
        if (savedSegments.Remove(new Vector2Int(startIndex, endIndex))) OnSavedSegmentsChanged?.Invoke();
    }

    public List<Vector2Int> GetSavedSegments() => new List<Vector2Int>(savedSegments);
    private string GetSegmentsSessionPrefsKey() => $"ruler_segments";

    private void SaveSegmentsToSessionPrefs()
    {
        if (savedSegments.Count == 0)
        {
            SessionPrefs.Instance.DeleteKey(GetSegmentsSessionPrefsKey());
            return;
        }
        var segmentStrings = new List<string>();
        foreach (var seg in savedSegments) segmentStrings.Add($"{seg.x}:{seg.y}");
        SessionPrefs.Instance.SetString(GetSegmentsSessionPrefsKey(), string.Join("|", segmentStrings));
        PlayerPrefs.Save();
    }

    private void LoadSegmentsFromSessionPrefs()
    {
        string encoded = SessionPrefs.Instance.GetString(GetSegmentsSessionPrefsKey(), "");
        if (string.IsNullOrEmpty(encoded)) return;
        savedSegments.Clear();
        foreach (var part in encoded.Split('|'))
        {
            if (string.IsNullOrWhiteSpace(part)) continue;
            var indices = part.Split(':');
            if (indices.Length != 2) continue;
            if (int.TryParse(indices[0], out int startIdx) && int.TryParse(indices[1], out int endIdx))
                if (startIdx >= 0 && endIdx <= localPoints.Count && startIdx < endIdx)
                    savedSegments.Add(new Vector2Int(startIdx, endIdx));
        }
        if (savedSegments.Count > 0) OnSavedSegmentsChanged?.Invoke();
    }

    private void ShowMarkers(bool show = true)
    {
        if (pointMarkers != null) foreach (var t in pointMarkers) if (t != null) t.gameObject.SetActive(show);
        if (labels != null && show) foreach (var l in labels) if (l != null) l.SetActive(show);
    }

    private void ClearVisuals()
    {
        StopAllTracingAnimations();
        foreach (var label in labels)
        {
            if (label != null)
            {
                DOTween.Kill(label.GetInstanceID());
                Destroy(label);
            }
        }
        labels.Clear();
    }

    private void RecreatePointVisuals()
    {
        foreach (var l in labels) Destroy(l);
        labels.Clear();
        if (pointMarkers != null)
        {
            for (int i = 0; i < pointMarkers.Count; i++)
                if (pointMarkers[i] != null) ShowLabel(Vector3.zero, (i+1).ToString(), pointMarkers[i]);
        }
    }
    
    public void HighlightSegmentNetworked(int startPoint, int endPoint, bool directConnection = false)
    {
        if (rulerSync != null && rulerSync.IsSpawned && NetworkManager.Singleton.IsConnectedClient)
        {
            rulerSync.RequestTriggerHighlight(startPoint, endPoint, directConnection);
        }
        else
        {
            HighlightSegmentLocalOnly(startPoint, endPoint, directConnection);
        }
    }

    public void HighlightSegmentLocalOnly(int startPoint, int endPoint, bool directConnection)
    {
        if (startPoint < 0 || endPoint > localPoints.Count || startPoint >= endPoint || measuredObject == null) return;
        StopAllTracingAnimations();
        highlightCoroutine = StartCoroutine(AnimateSegmentTracing(startPoint, endPoint, directConnection, highlightLineRenderer, true));
    }
    
    public void DeleteSegmentNetworked(int start, int end)
    {
        if (rulerSync != null && rulerSync.IsSpawned && NetworkManager.Singleton.IsConnectedClient)
            rulerSync.RequestDeleteSegment(start, end);
        else
            DeleteSegmentLocalOnly(start, end);
    }

    public void DeleteSegmentLocalOnly(int start, int end)
    {
        RemoveSavedSegment(start, end);
        OnSegmentRemoved(start, end);
        
        if (rulerSync != null && rulerSync.sharedSegment.Value.IsSharing && 
            rulerSync.sharedSegment.Value.StartPoint == start && rulerSync.sharedSegment.Value.EndPoint == end)
        {
            if (rulerSync.IsOwner)
                rulerSync.RequestShareSegment(0, 0, false, false, 0f);
        }

        OnSharedSegmentDeleted?.Invoke(start, end);
    }
    
    public virtual void HighlightSegment(int startPoint, int endPoint, bool directConnection = false)
    {
        if (startPoint < 0 || endPoint > localPoints.Count || startPoint >= endPoint || measuredObject == null) return;
        StopAllTracingAnimations();
        highlightCoroutine = StartCoroutine(AnimateSegmentTracing(startPoint, endPoint, directConnection, highlightLineRenderer, true));
    }

    private IEnumerator AnimateSegmentTracing(int startPoint, int endPoint, bool directConnection, LineRenderer targetLineRenderer, bool autoHide)
    {
        int startIndex = startPoint - 1;
        int endIndex = endPoint - 1;
        List<Vector3> segmentPoints = GetInterpolatedSegmentPoints(startIndex, endIndex, directConnection);
        
        float currentDuration = minAnimationDuration;
        if (!directConnection)
        {
            int numberOfSegments = endIndex - startIndex;
            currentDuration = Mathf.Clamp(minAnimationDuration + (numberOfSegments - 1) * timePerExtraSegment, minAnimationDuration, maxAnimationDuration);
        }
        
        targetLineRenderer.positionCount = 0;
        float elapsedTime = 0f;

        while (elapsedTime < currentDuration)
        {
            elapsedTime += Time.deltaTime;
        
            float progress = Mathf.Clamp01(elapsedTime / currentDuration);
            int targetPointCount = Mathf.Clamp(Mathf.CeilToInt(progress * segmentPoints.Count), 0, segmentPoints.Count);

            if (targetLineRenderer.positionCount < targetPointCount)
            {
                for (int i = targetLineRenderer.positionCount; i < targetPointCount; i++)
                {
                    targetLineRenderer.positionCount = i + 1;
                    targetLineRenderer.SetPosition(i, segmentPoints[i]);
                }
            }

            yield return null;
        }
        
        if (segmentPoints.Count > 0)
        {
            targetLineRenderer.positionCount = segmentPoints.Count;
            targetLineRenderer.SetPositions(segmentPoints.ToArray());
        }

        if (autoHide)
        {
            yield return new WaitForSeconds(idleTime);
            if (targetLineRenderer != null)
                targetLineRenderer.positionCount = 0;
            if (targetLineRenderer == highlightLineRenderer)
                highlightCoroutine = null;
            if (targetLineRenderer == sharedSegmentLineRenderer)
                sharedSegmentCoroutine = null;
        }
    }

    public void SyncSharedSegment(int startPoint, int endPoint, bool directConnection, bool isSharing)
    {
        float distance = GetDistanceBetweenPoints(startPoint, endPoint, directConnection);

        if (rulerSync != null && rulerSync.IsSpawned && NetworkManager.Singleton.IsConnectedClient)
            rulerSync.RequestShareSegment(startPoint, endPoint, directConnection, isSharing, distance);
        else
            ApplySharedSegmentFromNetwork(new SharedSegmentData { StartPoint = startPoint, EndPoint = endPoint, DirectConnection = directConnection, IsSharing = isSharing, Distance = distance });
    }

    public virtual void ApplySharedSegmentFromNetwork(SharedSegmentData data)
    {
        StopAllTracingAnimations();

        if (!data.IsSharing || measuredObject == null || data.StartPoint <= 0 || data.EndPoint > localPoints.Count)
        {
            return;
        }

        sharedSegmentCoroutine = StartCoroutine(AnimateSegmentTracing(data.StartPoint, data.EndPoint, data.DirectConnection, sharedSegmentLineRenderer, true));
    }

    private List<Vector3> GetInterpolatedSegmentPoints(int startIndex, int endIndex, bool directConnection)
    {
        List<Vector3> interpolatedPoints = new List<Vector3>();
        if (directConnection)
        {
            Vector3 startPos = measuredObject.TransformPoint(localPoints[startIndex]);
            Vector3 endPos = measuredObject.TransformPoint(localPoints[endIndex]);
            float dist = Vector3.Distance(startPos, endPos);
            int intermediatePoints = Mathf.Max(1, Mathf.CeilToInt(dist / pointSpacing));
            for (int j = 0; j <= intermediatePoints; j++) interpolatedPoints.Add(Vector3.Lerp(startPos, endPos, (float)j / intermediatePoints));
        }
        else
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                interpolatedPoints.Add(measuredObject.TransformPoint(localPoints[i]));
                if (i < endIndex)
                {
                    Vector3 currentWorldPoint = measuredObject.TransformPoint(localPoints[i]);
                    Vector3 nextWorldPoint = measuredObject.TransformPoint(localPoints[i + 1]);
                
                    float currentSegmentDist = Vector3.Distance(currentWorldPoint, nextWorldPoint);
                    int intermediatePoints = Mathf.Max(1, Mathf.CeilToInt(currentSegmentDist / pointSpacing));
                
                    for (int j = 1; j < intermediatePoints; j++) 
                        interpolatedPoints.Add(Vector3.Lerp(currentWorldPoint, nextWorldPoint, (float)j / intermediatePoints));
                }
            }
        }
        return interpolatedPoints;
    }

    public float GetDistanceBetweenPoints(int startIndex, int endIndex, bool directConnection = false)
    {
        if (startIndex < 0 || endIndex > localPoints.Count || startIndex >= endIndex) return 0f;
        int adjustedEndIndex = endIndex - 1, adjustedStartIndex = startIndex - 1;

        if (directConnection)
        {
            return Vector3.Distance(
                localPoints[adjustedStartIndex], 
                localPoints[adjustedEndIndex]);
        }

        float totalDistance = 0f;
        for (int i = adjustedStartIndex; i < adjustedEndIndex; i++) 
        {
            Vector3 p1 = localPoints[i];
            Vector3 p2 = localPoints[i + 1];
            totalDistance += Vector3.Distance(p1, p2);
        }
        return totalDistance;
    }

    public int GetPointCount() => localPoints.Count;
    public Vector3 GetWorldPointAt(int index) => (index < 0 || index >= localPoints.Count || measuredObject == null) ? Vector3.zero : measuredObject.TransformPoint(localPoints[index]);

    protected void ShowLabel(Vector3 localPosition, string text, Transform hitTransform = null)
    {
        if (!distanceLabelPrefab)
            return;
        
        GameObject label = Instantiate(distanceLabelPrefab, hitTransform != null && hitTransform != measuredObject ? hitTransform : measuredObject);
        label.transform.localPosition = localPosition;
        label.transform.localRotation = Quaternion.identity;

        TextMeshPro textMesh = label.GetComponentInChildren<TextMeshPro>();
        if (textMesh != null)
        {
            if (PlatformController.Instance.GetPlayerCharacterType().Equals(PlayerCharacterType.VR)) textMesh.fontSize *= 2f;
            textMesh.text = text;
        }
        labels.Add(label);
        
        if (playerCamera != null)
        {
            float initialScale = GetTargetLabelScale(label.transform.position);
            label.transform.localScale = Vector3.one * initialScale;
            if (hitTransform != null && hitTransform != measuredObject)
                hitTransform.localScale = Vector3.one * initialScale;
        }

        LookAtConstraint lookAtConstraint = label.GetComponentInChildren<LookAtConstraint>();
        if (lookAtConstraint != null && Camera.main != null)
        {
            lookAtConstraint.AddSource(new ConstraintSource { sourceTransform = Camera.main.transform, weight = 1f });
            lookAtConstraint.rotationOffset = new Vector3(0, 180, 0);
            lookAtConstraint.constraintActive = true;
        }
    }

    public virtual void ResetRuler()
    {
        if (rulerSync != null && rulerSync.IsSpawned && NetworkManager.Singleton.IsConnectedClient)
            rulerSync.RequestResetRuler();
        else
        {
            ResetVisualsLocalOnly();
            OnPointsReset?.Invoke();
        }
    }
    
    private void ResetVisualsLocalOnly()
    {
        StopAllTracingAnimations();
        
        localPoints.Clear();
        savedSegments.Clear();
        pointMarkers.Clear();
        ClearVisuals();
        
        if (_rulerPointsContainer != null)
        {
            var container = _rulerPointsContainer.transform;
            var children = new Transform[container.childCount];
            for (int i = 0; i < container.childCount; i++) children[i] = container.GetChild(i);
            foreach (var child in children)
            {
                DOTween.Kill(child.GetInstanceID());
                Destroy(child.gameObject);
            }
        }
    }
    
    public void ResetRulerNetworked()
    {
        ResetVisualsLocalOnly();
        OnPointsReset?.Invoke();
        
        if (ToolMessageHandler.Instance != null && NetworkManager.Singleton.LocalClientId != NetworkManager.ServerClientId)
        {
            ToolMessageHandler.Instance.ShowMessage("All ruler measurements have been cleared.", 3.5f, MessageType.Info);
        }
    }

    public virtual void OnSegmentRemoved(int startIndex, int endIndex)
    {
        StopAllTracingAnimations();
    }
    
    private float GetMasterPointWorldScale(float distance)
    {
        float baseDistanceScale = CalculateLabelScale(distance);
        
        Vector3 parentLossyScale = measuredObject != null ? measuredObject.lossyScale : Vector3.one;
        float parentScaleFactor = Mathf.Max((parentLossyScale.x + parentLossyScale.y + parentLossyScale.z) / 3f, 0.001f);
        
        float finalScaleFactor;

        if (parentScaleFactor >= 1f)
        {
            float scaleInfluence = 0.3f; 
            finalScaleFactor = Mathf.Lerp(1f, parentScaleFactor, scaleInfluence);
        }
        else
            finalScaleFactor = parentScaleFactor;
        
        return baseDistanceScale * finalScaleFactor;
    }

    private float GetTargetLabelScale(Vector3 worldPosition)
    {
        if (playerCamera == null)
            return 0.01f;
    
        float distance = Vector3.Distance(playerCamera.transform.position, worldPosition);
        float finalWorldScale = GetMasterPointWorldScale(distance);
        
        Vector3 parentLossyScale = measuredObject != null ? measuredObject.lossyScale : Vector3.one;
        float parentScaleFactor = Mathf.Max((parentLossyScale.x + parentLossyScale.y + parentLossyScale.z) / 3f, 0.001f);
        
        return finalWorldScale / parentScaleFactor;
    }

    private void UpdateLabelScaling()
    {
        if (playerCamera == null) return;
        
        float distanceToModel = measuredObject != null ? Vector3.Distance(playerCamera.transform.position, measuredObject.position) : 5f;
        
        float masterPointWorldScale = GetMasterPointWorldScale(distanceToModel);
        
        float lineToPointRatio = maxSize > 0 ? (baseLineWidth / maxSize) : 0.2f;
        
        float finalLineWidth = masterPointWorldScale * lineToPointRatio;
        
        if (highlightLineRenderer != null)
            highlightLineRenderer.widthMultiplier = finalLineWidth;
        
        if (sharedSegmentLineRenderer != null)
            sharedSegmentLineRenderer.widthMultiplier = finalLineWidth;

        if (labels.Count == 0)
            return;

        for (int i = 0; i < labels.Count; i++)
        {
            if (labels[i] == null) continue;
        
            float targetScale = GetTargetLabelScale(labels[i].transform.position);
            int labelId = labels[i].GetInstanceID();
            DOTween.Kill(labelId);
            labels[i].transform.DOScale(Vector3.one * targetScale, labelScalingUpdateInterval).SetId(labelId);
        
            if (i < pointMarkers.Count && pointMarkers[i] != null)
            {
                int markerId = pointMarkers[i].GetInstanceID();
                DOTween.Kill(markerId);
                pointMarkers[i].transform.DOScale(Vector3.one * targetScale, labelScalingUpdateInterval).SetId(markerId);
            }
        }
    }

    private float CalculateLabelScale(float distance)
    {
        if (distance <= minDistanceThreshold)
            return minSize;
        if (distance >= maxDistanceThreshold)
            return maxSize;
        
        return Mathf.Lerp(minSize, maxSize, (distance - minDistanceThreshold) / (maxDistanceThreshold - minDistanceThreshold));
    }
}