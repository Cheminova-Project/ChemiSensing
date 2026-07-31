using UnityEngine;

/// <summary>
/// Utilidades matemáticas para operaciones comunes en manipulación de handles.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Calcula la distancia entre dos puntos en el espacio 3D.
    /// </summary>
    /// <param name="a">Primer punto.</param>
    /// <param name="b">Segundo punto.</param>
    /// <returns>Distancia entre los puntos.</returns>
    public static float Distance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }

    /// <summary>
    /// Calcula el ángulo entre dos vectores.
    /// </summary>
    /// <param name="from">Vector de origen.</param>
    /// <param name="to">Vector de destino.</param>
    /// <returns>Ángulo en grados.</returns>
    public static float Angle(Vector3 from, Vector3 to)
    {
        return Vector3.Angle(from, to);
    }

    private const float PrecisionThreshold = 0.001f;
		
    public static float ClosestPointOnRay(Ray ray, Ray other)
    {
        // based on: https://math.stackexchange.com/questions/1036959/midpoint-of-the-shortest-distance-between-2-rays-in-3d
        // note: directions of both rays must be normalized
        // ray.origin -> a
        // ray.direction -> b
        // other.origin -> c
        // other.direction -> d

        var bd = Vector3.Dot(ray.direction, other.direction);
        var cd = Vector3.Dot(other.origin,  other.direction);
        var ad = Vector3.Dot(ray.origin,    other.direction);
        var bc = Vector3.Dot(ray.direction, other.origin);
        var ab = Vector3.Dot(ray.origin,    ray.direction);
			
        var bottom = bd * bd - 1f;
        if (Mathf.Abs(bottom) < PrecisionThreshold)
        {
            return 0;
        }

        var top = ab - bc + bd * (cd - ad);
        return top / bottom;
    }
}