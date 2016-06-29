using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Artoncode.Core.Utility;

namespace Artoncode.Core {

	public class SwipeGamepad {
		public static readonly int HOLD = 0;
		public static readonly int IDLE = -1;

		public enum ScaleType {
			Absolute,
			RelativeToWidth,
			RelativeToHeight,
			Resolution
		};

		private class Gesture {
			public float min;
			public float max;
			public int value;
			public bool repeat;
			public int fallbackValue;

			public Gesture (float min, float max, int value, bool repeat, int fallbackValue) {
				this.min = min;
				this.max = max;
				this.value = value;
				this.repeat = repeat;
				this.fallbackValue = fallbackValue;
			}
		};

		private Vector2 adjustedCenter;
		private int lastGestureValue;
		private float lastAngle;
		private float radius;
		private List<Gesture> gestures;

		public GUITexture visualUI;
		public bool invertX;

		/// <summary>
		/// Initializes a new instance of the <see cref="Artoncode.Core.SwipeGamepad"/> class.
		/// </summary>
		/// <param name="scaleType">Scale type.</param>
		/// <param name="radius">Radius.</param>
		public SwipeGamepad(ScaleType scaleType, float radius) {
			switch(scaleType) {
			case ScaleType.Absolute:
				this.radius = radius;
				break;
			case ScaleType.RelativeToWidth:
				this.radius = radius * Screen.width;
				break;
			case ScaleType.RelativeToHeight:
				this.radius = radius * Screen.height;
				break;
			case ScaleType.Resolution:
				this.radius = GameUtility.inchToPixel(radius);
				break;
			}
			gestures = new List<Gesture> ();
			invertX = false;
		}

		/// <summary>
		/// Detects a gesture based on a touch.
		/// </summary>
		/// <returns>The detected gesture.</returns>
		/// <param name="touch">Touch.</param>
		public int detectGesture(TouchInput touch) {
			if (touch.phase == TouchPhase.Began) {
				lastGestureValue = HOLD;
				adjustedCenter = touch.start;
				updateUI();
			}
			else {
				Vector2 relativePositionFromCenter = touch.end - adjustedCenter;
				float distanceFromCenter = relativePositionFromCenter.magnitude;
				if (distanceFromCenter > radius) {
					adjustCenter(touch.end);
					updateUI();
					float angle = Mathf.Atan2(relativePositionFromCenter.x, relativePositionFromCenter.y) * Mathf.Rad2Deg;
					if (angle < 0) {
						angle = 360 + angle;
					}
					if (invertX) {
						angle = 360 - angle;
					}
					Gesture gesture = getGesture(angle);
					if (gesture != null) {
						if (!gesture.repeat) {
							if (lastGestureValue == HOLD) {
								lastGestureValue = gesture.fallbackValue;
							}
							adjustedCenter = touch.end;
							updateUI();
							return gesture.value;
						}
						lastGestureValue = gesture.value;
					}
				}
			}
			return lastGestureValue;
		}

		/// <summary>
		/// Detects the angle based on a touch.
		/// </summary>
		/// <returns>The detected angle.</returns>
		/// <param name="touch">Touch.</param>
		public float detectAngle(TouchInput touch) {
			if (touch.phase == TouchPhase.Began) {
				lastAngle = -1f;
				adjustedCenter = touch.start;
				updateUI();
			}
			else {
				Vector2 relativePositionFromCenter = touch.end - adjustedCenter;
				float distanceFromCenter = relativePositionFromCenter.magnitude;
				if (distanceFromCenter > radius) {
					adjustCenter(touch.end);
					updateUI();
					float angle = Mathf.Atan2(relativePositionFromCenter.x, relativePositionFromCenter.y) * Mathf.Rad2Deg;
					if (angle < 0) {
						angle = 360 + angle;
					}
					if (invertX) {
						angle = 360 - angle;
					}
					lastAngle = angle;
				}
			}
			return lastAngle;
		}

		/// <summary>
		/// Register a gesture based on its angle.
		/// The angle is a positive value between 0 to 360 degree, starting at 12 o'clock clockwise.
		/// </summary>
		/// <param name="minAngle">Minimum angle threshold, inclusive.</param>
		/// <param name="maxAngle">Maximum angle threshold, exclusive.</param>
		/// <param name="gestureValue">An integer value that will be returned when this gesture happen. 0 is reserved for default touch and hold gesture. -1 is reserved for default fallback.</param>
		/// <param name="repeat">Should the gesture repeated if the user is holding the touch?</param>
		/// <param name="fallbackValue">For non-repeating gesture, if this is the first gesture done, the next detected gesture will fallback to this gesture</param>
		public void registerGesture (float minAngle, float maxAngle, int gestureValue, bool repeat=true, int fallbackValue=-1) {
			gestures.Add (new Gesture (minAngle, maxAngle, gestureValue, repeat, fallbackValue));
		}

		/// <summary>
		/// Gets the tolerance.
		/// </summary>
		/// <returns>The tolerance.</returns>
		public float getRadius() {
			return radius;
		}

		/// <summary>
		/// Gets the gesture of specified angle
		/// </summary>
		/// <returns>The gesture of the specified angle.</returns>
		/// <param name="angle">Angle.</param>
		private Gesture getGesture(float angle) {
			foreach (Gesture gesture in gestures) {
				if (gesture.min < gesture.max) {
					if (angle >= gesture.min && angle < gesture.max) {
						return gesture;
					}
				}
				else {
					if (angle >= gesture.min || angle < gesture.max) {
						return gesture;
					}
				}
			}
			return null;
		}

		private void adjustCenter(Vector2 currentTouchPosition) {
			adjustedCenter = currentTouchPosition + (adjustedCenter - currentTouchPosition).normalized * radius;
		}

		private void updateUI() {
			if (visualUI) {
				visualUI.transform.position = new Vector3(adjustedCenter.x/Screen.width,adjustedCenter.y/Screen.height,visualUI.transform.position.z);
			}
		}
	}

}