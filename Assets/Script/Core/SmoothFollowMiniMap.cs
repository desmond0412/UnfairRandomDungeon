using UnityEngine;
using System.Collections;
using Artoncode.Core;

namespace Artoncode.Core {
	
	public class SmoothFollowMiniMap : MonoBehaviour {
		public Transform target;

		void Update(){
			if (target) 
			{
				transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
			}
		}
	}

}