using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModHelper;
using HarmonyLib;
using Spine.Unity;
using UnityEngine;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Interface;
using UnityEngine.UI;
using System.Diagnostics;

namespace SkinChanger
{
	public class Skins : MonoBehaviour
	{
		public static Skins instance;
		static bool Preview = true;
		public static Rect windowRect = new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 1.5f, Screen.height / 1.5f);
		public static bool ShowMenu = false;

		static string _Name;
		public static string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_Name))
                {
					_Name = new string[]
					{
						"Skin Changer",
						"BF Skinner",
						"Can Has Skin",
						"Mr. Skinny",
						"Skin Head",
						"Very Skin Much Wow",
						"Skin Slave",
						//Wont Appear / just to mess with people
						"Cum Blast Me",
						"Stealing Account"
					}[Mathf.RoundToInt(UnityEngine.Random.Range(0, 5))];
                }
				return _Name;
            }
        }
		public void OnGUI()
        {
			if (ShowMenu)
			{
				windowRect = GUI.Window(0, windowRect, DoMyWindow, Name);
			}
		}
		// I am so sorry for this
		public IEnumerator BadFix()
        {
			for(; ; )
            {
				foreach(Assets.Scripts.UI.Controls.CharacterApply c in Resources.FindObjectsOfTypeAll<Assets.Scripts.UI.Controls.CharacterApply>())
                {
					Mod.Show(c);
                }
				yield return new WaitForSeconds(1);
            }
        }
		public static FilterMode Filter = FilterMode.Bilinear;
		public static string CurrentDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
		public void Start()
        {
			if (instance != null)
				Destroy(this);
			Directory.CreateDirectory(Path.Combine(CurrentDirectory, "Skins"));
			if (!File.Exists(Path.Combine(CurrentDirectory, "Skins\\Menukey.txt")))
            {
				File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Menukey.txt"),"Insert");
            }
			MenuKey = (KeyCode)Enum.Parse(typeof(KeyCode), File.ReadAllText(Path.Combine(CurrentDirectory, "Skins\\Menukey.txt")));
			StartCoroutine(BadFix());
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
			if(Input.GetKeyDown(MenuKey))
            {
				ShowMenu = !ShowMenu;
            }
        }
		public static bool extract = false;
		static Vector2 scroll = Vector2.zero;
		public void DoMyWindow(int windowID)
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
				/*if (GUILayout.Button("Filter Mode: " + Filter.ToString()))
				{
                    switch (Filter)
                    {
						case FilterMode.Point:
							Filter = FilterMode.Bilinear;
							break;
						case FilterMode.Bilinear:
							Filter = FilterMode.Trilinear;
							break;
						default:
							Filter = FilterMode.Point;
							break;
					}
				}*/
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
					else if(GUILayout.Button("default", GUILayout.Width(120)))
					{
						selected[i] = -1;
						File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selected, Newtonsoft.Json.Formatting.Indented));
					}
					GUI.contentColor = color;
					try
					{
						for (int j = 0; j < Back.skins[costume].Count; j++)
						{
							
							if (Preview)
							{
								if (File.Exists(Path.Combine(Back.GetSkin(i, j), "Preview.png")))
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
									if (GUILayout.Button(new DirectoryInfo(Back.skins[costume][j]).Name, GUILayout.Width(120), GUILayout.Height(120)))
									{
										selected[i] = j;
										File.WriteAllText(Path.Combine(CurrentDirectory, "Skins\\Saved.json"), Newtonsoft.Json.JsonConvert.SerializeObject(selected, Newtonsoft.Json.Formatting.Indented));
									}
									GUI.contentColor = color;
								}
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
			catch(Exception e)
			{
				ModLogger.AddLog(Name, "Menu", e);
			}
		}
		public Dictionary<int, int> selected = new Dictionary<int, int>();
	}
	public class Mod : IMod
    {
        public string Name => Skins.Name;

        public string Description => "Change the textures on the characters";

        public string Author => "BustR75";

        public string HomePage => "https://github.com/BustR75/MuseDashSkinChanger";
        public static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(Mod).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

		
		public static Harmony harmony;

		public void DoPatching()
        {
            
			harmony = new Harmony("Apotheosis.MuseDash.Skin");
			harmony.Patch(typeof(Assets.Scripts.GameCore.Managers.MainManager).GetMethod("InitLanguage", BindingFlags.NonPublic | BindingFlags.Instance), null, GetPatch(nameof(OnStart)));
			harmony.Patch(typeof(SkeletonGraphic).GetMethods().First(x => x.Name == "Update" && x.GetParameters().Length == 0), null, GetPatch(nameof(GraphicsApply)));
			harmony.Patch(typeof(SkeletonAnimation).GetMethod("Update",BindingFlags.Public|BindingFlags.Instance,null,new Type[] { typeof(float)} ,null), null, GetPatch(nameof(AnimApply)));
			

		}
		private static void OnStart()
        {
			GameObject gameObject = new GameObject("SkinMenu");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<Skins>();
        }
		private static readonly string[] shows = new string[]
		{
			"mainShow",
			"battleShow",
			"feverShow",
			"victoryShow",
			"failShow"
		};
		private static Spine.Skeleton last;
		public static List<Spine.Skeleton> changed = new List<Spine.Skeleton>();
		private static void GraphicsApply(SkeletonGraphic __instance)
		{
			if (changed.Contains(__instance.Skeleton))
				return;
			changed.Add(__instance.Skeleton);
			int change = -1;
			for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("character", false).Count; i++)
			{
				foreach (string s in shows)
				{
					if (__instance.gameObject.name.Replace("(Clone)", "").Contains(Singleton<ConfigManager>.instance["character"][i]["cosName"].ToObject<string>().ToLower()))
						change = i;
					if (__instance.gameObject.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("character", false)[i][s].ToObject<string>())
						change = i;
				}
			}
			//ext
			if (Skins.extract && change > -1)
			{
				Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\"));
				Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>()));
				Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default"));
				string dir = Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default");
				try
				{
					if (!File.Exists(Path.Combine(dir, __instance.OverrideTexture.name + ".png")))
						File.WriteAllBytes(Path.Combine(dir, __instance.OverrideTexture.name + ".png"), MakeReadable(__instance.OverrideTexture as Texture2D).EncodeToPNG());
				}
				catch { }
			}
			if (change > -1 && Skins.instance.selected[change] > -1)
			{
				try
				{

					if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), (__instance.mainTexture as Texture).name + ".png")))
						__instance.OverrideTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), (__instance.mainTexture as Texture).name + ".png"));
				}
				catch (Exception e) { ModLogger.AddLog("Skinchanger","",e.Message + "\n" + e.StackTrace); }
			}
		}
		static List<MeshRenderer> um = new List<MeshRenderer>();
		private static void AnimApply(SkeletonAnimation __instance)
		{
			var s = __instance.GetComponent<MeshRenderer>();
			if (um.Contains(s))
				return;
			um.Add(s);
			int change = -1;
			for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("character", false).Count; i++)
			{
				foreach (string s2 in shows)
				{
					if (__instance.gameObject.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("character", false)[i][s2].ToObject<string>())
						change = i;
				}
			}
			//ext
			if (Skins.extract && change > -1)
			{
				Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\"));
				Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>()));
				Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default"));
				string dir = Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default");
				try
				{
					if (s != null)
					{
						if (!File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s.sharedMaterial.mainTexture.name + ".png")))
							File.WriteAllBytes(Path.Combine(dir, s.sharedMaterial.mainTexture.name + ".png"), MakeReadable(s.sharedMaterial.mainTexture as Texture2D).EncodeToPNG());
					}
				}
				catch { }
				foreach (Spine.Slot s2 in __instance.Skeleton.Slots)
				{
					try
					{
						if (!File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s2.Attachment.GetMaterial().mainTexture.name + ".png")))
							File.WriteAllBytes(Path.Combine(dir, s2.Attachment.GetMaterial().mainTexture.name + ".png"), MakeReadable(s2.Attachment.GetMaterial().mainTexture as Texture2D).EncodeToPNG());
					}
					catch { }
				}
			}
			if (change > -1 && Skins.instance.selected[change] > -1)
			{
                if (s != null)
                {
					if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s.sharedMaterial.mainTexture.name + ".png")))
						s.sharedMaterial.mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s.sharedMaterial.mainTexture.name + ".png"));
				}
				try
				{
					foreach (Spine.Slot s2 in __instance.Skeleton.Slots)
					{
                        try
                        {
                            if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s2.Attachment.GetMaterial().mainTexture.name + ".png")))
                                s2.Attachment.GetMaterial().mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s2.Attachment.GetMaterial().mainTexture.name + ".png"));
                        }
                        catch { }

					}
				}
				catch (Exception e) { ModLogger.AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace); }
			}
		}
		static FieldInfo index;
		static FieldInfo Index
        {
            get
            {
				if(index == null)
                {
					index = typeof(Assets.Scripts.UI.Controls.CharacterApply).GetField("m_Index", BindingFlags.Instance | BindingFlags.NonPublic);
                }
				return index;
            }
        }
		public static void Show(Assets.Scripts.UI.Controls.CharacterApply __instance)
        {
			ShowFix(__instance, (int)Index.GetValue(__instance));
        }
		static Dictionary<int, string> omfg = new Dictionary<int, string>();
		private static void ShowFix(Assets.Scripts.UI.Controls.CharacterApply __instance, int ___m_Index)
		{
			try
			{
				Spine.Skeleton rend = __instance.gameObject.GetComponent<SkeletonMecanim>().skeleton;
				last = rend;
				//ext
				if (Skins.extract)
				{
					Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\"));
					Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][___m_Index]["cosName"].ToObject<string>()));
					Directory.CreateDirectory(Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][___m_Index]["cosName"].ToObject<string>() + "\\Default"));
					string dir = Path.Combine(Skins.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][___m_Index]["cosName"].ToObject<string>() + "\\Default");
					foreach (Spine.Slot s in rend.Slots)
					{
						try
						{
							if (!File.Exists(Path.Combine(dir, s.Attachment.GetMaterial().mainTexture.name + ".png")))
								File.WriteAllBytes(Path.Combine(dir, s.Attachment.GetMaterial().mainTexture.name + ".png"), MakeReadable(s.Attachment.GetMaterial().mainTexture as Texture2D).EncodeToPNG());
						}
						catch { }
					}
				}
				if (___m_Index > -1 && Skins.instance.selected[___m_Index] > -1)
				{
					try
					{
						foreach (Spine.Slot s in rend.Slots)
						{
							try
							{
								if (!omfg.ContainsKey(___m_Index))
									omfg.Add(___m_Index, s.Attachment.GetMaterial().mainTexture.name + ".png");
								if (File.Exists(Path.Combine(Back.GetSkin(___m_Index, Skins.instance.selected[___m_Index]), omfg[___m_Index])))
								{
									string skin = Back.GetSkin(___m_Index, Skins.instance.selected[___m_Index]);
									string dir = Path.Combine(skin, omfg[___m_Index]);
									Texture2D texture = Back.GetTexture(dir);
									s.Attachment.GetMaterial().mainTexture = texture;
								}
							}
							catch (NullReferenceException) { }
							catch (Exception e)
							{
							}
						}

					}
					catch (Exception e)
					{
					}
				}
			}
			catch { }
		}
		static Texture2D MakeReadable(Texture2D img)
		{
			img.filterMode = FilterMode.Point;
			RenderTexture rt = RenderTexture.GetTemporary(img.width, img.height);
			rt.filterMode = FilterMode.Point;
			RenderTexture.active = rt;
			UnityEngine.Graphics.Blit(img, rt);
			Texture2D img2 = new Texture2D(img.width, img.height);
			img2.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
			img2.Apply();
			RenderTexture.active = null;
			return img2;
		}
	}
}
