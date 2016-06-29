using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class MeshOrientationFix : MonoBehaviour {

	// Use this for initialization
	void Awake() {
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		
		Vector3[] initialVertices = mesh.vertices;
		for (int i=0; i<initialVertices.Length; i++) {
			initialVertices[i].Scale(transform.localScale);
			initialVertices[i] = transform.rotation * initialVertices[i];
		}

		mesh.vertices = initialVertices;
		
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();

		Transform [] children = GetComponentsInChildren<Transform>();
		for (int i=1; i<children.Length; i++) {
			children[i].parent = null;
		}

		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		for (int i=1; i<children.Length; i++) {
			children[i].parent = transform;
		}

		MeshCollider meshCollider = GetComponent<MeshCollider> ();
		if (meshCollider) {
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = mesh;
		}
	}

	void Start() {

	}
}
