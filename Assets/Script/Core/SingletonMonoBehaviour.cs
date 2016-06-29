using UnityEngine;
using System.Collections;

namespace Artoncode.Core
{
	public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T _instance;

		public virtual void Awake () {
			if (_instance == null) {
				_instance = this.GetComponent<T> ();
			}
			else {
				Debug.LogWarning("There is already another instance of " + this.GetType () + ", you will not be able to access this instance using singleton");
			}
		}

		public static T shared ()
		{
//			if (_instance == null) {
//				T[] objs = (T[])FindObjectsOfType (typeof(T));
//				if (objs.Length > 0) {
//					_instance = objs [0];
//				} else {
//					if (_instance == null)
//					{
////						Debug.LogWarning("There should be only 1 instance of in each scene");
////						_instance = (new GameObject(typeof(T).ToString())).AddComponent<T>();
//					}
//					return _instance;
//				}
//			}
			if (_instance != null &&_instance.enabled && _instance.gameObject.activeInHierarchy) {
				return _instance;
			}
			else {
				return null;
			}
		}

	}
	
}