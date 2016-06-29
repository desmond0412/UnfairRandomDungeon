using UnityEngine;
using System.Collections;

namespace Artoncode.Core {
	public class SmoothFollow : MonoBehaviour {
		private Vector3 velocity = Vector3.zero;
		private float scaleVelocity;
		private Vector3 vLookAt = Vector3.zero;
		private Vector3 cLookAt = Vector3.zero;
		public Transform target;
		public bool lookAtTarget;
		public bool smoothLookAtTarget = true;
		public Vector3 lookAtOffset;
		public float smoothTime = 0.3f;
		public Vector3 positionOffset = Vector3.zero;
		public float scale = 1f;
		public Vector3 min = Vector3.one * float.NegativeInfinity;
		public Vector3 max = Vector3.one * float.PositiveInfinity;
		private Vector3 cPosition;
		private bool isShaking;
		private float shakeOffset;
		private float shakeVelocity;

		private bool isLocked = false;
		public bool IsLocked{
			get;set;
		}

		void Awake () {
			cPosition = transform.position;
		}
		public Vector3 LookAt
		{
			set{
				cLookAt = value;
			}
		}

		public void Reinit()
		{
			if(target != null) {
				if (lookAtTarget) {
					cLookAt = target.position + lookAtOffset;
					transform.LookAt(cLookAt);
					cPosition = transform.position;
				}
			}
		}

		void FixedUpdate () {
			if(target != null) {
				Vector3 tPos = Vector3.zero;
				if (target) {
					tPos = target.position;
				}

				if (scale <= 0)
					scale = 0.1f;
				float x = Mathf.SmoothDamp (cPosition.x, tPos.x + positionOffset.x / scale, ref velocity.x, smoothTime);
				float y = Mathf.SmoothDamp (cPosition.y, tPos.y + positionOffset.y / scale, ref velocity.y, smoothTime);
				float z = Mathf.SmoothDamp (cPosition.z, tPos.z + positionOffset.z / scale, ref velocity.z, smoothTime);

				x = Mathf.Min (Mathf.Max (x, min.x), max.x);
				y = Mathf.Min (Mathf.Max (y, min.y), max.y);
				z = Mathf.Min (Mathf.Max (z, min.z), max.z);

				transform.position = new Vector3 (x, y, z);

				float s = Mathf.SmoothDamp (transform.localScale.x, scale, ref scaleVelocity, smoothTime);
				transform.localScale = Vector3.one * s;

				if (lookAtTarget) {
					if (!smoothLookAtTarget) {
						cLookAt = tPos + lookAtOffset;
						transform.LookAt(cLookAt);
						smoothLookAtTarget = true;
					}
					else {
						cLookAt = Vector3.SmoothDamp (cLookAt, tPos + lookAtOffset, ref vLookAt, smoothTime);
						transform.LookAt(cLookAt);
					}
				}
				cPosition = transform.position;
			}

			if (isShaking) {
				shakeOffset -= shakeVelocity * Time.deltaTime;
				if (shakeOffset <= 0) {
					shakeOffset = 0;
					isShaking = false;
				}
				transform.position = cPosition + Random.onUnitSphere * shakeOffset;
			}
		}

		public void shake (float shakeOffset, float shakeDuration) {
			this.shakeOffset = shakeOffset;
			this.shakeVelocity = shakeOffset / shakeDuration;
			isShaking = true;
		}
	}

}