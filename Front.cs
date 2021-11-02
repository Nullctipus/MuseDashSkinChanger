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
				selected = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json")));
			}
			else
			{
				for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
				{
					selected[i] = -1;
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
		public void SkinChangerWindow(int windowID)
		{
			try
			{
				scroll = GUILayout.BeginScrollView(scroll);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Reload"))
				{
					Back.Reload();
				}
				if (GUILayout.Button("Extract: " + extract))
				{
					extract = !extract;
				}
				if (GUILayout.Button("Deselect All"))
				{
					for (int i = 0; i < selected.Count; i++)
					{
						selected[i] = -1;
					}
				}
				if (GUILayout.Button(Preview ? "Hide Preview" : "Show Preview"))
				{
					Preview = !Preview;
				}
				if (GUILayout.Button("Download 2x (1GB)"))
				{
					Process.Start("https://github.com/BustR75/MuseDashSkinChanger/releases/download/1.5.1/Waifu2X_2X_CUnet_Level3_16Bit.7z");
				}
				if (GUILayout.Button("Forground " + Back.Forground))
				{
					Back.Forground = !Back.Forground;
				}
				if (GUILayout.Button("Reset Window"))
				{
					windowRect = new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 1.5f, Screen.height / 1.5f);
				}
				GUILayout.EndHorizontal();
				for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
				{
					GUILayout.BeginHorizontal();
					string costume = Singleton<ConfigManager>.instance.GetConfigStringValue("character_English", i, "cosName");
					GUILayout.Label(Singleton<ConfigManager>.instance.GetJson("character", true)[i]["cosName"].ToObject<string>(), GUILayout.Width(120));
					Color color = GUI.contentColor;
					Color color2 = GUI.backgroundColor;
					if (selected[i] == -1)
					{
						GUI.contentColor = Color.green;
					}
					if (Preview)
					{
						if (GUILayout.Button("default", GUILayout.Width(120), GUILayout.Height(120)))
						{
							selected[i] = -1;
							File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selected, Newtonsoft.Json.Formatting.Indented));
						}
					}
					else if (GUILayout.Button("default", GUILayout.Width(120)))
					{
						selected[i] = -1;
						File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selected, Newtonsoft.Json.Formatting.Indented));
					}
					GUI.contentColor = color;
					try
					{
						for (int j = 0; j < Back.skins[costume].Count; j++)
						{

							if (Preview && File.Exists(Path.Combine(Back.GetSkin(i, j), "Preview.png")))
							{
								if (selected[i] == j)
								{
									GUI.backgroundColor = Color.green;
								}
								if (GUILayout.Button(Back.GetTexture(Path.Combine(Back.GetSkin(i, j), "Preview.png")), GUILayout.Width(120), GUILayout.Height(120)))
								{
									selected[i] = j;
									File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selected, Newtonsoft.Json.Formatting.Indented));
								}
								GUI.backgroundColor = color2;
							}
							else
							{
								if (selected[i] == j)
								{
									GUI.contentColor = Color.green;
								}
								if (GUILayout.Button(new DirectoryInfo(Back.skins[costume][j]).Name, GUILayout.Width(120)))
								{
									selected[i] = j;
									File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selected, Newtonsoft.Json.Formatting.Indented));
								}
								GUI.contentColor = color;
							}
						}
					}
					catch { }
					GUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();
				GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
			}
			catch (Exception e)
			{
				ModLogger.AddLog(Back.instance.Name, "Menu", e);
			}
		}
		public Dictionary<int, int> selected = new Dictionary<int, int>();
	}
}
