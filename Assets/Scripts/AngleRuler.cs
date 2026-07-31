using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AngleRuler : Ruler
{
    [System.Serializable]
    public class AngleData
    {
        public int pointA;
        public int vertex;
        public int pointC;
        public bool isSharing;

        public AngleData(int a, int v, int c, bool sharing = false)
        {
            pointA = a;
            vertex = v;
            pointC = c;
            isSharing = sharing;
        }
    }

    protected List<AngleData> savedAngles = new List<AngleData>();
    private Coroutine angleHighlightCoroutine;
    
    /// <summary>
    /// Calcula el ángulo entre tres índices de puntos A -> Vértice <- C
    /// </summary>
    public float GetAngle(int indexA, int indexVertex, int indexC)
    {
        if (indexA < 0 || indexVertex < 0 || indexC < 0 || 
            indexA >= localPoints.Count || indexVertex >= localPoints.Count || indexC >= localPoints.Count)
            return 0f;

        Vector3 worldA = measuredObject.TransformPoint(localPoints[indexA]);
        Vector3 worldV = measuredObject.TransformPoint(localPoints[indexVertex]);
        Vector3 worldC = measuredObject.TransformPoint(localPoints[indexC]);

        Vector3 dirA = worldA - worldV;
        Vector3 dirC = worldC - worldV;

        return Vector3.Angle(dirA, dirC);
    }

    /// <summary>
    /// Dibuja la animación del ángulo.
    /// </summary>
    public void HighlightAngle(int indexA, int indexVertex, int indexC)
    {
        if (measuredObject == null) 
            return;
        
        StopAllTracingAnimations();
        
        if (angleHighlightCoroutine != null)
        {
            StopCoroutine(angleHighlightCoroutine);
            angleHighlightCoroutine = null;
        }

        if (highlightLineRenderer != null) 
            highlightLineRenderer.positionCount = 0;

        angleHighlightCoroutine = StartCoroutine(AnimateAngleTracing(indexA, indexVertex, indexC, highlightLineRenderer, true));
    }
    
    public void HighlightAngleNetworked(int indexA, int indexVertex, int indexC)
    {
        if (rulerSync != null && rulerSync.IsSpawned && NetworkManager.Singleton.IsConnectedClient)
        {
            rulerSync.RequestTriggerAngleHighlight(indexA, indexVertex, indexC);
        }
        else
        {
            HighlightAngle(indexA, indexVertex, indexC);
        }
    }

    public override void ApplySharedSegmentFromNetwork(SharedSegmentData data)
    {
        // Sobrescrito para no hacer nada en ángulos
    }

    public void SyncSharedAngle(int indexA, int indexVertex, int indexC, bool isSharing)
    {
        if (rulerSync != null && rulerSync.IsSpawned && NetworkManager.Singleton.IsConnectedClient)
            rulerSync.RequestShareAngle(indexA, indexVertex, indexC, isSharing);
        else
            ApplySharedAngleFromNetwork(new AngleData(indexA, indexVertex, indexC, isSharing));
    }
    
    public void ApplySharedAngleFromNetwork(AngleData data)
    {
        StopAllTracingAnimations();
        
        if (angleHighlightCoroutine != null)
        {
            StopCoroutine(angleHighlightCoroutine);
            angleHighlightCoroutine = null;
        }

        if (!data.isSharing || measuredObject == null) return;

        float angleVal = GetAngle(data.pointA, data.vertex, data.pointC);
        
        if (ToolMessageHandler.Instance != null)
        {
            ToolMessageHandler.Instance.ShowMessage($"Shared Angle ({data.pointA+1}-{data.vertex+1}-{data.pointC+1}): {angleVal:F2}°", 4f, MessageType.Info);
        }

        angleHighlightCoroutine = StartCoroutine(AnimateAngleTracing(data.pointA, data.vertex, data.pointC, sharedSegmentLineRenderer, true));
    }

    private List<Vector3> GetInterpolatedAnglePoints(int indexA, int indexVertex, int indexC)
    {
        List<Vector3> interpolatedPoints = new List<Vector3>();

        Vector3 worldA = measuredObject.TransformPoint(localPoints[indexA]);
        Vector3 worldV = measuredObject.TransformPoint(localPoints[indexVertex]);
        Vector3 worldC = measuredObject.TransformPoint(localPoints[indexC]);

        Vector3 dirA = worldA - worldV;
        Vector3 dirC = worldC - worldV;

        float proportion = 0.5f;
        float arcRadius = Mathf.Min(dirA.magnitude, dirC.magnitude) * proportion;

        Vector3 arcStart = worldV + dirA.normalized * arcRadius;
        Vector3 arcEnd = worldV + dirC.normalized * arcRadius;

        interpolatedPoints.Add(worldV);

        void AddStraightLine(Vector3 start, Vector3 end)
        {
            float dist = Vector3.Distance(start, end);
            if (dist < 0.001f) return; 

            int steps = Mathf.Max(1, Mathf.CeilToInt(dist / pointSpacing)); 
            for (int i = 1; i <= steps; i++)
            {
                interpolatedPoints.Add(Vector3.Lerp(start, end, (float)i / steps));
            }
        }

        AddStraightLine(worldV, worldA);
        AddStraightLine(worldA, arcStart);

        float totalAngle = Vector3.Angle(dirA, dirC);
        float arcLength = (totalAngle * Mathf.Deg2Rad) * arcRadius;
        int arcSegments = Mathf.Max(3, Mathf.CeilToInt(arcLength / pointSpacing));

        for (int i = 1; i <= arcSegments; i++)
        {
            float t = (float)i / arcSegments;
            Vector3 arcDirection = Vector3.Slerp(dirA.normalized, dirC.normalized, t);
            interpolatedPoints.Add(worldV + arcDirection * arcRadius);
        }

        AddStraightLine(arcEnd, worldC);
        AddStraightLine(worldC, worldV);

        return interpolatedPoints;
    }
    
    private IEnumerator AnimateAngleTracing(int indexA, int indexVertex, int indexC, LineRenderer targetLineRenderer, bool autoHide)
    {
        List<Vector3> anglePoints = GetInterpolatedAnglePoints(indexA, indexVertex, indexC);
        targetLineRenderer.positionCount = 0;
        
        float elapsedTime = 0f;
        float duration = minAnimationDuration; 

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            
            int targetPointCount = Mathf.Clamp(Mathf.CeilToInt(progress * anglePoints.Count), 0, anglePoints.Count);

            if (targetLineRenderer.positionCount < targetPointCount)
            {
                for (int i = targetLineRenderer.positionCount; i < targetPointCount; i++)
                {
                    targetLineRenderer.positionCount = i + 1;
                    targetLineRenderer.SetPosition(i, anglePoints[i]);
                }
            }
            yield return null;
        }

        targetLineRenderer.positionCount = anglePoints.Count;
        targetLineRenderer.SetPositions(anglePoints.ToArray());

        if (autoHide)
        {
            yield return new WaitForSeconds(idleTime); 
            if (targetLineRenderer != null) 
                targetLineRenderer.positionCount = 0;
            
            angleHighlightCoroutine = null;
        }
    }

    public void AddSavedAngle(int a, int v, int c)
    {
        savedAngles.Add(new AngleData(a, v, c));
    }

    public void RemoveSavedAngle(int a, int v, int c)
    {
        savedAngles.RemoveAll(angle => angle.pointA == a && angle.vertex == v && angle.pointC == c);
        StopAllTracingAnimations();
        
        if (angleHighlightCoroutine != null)
        {
            StopCoroutine(angleHighlightCoroutine);
            angleHighlightCoroutine = null;
        }
    }
    
    public List<AngleData> GetSavedAngles() => new List<AngleData>(savedAngles);

    public override void ResetRuler()
    {
        base.ResetRuler();
        savedAngles.Clear();
        StopAllTracingAnimations();
        
        if (angleHighlightCoroutine != null)
        {
            StopCoroutine(angleHighlightCoroutine);
            angleHighlightCoroutine = null;
        }
    }
}