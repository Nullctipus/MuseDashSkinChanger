using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.UI.Controls;
using HarmonyLib;
using Spine.Unity;
using System;
using System.Linq;
using Il2CppSystem.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using Spine;
using UnityEngine;

namespace SkinChanger
{
	public class Expression
	{
		public Expression(string anim, string[] audio, List<string> text, float weigh)
		{
			animName = anim;
			audioNames = audio;
			texts = text;
			weight = weigh;
		}

		public string animName;
		public string[] audioNames;
		public List<string> texts;
		public float weight;
	}

	public class Back : MelonLoader.MelonMod
	{
		#region vars

		private static readonly string[] characterShows = new string[]
		{
			"mainShow",
			"battleShow",
			//"feverShow",
			"victoryShow",
			"failShow"
		};

		private static readonly string[] elfinShows = new string[]
		{
			"mainShow",
			"prefab",
			"chipImage"
		};

		private static string _Name;

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
						"Skin Slave"
					}[Mathf.RoundToInt(UnityEngine.Random.Range(0, 6))];
				}

				return _Name;
			}
		}

		public static bool Forground = false;
		public static Back instance;
		private static Dictionary<int, string> TextureNameDictionary = new Dictionary<int, string>();

		private static Dictionary<CharacterApply, Spine.Skeleton> CharacterApplyToSkeleton =
			new Dictionary<CharacterApply, Spine.Skeleton>();

		private static Dictionary<int, Texture2D> CharacterApplyDefaultTextures = new Dictionary<int, Texture2D>();
		private static Dictionary<int, Texture2D> GraphicDefaultTextures = new Dictionary<int, Texture2D>();
		private static Dictionary<int, Material> CharacterApplyBackupMaterial = new Dictionary<int, Material>();
		private static Dictionary<int, Material> GraphicBackupMaterial = new Dictionary<int, Material>();

		#endregion vars

		public static HarmonyMethod GetPatch(string name)
		{
			return new HarmonyMethod(typeof(Back).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
		}

		public Back()
		{
			instance = this;
		}

		public static HarmonyLib.Harmony harmony;

		public override void OnApplicationStart()
		{
			harmony = new HarmonyLib.Harmony("Apotheosis.MuseDash.Skin");
			harmony.Patch(
				typeof(Assets.Scripts.GameCore.Managers.MainManager).GetMethod("InitLanguage",
					BindingFlags.NonPublic | BindingFlags.Instance), null, GetPatch(nameof(OnStart)));

			//Vict Fail Elfin
			harmony.Patch(AccessTools.Method(typeof(SkeletonGraphic), "Awake"), null, GetPatch(nameof(GraphicsApply)));
			//Char
			harmony.Patch(AccessTools.Method(typeof(SkeletonRenderer), "Awake"), null,
				GetPatch(nameof(SkeletonRendererApply)));
			//Expressions
			harmony.Patch(AccessTools.Method(typeof(CharacterExpression), "RefreshExpressions"),
				GetPatch(nameof(PreRefreshExpression)), GetPatch(nameof(RefreshExpression)));
			//Main Show
			harmony.Patch(AccessTools.Method(typeof(CharacterApply), "Awake"), null,
				GetPatch(nameof(CharacterApplyApply)));
		}

		public static Dictionary<string, List<string>> skins = new Dictionary<string, List<string>>();

		public static void Reload()
		{
			textures.Clear();
			skins.Clear();
			Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins"));
			foreach (string s in Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "Skins")))
			{
				List<string> skin = new List<string>();
				foreach (string s2 in Directory.GetDirectories(s))
				{
					skin.Add(s2);
				}

				skins.Add(new DirectoryInfo(s).Name, skin);
			}
		}

		public static string GetSkin(int character, int selected, bool elfin = false)
		{
			string name = Singleton<ConfigManager>.instance.GetConfigStringValue(
				elfin ? "eflin_English" : "character_English", character, elfin ? "name" : "cosName");
			try
			{
				return skins[name][selected];
			}
			catch
			{
				return "";
			}
		}

		public static bool IsModded(Texture2D texture)
		{
			foreach (var v in textures)
			{
				if (v.Value == texture)
					return true;
			}

			return false;
		}

		public static void AddLog(string Mod, string Class, object message)
		{
			MelonLogger.Msg($"[{Mod}:{Class}]: {message.ToString()}");
		}

		public static Material GetMaterial(Attachment a)
		{
			object rendererObject = null;
			var renderableAttachment = a.TryCast<IHasRendererObject>();
			if (renderableAttachment != null)
				rendererObject = renderableAttachment.RendererObject;

			if (rendererObject == null)
				return null;
			return (Material) ((AtlasRegion) rendererObject).page.rendererObject;
		}

		public static Texture2D GetTexture(string path, string PreName = "")
		{
			if (!textures.ContainsKey(path))
			{
				textures.Add(path, new Texture2D(1, 1, TextureFormat.RGBA32, false, true));
				Il2CppImageConversionManager.LoadImage(textures[path], File.ReadAllBytes(path));
				textures[path].filterMode = FilterMode.Bilinear;
				textures[path].mipMapBias = 0;
				textures[path].anisoLevel = 1;
				textures[path].Apply();
				textures[path].name = PreName;
				//Console.WriteLine(textures[path].height + ", " + textures[path].width);
			}

			return textures[path];
		}

		public static Il2CppSystem.Collections.Generic.List<T> ToIl2CPPList<T>(IEnumerable<T> items)
		{
			Il2CppSystem.Collections.Generic.List<T> ret = new Il2CppSystem.Collections.Generic.List<T>();
			foreach (var item in items)
			{
				ret.Add(item);
			}

			return ret;
		}

		private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

		#region PATCHES

		private static void OnStart()
		{
			GameObject gameObject = new GameObject("SkinMenu");
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.AddComponent<Front>();
		}

		private static void GraphicsApply(SkeletonGraphic __instance)
		{
			try
			{
				bool elfin = false;
				int change = Scan(__instance.gameObject);
				if (change == -1)
				{
					change = Scan(__instance.gameObject, true);
					elfin = true;
				}
#if DEBUG
				AddLog("GraphicsApply", change.ToString(), elfin);
#endif
				//Extract
				if (Front.extract && change > -1)
				{
					Extract((__instance.OverrideTexture ?? __instance.mainTexture) as Texture2D, change, elfin);
				}

				if (elfin)
				{
					if (change > -1 && Front.instance.selectedElfin[change] > -1)
					{
						try
						{
							AddLog("Skinchanger", "", change + " " + Front.instance.selectedElfin[change]);
							if (File.Exists(Path.Combine(GetSkin(change, Front.instance.selectedElfin[change], true),
									__instance.mainTexture.name + ".png")))
								__instance.OverrideTexture = GetTexture(Path.Combine(
									GetSkin(change, Front.instance.selectedElfin[change], true),
									__instance.mainTexture.name + ".png"));
							/*if (Forground) __instance.material.renderQueue = 3100;
							else __instance.material.renderQueue = 3000;*/
						}
						catch (Exception e)
						{
							AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace);
						}
					}
				}
				else if (change > -1 && Front.instance.selectedCharacter[change] > -1)
				{
					try
					{
						if (!GraphicDefaultTextures.ContainsKey(change) &&
							!IsModded((Texture2D) __instance.material.mainTexture))
							GraphicDefaultTextures.Add(change, (Texture2D) __instance.material.mainTexture);
						if (!GraphicBackupMaterial.ContainsKey(change))
							GraphicBackupMaterial.Add(change, new Material(__instance.material));
						__instance.material = new Material(GraphicBackupMaterial[change]);
						AddLog("Skinchanger", "", change + " " + Front.instance.selectedCharacter[change]);
						if (File.Exists(Path.Combine(GetSkin(change, Front.instance.selectedCharacter[change]),
								__instance.mainTexture.name + ".png")))
							__instance.OverrideTexture =
								GetTexture(
									Path.Combine(GetSkin(change, Front.instance.selectedCharacter[change]),
										__instance.mainTexture.name + ".png"), __instance.mainTexture.name);
						if (Forground) __instance.material.renderQueue = 3100;
						else __instance.material.renderQueue = 3000;
					}
					catch (Exception e)
					{
						AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace);
					}
				}
			}
			catch (Exception e)
			{
				AddLog("Skinchanger", "", e);
			}
		}

		private static void SkeletonRendererApply(SkeletonRenderer __instance)
		{
			//var __instance.meshRenderer = __instance.GetComponent<MeshRenderer>();
			int change = Scan(__instance.gameObject);

			//Extract
			if (Front.extract && change > -1)
			{
				try
				{
					if (__instance.meshRenderer != null)
					{
						if (!File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selectedCharacter[change]),
								__instance.meshRenderer.sharedMaterial.mainTexture.name + ".png")))
							Extract(__instance.meshRenderer.sharedMaterial.mainTexture as Texture2D, change);
					}
				}
				catch
				{
				}

				foreach (Spine.Slot s2 in __instance.Skeleton.Slots)
				{
					try
					{
						if (!File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selectedCharacter[change]),
								GetMaterial(s2.Attachment).mainTexture.name + ".png")))
							Extract(GetMaterial(s2.Attachment).mainTexture as Texture2D, change);
					}
					catch
					{
					}
				}
			}

			if (change > -1 && Front.instance.selectedCharacter[change] > -1)
			{
				if (__instance.meshRenderer != null)
				{
					if (Forground) __instance.meshRenderer.sharedMaterial.renderQueue = 3100;
					else __instance.meshRenderer.sharedMaterial.renderQueue = 3000;
					if (File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selectedCharacter[change]),
							__instance.meshRenderer.sharedMaterial.mainTexture.name + ".png")))
						__instance.meshRenderer.sharedMaterial.mainTexture = Back.GetTexture(
							Path.Combine(Back.GetSkin(change, Front.instance.selectedCharacter[change]),
								__instance.meshRenderer.sharedMaterial.mainTexture.name + ".png"),
							__instance.meshRenderer.sharedMaterial.mainTexture.name);
				}

				try
				{
					foreach (Spine.Slot s2 in __instance.Skeleton.Slots)
					{
						try
						{
							if (Forground) GetMaterial(s2.Attachment).renderQueue = 3100;
							else GetMaterial(s2.Attachment).renderQueue = 3000;
							if (File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selectedCharacter[change]),
									GetMaterial(s2.Attachment).mainTexture.name + ".png")))
								GetMaterial(s2.Attachment).mainTexture = Back.GetTexture(
									Path.Combine(Back.GetSkin(change, Front.instance.selectedCharacter[change]),
										GetMaterial(s2.Attachment).mainTexture.name + ".png"),
									GetMaterial(s2.Attachment).mainTexture.name);
						}
						catch
						{
						}
					}
				}
				catch (Exception e)
				{
					AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace);
				}
			}
		}

		private static void PreRefreshExpression(CharacterExpression __instance)
		{
			//Force refresh id
			__instance.m_CharacterIdx = -1;
		}

		private static void RefreshExpression(CharacterExpression __instance)
		{
			if (Front.extract)
			{
				List<Expression> expressions = new List<Expression>();
				foreach (var expression in CharacterExpression.m_Expressions)
				{
					expressions.Add(new Expression(expression.animName, expression.audioNames,
						expression.texts._items.ToList(), expression.weight));
				}

				File.WriteAllText(
					Path.Combine(Front.CurrentDirectory, "Skins",
						Singleton<ConfigManager>.instance["character_English"][__instance.m_CharacterIdx]["cosName"]
							.ToObject<string>(), "Default", "Expressions.json"),
					Newtonsoft.Json.JsonConvert.SerializeObject(expressions, Newtonsoft.Json.Formatting.Indented));
			}

			string skin = GetSkin(__instance.m_CharacterIdx, Front.instance.selectedCharacter[__instance.m_CharacterIdx]);
			if (File.Exists(Path.Combine(skin, "Expressions.json")))
			{
				List<Expression> expression =
					Newtonsoft.Json.JsonConvert.DeserializeObject<List<Expression>>(
						File.ReadAllText(Path.Combine(skin, "Expressions.json")));
				List<CharacterExpression.Expression> expressions = new List<CharacterExpression.Expression>();
				foreach (var v in expression)
				{
					expressions.Add(new CharacterExpression.Expression()
					{
						animName = v.animName,
						audioNames = v.audioNames,
						texts = ToIl2CPPList(v.texts),
						weight = v.weight
					});
				}

				CharacterExpression.m_Expressions.Clear();
				foreach (var ex in expressions)
				{
					CharacterExpression.m_Expressions.Add(ex);
				}
				
			}
		}


		private static void CharacterApplyApply(CharacterApply __instance)
		{
			try
			{
				if (!CharacterApplyToSkeleton.ContainsKey(__instance))
				{
					CharacterApplyToSkeleton.Add(__instance,
						__instance.gameObject.GetComponent<SkeletonMecanim>().skeleton);
				}

				Spine.Skeleton rend = CharacterApplyToSkeleton[__instance];
				//Extract
				if (Front.extract)
				{
					foreach (Spine.Slot s in rend.Slots)
					{
						try
						{
							if (GetMaterial(s.Attachment).mainTexture.name != "")
								Extract(GetMaterial(s.Attachment)?.mainTexture as Texture2D, __instance.m_Index);
						}
						catch
						{
						}
					}
				}

				//Reset To Default Skin
				if (__instance.m_Index > -1 && Front.instance.selectedCharacter[__instance.m_Index] == -1)
				{
					foreach (Spine.Slot slot in rend.Slots)
					{
						try
						{
							if (!TextureNameDictionary.ContainsKey(__instance.m_Index))
								TextureNameDictionary.Add(__instance.m_Index,
									GetMaterial(slot.Attachment).mainTexture.name + ".png");
							if (!CharacterApplyDefaultTextures.ContainsKey(__instance.m_Index) &&
								!IsModded((Texture2D) GetMaterial(slot.Attachment).mainTexture))
								CharacterApplyDefaultTextures.Add(__instance.m_Index,
									(Texture2D) GetMaterial(slot.Attachment).mainTexture);
							if (Forground) GetMaterial(slot.Attachment).renderQueue = 3100;
							else GetMaterial(slot.Attachment).renderQueue = 3000;
							GetMaterial(slot.Attachment).mainTexture = CharacterApplyDefaultTextures[__instance.m_Index];
						}
						catch
						{
						}
					}
				}

				//Set Modded Skin
				if (__instance.m_Index > -1 && Front.instance.selectedCharacter[__instance.m_Index] > -1)
				{
					foreach (Spine.Slot slot in rend.Slots)
					{
						try
						{
							if (!TextureNameDictionary.ContainsKey(__instance.m_Index))
								TextureNameDictionary.Add(__instance.m_Index,
									GetMaterial(slot.Attachment).mainTexture.name + ".png");
							if (!CharacterApplyBackupMaterial.ContainsKey(__instance.m_Index))
								CharacterApplyBackupMaterial.Add(__instance.m_Index,
									new Material(GetMaterial(slot.Attachment)));
							if (slot.Attachment is Spine.MeshAttachment)
							{
								try
								{
									((Spine.AtlasRegion) ((Spine.MeshAttachment) slot.Attachment).RendererObject).page
										.rendererObject = new Material(CharacterApplyBackupMaterial[__instance.m_Index]);
								}
								catch
								{
								}
							}

							if (File.Exists(Path.Combine(
									GetSkin(__instance.m_Index, Front.instance.selectedCharacter[__instance.m_Index]),
									TextureNameDictionary[__instance.m_Index])))
							{
								string skin = GetSkin(__instance.m_Index, Front.instance.selectedCharacter[__instance.m_Index]);
								string dir = Path.Combine(skin, TextureNameDictionary[__instance.m_Index]);
								Texture2D texture = GetTexture(dir);
								if (!CharacterApplyDefaultTextures.ContainsKey(__instance.m_Index) &&
									!IsModded((Texture2D) GetMaterial(slot.Attachment).mainTexture))
									CharacterApplyDefaultTextures.Add(__instance.m_Index,
										(Texture2D) GetMaterial(slot.Attachment).mainTexture);
								GetMaterial(slot.Attachment).mainTexture = texture;

								if (Forground) GetMaterial(slot.Attachment).renderQueue = 3100;
								else GetMaterial(slot.Attachment).renderQueue = 3000;
							}
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
		}

		#endregion PATCHES

		public static void ReloadAssets()
		{
			foreach (var c in Resources.FindObjectsOfTypeAll<CharacterApply>())
			{
				int index = Scan(c.gameObject);
				if (index != -1) CharacterApplyApply(c);
			}

			foreach (var c in Resources.FindObjectsOfTypeAll<SkeletonRenderer>())
			{
				SkeletonRendererApply(c);
			}

			foreach (var c in Resources.FindObjectsOfTypeAll<SkeletonGraphic>())
			{
				GraphicsApply(c);
			}
		}

		public static int Scan(GameObject obj, bool elfin = false)
		{
			string name = obj.name;
			if (obj.name == "default") name = obj.transform.parent.name;
			name = name.Replace("(Clone)", "");
			int change = -1;
			for (int i = 0;
				 i < Singleton<ConfigManager>.instance.GetJson(elfin ? "elfin" : "character", false).Count;
				 i++)
			{
				if (elfin)
					foreach (string s in elfinShows)
					{
						if (name.Contains(Singleton<ConfigManager>.instance["elfin"][i]["name"].ToObject<string>()
								.ToLower()))
							change = i;
						if (name.Contains(Singleton<ConfigManager>.instance.GetJson("elfin", false)[i][s]
								.ToObject<string>()))
							change = i;
					}
				else
					foreach (string s in characterShows)
					{
						if (name.Contains(Singleton<ConfigManager>.instance["character"][i]["cosName"]
								.ToObject<string>().ToLower()))
							change = i;
						if (name.Contains(Singleton<ConfigManager>.instance.GetJson("character", false)[i][s]
								.ToObject<string>()))
							change = i;
					}
			}
#if DEBUG
			AddLog("Scan", obj.name, change);
#endif
			return change;
		}

		public static void ExtractAll()
		{
			for (int i = 0; i < Singleton<ConfigManager>.instance["character_English"].Count; i++)
			{
				foreach (string s in characterShows)
				{
					try
					{
						AddLog("Extract",
							Singleton<ConfigManager>.instance["character_English"][i]["cosName"].ToObject<string>(), s);
						GameObject.Destroy(GameObject.Instantiate(
							PeroTools2.Commons.SingletonScriptableObject<PeroTools2.Resources.ResourcesManager>.instance
								.LoadFromName<GameObject>(
									Singleton<ConfigManager>.instance.GetConfigStringValue("character", i, s))));
					}
					catch (Exception e)
					{
						AddLog("Extract",
							Singleton<ConfigManager>.instance["character_English"][i]["cosName"].ToObject<string>(), e);
					}

					;
				}
			}

			for (int i = 0; i < Singleton<ConfigManager>.instance["elfin_English"].Count; i++)
			{
				foreach (string s in elfinShows)
				{
					GameObject.Instantiate(
						Singleton<PeroTools2.Resources.ResourcesManager>.instance.LoadFromName<GameObject>(
							Singleton<ConfigManager>.instance["elfin_English"][i][s].ToObject<string>()));
				}
			}
		}

		public static void Extract(Texture2D tex, int num, bool elfin = false)
		{
			Directory.CreateDirectory(Path.Combine(Front.CurrentDirectory, "Skins"));
			Directory.CreateDirectory(Path.Combine(Front.CurrentDirectory, "Skins",
				Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][
					elfin ? "name" : "cosName"].ToObject<string>()));
			Directory.CreateDirectory(Path.Combine(Front.CurrentDirectory, "Skins",
				Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][
					elfin ? "name" : "cosName"].ToObject<string>(), "Default"));
			try
			{
				string file = Path.Combine(
					Front.CurrentDirectory,
					"Skins",
					Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][
						elfin ? "name" : "cosName"].ToObject<string>(),
					"Default",
					tex.name + ".png");

				if (!File.Exists(file))
					File.WriteAllBytes(file, Il2CppImageConversionManager.EncodeToPNG(MakeReadable(tex)));
			}
			catch
			{
				AddLog("Skinchanger", "0", Front.CurrentDirectory);
				AddLog("Skinchanger", "1",
					Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"]);
				AddLog("Skinchanger", "2",
					Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num]);
				AddLog("Skinchanger", "3",
					Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][
						elfin ? "name" : "cosName"]);
				AddLog("Skinchanger", "4",
					Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][
						elfin ? "name" : "cosName"].ToObject<string>());
				AddLog("Skinchanger", "5", tex);
				AddLog("Skinchanger", "6", tex.name);
			}
		}

		public static Texture2D MakeReadable(Texture2D img)
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