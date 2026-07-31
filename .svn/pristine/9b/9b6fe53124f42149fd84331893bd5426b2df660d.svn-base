using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class UIFeedbackBox : MonoBehaviour
{
    [Header("References")]
    public BoxCollider referenceCollider;
    public Material uiEdgeHighlightMaterial;

    [Header("Box Settings")]
    public float borderThickness = 0.02f;
    public float cornerSize = 0.1f;

    [Header("Interaction Settings")]
    public bool showOnHover = true;
    public bool showOnGrab = true;

    public XRGrabInteractable grabInteractable;
    private MeshRenderer boxRenderer;
    private MeshFilter meshFilter;
    private GameObject feedbackBox;
    private bool isHovered = false;
    private bool isGrabbed = false;
    private float meshHeight = 0f;

    void Start()
    {
        if (referenceCollider == null)
        {
            referenceCollider = GetComponent<BoxCollider>();
        }

        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.AddListener(OnHoverEntered);
            grabInteractable.hoverExited.AddListener(OnHoverExited);
            grabInteractable.selectEntered.AddListener(OnGrabStarted);
            grabInteractable.selectExited.AddListener(OnGrabEnded);
        }

        SetupFeedbackBox();
        UpdateFeedbackVisibility();
    }

    private void SetupFeedbackBox()
    {
        feedbackBox = new GameObject("UI_FeedbackBox");
        feedbackBox.transform.SetParent(transform);
        
        Vector3 colliderWorldCenter = referenceCollider.transform.TransformPoint(referenceCollider.center);
        Vector3 localCenter = transform.InverseTransformPoint(colliderWorldCenter);

        feedbackBox.transform.localPosition = localCenter;
        feedbackBox.transform.localRotation = Quaternion.Euler(0, 180, 0);
        feedbackBox.transform.localScale = Vector3.one;
        
        meshFilter = feedbackBox.AddComponent<MeshFilter>();
        boxRenderer = feedbackBox.GetComponent<MeshRenderer>();
        if(boxRenderer == null)
            boxRenderer = feedbackBox.AddComponent<MeshRenderer>();

        GenerateBorderMesh();

        if (uiEdgeHighlightMaterial != null)
        {
            Material materialInstance = new Material(uiEdgeHighlightMaterial);
            boxRenderer.material = materialInstance;
            Debug.Log("[UIFeedbackBox] Material de borde asignado correctamente");
            Debug.Log("boxRenderer.material: " + boxRenderer.material.name);
        }
        else
        {
            Debug.LogError("[UIFeedbackBox] uiEdgeHighlightMaterial no está asignado en Inspector");
        }

        var feedbackBoxShaderController = feedbackBox.AddComponent<UIFeedbackBoxShaderController>();
        feedbackBoxShaderController.Initialize(30f);
    }

    private void GenerateBorderMesh()
    {
        Vector3 colliderSize = referenceCollider.size;
        Vector3 colliderScale = referenceCollider.transform.lossyScale;
        
        Vector3 scaledSize = Vector3.Scale(colliderSize, colliderScale);
        
        Vector3 thisScale = transform.lossyScale;
        Vector3 finalSize = new Vector3(
            scaledSize.x / thisScale.x,
            scaledSize.y / thisScale.y,
            scaledSize.z / thisScale.z
        );
        
        float halfWidth = finalSize.x * 0.5f;
        float halfHeight = finalSize.y * 0.5f;
        meshHeight = finalSize.y;
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        GenerateRectangleBorder(halfWidth, halfHeight, vertices, triangles, uvs);
        GenerateCornerElements(halfWidth, halfHeight, vertices, triangles, uvs);

        Mesh mesh = new Mesh();
        mesh.name = "UI_BorderMesh";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void GenerateRectangleBorder(float halfWidth, float halfHeight, 
                                        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, halfHeight, 0),
            new Vector3(-halfWidth, halfHeight, 0)
        };

        for (int i = 0; i < 4; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[(i + 1) % 4];
            
            GenerateLine(start, end, borderThickness, vertices, triangles, uvs, false, halfHeight);
        }
    }

    private void GenerateCornerElements(float halfWidth, float halfHeight,
                                       List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, halfHeight, 0),
            new Vector3(-halfWidth, halfHeight, 0)
        };

        Vector3[] directions1 = { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
        Vector3[] directions2 = { Vector3.up, Vector3.left, Vector3.down, Vector3.right };

        for (int i = 0; i < 4; i++)
        {
            Vector3 corner = corners[i];
            
            Vector3 end1 = corner + directions1[i] * cornerSize;
            Vector3 end2 = corner + directions2[i] * cornerSize;
            
            GenerateLine(corner, end1, borderThickness * 1.2f, vertices, triangles, uvs, true, halfHeight);
            GenerateLine(corner, end2, borderThickness * 1.2f, vertices, triangles, uvs, true, halfHeight);
        }
    }

    private void GenerateLine(Vector3 start, Vector3 end, float thickness,
                             List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, 
                             bool isCorner, float halfHeight)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * thickness * 0.5f;

        int vertexStart = vertices.Count;

        vertices.Add(start - perpendicular);
        vertices.Add(start + perpendicular);
        vertices.Add(end + perpendicular);
        vertices.Add(end - perpendicular);

        float uOffset = isCorner ? 0.5f : 0f;
        
        float uvStart = NormalizeHeight(start.y, halfHeight);
        float uvEnd = NormalizeHeight(end.y, halfHeight);
        
        uvs.Add(new Vector2(uOffset, uvStart));
        uvs.Add(new Vector2(uOffset + 0.5f, uvStart));
        uvs.Add(new Vector2(uOffset + 0.5f, uvEnd));
        uvs.Add(new Vector2(uOffset, uvEnd));

        triangles.Add(vertexStart + 0);
        triangles.Add(vertexStart + 1);
        triangles.Add(vertexStart + 2);
        triangles.Add(vertexStart + 0);
        triangles.Add(vertexStart + 2);
        triangles.Add(vertexStart + 3);
    }

    private float NormalizeHeight(float yPosition, float halfHeight)
    {
        return (yPosition + halfHeight) / (halfHeight * 2f);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        UpdateFeedbackVisibility();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isHovered = false;
        UpdateFeedbackVisibility();
    }

    private void OnGrabStarted(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        UpdateFeedbackVisibility();
    }

    private void OnGrabEnded(SelectExitEventArgs args)
    {
        isGrabbed = false;
        UpdateFeedbackVisibility();
    }

    private void UpdateFeedbackVisibility()
    {
        bool shouldShow = (showOnHover && isHovered) || (showOnGrab && isGrabbed);
        if (feedbackBox != null)
        {
            feedbackBox.SetActive(shouldShow);
        }
    }

    [ContextMenu("Regenerate Mesh")]
    public void RegenerateMesh()
    {
        if (meshFilter != null)
        {
            GenerateBorderMesh();
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            grabInteractable.hoverExited.RemoveListener(OnHoverExited);
            grabInteractable.selectEntered.RemoveListener(OnGrabStarted);
            grabInteractable.selectExited.RemoveListener(OnGrabEnded);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (referenceCollider == null) return;
        
        Vector3 colliderWorldCenter = referenceCollider.transform.TransformPoint(referenceCollider.center);
        Vector3 colliderSize = referenceCollider.size;
        Vector3 colliderScale = referenceCollider.transform.lossyScale;
        Vector3 scaledSize = Vector3.Scale(colliderSize, colliderScale);
        
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(
            colliderWorldCenter,
            referenceCollider.transform.rotation,
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, scaledSize);
        
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawSphere(colliderWorldCenter, 0.02f);
    }
}