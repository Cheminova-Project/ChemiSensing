using TransformHandles.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TransformHandles
{
	/// <summary>
	/// Controlador para un collider en forma de cono.
	/// Permite ajustar parámetros del cono y actualizar su malla.
	/// </summary>
	public class ConeColliderController : MonoBehaviour
	{
		#if UNITY_EDITOR
		[SerializeField] private int sideCount = 15;
		[SerializeField] private float topRadius = 0.02f;
		[SerializeField] private int heightSegmentCount = 1;
		
		[SerializeField] private Transform colliderTransform;

		[SerializeField] private float height;
		[SerializeField] private float bottomRadius;

		[SerializeField] private bool save;

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

		private void UpdateCollider()
		{
			var newMesh = MeshUtils.CreateCone(
				height, 
				bottomRadius, 
				topRadius, sideCount, heightSegmentCount);
			
			newMesh.name = "cone";
			
			_meshFilter.sharedMesh = newMesh;
			_meshCollider.sharedMesh = newMesh;
			
			AssetDatabase.CreateAsset(newMesh, "Assets/cone.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			
		}
#endif
	}
}