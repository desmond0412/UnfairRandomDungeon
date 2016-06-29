using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

using Artoncode.Core.Utility;

namespace Artoncode.Core.Fluid {
	[RequireComponent(typeof(Collider2D))]
	public class FluidFixtureEdge : FluidFixturePolygon {
		public float width;
		private EdgeCollider2D edge;
		//private Vector2[] _prevVertices;
		private float scaledWidth;

		void Awake () {
			edge = GetComponent<EdgeCollider2D> ();
			vertices = new Vector2[2];
			resetVertices ();
			normals = new Vector2[2];
			updateVertices ();
			//_prevVertices = vertices;
		}

		void FixedUpdate () {
			//_prevVertices = vertices;
			resetVertices ();
			updateVertices ();
		}

		public override void resetVertices () {
			vertices [0] = edge.points [0];
			vertices [1] = edge.points [1];
			scaledWidth = width * (transform.lossyScale.x > transform.lossyScale.y ? 
			                          transform.lossyScale.x : 
			                          transform.lossyScale.y);
		}

		public override void computeDistance (FluidParticle fp, out float distance, out Vector2 normal) {
			bool onEdge = computeDistance (vertices, fp.position, out distance, out normal);
			if (onEdge) {
				float dot = Vector2.Dot (normals [0], normal);
				if (Mathf.Approximately (dot, -1f)) {
					if (distance < scaledWidth) {
						distance = -distance;
						normal = normals [0];
					} else {
						distance -= scaledWidth;
					}
				}
			}
		}

		private bool computeDistance (Vector2[] edge, Vector2 p, out float distance, out Vector2 normal) {
			bool onEdge = false;
			Vector2 d = p - edge [0];
			Vector2 s = edge [1] - edge [0];
		
			float ds = Vector2.Dot (d, s);
			if (ds > 0) {
				float s2 = Vector2.Dot (s, s);
				if (ds > s2) {
					d = p - edge [1];
				} else {
					d -= ds / s2 * s;
					onEdge = true;
				}
			}
			float d1 = d.magnitude;
			distance = d1;
			normal = d1 > 0 ? 1 / d1 * d : Vector2.zero;

			return onEdge;
		}
		              
		public override bool isTrigger () {
			return edge.isTrigger;
		}

		#if UNITY_EDITOR
		void OnDrawGizmos () {
			bool selected = false;
			Transform t = transform;
			while (t) {
				if (Selection.Contains(t.gameObject)) {
					selected = true;
					break;
				}
				t = t.parent;
			}
			if (selected) {
				Vector2[]vs = GetComponent<EdgeCollider2D>().points;
				for (int i=0; i<vs.Length; i++) {
					vs [i].Scale (transform.lossyScale);
					vs [i] = transform.rotation * vs [i] + transform.position;
				}
				Vector2 v1 = vs[0] - vs[1];
				Vector2 v2 = GameUtility.cross (v1, 1f).normalized * width * (transform.lossyScale.x > transform.lossyScale.y ? 
				                                                              transform.lossyScale.x : 
				                                                              transform.lossyScale.y) + vs [0];
				Handles.color = new Color(135/255.0f, 228/255.0f, 129/255.0f);
				Handles.DrawLine (vs[0], vs[1]);
				Handles.DrawLine (v2, v2-v1);
			}
		}
		#endif
	}
}
