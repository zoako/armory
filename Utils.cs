//using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
namespace ZKTools {
public static class Utils {
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
	
	private class Edge {
		public int [] vertices;
		public int [] faces = new int[2]{-1, -1};

		public Edge(int i1, int i2) {
			vertices = new int[2]{i1, i2};
		}
		// If a face is already stored, makes just the second index the new face
		// If both faces are the same, that means edge piece
		public void AddFace(int f) {
			if (faces[0] == -1) faces[0] = f;
			faces[1] = f;
		}

		public bool SingleFaced() {
			return faces[0] != -1 && faces[0] == faces[1];
		}

		public override bool Equals(System.Object obj) {
			if ((object)obj == null) return false;
			Edge e1 = obj as Edge;
			return 	(e1.vertices[0] == vertices[0] && e1.vertices[1] == vertices[1]) ||
					(e1.vertices[0] == vertices[1] && e1.vertices[1] == vertices[0]);
		}

		public static bool operator == (Edge e1, Edge e2) {
			if ((object)e2 == null) return false;
			return e1.Equals(e2);
		}

		public static bool operator != (Edge e1, Edge e2) {
			if ((object)e2 == null) return false;
			return 	!(e1.vertices[0] == e2.vertices[0] && e1.vertices[1] == e2.vertices[1]) &&
					!(e1.vertices[0] == e2.vertices[1] && e1.vertices[1] == e2.vertices[0]);
		}

		public bool sharesVertex(Edge e2) {
			return 	vertices[0] == e2.vertices[0] || vertices[0] == e2.vertices[1] ||
					vertices[1] == e2.vertices[0] || vertices[1] == e2.vertices[1];
		}
		public override string ToString() {
			return "(" + vertices[0] + ", " + vertices[1] + ")";
		}
	}

	private static List<Edge> UniqueEdges(int[] triangles) {
		List<Edge> result = new List<Edge>();
		int tCount = triangles.Length/3;

		for (int f = 0; f < tCount; f++) {
			int i = f*3;
			Edge [] es = new Edge[3]{
				new Edge(triangles[i], triangles[i+1]),
				new Edge(triangles[i], triangles[i+2]),
				new Edge(triangles[i+1], triangles[i+2])};
			for (int q = 0; q < 3; q++) {
				Edge e = es[q];
				e.AddFace(f);
				if (!result.Contains(e)) {
					result.Add(e);
				} else result.Find(x => x==e).AddFace(f);
			}
		}
		return result;
	}

    /// <summary>
    /// Returns ordered vertices around edge and surface normals.
    /// Mark isStatic true to get transform scale, translation and rotation integrated in vertices
    /// </summary>
    /// <param name="target">Transform</param>
    /// <param name="isStatic">boolean</param>
	public static Dictionary<Vector3, Vector3> EdgeVertices(Transform target, bool isStatic = false) {
		Mesh m = target.GetComponent<MeshCollider>().mesh;
		// TODO: Verify that the mesh is open. Right now assume open

		// Clean out internal vertices: only singlefaced edges are at mesh edge
		List<Edge> outer = new List<Edge>();
		foreach (Edge e in UniqueEdges(m.triangles)) {
			if (e.SingleFaced()) {
				outer.Add(e);
			}
		}

		// Arranges the edges by continuity
		List<Edge> temp = new List<Edge>();
		temp.Add(outer.Pop(0));
		// Some duct-tapey error checks
		int passes = 0;
		int lastPassCount = outer.Count;
		while (outer.Count > 0) {
			for (int i = 0; i < outer.Count; i++) {
				int last = temp.Count - 1;
				if (temp[last].sharesVertex(outer[i])) {
					temp.Add(outer.Pop(i));
				}
			}
			// If we made no progress, flip the list, maybe we should add to the other end
			if (outer.Count == lastPassCount) {
				Debug.Log("Flipping because no progress");
				temp.Reverse();
				if (++passes > 2)  {
					Debug.LogError("Too many flips made, edge finding must have failed");
					return null;
				}
			}
			lastPassCount = outer.Count;
		}
		Dictionary<Vector3, Vector3> result = new Dictionary<Vector3, Vector3>();
		Debug.Log("Done Reordering");
		foreach (Edge e in temp) {
			foreach (int i in e.vertices) {
				Vector3 v = m.vertices[i];
				if (isStatic) v = 	target.position + 
									target.rotation*Vector3.Scale(target.localScale, v);
				if (!result.ContainsKey(v)) {
					result.Add(v, isStatic?target.rotation*m.normals[i]:m.normals[i]);
				}
			}
		}
		// Outline over the edges for debug
		List<Vector3> reskeys = result.Keys.ToList();
		for (int i = 0; i < result.Count-1; i++) {
			Debug.DrawLine(reskeys[i], reskeys [i+1], Color.red, 30f);
		}
		
		return result;
	}


	// Good for editor background colors
	public static Texture2D MakeTex(int width, int height, Color col){
        Color[] pix = new Color[width*height];
 
        for(int i = 0; i < pix.Length; i++)
            pix[i] = col;
 
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
 
        return result;
    }

	// Not really very useful?
	public static bool NamespaceExists(string namespaceChecked) {
		var namespaceFound = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
		                      from type in assembly.GetTypes()
		                      where type.Namespace == namespaceChecked
		                      select type).Any();
		
		return (bool) namespaceFound;
	}	
/*
	public static List<Vector3> EdgeVertices(Transform target, bool isStatic = false) {
		Mesh m = target.GetComponent<MeshCollider>().mesh;
		// TODO: Verify that the mesh is open. Right now assume open
		List<Vector3> temp = new List<Vector3>();

		// TODO: It would be more efficient to sort the edges in order 
		//  first and then take unique vertices from that ordered list

		// Clean out internal vertices: only singlefaced edges are at mesh edge
		foreach (Edge e in UniqueEdges(m.triangles)) {
			if (e.SingleFaced()) {
				foreach (int v in e.vertices) {
					if (!temp.Contains(m.vertices[v])) {
						Vector3 vToAdd = isStatic?
										(target.position + target.rotation*m.vertices[v]):
										m.vertices[v];
						temp.Add(vToAdd);
					}
				}
			}
		}

		List<Vector3> result = new List<Vector3>();
		result.Add(temp[0]);
		temp.RemoveAt(0);
		// This bit is a bit expensive -- Can we optimize somehow?
		while (temp.Count > 0) {
			temp = temp.OrderBy(v => Vector3.Distance(v, result[result.Count-1])).ToList();
			result.Add(temp[0]);
			temp.RemoveAt(0);
		}
		
		// Outline over the edges
		for (int i = 0; i < result.Count-1; i++) {
			Debug.DrawLine(result[i], result [i+1], Color.red, 30f);
		}

		// Sort the vertices by adjacency
		return result;
	}
*/
}

static class Extensions
{
	public static T Pop<T>(this IList<T> listToPop, int indexToPop) {
		T val = listToPop[indexToPop];
		listToPop.RemoveAt(indexToPop);
		return val;
	}
}
}