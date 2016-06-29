using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	public class FluidAABB {

		public Vector2 lowerBound;
		public Vector2 upperBound;

		public Vector2 getCenter () {
			return 0.5f * (lowerBound + upperBound);
		}

		public Vector2 getExtents () {
			return 0.5f * (upperBound - lowerBound);
		}

		public float getPerimeter () {
			float wx = upperBound.x - lowerBound.x;
			float wy = upperBound.y - lowerBound.y;
			return 2.0f * (wx + wy);
		}

		public void combine (FluidAABB aabb) {
			lowerBound = Vector2.Min (lowerBound, aabb.lowerBound);
			upperBound = Vector2.Max (upperBound, aabb.upperBound);
		}

		public void combine (FluidAABB aabb1, FluidAABB aabb2) {
			lowerBound = Vector2.Min (aabb1.lowerBound, aabb2.lowerBound);
			upperBound = Vector2.Max (aabb1.upperBound, aabb2.upperBound);
		}

		public bool contains (FluidAABB aabb) {
			bool result = true;
			result = result && lowerBound.x <= aabb.lowerBound.x;
			result = result && lowerBound.y <= aabb.lowerBound.y;
			result = result && aabb.upperBound.x <= upperBound.x;
			result = result && aabb.upperBound.y <= upperBound.y;
			return result;
		}

		public bool contains (Vector2 point) {
			if (point.x < lowerBound.x)
				return false;
			if (point.x > upperBound.x)
				return false;
			if (point.y < lowerBound.y)
				return false;
			if (point.y > upperBound.y)
				return false;
			return true;
		}

		public bool isOverlap (FluidAABB b) {
			Vector2 d1, d2;
			d1 = b.lowerBound - upperBound;
			d2 = lowerBound - b.upperBound;
		
			if (d1.x > 0.0f || d1.y > 0.0f)
				return false;
		
			if (d2.x > 0.0f || d2.y > 0.0f)
				return false;
		
			return true;
		}

		public void debug () {
			Debug.DrawLine (lowerBound, new Vector2 (upperBound.x, lowerBound.y));
			Debug.DrawLine (lowerBound, new Vector2 (lowerBound.x, upperBound.y));
			Debug.DrawLine (new Vector2 (lowerBound.x, upperBound.y), upperBound);
			Debug.DrawLine (new Vector2 (upperBound.x, lowerBound.y), upperBound);
		}
	}
}
