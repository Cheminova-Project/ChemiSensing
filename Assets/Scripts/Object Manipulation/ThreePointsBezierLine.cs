using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa una línea de Bézier definida por tres puntos.
/// Permite calcular posiciones a lo largo de la curva para manipulación visual.
/// </summary>
public class ThreePointsBezierLine : MonoBehaviour
{
    /// <summary>
    /// Punto inicial de la curva Bézier.
    /// </summary>
    public Vector3 pointA;
    /// <summary>
    /// Punto de control de la curva Bézier.
    /// </summary>
    public Vector3 pointB;
    /// <summary>
    /// Punto final de la curva Bézier.
    /// </summary>
    public Vector3 pointC;

    /// <summary>
    /// LineRenderer para dibujar la curva.
    /// </summary>
    LineRenderer lineRenderer;

    /// <summary>
    /// Número de segmentos en la línea.
    /// </summary>
    [SerializeField] private int lineSegments = 20;

    /// <summary>
    /// Lista de puntos en la curva.
    /// </summary>
    private List<Vector3> points = new List<Vector3>();

    public void SetPoints(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        this.pointA = p0;
        this.pointB = p1;
        this.pointC = p2;
        UpdateCurvedRay();
    }
    
    public void SetLineWidth(float width)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }
    
    public void DisableLine()
    {
        lineRenderer.positionCount = 0;
    }
    
    
    private void UpdateCurvedRay()
    {
        List<Vector3> points = GenerateSmoothCurve(pointA, pointB, pointC, lineSegments - 1);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
    
    static List<Vector3> GenerateSmoothCurve(Vector3 p0, Vector3 p1, Vector3 p2, int resolution)
    {
        List<Vector3> points = new List<Vector3>();

        // Generamos un punto de control intermedio para suavizar la curva
        Vector3 p1_control = p1 + (p0 - p1) * 0.5f; // Control hacia A
        Vector3 p2_control = p1 + (p2 - p1) * 0.5f; // Control hacia C

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = CalculateBezierPoint(t, p0, p1_control, p2_control, p2);
            points.Add(point);
        }

        return points;
    }

    static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
    }

    public void CreateLineRenderer(float width, Material material)
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = material;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }
}
