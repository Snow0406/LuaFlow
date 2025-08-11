using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LuaFlow.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LuaFlow.Editor.HotReload
{
    public class LuaFlowHotReloadWindow : EditorWindow
    {
        private static readonly Regex ChapRegex = new Regex(@"Chap(\d+)", RegexOptions.Compiled);
        
        private const string AUTO_START_ON_SCENE_LOAD_KEY = "LuaFlow.AutoStartOnSceneLoad";
        private const string AUTO_START_ON_FILE_CHANGE_KEY = "LuaFlow.AutoStartOnFileChange";
        private const string ENABLE_LOG_KEY = "LuaFlow.EnableLog";
        private const string WATCH_FOLDER_PATH_KEY = "LuaFlow.WatchFolderPath";
        private const string SELECT_SCRIPT_PATH_KEY = "LuaFlow.SelectedScriptPath";
        
        // UI Elements
        private Toggle _autoStartOnSceneLoadToggle;
        private Toggle _autoStartOnFileChangeToggle;
        private Toggle _enableLogToggle;
        private Button _pauseButton;
        
        // DropdownField
        private DropdownField _luaScriptDropdown;
        private readonly List<string> _availableLuaScripts = new List<string>();
        private readonly Dictionary<string, string> _luaPathMap = new Dictionary<string, string>();
        
        // File Watcher
        private FileSystemWatcher _luaFileWatcher;
        private string _currentLuaScriptPath;
        private string _normalizedWatchPath;
        
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeEntered;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeEntered;
            _luaFileWatcher?.Dispose();
        }
        
        private void OnDestroy()
        {
            _luaFileWatcher?.Dispose();
        }
        
        #region GUI

        [MenuItem("LuaFlow/Hot Reload Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<LuaFlowHotReloadWindow>();
            window.titleContent = new GUIContent("Lua Hot Reload");
            window.maxSize = new Vector2(400, 230);
            window.minSize = new Vector2(400, 229);
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingTop = 10;

            CreateHeader(root);
            CreateControlsSection(root);
            CreateSettingsSection(root);
            
            EditorApplication.delayCall += () => {
                LoadSettings();
                RefreshLuaScriptDropdown();
                
                if (!string.IsNullOrEmpty(_currentLuaScriptPath))
                {
                    SetupFileWatcher(_currentLuaScriptPath);
                }
            };
        }

        private void CreateHeader(VisualElement root)
        {
            var header = new Label("LuaFlow Hot Reload System")
            {
                style =
                {
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 10,
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            root.Add(header);
        }
        
        private void CreateControlsSection(VisualElement root)
        {
            var buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    justifyContent = Justify.Center,
                    marginBottom = 10,
                }
            };
            
            var playButton = new Button(PlayCutscene)
            {
                text = "Play",
                style =
                {
                    width = 80,
                    marginRight = 5,
                }
            };

            _pauseButton = new Button(PauseScene)
            {
                text = "Pause",
                style =
                {
                    width = 80,
                    marginRight = 5
                }
            };

            var stopButton = new Button(StopScene)
            {
                text = "Stop",
                style =
                {
                    width = 80
                }
            };

            buttonContainer.Add(playButton);
            buttonContainer.Add(_pauseButton);
            buttonContainer.Add(stopButton);
            
            root.Add(buttonContainer);
        }
        
        private void CreateSettingsSection(VisualElement root)
        {
            var settingsContainer = new VisualElement
            {
                style = {
                    backgroundColor = new Color(0.35f, 0.35f, 0.35f, 0.3f),
                    borderTopWidth = 2,
                    borderTopColor = new Color(0.4f, 0.6f, 1f, 0.8f),
                    marginTop = 10,
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 10,
                    paddingRight = 10,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4
                }
            };
            
            var settingsHeader = new Label("🔧 Settings")
            {
                style = {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 8,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };
            
            settingsContainer.Add(settingsHeader);
            
            var contentContainer = new VisualElement
            {
                style = {
                    marginLeft = 15,
                    marginTop = 5
                }
            };

            #region LuaScriptContainer

            var luaScriptContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 10
                }
            };

            _luaScriptDropdown = new DropdownField("Lua Script")
            {
                style = { width = 300 }
            };
            _luaScriptDropdown.RegisterValueChangedCallback(evt => OnLuaScriptSelected(evt.newValue));

            var changeFolderButton = new Button(ShowFolderSelectionMenu)
            {
                text = "📁",
                style = { width = 25, marginLeft = 5 }
            };

            luaScriptContainer.Add(_luaScriptDropdown);
            luaScriptContainer.Add(changeFolderButton);

            #endregion

            #region Toggle

            _autoStartOnSceneLoadToggle = new Toggle
            {
                text = "Auto start on scene load",
                style = { marginBottom = 5 }
            };
            _autoStartOnSceneLoadToggle.RegisterValueChangedCallback(evt => 
            {
                EditorPrefs.SetBool(AUTO_START_ON_SCENE_LOAD_KEY, evt.newValue);
            });
            
            _autoStartOnFileChangeToggle = new Toggle
            {
                text = "Auto start on file change",
                style = { marginBottom = 5 }
            };
            _autoStartOnFileChangeToggle.RegisterValueChangedCallback(evt => 
            {
                EditorPrefs.SetBool(AUTO_START_ON_FILE_CHANGE_KEY, evt.newValue);
            });

            _enableLogToggle = new Toggle
            {
                text = "Enable Debug Log",
            };
            _enableLogToggle.RegisterValueChangedCallback(evt => 
            {
                EditorPrefs.SetBool(ENABLE_LOG_KEY, evt.newValue);
            });

            #endregion
            
            contentContainer.Add(luaScriptContainer);
            contentContainer.Add(_autoStartOnSceneLoadToggle);
            contentContainer.Add(_autoStartOnFileChangeToggle);
            contentContainer.Add(_enableLogToggle);
            
            settingsContainer.Add(contentContainer);
            root.Add(settingsContainer);
        }

        private void RefreshLuaScriptDropdown()
        {
            string searchFolder = EditorPrefs.GetString(WATCH_FOLDER_PATH_KEY, "Assets/Cutscene");
            
            if (!Directory.Exists(searchFolder))
            {
                _luaScriptDropdown.choices = new List<string> { "Folder not found" };
                return;
            }
            
            var luaFiles = Directory.GetFiles(searchFolder, "*.lua", SearchOption.AllDirectories)
                                   .Select(path => path.Replace('\\', '/'))
                                   .Where(path => !Path.GetFileName(path).StartsWith("."))
                                   .OrderBy(Path.GetFileNameWithoutExtension)
                                   .ToList();
            
            _luaPathMap.Clear();
            _availableLuaScripts.Clear();
            
            foreach (var path in luaFiles)
            {
                string displayName = Path.GetFileNameWithoutExtension(path);
                if (_availableLuaScripts.Contains(displayName))
                {
                    displayName = $"{displayName} ({Path.GetFileName(Path.GetDirectoryName(path))})";
                }
                
                _availableLuaScripts.Add(displayName);
                _luaPathMap[displayName] = path;
            }
            
            _luaScriptDropdown.choices = _availableLuaScripts;
            
            string savedScript = EditorPrefs.GetString(SELECT_SCRIPT_PATH_KEY, "");
            if (!string.IsNullOrEmpty(savedScript))
            {
                string savedDisplayName = Path.GetFileNameWithoutExtension(savedScript);
                if (_availableLuaScripts.Contains(savedDisplayName))
                {
                    _luaScriptDropdown.value = savedDisplayName;
                }
            }
        }

        private void ShowFolderSelectionMenu()
        {
            var menu = new GenericMenu();
            
            string currentFolder = EditorPrefs.GetString(WATCH_FOLDER_PATH_KEY, "Assets/Cutscene");
            menu.AddItem(new GUIContent($"Current: {currentFolder.Replace("/", " > ")}"), false, null);
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Browse..."), false, () => {
                string path = EditorUtility.OpenFolderPanel("Select Lua Folder", currentFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    string relativePath = FileUtil.GetProjectRelativePath(path);
                    EditorPrefs.SetString(WATCH_FOLDER_PATH_KEY, relativePath);
                    RefreshLuaScriptDropdown();
                }
            });
            
            menu.ShowAsContext();
        }

        #endregion

        #region Handler

        private async void PlayCutscene()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[<color=#83b3f6>LuaFlow</color>] Please start the game first.");
                return;
            }
            
            if (string.IsNullOrEmpty(_currentLuaScriptPath))
            {
                Debug.LogWarning("[<color=#83b3f6>LuaFlow</color>] Please select a Lua script first.");
                return;
            }
            
            await CutsceneManager.Instance.SkipCutscene(true);
            var cutsceneData = GetChapterAndName(_currentLuaScriptPath);
            _ = CutsceneManager.Instance.PlayCutsceneAsync(cutsceneData.Item1, cutsceneData.Item2);
            
            if (_enableLogToggle?.value == true) 
                Debug.Log($"[<color=#83b3f6>LuaFlow</color>] Play Cutscene! (Chap{cutsceneData.Item1} {cutsceneData.Item2}.lua)");
        }

        private void PauseScene()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
            _pauseButton.text = EditorApplication.isPaused ? "Resume" : "Pause";
        }

        private void StopScene()
        {
            _ = CutsceneManager.Instance.SkipCutscene();
            _pauseButton.text = "Pause";
            if (_enableLogToggle?.value == true) 
                Debug.Log($"[<color=#83b3f6>LuaFlow</color>] Stop Cutscene!");
        }

        #endregion

        #region File Watcher

        private void SetupFileWatcher(string luaFilePath)
        {
            _luaFileWatcher?.Dispose();
            
            if (string.IsNullOrEmpty(luaFilePath) || !File.Exists(luaFilePath))
            {
                Debug.LogWarning($"[<color=#83b3f6>LuaFlow</color>] Lua file does not exist: {luaFilePath}");
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(luaFilePath);
                string fileName = Path.GetFileName(luaFilePath);
                
                _normalizedWatchPath = Path.GetFullPath(luaFilePath).Replace('\\', '/');
                
                _luaFileWatcher = new FileSystemWatcher(directory!, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };

                _luaFileWatcher.Changed += OnLuaFileChanged;
                _luaFileWatcher.Error += OnFileWatcherError;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[<color=#83b3f6>LuaFlow</color>] Failed to setup file watcher: {ex.Message}");
                _luaFileWatcher?.Dispose();
                _luaFileWatcher = null;
            }
        }

        private void OnLuaFileChanged(object sender, FileSystemEventArgs e)
        {
            string eventPath = e.FullPath.Replace('\\', '/');
            if (eventPath != _normalizedWatchPath) return;
    
            EditorApplication.delayCall += () =>
            {
                if (!this) return;
        
                string fileName = Path.GetFileName(e.FullPath);
                if (_autoStartOnFileChangeToggle?.value == true)
                {
                    if (_enableLogToggle?.value == true) 
                        Debug.Log($"[<color=#83b3f6>LuaFlow</color>] Auto-Restarting due to file change: {fileName}");
                    
                    PlayCutscene();
                }
            };
        }
        
        private void OnFileWatcherError(object sender, ErrorEventArgs e)
        {
            EditorApplication.delayCall += () =>
            {
                Debug.LogError($"[<color=#83b3f6>LuaFlow</color>] File watcher error: {e.GetException().Message}");
            };
        }

        #endregion
        
        #region Handler
        
        private void OnPlayModeEntered(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall += () =>
                {
                    if (_autoStartOnSceneLoadToggle?.value == true) PlayCutscene();
                };
            }
        }
        
        private void OnLuaScriptSelected(string displayName)
        {
            if (_luaPathMap.TryGetValue(displayName, out string scriptPath))
            {
                _currentLuaScriptPath = scriptPath;
                EditorPrefs.SetString(SELECT_SCRIPT_PATH_KEY, scriptPath);
                SetupFileWatcher(scriptPath);
            }
        }
        
        #endregion

        #region Utility

        private void LoadSettings()
        {
            if (_autoStartOnSceneLoadToggle != null)
                _autoStartOnSceneLoadToggle.value = EditorPrefs.GetBool(AUTO_START_ON_SCENE_LOAD_KEY, false);
            if (_autoStartOnFileChangeToggle != null)
                _autoStartOnFileChangeToggle.value = EditorPrefs.GetBool(AUTO_START_ON_FILE_CHANGE_KEY, false);
            if (_enableLogToggle != null)
                _enableLogToggle.value = EditorPrefs.GetBool(ENABLE_LOG_KEY, true);
            
            _currentLuaScriptPath = EditorPrefs.GetString(SELECT_SCRIPT_PATH_KEY, "");
        }
        
        private (int, string) GetChapterAndName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string folderName = Path.GetFileName(Path.GetDirectoryName(filePath));
    
            var match = ChapRegex.Match(folderName!);
            string chapNumber = match.Success ? match.Groups[1].Value : "";
    
            return (Convert.ToInt32(chapNumber), fileName);
        }

        #endregion
    }
}
