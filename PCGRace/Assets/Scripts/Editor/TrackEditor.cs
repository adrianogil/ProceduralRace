using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TrackGeneration))]
public class TrackEditor : Editor {

	private TrackGeneration trackGeneration;

	public override void OnInspectorGUI() {

		trackGeneration = target as TrackGeneration;

		DrawDefaultInspector();

		if (GUILayout.Button("Generate Track")) {
			trackGeneration.GenerateTrack();
		}
	}	

}
