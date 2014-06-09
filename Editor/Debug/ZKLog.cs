using UnityEngine;
//using UnityEditor;
using System.Collections;
using ZKTools;

public class Log {
	public string message;
	public string stackTrace;
	public LogType type;

	public Log(string msg, string stack, LogType t) {
		message = msg;
		type = t;
		if (stack.Contains("UnityEngine.Debug:Log(Object)")) {
			stackTrace = stack.Substring(30);
		}
		calculateLines (800);
	}

	private int lines = 0;
	public int calculateLines(float width) {
		lines = 0;
		int lineChars = (int)(width/10);
		foreach (string s in message.Split ('\n')) {
			lines += Mathf.Max (s.Length/lineChars, 1);
		}
		foreach (string s in stackTrace.Split ('\n')) {
			lines += Mathf.Max (s.Length/lineChars, 1);
		}
		return lines;
	}

	public int getLines() {
		return lines;
	}

}

