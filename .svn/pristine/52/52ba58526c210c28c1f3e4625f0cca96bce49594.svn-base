using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode.Components;

public class VisualClonesManager : MonoBehaviour
{
    /// <summary>
    /// Tipo de personaje del jugador asociado a este manager.
    /// Determina el comportamiento de los clones visuales.
    /// </summary>
    public PlayerCharacterType playerCharacterType;

    /// <summary>
    /// Transform del objetivo original que se está inspeccionando.
    /// </summary>
    private Transform originalTargetTransform;

    /// <summary>
    /// Transform del clon del objetivo que se está inspeccionando.
    /// </summary>
    private Transform cloneTargetTransform;
    
    /// <summary>
    /// Diccionario que mapea el ID de cliente a su personaje clonado.
    /// </summary>
    private Dictionary<ulong, ClonedCharacter> _clonedCharacters = new Dictionary<ulong, ClonedCharacter>();

    /// <summary>
    /// Controlador del objeto inspeccionado.
    /// </summary>
    private InspectedObjectController inspectedObjectController;

    private void OnEnable()
    {
        if (!NetworkManager.Singleton.IsConnectedClient) return;
        if (!LocalRegistry.Instance) return;
        
        inspectedObjectController = InspectedObjectController.Instance;
        if (!inspectedObjectController) return;
        LocalRegistry.Instance.OnPlayerObjectRegistered += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        LocalRegistry.Instance.OnInitialPlayerListReceived += OnInitialPlayerListReceived;
        inspectedObjectController.OnInspectedObjectLoaded = CreateInspectedObjectVisualClone;
    }

    private void OnDisable()
    {
        if (!NetworkManager.Singleton) return;
        LocalRegistry.Instance.OnPlayerObjectRegistered -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        LocalRegistry.Instance.OnInitialPlayerListReceived -= OnInitialPlayerListReceived;
        inspectedObjectController.OnInspectedObjectLoaded -= CreateInspectedObjectVisualClone;
    }

    /// <summary>
    /// Crea un clon visual del objeto que se está inspeccionando actualmente.
    /// Este clon se utiliza para mostrar una representación del objeto en el modo AR.
    /// </summary>
    private void CreateInspectedObjectVisualClone()
    {
        originalTargetTransform = inspectedObjectController.GetInspectedObject().transform;
        cloneTargetTransform = Instantiate(originalTargetTransform.gameObject).transform;
        cloneTargetTransform.name = "VisualTargetClone";
        CleanCloneHierarchy(cloneTargetTransform.transform);
        RemoveNetworkingComponents(cloneTargetTransform.gameObject);

        var extendedNetworkTransforms = gameObject.GetComponentsInChildren<ExtendedNetworkTransform>();
        
        if(extendedNetworkTransforms == null)
        {
            Debug.LogError("No se encontró el componente ExtendedNetworkTransform en el objeto");
            return;
        }
        if(playerCharacterType == PlayerCharacterType.AR)
        {
            HideMeshes(originalTargetTransform);
            foreach (var clonedTransform in extendedNetworkTransforms)
                clonedTransform.SetTargetTransform(cloneTargetTransform);
        }
        else
        {
            HideMeshes(cloneTargetTransform);
            foreach (var clonedTransform in extendedNetworkTransforms)
                clonedTransform.SetTargetTransform(originalTargetTransform);
        }
    }

    /// <summary>
    /// Se llama cuando se recibe la lista inicial de jugadores.
    /// Registra a cada cliente en el manager para comenzar a seguir sus objetos.
    /// </summary>
    /// <param name="_clientToObjectIds">Diccionario que mapea IDs de cliente a objetos de red.</param>
    private void OnInitialPlayerListReceived(Dictionary<ulong, ClientInfo> _clientToObjectIds)
    {
        foreach (var clientId in _clientToObjectIds.Keys)
        {
            TryRegisterClient(clientId);
        }
    }
    
    /// <summary>
    /// Se llama cuando un nuevo cliente se conecta.
    /// Registra al cliente y crea los clones necesarios para su objeto.
    /// </summary>
    /// <param name="clientId">ID del cliente que se ha conectado.</param>
    private void OnClientConnected(ulong clientId)
    {
        TryRegisterClient(clientId);
    }

    /// <summary>
    /// Se llama cuando un cliente se desconecta.
    /// Elimina los clones visuales asociados a ese cliente.
    /// </summary>
    /// <param name="clientId">ID del cliente que se ha desconectado.</param>
    private void OnClientDisconnected(ulong clientId)
    {
        if (_clonedCharacters.TryGetValue(clientId, out var clonedCharacter))
        {
            //Destruimos los clones del objeto
            foreach (var clonedTransform in clonedCharacter.GetClonedTransforms())
            {
                Destroy(clonedTransform.VisualClone.gameObject);
            }
            _clonedCharacters.Remove(clientId);
        }
    }

