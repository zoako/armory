using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ZKTools;

public class ZKDebug : EditorWindow {
	private static ZKDebug window = null;
	static bool visible = false;

	// Set some style stuff
	protected static int logHeight = 35;
	GUIStyle boxStyle = new GUIStyle();
	GUIStyle detailStyle = new GUIStyle();
	GUIStyle [] logStyles = new GUIStyle[]{new GUIStyle(), new GUIStyle()};
	GUILayoutOption [] boxLayout;
	float h_list, h_details, h_dev;
	protected void Initialize() {
		needsReattachment = false;
		
		firstPassed = false;
		selectedLog = -1;

		h_list 		= 0.7f*position.height - 5f;
		h_details 	= 0.2f*position.height - 5f;
		h_dev 		= 0.2f*position.height - 5f;
		boxStyle.normal.background = Utils.MakeTex(1, 1, Color.black);
		boxStyle.margin = new RectOffset(3, 0, 0, 0);
		boxLayout = new GUILayoutOption [] {
			GUILayout.Height(2f),
			GUILayout.Width(window.position.width-5f)
		};

		detailStyle.richText = true;
		detailStyle.wordWrap = true;
		detailStyle.normal.textColor = Color.white;
		detailStyle.padding = new RectOffset(5, 0, 4, 0);
		Color selectColor = new Color(.2f, .4f, .8f);
		//logStyle.normal.background = Utils.MakeTex(1, 1, Color.yellow);
		foreach (GUIStyle logStyle in logStyles) {
			logStyle.richText = true;
			logStyle.padding = new RectOffset(5, 0, 3, 0);
			logStyle.wordWrap = true;

			logStyle.active.background = Utils.MakeTex(1, 1, Color.blue);
			logStyle.onActive.background = Utils.MakeTex(1, 1, selectColor);
			logStyle.onNormal.background = Utils.MakeTex(1, 1, selectColor);

			logStyle.normal.textColor = new Color(.8f, .8f, .8f);
			logStyle.active.textColor = Color.white;
			logStyle.onNormal.textColor = Color.white;
		}
		logStyles[0].normal.background = Utils.MakeTex(1, 1, new Color(0.24f, 0.24f, 0.24f));
		logStyles[1].normal.background = Utils.MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f));
	}

	public static void Clear() {
		if (window != null) {
			window.logs.Clear();
			window.Repaint();
		}
	}

	static bool consoleDevMode = true;

	// Visual elements:
	static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color> {
		{ LogType.Assert, Color.white },
		{ LogType.Error, Color.red },
		{ LogType.Exception, Color.red },
		{ LogType.Log, Color.white },
		{ LogType.Warning, Color.yellow },
	};

	// Properties we care about
	readonly List<Log> logs = new List<Log>();
	bool collapse;

	private void RenderLog(int i) {
		bool isSelected = (i == selectedLog);
		// Don't render unnecessarily
		if ((i+1)*logHeight < scroll_list.y || (i-1)*logHeight > scroll_list.y + h_list) return;

		isSelected = GUI.Toggle (new Rect(0, i*logHeight, position.width, logHeight),
				isSelected,
				logs[i].message,// + "\n" + logs[i].stackTrace,
				logStyles[i%2]);
		if (isSelected) selectedLog = i;
	}


	// Properties used for managing GUI stuff
	protected Vector2 scroll_list;
	protected Vector2 scroll_details;
	int selectedLog = -1;
	protected void GUILogListSection(float top) {
		scroll_list = GUI.BeginScrollView(
							new Rect (0, 0, position.width, h_list),
							scroll_list, 
							new Rect(0, 0, position.width - 20f, Mathf.Max(logs.Count*logHeight, h_list)),
							false, true);
		for (int i = 0; i < logs.Count; i++) {
			RenderLog(i);
		}
		GUI.EndScrollView();
	}

	protected void GUILogDetailsSection(float top) {
		GUI.Box(new Rect(0, top-2f, position.width, 2f), "", boxStyle);
		if (selectedLog < 0 || selectedLog > logs.Count) return;
		
		scroll_details = GUI.BeginScrollView(
							new Rect(0, top, position.width, h_details),
							scroll_details, 
							new Rect(0, 0, position.width - 50f, Mathf.Max(logs[selectedLog].getAllLines().Count*14f, h_details)),
							false, true);
		EditorGUI.SelectableLabel (
			new Rect(0, 0, position.width, logs[selectedLog].getAllLines().Count*14f),
			logs[selectedLog].message + "\n" + logs[selectedLog].stackTrace,
			detailStyle);
		
		GUI.EndScrollView();

	}

	protected void GUIDevSection(float top) {
		GUI.Box(new Rect(0, top, position.width, 2f), "", boxStyle);
		//GUILayout.Box("", boxStyle, boxLayout);
		if (consoleDevMode) {
			GUI.Label (new Rect(0, top, position.width, h_dev),
				"ZKDebug; selected: " + selectedLog + "; position: " + scroll_list + "; height: " + position.height,
                EditorStyles.boldLabel);
		}
	}

	// Nothing to really edit past this point

	// Public Usage Calls
	[MenuItem("ZKTools/ZKDebug Console #%w")]
	static void CreateOrReattach () {
		if (window == null) window = (ZKDebug) EditorWindow.GetWindow(typeof(ZKDebug));
		visible = true;

		Application.RegisterLogCallback(window.HandleLog);
		// Some intiialization stuff
		window.Initialize();
		Debug.Log("Reattached at " + System.DateTime.Now.TimeOfDay) ;
	}

	private bool firstPassed;
	private void FirstGUIPass() {
		if (firstPassed) return;

		firstPassed = true;
	}
	protected void OnGUI () {
		FirstGUIPass();
		GUILogListSection(0);
		GUILogDetailsSection(h_list);
		GUIDevSection(h_list + h_details);
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

	void HandleLog (string message, string stackTrace, LogType type) {
		logs.Add(new Log (message, stackTrace, type, position.width));
		//newLogArrived = true;
		Repaint ();
	}

	void OnDestroy() {
		window = null;
		visible = false;
		Application.RegisterLogCallback(null);
	}
	
	// Set a higher debug level to catch more debug statements
	private static int debugLevel = 10;
	public static void setDebugLevel(int level) {
		debugLevel = Mathf.Max (0, level);
	}


}