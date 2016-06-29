using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	[RequireComponent(typeof(Collider2D))]
	public class FluidFixtureCircle : FluidFixtureShape {
		private CircleCollider2D circle;
		private float radius;

		void Awake () {
			circle = GetComponent<CircleCollider2D> ();
			radius = circle.radius * (transform.lossyScale.x > transform.lossyScale.y ? 
			                          transform.lossyScale.x : 
			                          transform.lossyScale.y);
		}

		void FixedUpdate () {
			radius = circle.radius * (transform.lossyScale.x > transform.lossyScale.y ? 
			                          transform.lossyScale.x : 
			                          transform.lossyScale.y);
		}

		public override void computeDistance (FluidParticle fp, out float distance, out Vector2 normal) {
			Vector2 center = new Vector2 (transform.position.x, transform.position.y) + circle.offset;
			Vector2 d = fp.position - center;
			float d1 = d.magnitude;
			distance = d1 - radius;
			normal = d / d1;
		}

		public override FluidAABB getAABB () {
			FluidAABB aabb = new FluidAABB ();
		
			aabb.lowerBound.x = transform.position.x - radius;
			aabb.lowerBound.y = transform.position.y - radius;
			aabb.upperBound.x = transform.position.x + radius;
			aabb.upperBound.y = transform.position.y + radius;

			return aabb;
		}

		public override bool testPoint (Vector2 p) {
			Vector2 center = new Vector2 (transform.position.x, transform.position.y) + circle.offset;
			Vector2 d = p - center;
			return d.sqrMagnitude <= radius * radius;
		}

		public override bool isTrigger () {
			return circle.isTrigger;
		}
	}
}