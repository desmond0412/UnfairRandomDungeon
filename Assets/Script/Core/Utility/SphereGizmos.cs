using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Utility {

	[RequireComponent (typeof (SphereCollider))]
	public class SphereGizmos : MonoBehaviour {

		public Color gizmoColour = Color.blue;

		void OnDrawGizmos() {
			Gizmos.color = gizmoColour;
			Gizmos.matrix = Matrix4x4.TRS (transform.position, transform.rotation, transform.lossyScale);
			Gizmos.DrawSphere(GetComponent<SphereCollider>().center,GetComponent<SphereCollider>().radius);
		}
	}

}