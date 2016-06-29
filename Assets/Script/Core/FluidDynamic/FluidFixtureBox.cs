using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	[RequireComponent(typeof(Collider2D))]
	public class FluidFixtureBox : FluidFixturePolygon {
		private BoxCollider2D box;
	
		void Awake () {
			box = GetComponent<BoxCollider2D> ();
			resetVertices ();
			normals = new Vector2[4];
			updateVertices ();
		}

		public override void resetVertices () {
			vertices = new Vector2[4];
			vertices [0] = new Vector2 (box.offset.x - box.size.x / 2, box.offset.y + box.size.y / 2);
			vertices [1] = new Vector2 (box.offset.x - box.size.x / 2, box.offset.y - box.size.y / 2);
			vertices [2] = new Vector2 (box.offset.x + box.size.x / 2, box.offset.y - box.size.y / 2);
			vertices [3] = new Vector2 (box.offset.x + box.size.x / 2, box.offset.y + box.size.y / 2);
		}

		public override bool isTrigger () {
			return box.isTrigger;
		}
	}
}