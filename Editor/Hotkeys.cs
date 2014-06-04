using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using ZKTools;

public class Hotkeys : Editor {
	[MenuItem ("ZKTools/Sync Code %#r")]
	static void ReSyncProject() {
		zk_clear_console ();
		EditorApplication.ExecuteMenuItem("Assets/Refresh");
	}

	[MenuItem("ZKTools/Clear Console &x")]
	static void zk_clear_console() {
		Debug.Log("Executing Console Clear [You shouldn't see this message]");
		// Debug.ClearDeveloperConsole(); //does not do anything!
		ZKDebug.Clear();

		Assembly assembly = Assembly.GetAssembly(typeof(SceneView)); 
		Type type = assembly.GetType("UnityEditorInternal.LogEntries");
		MethodInfo method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}

	[MenuItem("ZKTools/Create C# script &#3")]
	static void zk_create_cscript() {
		EditorApplication.ExecuteMenuItem("Assets/Create/C# Script");
	}

	[MenuItem("ZKTools/Create Cube &a")]
	static void zk_create_cube() {
		Debug.Log("Creating cube");
		EditorApplication.ExecuteMenuItem("GameObject/Create Other/Cube");
	}

	[MenuItem("ZKTools/Create Quad &s")]
	static void zk_create_sphere() {
		Debug.Log("Creating quad");
		EditorApplication.ExecuteMenuItem("GameObject/Create Other/Quad");
	}

	[MenuItem("ZKTools/Create Point Light &d")]
	static void zk_create_d_light() {
		Debug.Log("Creating Point light");
		EditorApplication.ExecuteMenuItem("GameObject/Create Other/Point Light");
	}

	[MenuItem("ZKTools/Edit Physics &p")]
	static void zk_edit_physics() {
		EditorApplication.ExecuteMenuItem("Edit/Project Settings/Physics");
	}

	[MenuItem ("ZKTools/Activate\\Deactivate selected objects %#d")]
	static void ChangeActivate() {
		foreach (GameObject go in Selection.gameObjects) {
			go.SetActive(!go.activeInHierarchy);
		}
	}

	[MenuItem ("ZKTools/Apply changes to all selected prefabs %&a")]
	static void MassPrefabApply() {
		foreach (GameObject go in Selection.gameObjects) {
			PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go),
						ReplacePrefabOptions.ConnectToPrefab);
		}
		Debug.Log("Applied to " + Selection.gameObjects.Length + " game object prefabs");
	}

	[MenuItem("ZKTools/Flip Normals %#i")]
	static void NormalReversedMesh () {
		Mesh filter = Selection.activeObject as Mesh;
		if (filter != null) {
			string path = Directory.GetParent(AssetDatabase.GetAssetPath(filter)).ToString() + "/";
			Mesh mesh = Utils.CopyMesh(filter);

			Vector3[] normals = mesh.normals;
			for (int i=0;i<normals.Length;i++)
			normals[i] = -normals[i];
			mesh.normals = normals;

			for (int m=0;m<mesh.subMeshCount;m++) {
				int[] triangles = mesh.GetTriangles(m);
				for (int i=0;i<triangles.Length;i+=3) {
					int temp = triangles[i + 0];
					triangles[i + 0] = triangles[i + 1];
					triangles[i + 1] = temp;
				}
				mesh.SetTriangles(triangles, m);
			}
			AssetDatabase.CreateAsset(mesh, path + filter.name + "_inverted.asset");
			Debug.Log("Success. Inverted mesh placed in " + path);
		} else {
			Debug.Log ("Failed. Invalid selection for inversion");
		}
	}

	/*
	// Experimental, trying to get APM while in editor. Work faster!
	static bool showAPM = false;
	static int totalActions = 0;

	 void OnGUI() {
	 	if (showAPM) Debug.Log("Showing APM View");
	 	GUILayout.Label("Total actions performed this session", EditorStyles.boldLabel);
	 	EditorGUILayout.TextField("0", String.Format("{0000}", totalActions));

	 }

	 void Update() {
	 	Debug.Log("New action!");
	 	if (Event.current.type == EventType.MouseUp) {
	 		Debug.Log("New action!");
	 		totalActions++;
	 	}
	 }
	[MenuItem("ZKTools/Show Editor APM _?")]
	static void zk_show_APM_view() {		
		showAPM = !showAPM;
		Debug.Log((showAPM?"Opened":"Closed") +" the APM Window");
	}
	*/
}
