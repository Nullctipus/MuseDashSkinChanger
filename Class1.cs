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
		static bool character = true;
		static bool elfin = false;
		public static Rect windowRect = new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 1.5f, Screen.height / 1.5f);
		public static bool ShowMenu = false;
		
		public void OnGUI()
        {
			if (ShowMenu)
			{
				windowRect = GUI.Window(0, windowRect, DoMyWindow, "BF Skinner");
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
		public void Start()
        {
			if (instance != null)
				Destroy(this);
			Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins"));
			if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "Skins\\Menukey.txt")))
            {
				File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "Skins\\Menukey.txt"),"Insert");
            }
			MenuKey = (KeyCode)Enum.Parse(typeof(KeyCode), File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Skins\\Menukey.txt")));
			StartCoroutine(BadFix());
		}
		KeyCode MenuKey = KeyCode.Insert;
		public void Update()
        {
			if(Input.GetKeyDown(MenuKey))
            {
				ModLogger.AddLog("", "", ShowMenu);
				ShowMenu = !ShowMenu;
            }
        }
		public static bool extract = false;
		public void DoMyWindow(int windowID)
		{
			try
			{
				if (instance == null)
				{
					for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
					{
						selected[i] = -1;
					}
					for (int i = 0; i < Singleton<ConfigManager>.instance["elfin_English"].Count; i++)
					{
						selectedelfin[i] = -1;
					}
					instance = this;
				}
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Reload"))
				{
					Back.Reload();
				}
				if (GUILayout.Button("Extract: " + extract))
				{
					extract = !extract;
				}
				GUILayout.EndHorizontal();
				character = GUILayout.Toggle(character, "Characters");
				if (character)
					for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
					{
						GUILayout.BeginHorizontal();
						string costume = Singleton<ConfigManager>.instance.GetJson("character", true)[i]["cosName"].ToObject<string>();
						GUILayout.Label(costume, GUILayout.Width(120));
						Color color = GUI.contentColor;
						if (selected[i] == -1)
						{
							GUI.contentColor = Color.green;
						}
						if (GUILayout.Button("default", GUILayout.Width(120)))
						{
							selected[i] = -1;
						}
						GUI.contentColor = color;
						try
						{
							for (int j = 0; j < Back.skins[costume].Count; j++)
							{
								if (selected[i] == j)
								{
									GUI.contentColor = Color.green;
								}
								if (GUILayout.Button(new DirectoryInfo(Back.skins[costume][j]).Name, GUILayout.Width(120)))
								{
									selected[i] = j;
								}
								GUI.contentColor = color;
							}
						}
						catch { }
						GUILayout.EndHorizontal();
					}
				/*
				elfin = GUILayout.Toggle(elfin, "Elfins");
				if (elfin)
					for (int i = 0; i < Singleton<ConfigManager>.instance["elfin_English"].Count; i++)
					{
						GUILayout.BeginHorizontal();
						string costume = Singleton<ConfigManager>.instance.GetJson("elfin", true)[i]["name"].ToObject<string>();
						GUILayout.Label(costume, GUILayout.Width(120));
						Color color = GUI.contentColor;
						if (selectedelfin[i] == -1)
						{
							GUI.contentColor = Color.green;
						}
						if (GUILayout.Button("default", GUILayout.Width(120)))
						{
							selectedelfin[i] = -1;
						}
						GUI.contentColor = color;
						try
						{
							for (int j = 0; j < Back.skins[costume].Count; j++)
							{
								if (selectedelfin[i] == j)
								{
									GUI.contentColor = Color.green;
								}
								if (GUILayout.Button(new DirectoryInfo(Back.skins[costume][j]).Name, GUILayout.Width(120)))
								{
									selectedelfin[i] = j;
								}
								GUI.contentColor = color;
							}
						}
						catch { }
						GUILayout.EndHorizontal();
					}*/
				GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
			}
            catch
            {
				// (:
            }
		}
		public Dictionary<int, int> selected = new Dictionary<int, int>();
		public Dictionary<int, int> selectedelfin = new Dictionary<int, int>();
	}
	public class Mod : IMod
    {
        public string Name => "Skin Changer";

        public string Description => "Change the textures on the characters";

        public string Author => "BustR75";

        public string HomePage => "https://github.com/BustR75/MuseDashSkinChanger";
        public static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(Mod).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

		private bool Check()
        {
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				using (var stream = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "MuseDash_Data\\Plugins\\steam_api.dll")))
				{
					var hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
					if (hash != "67365492ec0c8076840b1764ad2eca5f")
					{
						return true;
					}
				}
			}
			if (File.Exists(Path.Combine(Environment.CurrentDirectory, "MuseDash_Data\\Plugins\\cream_api.ini")) || File.Exists(Path.Combine(Environment.CurrentDirectory, "SmartSteamLoader.exe")) || File.Exists(Path.Combine(Environment.CurrentDirectory, "SmartSteamEmu.ini")))
				return true;
			return false;
		}
		public static Harmony harmony;

		public void DoPatching()
        {
            if (Check())
            {
				Process.Start("https://store.steampowered.com/app/774171");
				Application.OpenURL("steam://advertise/774171");
				Process.GetCurrentProcess().Kill();
				return;
			}
			harmony = new Harmony("Apotheosis.MuseDash.Skin");
			//VisManager:CalculateFrequencyResolution
			//Assets.Scripts.GameCore.Managers.MainManager:InitLanguage
			//harmony.Patch(typeof(VisManager).GetMethod("CalculateFrequencyResolution", BindingFlags.Public|BindingFlags.Instance), null, GetPatch(nameof(OnStart)));
			harmony.Patch(typeof(Assets.Scripts.GameCore.Managers.MainManager).GetMethod("InitLanguage", BindingFlags.NonPublic | BindingFlags.Instance), null, GetPatch(nameof(OnStart)));

			//harmony.Patch(typeof(GirlManager).GetMethod("Reset"), null, GetPatch(nameof(Skinnn)));
			//harmony.Patch(typeof(SpineActionController).GetMethod("Init"), null, GetPatch(nameof(ActionController)));
			//harmony.Patch(typeof(Assets.Scripts.UI.Controls.CharacterApply).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic), null, GetPatch(nameof(ShowFix)));

			harmony.Patch(typeof(SkeletonGraphic).GetMethods().First(x => x.Name == "Update" && x.GetParameters().Length == 0), null, GetPatch(nameof(GraphicsApply)));

			//harmony.Patch(typeof(SkeletonMecanim).GetMethod("Update"), null, GetPatch(nameof(MechanApply)));
			harmony.Patch(typeof(SkeletonAnimation).GetMethod("Update",BindingFlags.Public|BindingFlags.Instance,null,new Type[] { typeof(float)} ,null), null, GetPatch(nameof(AnimApply)));

			//public static void LoadScene(string sceneName, [DefaultValue("LoadSceneMode.Single")] LoadSceneMode mode)
			

		}
		private static void RES()
        {
			changed.Clear();
        }
		private static void OnStart()
        {
			GameObject gameObject = new GameObject("SkinMenu");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<Skins>();
        }
		private static string[] shows = new string[]
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
			bool changetype = false;
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
			if (change == -1)
			{
				changetype = true;
				for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("character", false).Count; i++)
				{
					try
					{
						if (__instance.gameObject.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("elfin", false)[i]["mainShow"].ToObject<string>())
							change = i;
					}
					catch { }
				}

			}
			//ext
			if (Skins.extract && change > -1)
			{
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\"));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance[changetype ? "elfin_English" : "character_English"][change][changetype ? "name" : "cosName"].ToObject<string>()));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance[changetype ? "elfin_English" : "character_English"][change][changetype ? "name" : "cosName"].ToObject<string>() + "\\Default"));
				string dir = Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance[changetype ? "elfin_English" : "character_English"][change][changetype ? "name" : "cosName"].ToObject<string>() + "\\Default");
				try
				{
					if (!File.Exists(Path.Combine(dir, __instance.OverrideTexture.name + ".png")))
						File.WriteAllBytes(Path.Combine(dir, __instance.OverrideTexture.name + ".png"), MakeReadable(__instance.OverrideTexture as Texture2D).EncodeToPNG());
				}
				catch { }
			}
			if (changetype && change > -1 && Skins.instance.selectedelfin[change] > -1)
			{
				try
				{
					if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), (__instance.mainTexture as Texture).name + ".png")))
						__instance.OverrideTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), (__instance.mainTexture as Texture).name + ".png"));
				}
				catch (Exception e) { ModLogger.AddLog("Skinchanger","",e.Message + "\n" + e.StackTrace); }
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
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\"));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>()));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default"));
				string dir = Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default");
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
		private static void MechanApply(SkeletonMecanim __instance)
		{
			try
			{
				if (changed.Contains(__instance.Skeleton))
					return;
				changed.Add(__instance.Skeleton);
				int change = -1;
				for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("character", false).Count; i++)
				{
					try
					{
						if (__instance.gameObject.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("elfin", false)[i]["prefab"].ToObject<string>())
							change = i;
					}
					catch { }
				}
				if (change > -1 && Skins.instance.selectedelfin[change] > -1)
				{
					try
					{
						foreach (Spine.Slot s in __instance.skeleton.Slots)
						{
							if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), s.Attachment.GetMaterial().mainTexture.name + ".png")))
								s.Attachment.GetMaterial().mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), s.Attachment.GetMaterial().mainTexture.name + ".png"));
						}
					}
					catch (Exception e) { Console.WriteLine(e.Message + "\n" + e.StackTrace); }
				}
			}
			catch (Exception e) { ModLogger.Debug(e.Message + "\n" + e.StackTrace); }
		}
		/*private static void MechanApply(SkeletonMecanim __instance)
        {
		try{
			if (changed.Contains(__instance.Skeleton))
				return;
			changed.Add(__instance.Skeleton);
			int change = -1;
			bool changetype = false;
			for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("character", false).Count; i++)
			{
				foreach (string s in shows)
				{
					if (__instance.transform.parent.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("character", false)[i][s].ToObject<string>())
						change = i;
				}
			}
			if (change == -1)
			{
				changetype = true;
				for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("character", false).Count; i++)
				{
					try
					{
						if (__instance.gameObject.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("elfin", false)[i]["mainShow"].ToObject<string>())
							change = i;
					}
					catch { }
				}

			}
			//ext
			if (Skins.extract && change > -1)
			{
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\"));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance[changetype ? "elfin_English" : "character_English"][change][changetype ? "name" : "cosName"].ToObject<string>()));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance[changetype ? "elfin_English" : "character_English"][change][changetype ? "name" : "cosName"].ToObject<string>() + "\\Default"));
				string dir = Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance[changetype ? "elfin_English" : "character_English"][change][changetype ? "name" : "cosName"].ToObject<string>() + "\\Default");
				
				foreach (Spine.Slot s in __instance.skeleton.Slots)
				{
					try
					{
						if (!File.Exists(Path.Combine(dir, s.Attachment.GetMaterial().mainTexture.name + ".png")))
							File.WriteAllBytes(Path.Combine(dir, s.Attachment.GetMaterial().mainTexture.name + ".png"), MakeReadable(s.Attachment.GetMaterial().mainTexture as Texture2D).EncodeToPNG());
					}
					catch { }
				}
			}
			if (changetype && change > -1 && Skins.instance.selectedelfin[change] > -1)
			{
				try
				{
					foreach (Spine.Slot s in __instance.skeleton.Slots)
					{
						try
						{
							if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), s.Attachment.GetMaterial().mainTexture.name + ".png")))
								s.Attachment.GetMaterial().mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), s.Attachment.GetMaterial().mainTexture.name + ".png"));
						}
						catch { }
					}
				}
				catch (Exception e) { ModLogger.AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace); }
			}
			if (change > -1 && Skins.instance.selected[change] > -1)
			{
				try
				{
					foreach (Spine.Slot s in __instance.skeleton.Slots)
					{
						try
						{
							if (!omfg.ContainsKey(change))
								omfg.Add(change, s.Attachment.GetMaterial().mainTexture.name + ".png");
							if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), omfg[change])))
								s.Attachment.GetMaterial().mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), omfg[change]));
						}
						catch { }
					}
				}
				catch (Exception e) { ModLogger.AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace); }
			}
		}
		catch{}
		}*/
		/*private static void MechanApply(SkeletonMecanim __instance)
		{
			if (changed.Contains(__instance.Skeleton))
				return;
			changed.Add(__instance.Skeleton);
			int change = -1;
			for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("elfin", false).Count; i++)
			{
				try
				{
					if (__instance.gameObject.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("elfin", false)[i]["prefab"].ToObject<string>())
						change = i;
				}
				catch { }
			}
			//ext
			if (Skins.extract && change > -1)
			{
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\"));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["elfin_English"][change]["name"].ToObject<string>()));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["elfin_English"][change]["name"].ToObject<string>() + "\\Default"));
				string dir = Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["elfin_English"][change]["name"].ToObject<string>() + "\\Default");
				foreach (Spine.Slot s in __instance.skeleton.Slots)
				{
					try
					{
						if (!File.Exists(Path.Combine(dir, s.Attachment.GetMaterial().mainTexture.name + ".png")))
							File.WriteAllBytes(Path.Combine(dir, s.Attachment.GetMaterial().mainTexture.name + ".png"), MakeReadable(s.Attachment.GetMaterial().mainTexture as Texture2D).EncodeToPNG());
					}
					catch { }
				}
			}
			if (change > -1 && Skins.instance.selectedelfin[change] > -1)
			{
				try
				{
					foreach (Spine.Slot s in __instance.skeleton.Slots)
					{
						try
						{
							if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), s.Attachment.GetMaterial().mainTexture.name + ".png")))
								s.Attachment.GetMaterial().mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selectedelfin[change], true), s.Attachment.GetMaterial().mainTexture.name + ".png"));
						}
						catch { }
					}
				}
				catch (Exception e) { ModLogger.AddLog("Skinchanger","",e.Message + "\n" + e.StackTrace); }
			}
		}*/
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
					Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\"));
					Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][___m_Index]["cosName"].ToObject<string>()));
					Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][___m_Index]["cosName"].ToObject<string>() + "\\Default"));
					string dir = Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][___m_Index]["cosName"].ToObject<string>() + "\\Default");
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
		/*private static void Skinnn(GirlManager __instance)
		{
            try { 
			CheckInstanciate(__instance.girl);
			//CheckInstanciate(__instance.girlGhost);
			}
			catch(Exception e)
            {
				ModLogger.AddLog("Uhh", "Fuck", e);
            }
		}
		private static void CheckInstanciate(GameObject __result)
		{
			int change = -1;
			for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson("character", false).Count; i++)
			{
				foreach (string s in shows)
				{
					if (__result.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("character", false)[i][s].ToObject<string>())
						change = i;
				}
			}
			Spine.Skeleton rend = __result.GetComponent<SkeletonMecanim>().skeleton;
			last = rend;
			//ext
			if (change > -1 && Skins.extract)
			{
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\"));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>()));
				Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default"));
				string dir = Path.Combine(Environment.CurrentDirectory, "Skins\\" + Singleton<ConfigManager>.instance["character_English"][change]["cosName"].ToObject<string>() + "\\Default");
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
			if (change > -1 && Skins.instance.selected[change] > -1)
			{
				try
				{
					foreach (Spine.Slot s in rend.Slots)
					{
						try
						{
							if (File.Exists(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s.Attachment.GetMaterial().mainTexture.name + ".png")))
								s.Attachment.GetMaterial().mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Skins.instance.selected[change]), s.Attachment.GetMaterial().mainTexture.name + ".png"));
						}
						catch { }
					}

				}
				catch (Exception e)
				{
					ModLogger.AddLog("Skinchanger","","Skeleton: " + e.Message + "\n" + e.StackTrace);
				}
			}
		}*/
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
