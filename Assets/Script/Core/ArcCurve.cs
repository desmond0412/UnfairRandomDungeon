using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artoncode.Core {
	[ExecuteInEditMode]
	[RequireComponent (typeof(LineRenderer))]
	public class ArcCurve : MonoBehaviour {
		
		public float startAngle = 0;
		[Range (-360, 360)] public float arcAngle = 360;
		[Range (3, 360)] public int sides = 128;
		public float radius = 1f;
		public Material material;
		public Color color = Color.white;
		public float width = 0.2f;
		private LineRenderer lineRenderer;

		void Start () {
			lineRenderer = GetComponent<LineRenderer> ();
			lineRenderer.useWorldSpace = false;
		}

		void LateUpdate () {
			updateArc ();
		}

		void updateArc () {
			lineRenderer.SetColors (color, color);
			lineRenderer.SetWidth (width, width);
			lineRenderer.material = material;

			int nv = sides + 1;

			lineRenderer.SetVertexCount (nv);
			for (int i = 0; i < nv; i++) {
				float x = radius * Mathf.Cos (Mathf.Deg2Rad * (arcAngle * i / sides + startAngle));
				float z = radius * Mathf.Sin (Mathf.Deg2Rad * (arcAngle * i / sides + startAngle));
				lineRenderer.SetPosition (i, new Vector3(x, 0, z));
			}
		}
	}
}