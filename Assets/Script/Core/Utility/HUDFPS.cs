using UnityEngine;

namespace Artoncode.Core.Utility {

	public class HUDFPS : MonoBehaviour 
	{
		public  float updateInterval = 0.5F;
		
		private float _accum; // FPS accumulated over the interval
		private int   _frames; // Frames drawn over the interval
		private float _timeleft; // Left time for current interval
		private string _format;

		void Start()
		{
			_timeleft = updateInterval;  
		}

		void OnGUI () {
			_timeleft -= Time.deltaTime;
			_accum += Time.timeScale/Time.deltaTime;
			++_frames;
			
			// Interval ended - update GUI text and start new interval
			if( _timeleft <= 0.0 )
			{
				// display two fractional digits (f2 format)
				float fps = _accum/_frames;
				_format = string.Format("{0:F2} FPS",fps);

				_timeleft = updateInterval;
				_accum = 0.0F;
				_frames = 0;
			}
#if !UNITY_EDITOR
			GUI.Box (new Rect(10,10,100,25), _format);
#endif
		}
	}

}