using Adinmo.Flatbufs;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Adinmo
{
    public class AdinmoPlacementTreeViewItem : TreeViewItem
    {
        public AdinmoPlacementTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
        }

        public ImpressionsSummary GetImpressionsSummary()
        {
            return AdinmoManager.Sampler.GetImpressionsSummary(displayName);
        }

        public Placement GetPlacement()
        {
            return AdinmoManager.Downloader.GetPlacement(displayName);
        }

        public string GetError()
        {
            if (!AdinmoManager.IsReady())
                return null;
            GameStatusFB gameStatus = AdinmoManager.s_manager != null ? AdinmoManager.s_manager.GameStatus : GameStatusFB.Unknown;
            string placementError = "";

            if (string.IsNullOrEmpty(displayName))
            {
                placementError = "Placement Key not set on the following objects";
            }
            else if (gameStatus == GameStatusFB.Active || gameStatus == GameStatusFB.Sandbox)
            {
                if (AdinmoManager.Downloader!=null && AdinmoManager.Downloader.IsPlacementPaused(displayName))
                {
                    placementError = "Placement is paused";
                }
                else if (AdinmoManager.Downloader != null || AdinmoManager.Downloader.IsPlacementDeleted(displayName))
                {
                    placementError = "Placement is deleted";
                }
                else
                {
                    placementError = "Placement Key is invalid on the following objects";
                }
            }
            return placementError;
        }
    }

    public class AdinmoReplaceTreeViewItem : TreeViewItem
    {
        public AdinmoReplace m_adinmoReplace;

        public AdinmoReplaceTreeViewItem(int id, int depth, AdinmoReplace adinmoReplace) : base(id, depth, adinmoReplace.name)
        {
            m_adinmoReplace = adinmoReplace;
        }
    }

    public class AdinmoPlacementTreeView : TreeView
    {
        const int LABEL_WIDTH = 170;
        const int VALUE_WIDTH = 75;
        Texture2D greyTexture;

        public AdinmoPlacementTreeView(TreeViewState state, string editorScriptPath) : base(state)
        {
            greyTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(editorScriptPath + "/Textures/greytexture.png", typeof(Texture2D));
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            AdinmoReplace[] adinmoReplacements = (AdinmoReplace[])GameObject.FindObjectsOfType(typeof(AdinmoReplace));
            Dictionary<string, List<AdinmoReplace>> placementsDictionary = new();
            foreach (AdinmoReplace adinmoReplace in adinmoReplacements)
            {
                if (!placementsDictionary.ContainsKey(adinmoReplace.m_placementKey))
                {
                    placementsDictionary.Add(adinmoReplace.m_placementKey, new List<AdinmoReplace>());
                }
                placementsDictionary[adinmoReplace.m_placementKey].Add(adinmoReplace);
            }
            int currentId = 0;
            if (placementsDictionary.Count == 0)
            {
                var tempRoot = new TreeViewItem { id = currentId++, depth = -1, displayName = "Root" };
                var info = new TreeViewItem { id = currentId++, depth = 0, displayName = "" };
                tempRoot.AddChild(info);
                return tempRoot;
            }
            var root = new TreeViewItem { id = currentId++, depth = -1, displayName = "Root" };
            foreach (KeyValuePair<string, List<AdinmoReplace>> entry in placementsDictionary)
            {
                TreeViewItem placement = null;
                if (Application.isPlaying)
                {
                    placement = new AdinmoPlacementTreeViewItem(currentId++, 0, entry.Key);
                }
                else
                {
                    placement = new TreeViewItem(currentId++, 0, entry.Key);
                }
                root.AddChild(placement);
                foreach (AdinmoReplace adinmoReplace in entry.Value)
                {
                    var adinmoReplaceItem = new TreeViewItem(currentId++, 1, adinmoReplace.name);
                    placement.AddChild(adinmoReplaceItem);
                    if (Application.isPlaying)
                    {
                        var childItem = new AdinmoReplaceTreeViewItem(currentId++, 2, adinmoReplace);
                        adinmoReplaceItem.AddChild(childItem);
                    }
                }
            }
            return root;
        }

        protected override float GetCustomRowHeight(int rowno, TreeViewItem item)
        {
            if (item is AdinmoReplaceTreeViewItem)
            {
                if (((AdinmoReplaceTreeViewItem)item).m_adinmoReplace.GetObjectType() == AdinmoReplace.ObjectType.Image || !AdinmoManager.s_manager.imageRenderDebug)
                {
                    return base.GetCustomRowHeight(rowno, item);
                }
                else
                {
                    return 50;
                }
            }
            else if (item is AdinmoPlacementTreeViewItem)
            {
                if (Application.isPlaying && IsExpanded(item.id))
                {

                    Placement placement = AdinmoManager.Downloader != null ? AdinmoManager.Downloader.GetPlacement(item.displayName) : null;

                    if (placement == null)
                    {
                        GameStatusFB gameStatus = AdinmoManager.s_manager != null ? AdinmoManager.s_manager.GameStatus : GameStatusFB.Unknown;
                        string error = ((AdinmoPlacementTreeViewItem)item).GetError();
                        if (gameStatus != GameStatusFB.Sandbox && gameStatus != GameStatusFB.Active)
                            return rowHeight;
                        if (!string.IsNullOrEmpty(error))
                            return rowHeight * 2;
                        else
                            return rowHeight;
                    }
                    else
                        return rowHeight * 7 + 5;
                }
                else
                    return base.GetCustomRowHeight(rowno, item);
            }
            else
            {
                return base.GetCustomRowHeight(rowno, item);
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            GUIStyle headerStyle = new(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold
            };
            GUIStyle labelStyle = new(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft
            };
            GUIStyle valueStyle = new(GUI.skin.label)
            {
                alignment = TextAnchor.UpperRight
            };
            if (Application.isPlaying)
            {
                if (args.item is AdinmoReplaceTreeViewItem)
                {

                    AdinmoReplace adinmoReplace = ((AdinmoReplaceTreeViewItem)args.item).m_adinmoReplace;
                    if (adinmoReplace.SampleTexture)
                    {
                        Rect rect = args.rowRect;
                        rect.x += GetFoldoutIndent(args.item);
                        rect.width = 350;
                        if (adinmoReplace.GetObjectType() == AdinmoReplace.ObjectType.Image || !AdinmoManager.s_manager.imageRenderDebug)
                        {
                            string str = "Latest Sample: " + (adinmoReplace.LatestSample.sample * 100).ToString("F2") + "%";
                            Color oldColour = GUI.contentColor;
                            if (adinmoReplace.LatestFailureReason != "None")
                            {
                                str = str + " " + adinmoReplace.LatestFailureReason;
                                var textDimensions = GUI.skin.label.CalcSize(new GUIContent(str));
                                GUI.contentColor = new Color(0, 0, 0, 0.75f);
                                rect.width = textDimensions.x + 5;
                                GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), str);
                                GUI.contentColor = Color.red;
                                GUI.Label(rect, str);
                            }
                            else
                                GUI.Label(rect, str);
                            GUI.contentColor = oldColour;
                        }
                        else
                        {
                            float imageWidth;
                            float imageHeight;
                            float widthOffset = 0;
                            float heightOffset = 0;

                            if (adinmoReplace.SampleTexture.width > adinmoReplace.SampleTexture.height)
                            {
                                imageWidth = 48;
                                imageHeight = (adinmoReplace.SampleTexture.height * 48.0f) / adinmoReplace.SampleTexture.width;
                                heightOffset = (48 - imageHeight) / 2;
                            }
                            else
                            {
                                imageWidth = (adinmoReplace.SampleTexture.width * 48.0f) / adinmoReplace.SampleTexture.height;
                                imageHeight = 48;
                                widthOffset = (48 - imageWidth) / 2;
                            }
                            GUI.DrawTexture(new Rect(rect.x, rect.y, 48, 48), greyTexture);
                            GUI.DrawTexture(new Rect(rect.x + widthOffset, rect.y + heightOffset, imageWidth, imageHeight), adinmoReplace.SampleTexture);
                            GUI.Label(new Rect(rect.x + 55, rect.y + 5, 150, 16), "Latest Sample: " + (adinmoReplace.LatestSample.sample * 100).ToString("F2") + "%");
                            if (adinmoReplace.LatestFailureReason != "None")
                            {
                                string failReasonStr = "Fail Reason: " + adinmoReplace.LatestFailureReason;
                                var textDimensions = GUI.skin.label.CalcSize(new GUIContent(failReasonStr));
                                Color oldColour = GUI.contentColor;
                                GUI.contentColor = new Color(0, 0, 0, 0.75f);
                                GUI.Label(new Rect(rect.x + 56, rect.y + 22, textDimensions.x + 5, 16), failReasonStr);
                                GUI.contentColor = Color.red;
                                GUI.Label(new Rect(rect.x + 55, rect.y + 21, textDimensions.x + 5, 16), failReasonStr);
                                GUI.contentColor = oldColour;
                            }
                        }
                        //GUI.Label(rect,"Latest Sample");
                    }
                }
                else if (args.item is AdinmoPlacementTreeViewItem)
                {

                    // base.RowGUI(args);
                    Rect rect = args.rowRect;
                    rect.x += GetContentIndent(args.item);
                    if (IsExpanded(args.item.id))
                    {

                        GUI.Label(rect, args.item.displayName, headerStyle);
                        ImpressionsSummary impressionsSummary = ((AdinmoPlacementTreeViewItem)args.item).GetImpressionsSummary();
                        var treeItem = (AdinmoPlacementTreeViewItem)args.item;
                        Placement placement = treeItem.GetPlacement();
                        float bestSample = 0;
                        foreach (TreeViewItem replaceItem in args.item.children)
                        {
                            bestSample = Mathf.Max(bestSample, ((AdinmoReplaceTreeViewItem)replaceItem.children[0]).m_adinmoReplace.LatestSample.sample);
                        }

                        string currentNumImpressions = " n/a";
                        string pCSamplesRemaining = " n/a";
                        if (impressionsSummary.CurrentImageValid)
                        {
                            currentNumImpressions = string.Format("{0} / {1}", impressionsSummary.CurrentNumberImpressions, (impressionsSummary.CurrentNumberImpressions + impressionsSummary.CurrentNumberFailedImpressions));
                            pCSamplesRemaining = (impressionsSummary.PCSamplesRemaining * 100).ToString("F1");
                        }
                        float rowposy = rect.y + rowHeight;

                        if (placement != null)
                        {
                            GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH + VALUE_WIDTH, rowHeight), "Name:    " + placement.name, headerStyle);
                            rowposy += rowHeight + 3;
                            GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH, rowHeight), "Aspect Ratio:", labelStyle);
                            if (placement != null)
                            {
                                string aspectRatioText = placement.AspectRatioString;
                                GUI.Label(new Rect(rect.x + LABEL_WIDTH, rowposy, VALUE_WIDTH, rowHeight), aspectRatioText, valueStyle);
                            }
                            rowposy += rowHeight;
                            GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH, rowHeight), "Samples Countdown:", labelStyle);
                            GUI.Label(new Rect(rect.x + LABEL_WIDTH, rowposy, VALUE_WIDTH, rowHeight), pCSamplesRemaining, valueStyle);
                            rowposy += rowHeight;
                            GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH, rowHeight), "Impressions Current Image:", labelStyle);
                            GUI.Label(new Rect(rect.x + LABEL_WIDTH, rowposy, VALUE_WIDTH, rowHeight), currentNumImpressions, valueStyle);
                            rowposy += rowHeight;
                            GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH, rowHeight), "Impressions Total:", labelStyle);
                            GUI.Label(new Rect(rect.x + LABEL_WIDTH, rowposy, VALUE_WIDTH, rowHeight), string.Format("{0} / {1}", impressionsSummary.TotalNumberImpressions, (impressionsSummary.TotalNumberImpressions + impressionsSummary.TotalNumberFailedImpressions)), valueStyle);
                            rowposy += rowHeight;
                            GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH, rowHeight), "Best Sample:", labelStyle);
                            GUI.Label(new Rect(rect.x + LABEL_WIDTH, rowposy, VALUE_WIDTH, rowHeight), (bestSample * 100).ToString("F1"), valueStyle);
                            GUI.Label(new Rect(rect.x + LABEL_WIDTH + VALUE_WIDTH, rowposy, LABEL_WIDTH, rowHeight), "%", labelStyle);
                        }
                        else if (AdinmoManager.IsReady())
                        {
                            Color oldColour = GUI.contentColor;
                            string placementError = treeItem.GetError();
                            if (!string.IsNullOrEmpty(placementError))
                            {
                                GUI.contentColor = new Color(0, 0, 0, 0.75f);
                                GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH + VALUE_WIDTH + 100, rowHeight), placementError, headerStyle);

                                GUI.contentColor = Color.red;
                                GUI.Label(new Rect(rect.x, rowposy, LABEL_WIDTH + VALUE_WIDTH + 100, rowHeight), placementError, headerStyle);
                                GUI.contentColor = oldColour;
                            }

                        }



                    }
                    else
                    {
                        GUI.Label(rect, args.item.displayName, headerStyle);
                    }
                }
                else
                {
                    base.RowGUI(args);
                }
            }
            else if (args.item is AdinmoPlacementTreeViewItem)
            {
                Rect rect = args.rowRect;
                rect.x += GetContentIndent(args.item);
                GUI.Label(rect, args.item.displayName, headerStyle);
            }
            else
            {
                base.RowGUI(args);
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            TreeViewItem item = FindItem(id, rootItem);
            if (item.depth == 1)
            {
                GameObject foundObj = GameObject.Find(item.displayName);
                Selection.activeGameObject = foundObj;
            }
        }
    }

    internal static class AdinmoEditorCommon //Common editor code that can be reused
    {
        public static void RenderAdinmoManagerUI(SerializedObject serializedObject, Texture2D AdinmoLogo, ref Rect rect, bool isPopOutWindow, string editorScriptPath)
        {
            //Retrieve all the variables required from the Adinmo Manager
            var debug = serializedObject.FindProperty("basicDebug");
            var showAdvancedDebug = serializedObject.FindProperty("showAdvancedDebug");
            var debugOnDevice = serializedObject.FindProperty("m_debugOnDevice");
            var placementDebug = serializedObject.FindProperty("placementDebug");
            var debugSize = serializedObject.FindProperty("debugFontSize");
            var imageRenderDebug = serializedObject.FindProperty("imageRenderDebug");
            var applyAds = serializedObject.FindProperty("m_applyAds");
            var camera = serializedObject.FindProperty("m_camera");
            var gamekey = serializedObject.FindProperty("m_gameKey");
            var applicationVersion = serializedObject.FindProperty("applicationVersion");
            var androidAdmobAppId = serializedObject.FindProperty("m_AndroidAdMobAppId");
            var magnifierSortOrder = serializedObject.FindProperty("MagnifierSortOrder");
            var customMagnifierTemplate = serializedObject.FindProperty("CustomMagnifierTemplate");
            if (isPopOutWindow) //Specific formatting for the popout window
            {
                EditorGUILayout.Space();
                EditorGUIUtility.labelWidth = 180;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(GUILayout.Height(AdinmoLogo.height));
                EditorGUILayout.PrefixLabel("  ");
            }
            //Render the Adinmo Logo
            rect = EditorGUILayout.GetControlRect();
            rect.height = AdinmoLogo.height;
            rect.width = AdinmoLogo.width;
            if (isPopOutWindow)
                rect.x = rect.x - (AdinmoLogo.width / 2) - 20;
            GUI.DrawTexture(rect, AdinmoLogo, ScaleMode.ScaleToFit);

            if (isPopOutWindow) //Specific formatting for the popout window
                EditorGUILayout.EndHorizontal();
            else //Specific formatting for the inspector script
            {
                for (int i = 0; i < 6; i++)
                    EditorGUILayout.Space();
                serializedObject.Update();
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Version"), new GUIContent("3.2." + AdinmoSender.GetVersion() + " s" + AdinmoSender.GetSdkSource()));

            if (EditorGUILayout.LinkButton(new GUIContent("Documentation")))
            {
                Application.OpenURL("http://documentation.adinmo.com");
            }


            //Display the AdinmoManager debug options
            EditorGUILayout.PropertyField(debug);
            if (debug.boolValue)
            {
                EditorGUILayout.PropertyField(debugOnDevice);
                string buttonText = "Show Advanced Debug Config";
                if (showAdvancedDebug.boolValue) { buttonText = "Hide Advanced Debug Config"; }
                if (GUILayout.Button(buttonText))
                    showAdvancedDebug.boolValue = !showAdvancedDebug.boolValue;
                if (showAdvancedDebug.boolValue)
                {
                    EditorGUILayout.PropertyField(placementDebug);
                    EditorGUILayout.PropertyField(debugSize);
                    EditorGUILayout.PropertyField(imageRenderDebug);
                }
            }

            //Display the general AdinmoManager configuration properties
            EditorGUILayout.PropertyField(applyAds);
            EditorGUILayout.PropertyField(camera);
            EditorGUILayout.PropertyField(gamekey);
            if (Application.isPlaying)
            {
                GameStatusFB gameStatus = AdinmoManager.s_manager!=null ? AdinmoManager.s_manager.GameStatus : GameStatusFB.Unknown;
                GUIStyle warningLabelStyle = new(EditorStyles.label);
                warningLabelStyle.normal.textColor = Color.red;
                warningLabelStyle.fontSize = 18;
                warningLabelStyle.fixedHeight = 22;
                warningLabelStyle.fontStyle = FontStyle.Bold;

                if (string.IsNullOrEmpty(gamekey.stringValue))
                {
                    EditorGUILayout.LabelField("Game Key must have a value", warningLabelStyle);
                }
                else if (gamekey.stringValue.Length != 35)
                {
                    EditorGUILayout.LabelField("Game Key is invalid", warningLabelStyle);
                }
                else if (gameStatus == GameStatusFB.Unknown || gameStatus == GameStatusFB.Paused || gameStatus == GameStatusFB.Deleted)
                {
                    EditorGUILayout.LabelField("Game Status is " + gameStatus, warningLabelStyle);
                }
            }
            EditorGUILayout.PropertyField(applicationVersion);
            EditorGUILayout.PropertyField(customMagnifierTemplate);
            EditorGUILayout.PropertyField(magnifierSortOrder);
            EditorGUILayout.PropertyField(androidAdmobAppId);


            string newiOSAdMobAppId = EditorGUI.TextField(EditorGUILayout.GetControlRect(), new GUIContent("iOS Ad Mob App Id", "The iOS Admob Application Id for this App"), AdinmoManager.m_iOSAdMobAppId);
            List<string> labels = new();
            int clickDisplayMask = 0;
            int currentLayerMask = serializedObject.FindProperty("clickLayerMask").intValue;
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) != "")
                {
                    int tempMask = 1 << labels.Count;
                    labels.Add(LayerMask.LayerToName(i));
                    if ((1 << i & currentLayerMask) != 0)
                        clickDisplayMask |= tempMask;

                }
            }

            if (labels.Count > 0)
            {
                int newclickDisplaymask = EditorGUILayout.MaskField(new GUIContent("Click ignores Layers", "Objects on selected layers won't block click detection"), clickDisplayMask, labels.ToArray());
                if (newclickDisplaymask != -1 && newclickDisplaymask != clickDisplayMask)
                {
                    int newClickMask = 0;
                    for (int i = 0; i < labels.Count; i++)
                    {
                        if ((newclickDisplaymask & 1 << i) != 0)
                        {
                            newClickMask |= 1 << LayerMask.NameToLayer(labels[i]);
                        }
                    }

                    serializedObject.FindProperty("clickLayerMask").intValue = newClickMask;
                }
                else if (newclickDisplaymask == -1)
                {
                    Debug.LogError("Can't filter out all layers");
                }
            }

            //Apply the visual changes to properties
            bool changed = serializedObject.ApplyModifiedProperties();
            if (newiOSAdMobAppId != AdinmoManager.m_iOSAdMobAppId)
            {
                if (!string.IsNullOrEmpty(newiOSAdMobAppId))
                {
                    Directory.CreateDirectory(Application.dataPath + "/AdinmoData");
                    StreamWriter writer = new(Application.dataPath + "/AdinmoData/iosUMPCode.txt", false);
                    writer.Write(newiOSAdMobAppId);
                    writer.Close();

                }
                else if (File.Exists(Application.dataPath + "/AdinmoData/iosUMPCode.txt"))
                {
                    File.Delete(Application.dataPath + "/AdinmoData/iosUMPCode.txt");
                }
                AdinmoManager.m_iOSAdMobAppId = newiOSAdMobAppId;
            }
        }

        public static string GetEditorScriptPath()
        {
            var guid = AssetDatabase.FindAssets($"t:Script {nameof(AdinmoEditor)}")[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return path[..path.LastIndexOf("/")];
        }

        public static string GetiOSCode()
        {
            string iosCode = null;
            try
            {
                StreamReader reader = new(Application.dataPath + "/AdinmoData/iosUMPCode.txt");
                iosCode = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception)
            {

            }
            return iosCode;
        }
    }

    [InitializeOnLoad]
    public class AdinmoEditor : EditorWindow
    {
        private Texture2D AdinmoLogo;
        private TreeViewState m_TreeViewState;
        private AdinmoPlacementTreeView placementTreeView;
        private Vector2 scrollPos;
        private bool m_gameRunning;
        private bool m_gameReady;
        private bool m_debugImageRender;
        private static string currentScene;
        // Add menu named "My Window" to the Window menu
        [MenuItem("Tools/Adinmo/Manager")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            AdinmoEditor window = (AdinmoEditor)EditorWindow.GetWindow(typeof(AdinmoEditor));
            window.Show();
        }

        static AdinmoEditor()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && Application.isEditor)
            {
                try
                {
                    int.TryParse(Application.unityVersion[..4], out int unityVersion);
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)[0] == UnityEngine.Rendering.GraphicsDeviceType.Vulkan && unityVersion < 2019)
                    {
                        Debug.LogWarning("Vulkan support for Android in Adinmo only fully implemented for Unity 2019 and above");
                    }
                }
                catch (System.Exception) { }
            }
        }

        private void OnEnable()
        {
            MonoScript ms = MonoScript.FromScriptableObject(this);
            string editorScriptPath = AssetDatabase.GetAssetPath(ms);
            editorScriptPath = editorScriptPath[..editorScriptPath.LastIndexOf("/")];
            AdinmoLogo = (Texture2D)AssetDatabase.LoadAssetAtPath(editorScriptPath + "/Textures/AdinMoLogo.png", typeof(Texture2D));
            Texture2D AdinmoIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(editorScriptPath + "/Textures/AdinmoIcon.png", typeof(Texture2D));
            titleContent = new GUIContent("Adinmo", AdinmoIcon);
            m_TreeViewState ??= new TreeViewState();
            m_gameRunning = Application.isPlaying;
            placementTreeView = new AdinmoPlacementTreeView(m_TreeViewState, editorScriptPath);

            EditorSceneManager.sceneOpened += OnLevelFinishedLoading;
            AdinmoReplace.OnChange += RefreshTree;
        }

        void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnLevelFinishedLoading;
            AdinmoReplace.OnChange -= RefreshTree;
        }

        void OnLevelFinishedLoading(Scene scene, OpenSceneMode mode)
        {
            RefreshTree(true);
        }

        public void RefreshTree(bool recreate)
        {
            if (recreate)
            {
                placementTreeView.Reload();
            }
            else
            {
                placementTreeView.Repaint();
            }
        }

        private void Update()
        {
            if (m_gameRunning != Application.isPlaying || AdinmoManager.IsReady() != m_gameReady || (AdinmoManager.s_manager != null && m_debugImageRender != AdinmoManager.s_manager.imageRenderDebug))
            {
                placementTreeView.Reload();
                m_gameRunning = Application.isPlaying;
                m_gameReady = AdinmoManager.IsReady();
                m_debugImageRender = (AdinmoManager.s_manager != null && AdinmoManager.s_manager.imageRenderDebug);
            }
        }

        void OnGUI() //This controls how the GUI should be rendered in the AdinmoManager Popout Window
        {
            //Retrieve the AdInMo manager
            AdinmoManager adinmoManager = GameObject.FindObjectOfType<AdinmoManager>();
            if (adinmoManager == null)
            {
                EditorGUILayout.LabelField("Can't find an AdinmoManager in Scene");
                return;
            }
            //Render the common AdinmoManagerUI
            var serializedObject = new SerializedObject(adinmoManager);
            Rect rect = new();
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            AdinmoEditorCommon.RenderAdinmoManagerUI(serializedObject, AdinmoLogo, ref rect, true, AdinmoEditorCommon.GetEditorScriptPath());
            //Render the UI that is exclusive to the popout window
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Placements in scene", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("A list of placements in the scene. This will also contain impression information at runtime.");
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            rect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true));
            placementTreeView.OnGUI(rect);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}