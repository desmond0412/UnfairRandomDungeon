using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Artoncode.Core.Utility {
	public struct SystemTime
	{
		static string[] _mmm = new string[]{"","Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"};
		static string[] _DDD = new string[]{"SUN","MON","TUE","WED","THU","FRI","SAT"};
		
		public ushort Year;
		public ushort Month;
		public ushort DayOfWeek;
		public ushort Day;
		public ushort Hour;
		public ushort Minute;
		public ushort Second;
		public ushort Millisecond;
		public string mmm {
			get {
				return _mmm [Month];
			}
		}
		public string DDD{
			get {
				return _DDD [DayOfWeek];
			}
		}

		public static SystemTime Now()
		{
			SystemTime time = new SystemTime ();
			SystemTimeUtility.GetSystemTime (ref time);
			return time;
		}

	
		public string GetSystemTimeFormat()
		{
			string timeFormat = "";
			timeFormat += Day.ToString("D2") + "/";
			timeFormat += mmm + "/";
			timeFormat += Year + " ";
			timeFormat += DDD + " ";
			timeFormat += Hour.ToString("D2") + ":";
			timeFormat += Minute.ToString("D2") + ":";
			timeFormat += Second.ToString("D2");
			return timeFormat;
		}

		public override string ToString()
		{
			return GetSystemTimeFormat ();
		}
	};

	public class SystemTimeUtility {

		#if !UNITY_EDITOR_OSX
		[DllImport("kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true)]
		public extern static void GetSystemTime(ref SystemTime sysTime);
		
		[DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
		public extern static bool SetSystemTime(ref SystemTime sysTime);
		#else
		public static void GetSystemTime(ref SystemTime sysTime)
		{
			sysTime = SystemTime.Now ();
		}

		public static void SetSystemTime(ref SystemTime sysTime)
		{
//			System.DateTime.
//			sysTime = SystemTime.Now ();
		}


		#endif

		public static void printSystemTime () {
			SystemTime stime = new SystemTime();
			GetSystemTime(ref stime);
			Debug.Log (stime.Hour+":"+stime.Minute+":"+stime.Second);
		}

		public static void addOneHour () {
			SystemTime stime = new SystemTime();
			GetSystemTime(ref stime);
			stime.Hour += 1;
			SetSystemTime (ref stime);
		}
	}
}