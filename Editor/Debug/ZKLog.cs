using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using ZKTools;

public class Log {
	public string message;
	public string stackTrace;
	public LogType type;

	public Log(string msg, string stack, LogType t, float winWidth) {
		message = msg;
		type = t;
		if (stack.Contains("UnityEngine.Debug:Log(Object)")) {
			stackTrace = stack.Substring(30);
		}
		calculateLines (winWidth);
	}

	public List<string> messageLines = new List<string>();
	public List<string> stackLines = new List<string>();
	public int calculateLines(float width) {
		int lineChars = (int)(width/10);
		foreach (string s in message.Split ('\n')) {
			messageLines.Add(s);
		}
		
		foreach (string s in stackTrace.Split ('\n')) {
			stackLines.Add(s);
		}
		return messageLines.Count + stackLines.Count;
	}

	public List<string> getAllLines() {
		return (List<string>)(messageLines.Concat(stackLines).ToList());
	}


}

