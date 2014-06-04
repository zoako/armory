using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ZKTools;

public class ZKDebug : EditorWindow {

	private static ZKDebug window = null;
	static bool visible = false;

	// Public Usage Calls
	[MenuItem("ZKTools/ZKDebug Console #%w")]
	static void CreateOrReattach () {
		if (window == null) window = (ZKDebug) EditorWindow.GetWindow(typeof(ZKDebug));
		visible = true;

		Application.RegisterLogCallback(window.HandleLog);
		window.needsReattachment = false;
		window.InitializeConsole();
		
		Debug.Log("Reattached at " + System.DateTime.Now.TimeOfDay) ;	
	}

	void InitializeConsole() {
		window.attemptedScroll = false;
		window.scrolledDown = false;
		window.newLogArrived = false;
		window.eventsWaited = 0;
	}

	public static void Clear() {
		if (window != null) {
			window.logs.Clear();
			window.Repaint();
		}
	}

	void RenderLog(Log log) {
		EditorGUILayout.SelectableLabel (log.message + "\n" + log.stackTrace, 
		                                 GUILayout.Height (logHeight));
		GUILayout.Box("", GUILayout.Height(2), GUILayout.Width(position.width-10f));
	}
	

	// Set a higher debug level to catch more debug statements
	private static int debugLevel = 10;
	public static void setDebugLevel(int level) {
		debugLevel = Mathf.Max (0, level);
	}
	
	static bool consoleDevMode = true;
	static float devLabelHeight = 30;

	// Visual elements:
	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
	{
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};

	// Layout properties and settings
	protected static int logHeight = 42;
	protected static int eventsSinceScrollToWait = 10;

	// Properties we care about
	readonly List<Log> logs = new List<Log>();
	bool collapse;

	// Properties used for managing scroll position
	protected Vector2 scrollPosition;

	// These are pretty much private to the autoscroll system
	float scrollBottom = 0;
	bool attemptedScroll = false;
	bool scrolledDown = false;
	bool newLogArrived = false;
	int eventsWaited;

	private bool IsNearBottom() {
		return (scrollPosition.y > (scrollBottom - (logHeight + 10f)));
	}
	
	Vector2 BeginAutoScrollView (params GUILayoutOption[] options) {
		// Figuring out scroll-mode
		scrollBottom = Mathf.Max (logs.Count * (logHeight + 2f) - position.height + devLabelHeight + 10f, 0f);
		bool scrollToBottom = 	!(attemptedScroll && !scrolledDown) && 		// DONT, scrolled up
			IsNearBottom() && (newLogArrived ||			// DO, looking at last and new log is here
			                   (attemptedScroll && scrolledDown));			// DO, scrolling down near the bottom
		if (scrollToBottom) 
			return EditorGUILayout.BeginScrollView(new Vector2(0, scrollBottom), options);
		else 
			return EditorGUILayout.BeginScrollView(scrollPosition, options);
	}
	
	void EndAutoScrollView () {
		// Manipulating events
		if (Event.current.type == EventType.scrollWheel) {
			attemptedScroll = true;
			scrolledDown = Event.current.delta.y > 0;
			eventsWaited = 0;
		} else {
			if (eventsWaited > eventsSinceScrollToWait) attemptedScroll = false;
			else eventsWaited++;
		}
		newLogArrived = false;
		EditorGUILayout.EndScrollView();
	}

	void RenderAllLogs() {
		GUIContent [] content = new GUIContent[logs.Count];
		int i = 0;
		foreach (Log l in logs) {
			content[i] = new GUIContent(l.message);
			i++;
		}
		GUILayout.SelectionGrid(0, content, 1);
	}

	protected void OnGUI () {
		scrollPosition = BeginAutoScrollView (GUILayout.MinHeight(position.height - devLabelHeight - 10f));
		// Drawing
		RenderAllLogs();
		/*
		foreach (Log log in logs) {
			RenderLog (log);
		}
		*/
		EndAutoScrollView();
		
		if (consoleDevMode) {
			GUILayout.Label ("ZKDebug; atBottom: " + IsNearBottom() + "\nscroll: " + scrollBottom + " position: " + scrollPosition.y + " height: " + position.height, 
			                 EditorStyles.boldLabel,
			                 GUILayout.MinHeight(devLabelHeight),
			                 GUILayout.MaxHeight(devLabelHeight));
		}	
	}

	void HandleLog (string message, string stackTrace, LogType type) {
		logs.Add(new Log (message, stackTrace, type));
		newLogArrived = true;
		Repaint ();
	}

	int lastState = 0;
	private bool needsReattachment = false;
	void Update() {
		int thisState = EditorApplication.isCompiling?1:
						EditorApplication.isUpdating?2:
						EditorApplication.isPlaying?3:0;
		needsReattachment = (thisState != lastState);
		if (needsReattachment) CreateOrReattach();
		lastState = EditorApplication.isCompiling?1:
					EditorApplication.isUpdating?2:
					EditorApplication.isPlaying?3:0;
	}

	void OnDestroy() {
		window = null;
		visible = false;
		Application.RegisterLogCallback(null);
	}
	
}