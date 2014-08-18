#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.Collections;

public static class ParticlePropertyGetter {

	// Pass in module, then property name
	public static SerializedProperty getProperty(	this SerializedObject so, 
													params string [] propParts) {
		SerializedProperty prop = so.GetIterator();
		string propName = String.Join(".", propParts);
		while (prop.Next(true) && prop.propertyPath != propName) {
			//Debug.Log(prop.propertyPath + ": " + prop.propertyType);
		}
		return prop;
	}
}

#endif