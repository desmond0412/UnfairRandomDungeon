using UnityEngine;
using System.Collections;

using Artoncode.Core;

public class FOVController : MonoBehaviour {

	private float visibleHeight;
	private SmoothFollow cam;

	private float angle;
	private float yOffset;
	private float currentFOV;

	// Use this for initialization
	void Start () {
		cam = Camera.main.GetComponent<SmoothFollow>();
		angle = Camera.main.transform.rotation.eulerAngles.x;
		yOffset = cam.positionOffset.y;
		currentFOV = Camera.main.fieldOfView;
		visibleHeight = Mathf.Tan(Mathf.Deg2Rad*currentFOV/2)*2*Mathf.Abs(cam.positionOffset.z);
	}

	void OnGUI() {
		GUI.Box(new Rect(20, Screen.height-105, 50, 25), "Angle");
		angle = GUI.HorizontalSlider(new Rect(80, Screen.height-100, Screen.width-180, 25), angle, -60f, 60f);
		GUI.Box(new Rect(Screen.width-90, Screen.height-105, 80, 25), angle+"");

		GUI.Box(new Rect(20, Screen.height-70, 50, 25), "Y");
		yOffset = GUI.HorizontalSlider(new Rect(80, Screen.height-65, Screen.width-180, 25), yOffset, -20f, 20f);
		GUI.Box(new Rect(Screen.width-90, Screen.height-70, 80, 25), yOffset+"");

		GUI.Box(new Rect(20, Screen.height-35, 50, 25), "FOV");
		currentFOV = GUI.HorizontalSlider(new Rect(80, Screen.height-30, Screen.width-180, 25), currentFOV, 1.0f, 90.0f);
		GUI.Box(new Rect(Screen.width-90, Screen.height-35, 80, 25), currentFOV+"");

		Vector3 currentAngle = Camera.main.transform.rotation.eulerAngles;
		currentAngle.x = angle;
		Camera.main.transform.rotation = Quaternion.Euler(currentAngle);

		cam.positionOffset.y = yOffset;

		Camera.main.fieldOfView = currentFOV;
		float d = Mathf.Sign(cam.positionOffset.z) * visibleHeight / (Mathf.Tan(Mathf.Deg2Rad*currentFOV/2)*2);
		cam.positionOffset.z = d;
		Vector3 currentPos = Camera.main.transform.position;
		currentPos.z = d;
		Camera.main.transform.position = currentPos;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
