using System.Net;
using UnityEditor;
using UnityEngine;
#if GSTU_Legacy
GoogleSheetsToUnity.Legacy
#endif

namespace GoogleSheetsToUnity
{
#if false //[fov-studio update] 2024.04 사용하지 않음
    public class _GoogleSheetsToUnityEditorWindow : Editor.GoogleSheetsToUnityEditorWindow
    {
        private static string LOG_FORMAT = "<color=#37AFB6><b>[_GoogleSheetsToUnityEditorWindow]</b></color> {0}";

        protected _GoogleSheetsToUnityConfig _config
        {
            get
            {
                return config as _GoogleSheetsToUnityConfig;
            }
        }

        [MenuItem("GoogleSheetsToUnity/Open Config(Base_Framework)")]
        private static void Open()
        {
            _GoogleSheetsToUnityEditorWindow win = EditorWindow.GetWindow<_GoogleSheetsToUnityEditorWindow>("_Google Sheets To Unity");
            ServicePointManager.ServerCertificateValidationCallback = Validator;

            win.Init();
        }

        public override void Init()
        {
            string path = _SpreadSheetManager._GSTU_ConfigPath;
            Debug.LogFormat(LOG_FORMAT, "Init(), Resources.Load(), path : <b>" + path + "</b>");

            config = (_GoogleSheetsToUnityConfig)Resources.Load(path);
            Debug.Assert(_config != null);
        }

        protected override void OnGUI()
        {
            tabID = GUILayout.Toolbar(tabID, new string[] { "Private", /*"Private (Legacy)",*/ "Public" });

            if (_config == null)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Error: no config file");
                return;
            }

            switch (tabID)
            {
                case 0:
                    {
                        _config.CLIENT_ID = EditorGUILayout.TextField("Client ID", _config.CLIENT_ID);

                        GUILayout.BeginHorizontal();
                        if (showSecret == true)
                        {
                            _config.CLIENT_SECRET = EditorGUILayout.TextField("Client Secret Code", _config.CLIENT_SECRET);
                        }
                        else
                        {
                            _config.CLIENT_SECRET = EditorGUILayout.PasswordField("Client Secret Code", _config.CLIENT_SECRET);
                        }
                        showSecret = GUILayout.Toggle(showSecret, "Show");
                        GUILayout.EndHorizontal();

                        _config.PORT = EditorGUILayout.IntField("Port number", _config.PORT);

                        if (GUILayout.Button("Build Connection"))
                        {
                            Debug.LogFormat(LOG_FORMAT, "Build Connection");

                            _GoogleAuthrisationHelper.BuildHttpListener();
                        }

                        break;
                    }
#if false
                case 1:
                    {
#if GSTU_Legacy
                        _config.CLIENT_ID = EditorGUILayout.TextField("Client ID", _config.CLIENT_ID);

                        GUILayout.BeginHorizontal();
                        if (showSecret)
                        {
                            _config.CLIENT_SECRET = EditorGUILayout.TextField("Client Secret Code", _config.CLIENT_SECRET);
                        }
                        else
                        {
                            _config.CLIENT_SECRET = EditorGUILayout.PasswordField("Client Secret Code", _config.CLIENT_SECRET);

                        }
                        showSecret = GUILayout.Toggle(showSecret, "Show");
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button("Get Access Code"))
                        {
                            string authUrl = oAuth2.GetAuthURL();
                            Application.OpenURL(authUrl);
                        }

                        _config.ACCESS_TOKEN = EditorGUILayout.TextField("Access Code", _config.ACCESS_TOKEN);

                        if (GUILayout.Button("Authentication with Acceess Code"))
                        {
                            SecurityPolicy.Instate();
                            _config.REFRESH_TOKEN = oAuth2.AuthWithAccessCode(_config.ACCESS_TOKEN);
                        }

                        if (GUILayout.Button("Debug Information (WARNING: This may take some time)"))
                        {
                            knownData.Clear();
                            spreadsheetNames.Clear();
                            isDebugOn = true;

                            if (_config.REFRESH_TOKEN != "")
                            {
                                if (spreadSheet == null)
                                {
                                    spreadSheet = new SpreadSheetManager();
                                }

                                var tempData = spreadSheet.GetAllSheets();
                                for (int i = 0; i < tempData.Count; i++)
                                {
                                    knownData.Add(new KnownData(tempData[i], tempData[i].GetAllWorkSheetsNames()));
                                    spreadsheetNames.Add(tempData[i].spreadsheetEntry.Title.Text);
                                }
                            }
                        }

                        if (isDebugOn)
                        {
                            DrawPreview();
                        }
#else
                        GUILayout.Label("This is the legacy version of GSTU and will be removed at a future date, if you wish to use it please press the button below");
                        if (GUILayout.Button("Use Legacy Version"))
                        {
                            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + "GSTU_Legacy"));
                        }
#endif
                        break;
                    }

                case 2:
#else
                case 1:
#endif
                    {
                        _config.API_Key = EditorGUILayout.TextField("API Key", _config.API_Key);
                        break;
                    }

                default:
                    Debug.Assert(false);
                    break;
            }

            EditorUtility.SetDirty(_config);
        }
    }
#endif
}