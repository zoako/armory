using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
namespace ZKTools {
public class Utils {
	// String stuff
	public static string Timerify (float timer) {
		int displayMinutes = (int)(timer/60);
		int displaySeconds = (int)(timer)%60;
		string text = timer<0?"--:--":string.Format ("{0:00}:{1:00}", displayMinutes, displaySeconds);
		return text;
	}
	
	// Shorthand for unity stuff
	public static Rect RelRect(float left, float top, float width, float height) {
		return new Rect(0.01f*left*Screen.width, 0.01f*top*Screen.height, 0.01f*width*Screen.width, 0.01f*height*Screen.height);
	}
	
	public static Vector3 SetX(Vector3 input, float x) {
		return new Vector3(x, input.y, input.z);
	}
	public static Vector3 SetY(Vector3 input, float y) {
		return new Vector3(input.x, y, input.z);
	}

	public static Vector3 SetZ( Vector3 input, float z) {
		return new Vector3(input.x, input.y, z);
	}

	public static bool SetActive(GameObject obj, bool active) {
		if (obj.activeInHierarchy == active) return false;
		obj.SetActive(active);
		return true;
	}

	// Math operations
	public static int mod(int a, int b) {
		return a - Mathf.FloorToInt((float)a/b)*b;
	}
	
	public static int wrap(int val, int lbd, int ubd) {
		return lbd + mod(val-lbd, ubd+1-lbd);
	}
	
	public static float wrap(float val, float lbd = 0f, float ubd = 1f) {
		float wrapval = val/(ubd-lbd);
		return lbd + val - Mathf.Floor(wrapval)*(ubd-lbd);
	}
	
	public static float clamp(float val, float lbd = 0f, float ubd = 1f) {
		return Mathf.Max (lbd, Mathf.Min(val, ubd));
	}

	public static float Sum(float [] numbers) {
		float sum = 0;
		foreach (float f in numbers) sum += f;
		return sum;
	}
	
	public static int Sum(int [] numbers) {
		int sum = 0;
		foreach (int i in numbers) sum += i;
		return sum;
	}
	
	public static int Sum(bool [] bools) {
		int sum = 0;
		foreach (bool b in bools) if (b) sum++;
		return sum;
	}
	
	public static int indexOfMax(float [] inVals) {
		int maxIn = -1;
		for (int i = 0; i < inVals.Length; i++) {
			if (maxIn < 0 || inVals[i] > inVals[maxIn])
				maxIn = i;
		}
		return maxIn;
	}
	
	public static int indexOfMax(bool [] inVals) {
		for (int i = 0; i < inVals.Length; i++) {
			if (inVals[i]) return i;
		}
		return -1;
	}
	
	// Mesh tools
	public static float Area (Mesh m) {
		Vector3 [] mVertices = m.vertices;
		
		float result = 0;
		for(int p = mVertices.Length-1, q = 0; q < mVertices.Length; p = q++) {
			result += (Vector3.Cross(mVertices[q], mVertices[p])).magnitude;
		}
		
		result *= 0.5f;
		return result;		
	}

	public static Mesh CopyMesh(Mesh orginalMesh) {
		var newmesh = new Mesh {
			vertices = orginalMesh.vertices,
			triangles = orginalMesh.triangles,
			uv = orginalMesh.uv,
			normals = orginalMesh.normals,
			colors = orginalMesh.colors,
			tangents = orginalMesh.tangents
		};
		return newmesh;
	}
	
	// Not really very useful?
	public static bool NamespaceExists(string namespaceChecked) {
		var namespaceFound = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
		                      from type in assembly.GetTypes()
		                      where type.Namespace == namespaceChecked
		                      select type).Any();
		
		return (bool) namespaceFound;
	}	
}
}