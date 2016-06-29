using UnityEngine;  
using UnityEditor;  
using UnityEditorInternal;

[CustomEditor(typeof(TextureAnimation))]
public class TextureAnimationEditor : Editor {  
	private ReorderableList list;
	
	private void OnEnable() {
		list = new ReorderableList(serializedObject, 
		                           serializedObject.FindProperty("animationList"), 
		                           true, true, true, true);

		list.drawElementCallback =  
		(Rect rect, int index, bool isActive, bool isFocused) => {
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;
			EditorGUI.PropertyField(
				new Rect(rect.x, rect.y, rect.width - 130, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("name"), GUIContent.none);
			EditorGUI.PropertyField(
				new Rect(rect.x + rect.width - 120, rect.y, 30, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("startFrame"), GUIContent.none);
			EditorGUI.PropertyField(
				new Rect(rect.x + rect.width - 90, rect.y, 30, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("endFrame"), GUIContent.none);
			EditorGUI.PropertyField(
				new Rect(rect.x + rect.width - 50, rect.y, 50, EditorGUIUtility.singleLineHeight),
				element.FindPropertyRelative("type"), GUIContent.none);
		};
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		TextureAnimation myTarget = (TextureAnimation)target;

		myTarget.variableName = EditorGUILayout.TextField ("ID", myTarget.variableName);
		myTarget.fps = EditorGUILayout.FloatField ("FPS", myTarget.fps);
		myTarget.row = EditorGUILayout.IntField ("Row", myTarget.row);
		myTarget.col = EditorGUILayout.IntField ("Col", myTarget.col);
		list.DoLayoutList();
		myTarget.debug = EditorGUILayout.Toggle ("Debug", myTarget.debug);
		serializedObject.ApplyModifiedProperties();
	}
}