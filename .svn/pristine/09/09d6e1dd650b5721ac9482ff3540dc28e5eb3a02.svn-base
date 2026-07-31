using TransformHandles.Utils;
using UnityEditor;
using UnityEngine;
using VInspector;

namespace TransformHandles
{
	/// <summary>
	/// Controlador para un collider en forma de torus.
	/// Permite ajustar parámetros del torus y actualizar su malla.
	/// </summary>
	public class TorusColliderController : MonoBehaviour
	{
		#if UNITY_EDITOR
		[SerializeField] private int segmentCount = 32;
		[SerializeField] private int sideCount = 15;
		[SerializeField] private float radius;
		[SerializeField] private float thickness;

		[SerializeField] private Transform colliderTransform;
		
		private MeshCollider _meshCollider;
		private MeshFilter _meshFilter;

		private void Awake()
		{
			_meshCollider = colliderTransform.GetComponent<MeshCollider>();
			_meshFilter = colliderTransform.GetComponent<MeshFilter>();
		}

		private void Start()
		{
			UpdateCollider();
		}
		
		private void Update()
		{
	
		}
		[Button("Update Collider")]
		private void UpdateCollider()
		{
			if(!_meshCollider)
				_meshCollider = colliderTransform.GetComponent<MeshCollider>();
			if(!_meshFilter)
				_meshFilter = colliderTransform.GetComponent<MeshFilter>();
			
			var newMesh = MeshUtils.CreateTorus(radius, thickness, segmentCount, sideCount);
			newMesh.name = "torus_" + _meshCollider.name;
			
			_meshFilter.sharedMesh = newMesh;
			_meshCollider.sharedMesh = newMesh;
			
			
			AssetDatabase.CreateAsset(newMesh, "Assets/" + _meshCollider.transform.parent.name + ".asset");
			
		}
#endif
	}
}