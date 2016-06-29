using Artoncode.Core.Utility;
using UnityEngine;

namespace Artoncode.Core.Fluid {
	[RequireComponent(typeof(Collider2D))]
	public class FluidFixturePolygon : FluidFixtureShape {
		private PolygonCollider2D polygon;
		protected Vector2[] vertices;
		protected Vector2[] normals;
	
		void Awake () {
			polygon = GetComponent<PolygonCollider2D> ();
			resetVertices ();
			normals = new Vector2[vertices.Length];
			updateVertices ();

			FluidFixturePolygon[] polys = GetComponents<FluidFixturePolygon> ();
			if (polys.Length == 1) {
				gameObject.AddComponent<FluidFixturePolygon> ();

			}
		}

		void FixedUpdate () {
			resetVertices ();
			updateVertices ();
		}

		public virtual void resetVertices () {
			vertices = polygon.points;
		}

		protected void updateVertices () {
			for (int i=0; i<vertices.Length; i++) {
				vertices [i].Scale (transform.lossyScale);
				vertices [i] = transform.rotation * vertices [i] + transform.position;
			}
			for (int i=0; i<vertices.Length; i++) {
				int i1 = i;
				int i2 = i + 1 < vertices.Length ? i + 1 : 0;
				Vector2 edge = vertices [i2] - vertices [i1];
				normals [i] = GameUtility.cross (edge, 1f).normalized;
			}
		}
	
		public override void computeDistance (FluidParticle fp, out float distance, out Vector2 normal) {
			Vector2 p = fp.position;
			float maxDistance = -float.MaxValue;
			Vector2 normalForMaxDistance = p;

			for (int i=0; i<vertices.Length; i++) {
				float dot = Vector2.Dot (normals [i], p - vertices [i]);
				if (dot > maxDistance) {
					maxDistance = dot;
					normalForMaxDistance = normals [i];
				}
			}

			if (maxDistance > 0) {
				Vector2 minDistance = normalForMaxDistance;
				float minDistance2 = maxDistance * maxDistance;
				for (int i=0; i<vertices.Length; i++) {
					Vector2 dist = p - vertices [i];
					float dist2 = dist.sqrMagnitude;
					if (minDistance2 > dist2) {
						minDistance = dist;
						minDistance2 = dist2;
					}
				}
			
				distance = Mathf.Sqrt (minDistance2);
				normal = minDistance.normalized;
			} else {
				distance = maxDistance;
				normal = normalForMaxDistance;
			}
		}

		public override FluidAABB getAABB () {
			FluidAABB aabb = new FluidAABB ();

			aabb.lowerBound.x = +float.MaxValue;
			aabb.lowerBound.y = +float.MaxValue;
			aabb.upperBound.x = -float.MaxValue;
			aabb.upperBound.y = -float.MaxValue;

			for (int i=0; i<vertices.Length; i++) {
				aabb.lowerBound = Vector2.Min (aabb.lowerBound, vertices [i]);
				aabb.upperBound = Vector2.Max (aabb.upperBound, vertices [i]);
			}
		
			return aabb;
		}

		public override bool testPoint (Vector2 p) {
			for (int i=0; i<vertices.Length; i++) {
				float dot = Vector2.Dot (normals [i], p - vertices [i]);
				if (dot > 0) {
					return false;
				}
			}
			return true;
		}

		public override bool isTrigger () {
			return polygon.isTrigger;
		}
	}
}
