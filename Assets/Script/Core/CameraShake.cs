using UnityEngine;
using System.Collections;

namespace Artoncode.Core
{
	public class CameraShake : MonoBehaviour
	{
		private Vector3 originPosition;
		private Quaternion originRotation;
		private float shake_decay;
		private float shake_intensity;
		private bool isShaking;
		
		void Update ()
		{
			if (isShaking) {
				transform.position = originPosition + Random.insideUnitSphere * shake_intensity;
				transform.rotation = new Quaternion (
					originRotation.x + Random.Range (-shake_intensity, shake_intensity) * .2f,
					originRotation.y + Random.Range (-shake_intensity, shake_intensity) * .2f,
					originRotation.z + Random.Range (-shake_intensity, shake_intensity) * .2f,
					originRotation.w + Random.Range (-shake_intensity, shake_intensity) * .2f);
				shake_intensity -= shake_decay;
				
				if (shake_intensity <= 0) {
					isShaking = false;
					transform.localPosition = originPosition;
					transform.localRotation = originRotation;
				}
			}
		}
		
		public void Shake (float Strength)
		{
			if (isShaking)
				return;
			
			originPosition = transform.position;
			originRotation = transform.rotation;
			
			isShaking = true;
			
			shake_intensity = 0.03f * Strength;
			shake_decay = 0.0002f * Strength;
		}
		
	}
}