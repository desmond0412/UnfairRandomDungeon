using UnityEngine;
using System.Collections;

namespace Artoncode.Core {
	[RequireComponent (typeof(Collider))]
	public class SmoothFollowAreaTrigger : MonoBehaviour {
		
		public SmoothFollow cam;
		private bool newIsLookAtTarget = true;
		private bool newIsSmoothLookAt = true;
		public Vector3 newLookAtOffset;
		public Vector3 newOffset;
		public Transform newTarget;
		public float smoothTransition;

		private Vector3 prevLookAt;
		private Vector3 prevOffset;
		private Transform prevTarget;
		private float prevSmoothTime;
		private bool prevIsLookAtTarget;
		private bool prevIsSmoothLookAt;


		private void StoreOriginalSettings()
		{
			prevIsLookAtTarget = cam.lookAtTarget;
			prevLookAt = cam.lookAtOffset;
			prevOffset = cam.positionOffset;
			prevSmoothTime = cam.smoothTime;
			prevIsSmoothLookAt = cam.smoothLookAtTarget;
			prevTarget = cam.target;

		}

		public void RestoreOriginalSettings()
		{
			if(cam.IsLocked) return;
			cam.positionOffset = prevOffset;
			cam.target = prevTarget;
			cam.smoothTime = prevSmoothTime;
			cam.lookAtOffset = prevLookAt;
			cam.lookAtTarget = prevIsLookAtTarget;
			cam.smoothLookAtTarget = prevIsSmoothLookAt;
		}

		public void RevertSettings()
		{
			if(cam.IsLocked) return;
			newIsLookAtTarget	= prevIsLookAtTarget;
			newLookAtOffset		= prevLookAt;
			newOffset 			= prevOffset;
			smoothTransition 	= prevSmoothTime;
			newIsSmoothLookAt	= prevIsSmoothLookAt;
			newTarget			= prevTarget;
		}

		public void AssignNewSettings()
		{
			if(cam.IsLocked) return;
			cam.lookAtTarget = newIsLookAtTarget;
			cam.lookAtOffset = newLookAtOffset;
			cam.positionOffset = newOffset;
			cam.smoothTime = smoothTransition;
			cam.smoothLookAtTarget = newIsSmoothLookAt;

			if (newTarget) {
				cam.target = newTarget;
			}
		}

		void OnTriggerEnter (Collider c) {
			if (c.CompareTag("Player") && !c.isTrigger) {
				cam = Camera.main.GetComponent<SmoothFollow>();
				if(cam.IsLocked) return;

				StoreOriginalSettings();
				AssignNewSettings();
			
			}
		}
		
		void OnTriggerExit (Collider c) {
			if (c.CompareTag("Player") && !c.isTrigger) {
				if(cam.IsLocked) return;
				RestoreOriginalSettings();
			}
		}
	}
}