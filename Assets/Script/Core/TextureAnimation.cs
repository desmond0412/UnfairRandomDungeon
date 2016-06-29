using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureAnimation : MonoBehaviour {
	public enum TextureAnimationLoopType {
		Loop,
		Once,
		PingPong,
		OnceAndDestroy,
	}

	[System.Serializable]
	public class TextureAnimationGroup {
		public string name;
		public int startFrame;
		public int endFrame;
		public TextureAnimationLoopType type;
	}

	public float fps;
	public string variableName;
	public int row;
	public int col;
	public List<TextureAnimationGroup> animationList;
	public bool debug;
	private TextureAnimationGroup currentAnimation;
	private int currentFrame;
	private float timeUntilNextFrame;
	private int animationDirection;

	private Material mat;
	private Vector2 offset;

	void Start () {
		if (animationList.Count > 0) {
			currentAnimation = animationList[0];
		}
		currentFrame = 0;
		timeUntilNextFrame = 1 / fps;
		animationDirection = 1;

		mat = GetComponent<Renderer> ().material;
		offset = mat.GetTextureOffset(variableName);
	}

	void Update () {
		if (currentAnimation != null) {
			while (timeUntilNextFrame < 0) {
				currentFrame += animationDirection;
				if ((currentAnimation.startFrame + currentFrame > currentAnimation.endFrame) ||
				    (currentFrame < 0)) {
					switch (currentAnimation.type) {
					case TextureAnimationLoopType.Once:
						currentFrame = currentAnimation.endFrame - currentAnimation.startFrame;
						break;
					case TextureAnimationLoopType.Loop:
						currentFrame = 0;
						break;
					case TextureAnimationLoopType.PingPong:
						if (animationDirection == 1) {
							currentFrame = currentAnimation.endFrame - 1;
							animationDirection = -1;
						}
						else {
							currentFrame = currentAnimation.startFrame + 1;
							animationDirection = 1;
						}
						break;
					case TextureAnimationLoopType.OnceAndDestroy:
					
						break;
					}
				}
				timeUntilNextFrame += 1 / fps;
			}

			int f = currentAnimation.startFrame + currentFrame;
			int x = f % col;
			int y = row - f / col - 1;
			offset.x = x * (1f / col);
			offset.y = y * (1f / row);

			mat.SetTextureOffset (variableName, offset);

			timeUntilNextFrame -= Time.deltaTime;
		}
	}

	public void setAnimation(string name) {
		if (name != null) {
			foreach (TextureAnimationGroup anim in animationList) {
				if (anim.name == name) {
					currentAnimation = anim;
					currentFrame = 0;
					timeUntilNextFrame = 1 / fps;
					animationDirection = 1;
				}
			}
		}
	}

	void OnGUI () {
		if (debug) {
			for (int i=0; i<animationList.Count; i++) {
				if (GUI.Button (new Rect(Screen.width - 110, i * 35 + 10, 100, 30), animationList[i].name)) {
					setAnimation (animationList[i].name);
				}
			}
		}
	}
}