    /// <summary>
    /// Intenta registrar a un cliente en el manager.
    /// Si el cliente ya es el local, no se realiza ninguna acción.
    /// </summary>
    /// <param name="clientId">ID del cliente a registrar.</param>
    private void TryRegisterClient(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
            return;

        NetworkObject playerNetworkObject = LocalRegistry.Instance.GetNetworkObjectOfClient(clientId);
        if (playerNetworkObject == null)
        {
            Debug.LogError($"No se encontró el objeto de red para el cliente {clientId}");
            return;
        }
        
        GeneralCharacter generalCharacter = playerNetworkObject.GetComponent<GeneralCharacter>();
        if (generalCharacter == null)
        {
            Debug.LogError($"No se encontró el componente GeneralCharacter en el objeto de red del cliente {clientId}");
            return;
        }
        
        //Descartaremos aquellos que no haya que trasnformar: En caso de R, debemos transformar el objeto y todoslos jugadores que no sean de tipo AR. En modo VR, desktop y mobile debemos transformar el objeto y todos los jugadores que sean de tipo AR.
        if (playerCharacterType == PlayerCharacterType.AR)
        {
            if (generalCharacter is ARCharacter)
            {
                return;
            }
        }
        else
        {
            if (!(generalCharacter is ARCharacter))
            {
                return;
            }
        }
        
        List<ExtendedNetworkTransform> networkTransforms = HierarchyUtils.GetComponentsInChildrenHierarchyOrdered<ExtendedNetworkTransform>(playerNetworkObject.transform);

        if (networkTransforms == null || networkTransforms.Count == 0)
        {
            Debug.LogWarning($"No se encontraron NetworkTransforms para el cliente {clientId}");
            return;
        }
        
        ClonedCharacter clonedCharacter = new ClonedCharacter();
        ExtendedNetworkTransform rootExtendedNetworkTransform = networkTransforms[0];
       
        //Clonaremos aquel que sea "padre"
        var cloneRoot = Instantiate(rootExtendedNetworkTransform.transform.gameObject);
        //Ahora debemos relacionar cada extendednetworktransform con su clon
        //Debería bastar con coger los ExtendedNetworkTransform del clon en orden
        var clonedExtendedNetworkTransforms = HierarchyUtils.GetComponentsInChildrenHierarchyOrdered<ExtendedNetworkTransform>(cloneRoot.transform);
        
        if(clonedExtendedNetworkTransforms.Count != networkTransforms.Count)
        {
            Debug.LogError($"El número de ExtendedNetworkTransforms clonados ({clonedExtendedNetworkTransforms.Count}) no coincide con el original ({networkTransforms.Count}).");
            return;
        }
        for (var i = 0; i < networkTransforms.Count; i++)
        {
            var extendedNetworkTransform = networkTransforms[i];
            var clonedExtendedNetworkTransform = clonedExtendedNetworkTransforms[i];
            clonedCharacter.AddClonedTransform(new ClonedTransform(
                extendedNetworkTransform,
                clonedExtendedNetworkTransform.transform));
        }
        
        CleanCloneHierarchy(cloneRoot.transform);
        HideMeshes(rootExtendedNetworkTransform.transform);
        
        _clonedCharacters.Add(clientId, clonedCharacter);
    }

    
    /// <summary>
    /// Limpia la jerarquía del clon, eliminando componentes innecesarios.
    /// </summary>
    /// <param name="root">Transform raíz del clon a limpiar.</param>
    private void CleanCloneHierarchy(Transform root)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            Component[] components = t.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp is MeshFilter || comp is MeshRenderer || comp is Transform)
                    continue;
                DestroyImmediate(comp);
            }
        }
    }
    
    /// <summary>
    /// Oculta las mallas en el transform raíz y sus hijos.
    /// </summary>
    /// <param name="root">Transform raíz donde ocultar las mallas.</param>
    private void HideMeshes(Transform root)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            MeshRenderer[] meshRenderers = t.GetComponents<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = false;
            }
        }
    }

    /// <summary>
    /// Elimina los componentes de red del objeto dado.
    /// Esto incluye NetworkObject y NetworkTransform.
    /// </summary>
    /// <param name="obj">GameObject del cual eliminar los componentes de red.</param>
    private void RemoveNetworkingComponents(GameObject obj)
    {
        var networkObject = obj.GetComponent<NetworkObject>();
        var networkTransforms = obj.GetComponentsInChildren<NetworkTransform>();
        if (networkObject)
            Destroy(obj.GetComponent<NetworkObject>());

        if (networkTransforms != null && networkTransforms.Length > 0)
            foreach (var nt in networkTransforms)
                Destroy(nt);
    }

    private void LateUpdate()
    {
        if (originalTargetTransform == null)
        {
            Debug.LogWarning("Original target transform is null. Cannot update cloned transforms.");
            return;
        }

        if (cloneTargetTransform == null)
        {
            Debug.LogWarning("Clone target transform is null. Cannot update cloned transforms.");
            return;
        }

        Transform inspectedObjectTransform = null;
        
        if(playerCharacterType == PlayerCharacterType.AR)
            inspectedObjectTransform = cloneTargetTransform;
        else
            inspectedObjectTransform = originalTargetTransform; // Cambia esto si necesitas otro objeto

        Matrix4x4 targetTransformMatrix = inspectedObjectTransform.localToWorldMatrix; // Implementa esta función para obtener la matriz actual
        
        foreach (var kvp in _clonedCharacters)
        {
            ulong clientId = kvp.Key;
            List<ClonedTransform> clonedTransforms = kvp.Value.GetClonedTransforms();

            for (int i = 0; i < clonedTransforms.Count; i++)
            {
                ExtendedNetworkTransform extTransform = clonedTransforms[i].ExtendedNetworkTransform;
                Transform clone = clonedTransforms[i].VisualClone;

                if (extTransform == null)
                {
                    Debug.LogWarning($"ExtendedNetworkTransform is null for client {clientId}, part {i}, {clone.name}. Skipping update.");
                    continue;
                }
                    

                Matrix4x4 relativeMatrix = extTransform.RelativeMatrix;

                // Aquí aplicas la transformación contraria para simular el cambio del objeto en AR
                Matrix4x4 cloneWorldMatrix = targetTransformMatrix * relativeMatrix;

                // Descomponemos la matriz para asignar posición, rotación y escala
                clone.position = cloneWorldMatrix.GetColumn(3);
                
                if(cloneWorldMatrix.GetColumn(2).magnitude < float.Epsilon || cloneWorldMatrix.GetColumn(1).magnitude < float.Epsilon)
                    clone.rotation = Quaternion.identity; // Asignar una rotación por defecto si la dirección es muy pequeña
                else
                    clone.rotation = Quaternion.LookRotation(
                        cloneWorldMatrix.GetColumn(2),
                        cloneWorldMatrix.GetColumn(1)
                    );

                clone.localScale = new Vector3(
                    cloneWorldMatrix.GetColumn(0).magnitude,
                    cloneWorldMatrix.GetColumn(1).magnitude,
                    cloneWorldMatrix.GetColumn(2).magnitude
                );
            }
        }
    }


    private Vector3 GetTotalScale(Transform t)
    {
        Vector3 scale = t.localScale;
        Transform current = t.parent;

        while (current != null)
        {
            Vector3 parentScale = current.localScale;
            scale = new Vector3(
                scale.x * parentScale.x,
                scale.y * parentScale.y,
                scale.z * parentScale.z
            );

            current = current.parent;
        }

        return scale;
    }

    private float SafeDiv(float a, float b)
    {
        return Mathf.Approximately(b, 0f) ? 0f : a / b;
    }
}

