using UnityEngine;
using System.Collections;

namespace Artoncode.Core {
	
	public class SmoothFollow2D : MonoBehaviour {
		private Transform cTransform;
		
		private Vector2 velocity = new Vector2();
		public Transform target;
		public float smoothTime = 0.3f;
		public Vector2 positionOffset = Vector2.zero;
		
		void Start(){
			cTransform = transform;
		}
		
		void Update(){
			if (target) {
				float x = Mathf.SmoothDamp(cTransform.position.x, target.position.x+positionOffset.x, ref velocity.x, smoothTime);
				float y = Mathf.SmoothDamp(cTransform.position.y, target.position.y+positionOffset.y, ref velocity.y, smoothTime);
				cTransform.position = new Vector3(x, y, cTransform.position.z);
			}
		}
	}

}