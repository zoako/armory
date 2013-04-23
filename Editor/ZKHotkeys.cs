using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

public class ZKHotkeys : Editor {
	
	/* Initialize will build the basic folder structure, and as we progress will load any
	 * assets that ZoaKo games will use in the future */
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
	
	[MenuItem("ZKTools/Initialize %i")]
	static void zk_initialize() {
		Debug.Log("Initialized ZK Editor Tools!");
		EditorWindow.GetWindow(typeof(ZKHotkeys));
	}
	
	[MenuItem("ZKTools/Clear Console &x")]
	static void zk_clear_console() {
		Debug.Log("Executing Console Clear");
		//Debug.ClearDeveloperConsole();
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
		Debug.Log("Create a cube");
		EditorApplication.ExecuteMenuItem("GameObject/Create Other/Cube");
	}
	
	[MenuItem("ZKTools/Create Sphere &s")]
	static void zk_create_sphere() {
		Debug.Log("Create a sphere");
		EditorApplication.ExecuteMenuItem("GameObject/Create Other/Sphere");
	}
	
	[MenuItem("ZKTools/Create Directional Light &d")]
	static void zk_create_d_light() {
		Debug.Log("Create a directional light");
		EditorApplication.ExecuteMenuItem("GameObject/Create Other/Directional Light");
	}
	
	[MenuItem("ZKTools/Edit Physics &p")]
	static void zk_edit_physics() {
		EditorApplication.ExecuteMenuItem("Edit/Project Settings/Physics");
	}
	[MenuItem("ZKTools/Show Editor APM _?")]
	static void zk_show_APM_view() {		
		showAPM = !showAPM;
		Debug.Log((showAPM?"Opened":"Closed") +" the APM Window");
	}

}
