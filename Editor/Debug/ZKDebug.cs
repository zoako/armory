using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ZKTools;

public class ZKDebug : EditorWindow {

	// Set a higher debug level to catch more debug statements
	private static int debugLevel = 10;
	// Public Usage Calls
	[MenuItem("ZKTools/ZKDebug Console #%w")]
	static void CreateOrReattach () {
		if (window == null) window = (ZKDebug) EditorWindow.GetWindow(typeof(ZKDebug));
		visible = true;

		Application.RegisterLogCallback(window.HandleLog);
		window.needsReattachment = false;

		Debug.Log("Reattached at " + System.DateTime.Now.TimeOfDay) ;

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

	public static void setDebugLevel(int level) {
		debugLevel = Mathf.Max (0, level);
	}


	class Log {
		public string message;
		public string stackTrace;
		public LogType type;
		private GUILayoutOption[] layoutParams;

		public void SetLayout(params GUILayoutOption[] options) {
			layoutParams = options;
		}

		public void RenderLog() {
			EditorGUILayout.SelectableLabel (message + "\n" + stackTrace, 
			                                 layoutParams);
		}
	}

	static bool visible = false;
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

	private bool IsNearBottom() {
		return (scrollPosition.y > (scrollBottom - 2f*(logHeight + 10f)));
	}

	// Layout properties and settings
	protected static float logHeight = 30;
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

	Vector2 BeginAutoScrollView (params GUILayoutOption[] options) {
		// Figuring out scroll-mode
		scrollBottom = logs.Count * (logHeight + 2f) - position.height + devLabelHeight + 10f;
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
			Debug.Log ("Scrolling");
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
	
	protected void OnGUI () {
		BeginAutoScrollView (GUILayout.MinHeight(position.height - devLabelHeight - 10f));
		// Drawing
		foreach (Log log in logs) {
			log.RenderLog ();
		}
		EndAutoScrollView();

		if (consoleDevMode) {
			GUILayout.Label ("ZKDebug; atBottom: " + IsNearBottom() + "\nscroll: " + scrollBottom + " position: " + scrollPosition.y + " height: " + position.height, 
	                 EditorStyles.boldLabel,
	                 GUILayout.MinHeight(devLabelHeight),
	                 GUILayout.MaxHeight(devLabelHeight));
		}	
	}

	void HandleLog (string message, string stackTrace, LogType type) {
		logs.Add(new Log {
			message = message,
			stackTrace = stackTrace,
			type = type,
		});
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

	private static ZKDebug window = null;
	void OnDestroy() {
		window = null;
		visible = false;
		Application.RegisterLogCallback(null);
	}
	
}