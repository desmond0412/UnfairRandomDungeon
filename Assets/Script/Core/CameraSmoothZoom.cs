using UnityEngine;
using System.Collections;

namespace Artoncode.Core {

	[RequireComponent(typeof(Camera))]
	public class CameraSmoothZoom : MonoBehaviour {

		private Camera cam;

		public float zoom = 1f;
		public float smoothTime = 0.3f;
		private float currZoom;
		private float velocity;
		private float fov;
		private float orthoSize;

		void Awake () {
			cam = GetComponent<Camera>();
		}

		void Start () {
			fov = cam.fieldOfView;
			orthoSize = cam.orthographicSize;
			currZoom = zoom;
		}

		void Update () {
			currZoom = Mathf.SmoothDamp(currZoom, zoom, ref velocity, smoothTime);

			cam.fieldOfView = fov / currZoom;
			cam.orthographicSize = orthoSize / currZoom;
		}
	}

}