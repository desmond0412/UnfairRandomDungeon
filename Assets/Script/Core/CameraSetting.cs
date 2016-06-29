using UnityEngine;
using System.Collections;

namespace Artoncode.Core {
	public enum CameraScaleMode {
		AspectFit,
		FitHeight,
		FitWidth
	}

	public class CameraSetting : MonoBehaviour {

		public CameraScaleMode scaleMode = CameraScaleMode.FitHeight;
		private Camera cam;
		public Vector3 focusPoint;
		public float widthRatio = 4f;
		public float heightRatio = 3f;

		// Use this for initialization
		void Awake () {
			// set default fps to 60 (for iOS devices)
			Application.targetFrameRate = 60;

			cam = GetComponent<Camera> ();

			switch (scaleMode) {
				case CameraScaleMode.AspectFit:
					scaleAspectFit ();
					break;
				case CameraScaleMode.FitWidth:
					scaleFitWidth ();
					break;
			}
		}

		private void scaleFitWidth () {
			// set the desired aspect ratio
			float targetAspect = widthRatio / heightRatio;
			
			// determine the game window's current aspect ratio
			float windowAspect = (float)Screen.width / (float)Screen.height;

			float targetHeight = 2 * Mathf.Tan (0.5f * cam.fieldOfView * Mathf.Deg2Rad) * (cam.transform.position - focusPoint).magnitude;
			float targetWidth = targetHeight * targetAspect;
			float visibleHeight = targetWidth / windowAspect;
			cam.fieldOfView = Mathf.Atan (visibleHeight / (2 * (cam.transform.position - focusPoint).magnitude)) * Mathf.Rad2Deg * 2;

			cam.orthographicSize = cam.orthographicSize * targetAspect / windowAspect;
		}

		private void scaleAspectFit () {
			// set the desired aspect ratio
			float targetAspect = widthRatio / heightRatio;
			
			// determine the game window's current aspect ratio
			float windowAspect = (float)Screen.width / (float)Screen.height;
			
			// current viewport height should be scaled by this amount
			float scaleHeight = windowAspect / targetAspect;
			
			// obtain camera component so we can modify its viewport
			Camera camera = GetComponent<Camera> ();
			
			// if scaled height is less than current height, add letterbox
			if (scaleHeight < 1.0f) {  
				Rect rect = camera.rect;
				
				rect.width = 1.0f;
				rect.height = scaleHeight;
				rect.x = 0;
				rect.y = (1.0f - scaleHeight) / 2.0f;
				
				camera.rect = rect;
			} else { // add pillarbox
				float scaleWidth = 1.0f / scaleHeight;
				
				Rect rect = camera.rect;
				
				rect.width = scaleWidth;
				rect.height = 1.0f;
				rect.x = (1.0f - scaleWidth) / 2.0f;
				rect.y = 0;
				
				camera.rect = rect;
			}
		}
	}

}
