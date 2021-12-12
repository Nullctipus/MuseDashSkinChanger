using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using ModHelper;
using UnityEngine;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using System.Diagnostics;

namespace SkinChanger
{
    public class Front : MonoBehaviour
    {
        public static Front instance;
        static bool Preview = false;
        public static Rect windowRect = new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 1.5f, Screen.height / 1.5f);
        public static bool ShowMenu = false;
		public void OnGUI()
		{
			if (ShowMenu)
			{
				windowRect = GUI.Window(0, windowRect, SkinChangerWindow, Back.instance.Name);
			}
		}
		public static FilterMode Filter = FilterMode.Bilinear;
		public static string CurrentDirectory;
		public void Start()
		{
			if (!string.IsNullOrEmpty(Assembly.GetExecutingAssembly().Location))
				CurrentDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
			else
				CurrentDirectory = Environment.CurrentDirectory;

			if (instance != null)
				Destroy(this);
			Directory.CreateDirectory(Path.Combine(CurrentDirectory, "Skins"));
			if (!File.Exists(Path.Combine(CurrentDirectory, "Skins\\Menukey.txt")))
			{
				File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Menukey.txt"), "Insert");
			}
			MenuKey = (KeyCode)Enum.Parse(typeof(KeyCode), File.ReadAllText(Path.Combine(CurrentDirectory, "Skins\\Menukey.txt")));

			if (File.Exists(Path.Combine(CurrentDirectory, "Skins\\Saved.json")))
			{
				selectedCharacter = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json")));
			}
			else
			{
				for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
				{
					selectedCharacter[i] = -1;
				}
			}
			if (File.Exists(Path.Combine(CurrentDirectory, "Skins\\SavedElfin.json")))
			{
				selectedElfin = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText(Path.Combine(CurrentDirectory, "Skins\\SavedElfin.json")));
			}
			else
			{
				for (int i = 0; i < Singleton<ConfigManager>.instance["elfin_English"].Count; i++)
				{
					selectedElfin[i] = -1;
				}
			}
			Back.Reload();
			instance = this;
			windowRect = new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 1.5f, Screen.height / 1.5f);
		}
		KeyCode MenuKey = KeyCode.Insert;
		public void Update()
		{
			if (Input.GetKeyDown(MenuKey))
			{
				ShowMenu = !ShowMenu;
			}
		}
		public static bool extract = false;
		static Vector2 scroll = Vector2.zero;
		static bool showElfins, ShowCharacters = false;
		public void SaveSelection(int charIndex, int costumeIndex, bool elfin)
        {
			if (elfin)
			{
				selectedElfin[charIndex] = costumeIndex;
				File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\SavedElfin.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selectedElfin, Newtonsoft.Json.Formatting.Indented));
			}
			else
			{
				selectedCharacter[charIndex] = costumeIndex;
				File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selectedCharacter, Newtonsoft.Json.Formatting.Indented));
			}
		}
		public void DrawMenu(bool elfin = false)
        {
			for (int i = 0; i < Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"].Count; i++)
			{
				GUILayout.BeginHorizontal();
				string costume = Singleton<ConfigManager>.instance.GetConfigStringValue(elfin ? "elfin_English" : "character_English", i, elfin ? "name" : "cosName");
				GUILayout.Label(Singleton<ConfigManager>.instance.GetJson(elfin ? "elfin" : "character", true)[i][elfin?"name":"cosName"].ToObject<string>(), GUILayout.Width(120));
				Color color = GUI.contentColor;
				Color color2 = GUI.backgroundColor;
				if (elfin ? (selectedElfin[i] == -1) : (selectedCharacter[i] == -1))
				{
					GUI.contentColor = Color.green;
				}
				if (Preview)
				{
					if (GUILayout.Button("default", GUILayout.Width(120), GUILayout.Height(120)))
					{
						SaveSelection(i, -1, elfin);
						
					}
				}
				else if (GUILayout.Button("default", GUILayout.Width(120)))
				{

					SaveSelection(i, -1, elfin);
				}
				GUI.contentColor = color;
				try
				{
					for (int j = 0; j < Back.skins[costume].Count; j++)
					{

						if (Preview && File.Exists(Path.Combine(Back.GetSkin(i, j,elfin), "Preview.png")))
						{
							if (elfin ? (selectedElfin[i] == j) : (selectedCharacter[i] == j))
							{
								GUI.backgroundColor = Color.green;
							}
							if (GUILayout.Button(Back.GetTexture(Path.Combine(Back.GetSkin(i, j,elfin), "Preview.png")), GUILayout.Width(120), GUILayout.Height(120)))
							{
								SaveSelection(i, j, elfin);
							}
							GUI.backgroundColor = color2;
						}
						else
						{
							if (elfin ? (selectedElfin[i] == j) : (selectedCharacter[i] == j))
							{
								GUI.contentColor = Color.green;
							}
							if (GUILayout.Button(new DirectoryInfo(Back.skins[costume][j]).Name, GUILayout.Width(120)))
							{
								SaveSelection(i, j, elfin);
							}
							GUI.contentColor = color;
						}
					}
				}
				catch { }
				GUILayout.EndHorizontal();
			}
		}
		public void SkinChangerWindow(int windowID)
		{
			try
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Reload"))
				{
					Back.Reload();
					Back.ReloadAssets();
				}
				if (GUILayout.Button("Extract All"))
				{
					extract = true;
					Back.ExtractAll();
				}
				if (GUILayout.Button("Extract: " + extract))
				{
					extract = !extract;
				}
				if (GUILayout.Button("Deselect All"))
				{
					for (int i = 0; i < selectedCharacter.Count; i++)
					{
						selectedCharacter[i] = -1;
					}
					for (int i = 0; i < selectedElfin.Count; i++)
					{
						selectedElfin[i] = -1;
					}
				}
				if (GUILayout.Button(Preview ? "Hide Preview" : "Show Preview"))
				{
					Preview = !Preview;
				}
				/*if (GUILayout.Button("Download 2x (1GB)"))
				{
					Process.Start("https://github.com/BustR75/MuseDashSkinChanger/releases/download/1.5.1/Waifu2X_2X_CUnet_Level3_16Bit.7z");
				}*/
				if (GUILayout.Button("Forground " + Back.Forground))
				{
					Back.Forground = !Back.Forground;
				}
				if (GUILayout.Button("Reset Window")) 
				{
					windowRect = new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 1.5f, Screen.height / 1.5f);
				}
				GUILayout.EndHorizontal();
				scroll = GUILayout.BeginScrollView(scroll);
				ShowCharacters = GUILayout.Toggle(ShowCharacters, "Show Characters");
				if (ShowCharacters) DrawMenu(false);
				//showElfins = GUILayout.Toggle(showElfins, "Show Elfins");
				//if (showElfins) DrawMenu(true);
					
				GUILayout.EndScrollView();
				GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
			}
			/*catch (IndexOutOfRangeException e)
            {
				scroll = GUILayout.BeginScrollView(scroll);
				if (GUILayout.Button("An error has occured...\n You likely uninstalled a skin without deleting Saved.json/SavedElfin.json\n Try to fix? (Will overwrite both Saved.json and SavedElfin.json)"))
				{
					for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
					{
						selectedCharacter[i] = -1;
					}
					for (int i = 0; i < Singleton<ConfigManager>.instance["elfin_English"].Count; i++)
					{
						selectedElfin[i] = -1;
					}
					File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selectedCharacter, Newtonsoft.Json.Formatting.Indented));
					File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\SavedElfin.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selectedElfin, Newtonsoft.Json.Formatting.Indented));
				}
				if(GUILayout.Button("If the button above doesn't work click me to copy the error to clipboard\nMessage: " + e.Message + "\n Stacktrace: " + e.StackTrace))
                {
					System.Windows.Forms.Clipboard.SetText("Message: " + e.Message + "\n Stacktrace: " + e.StackTrace);
                }
				GUILayout.EndScrollView();
				ModLogger.AddLog(Back.instance.Name, "Menu", e);
			}*/
			catch (Exception e)
			{
				if (GUILayout.Button("An error has occured...\n You likely uninstalled a skin without deleting Saved.json/SavedElfin.json\n Try to fix? (Will overwrite both Saved.json and SavedElfin.json)"))
				{
					for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
					{
						selectedCharacter[i] = -1;
					}
					for (int i = 0; i < Singleton<ConfigManager>.instance["elfin_English"].Count; i++)
					{
						selectedElfin[i] = -1;
					}
					File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selectedCharacter, Newtonsoft.Json.Formatting.Indented));
					File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\SavedElfin.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selectedElfin, Newtonsoft.Json.Formatting.Indented));
				}
				GUILayout.Label(e.ToString());
				ModLogger.AddLog(Back.instance.Name, "Menu", e);
			}
		}
		public Dictionary<int, int> selectedCharacter = new Dictionary<int, int>();
		public Dictionary<int, int> selectedElfin = new Dictionary<int, int>();
	}
}
