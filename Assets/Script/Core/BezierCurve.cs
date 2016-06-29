using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artoncode.Core {
	[ExecuteInEditMode]
	[RequireComponent (typeof(LineRenderer))]
	public class BezierCurve : MonoBehaviour {

		public List<GameObject> controlPoints = new List<GameObject>();
		public Material material;
		public Color color = Color.white;
		public float width = 0.2f;
		public int numberOfPoints = 10;
		private LineRenderer lineRenderer;

		void Start () {
			lineRenderer = GetComponent<LineRenderer> ();
			lineRenderer.useWorldSpace = true;
		}

		public int getControlPointCount () {
			normalizeControlPoint ();
			return controlPoints.Count;
		}

		public void addControlPoint (int idx, GameObject controlPoint) {
			if (controlPoints.Contains (controlPoint)) {
				return;
			}

			normalizeControlPoint ();
			controlPoints.Insert (idx, controlPoint);
		}

		public void addControlPoint (GameObject controlPoint) {
			normalizeControlPoint ();
			addControlPoint (controlPoints.Count, controlPoint);
		}

		public void deleteControlPoint (int idx) {
			normalizeControlPoint ();
			if (idx < controlPoints.Count) {
				controlPoints.RemoveAt (idx);
			}
		}

		public void deleteControlPoint (GameObject controlPoint) {
			normalizeControlPoint ();
			controlPoints.Remove (controlPoint);
		}

		void normalizeControlPoint () {
			if (controlPoints.Count > 1 && (controlPoints [0] == controlPoints [1])) {
				controlPoints.RemoveAt (0);
			}
			if (controlPoints.Count > 1 && (controlPoints [controlPoints.Count - 1] == controlPoints [controlPoints.Count - 2])) {
				controlPoints.RemoveAt (controlPoints.Count - 1);
			}
		}
	
		void Update () {
			lineRenderer.SetColors (color, color);
			lineRenderer.SetWidth (width, width);
			lineRenderer.material = material;

			if (controlPoints.Count == 1 || (controlPoints.Count > 1 && (controlPoints [0] != controlPoints [1]))) {
				controlPoints.Insert (0, controlPoints [0]);
			}
			if (controlPoints.Count > 1 && (controlPoints [controlPoints.Count - 1] != controlPoints [controlPoints.Count - 2])) {
				controlPoints.Add (controlPoints [controlPoints.Count - 1]);
			}

			if (numberOfPoints < 2) {
				numberOfPoints = 2;
			}
			lineRenderer.SetVertexCount (Mathf.Max(numberOfPoints * (controlPoints.Count - 2), 0));
			
			if (controlPoints.Count < 3) {
				return;
			}
			
			// loop over segments of spline
			Vector3 p0;
			Vector3 p1;
			Vector3 p2;
			for (int j = 0; j < controlPoints.Count - 2; j++) {
				// check control points
				if (controlPoints [j] == null || 
					controlPoints [j + 1] == null ||
					controlPoints [j + 2] == null) {
					return;  
				}

				// determine control points of segment
				p0 = 0.5f * (controlPoints [j].transform.position + controlPoints [j + 1].transform.position);
				p1 = controlPoints [j + 1].transform.position;
				p2 = 0.5f * (controlPoints [j + 1].transform.position + controlPoints [j + 2].transform.position);
				
				// set points of quadratic Bezier curve
				Vector3 position;
				float t; 
				float pointStep = 1f / numberOfPoints;
				if (j == controlPoints.Count - 3) {
					pointStep = 1f / (numberOfPoints - 1f);
				}  
				for (int i = 0; i < numberOfPoints; i++) {
					t = i * pointStep;
					position = (1f - t) * (1f - t) * p0 
						+ 2f * (1f - t) * t * p1
						+ t * t * p2;
					lineRenderer.SetPosition (i + j * numberOfPoints, position);
				}
			}
		}
	}
}