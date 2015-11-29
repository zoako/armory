#if UNITY_EDITOR
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


//	[MenuItem("ZKTools/Focus Camera %t")]
	static void FocusCamera() {
    	GameObject target = Selection.activeObject as GameObject;
        Quaternion rotation = SceneView.lastActiveSceneView.rotation;
        Vector3 position = target.transform.position;
        SceneView.lastActiveSceneView.pivot = position;
        SceneView.lastActiveSceneView.rotation = rotation;
        SceneView.lastActiveSceneView.Repaint();
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
}
#endif