public class ClonedCharacter
{
    public List<ClonedTransform> ClonedTransforms;
    public ClonedCharacter()
    {
        ClonedTransforms = new List<ClonedTransform>();
    }

    public void AddClonedTransform(ClonedTransform clonedTransform)
    {
        ClonedTransforms.Add(clonedTransform);
    }
    
    public bool IsAlreadyCloned()
    {
        return ClonedTransforms.Count > 0;
    }

    public List<ClonedTransform> GetClonedTransforms()
    {
        return ClonedTransforms;
    }
}

public struct ClonedTransform
{
    public ExtendedNetworkTransform ExtendedNetworkTransform { get; }
    public Transform VisualClone { get; }

    public ClonedTransform(ExtendedNetworkTransform extTransform, Transform clone)
    {
        ExtendedNetworkTransform = extTransform;
        VisualClone = clone;
    }
}

public static class HierarchyUtils
{
    public static List<T> GetComponentsInChildrenHierarchyOrdered<T>(Transform root, bool includeInactive = true) where T : Component
    {
        List<T> result = new();
        TraverseHierarchy<T>(root, includeInactive, result);
        return result;
    }

    private static void TraverseHierarchy<T>(Transform current, bool includeInactive, List<T> list) where T : Component
    {
        if (!includeInactive && !current.gameObject.activeInHierarchy)
            return;

        // Si el objeto actual tiene el componente, lo agregamos
        T component = current.GetComponent<T>();
        if (component != null)
            list.Add(component);

        // Recorremos hijos
        foreach (Transform child in current)
        {
            TraverseHierarchy<T>(child, includeInactive, list);
        }
    }
}