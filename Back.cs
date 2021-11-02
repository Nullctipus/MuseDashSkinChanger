using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using ModHelper;
using HarmonyLib;
using Spine.Unity;
using UnityEngine;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.PeroTools.Managers;

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
    public class Back : IMod
    {
        #region vars
        private static readonly string[] shows = new string[]
        {
            "mainShow",
            "battleShow",
            "feverShow",
            "victoryShow",
            "failShow"
        };
        static string _Name;
        public string Name
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
        static Dictionary<int, string> TextureNameDictionary = new Dictionary<int, string>();
        static Dictionary<CharacterApply, Spine.Skeleton> CharacterApplyToSkeleton = new Dictionary<CharacterApply, Spine.Skeleton>();
        static Dictionary<int, Texture2D> CharacterApplyDefaultTextures = new Dictionary<int, Texture2D>();
        static Material CharacterApplyBackupMaterial;

        public string Version = "1.7.0";
        public string Description => "Change the textures on the characters";

        public string Author => "BustR75";

        public string HomePage => "https://github.com/BustR75/MuseDashSkinChanger";
#endregion
        public static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(Back).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

        public Back()
        {
            instance = this;
        }

        public static Harmony harmony;

        public void DoPatching()
        {

            harmony = new Harmony("Apotheosis.MuseDash.Skin");
            harmony.Patch(typeof(Assets.Scripts.GameCore.Managers.MainManager).GetMethod("InitLanguage", BindingFlags.NonPublic | BindingFlags.Instance), null, GetPatch(nameof(OnStart)));

            harmony.Patch(AccessTools.Method(typeof(SkeletonGraphic),"Awake"), null, GetPatch(nameof(GraphicsApply)));
            harmony.Patch(AccessTools.Method(typeof(SkeletonAnimation),"Awake"), null, GetPatch(nameof(SkeletonAnimationApply)));
            harmony.Patch(AccessTools.Method(typeof(CharacterExpression),"RefreshExpressions"), GetPatch(nameof(PreRefreshExpression)), GetPatch(nameof(RefreshExpression)));
            harmony.Patch(AccessTools.Method(typeof(CharacterApply),"Awake"), null, GetPatch(nameof(CharacterApplyApply)));


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
        public static string GetSkin(int character, int selected)
        {
            string name = Singleton<ConfigManager>.instance.GetConfigStringValue("character_English", character, "cosName");
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
        public static string Combine(params string[] paths)
        {
            string s2 = paths[0];
            for(int i =1; i<paths.Length;i++)
            {
                s2 = Path.Combine(s2, paths[i]);
            }
            return s2;
        }
        public static Texture2D GetTexture(string path)
        {
            if (!textures.ContainsKey(path))
            {
                textures.Add(path, new Texture2D(1, 1, TextureFormat.RGBA32, false, true));
                textures[path].LoadImage(File.ReadAllBytes(path));
                textures[path].filterMode = FilterMode.Bilinear;
                textures[path].mipMapBias = 0;
                textures[path].anisoLevel = 1;
                textures[path].Apply();
                Console.WriteLine(textures[path].height + ", " + textures[path].width);
            }
            return textures[path];
        }
        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        #region PATCHES
        private static void OnStart()
        {
            GameObject gameObject = new GameObject("SkinMenu");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Front>();
        }
        private static void GraphicsApply(SkeletonGraphic __instance)
        {
            int change = Scan(__instance.gameObject);
            //Extract
            if(Front.extract && change > -1)
            {
                Extract(__instance.OverrideTexture as Texture2D, change);
            }
            if (change > -1 && Front.instance.selected[change] > -1)
            {
                try
                {
                    if (File.Exists(Path.Combine(GetSkin(change, Front.instance.selected[change]), __instance.mainTexture.name + ".png")))
                        __instance.OverrideTexture = GetTexture(Path.Combine(GetSkin(change, Front.instance.selected[change]), __instance.mainTexture.name + ".png"));
                    if (Forground) __instance.material.renderQueue = 3100;
                    else __instance.material.renderQueue = 3000;
                }
                catch (Exception e) { ModLogger.AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace); }
            }
        }
        private static void SkeletonAnimationApply(SkeletonAnimation __instance)
        {
            var s = __instance.GetComponent<MeshRenderer>();
            int change = Scan(__instance.gameObject);

            //Extract
            if (Front.extract && change > -1)
            {
                try
                {
                    if (s != null)
                    {
                        if (!File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selected[change]), s.sharedMaterial.mainTexture.name + ".png")))
                            Extract(s.sharedMaterial.mainTexture as Texture2D, change);
                    }
                }
                catch { }
                foreach (Spine.Slot s2 in __instance.Skeleton.Slots)
                {
                    try
                    {
                        if (!File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selected[change]), s2.Attachment.GetMaterial().mainTexture.name + ".png")))
                            Extract(s2.Attachment.GetMaterial().mainTexture as Texture2D,change);
                    }
                    catch { }
                }
            }
            if (change > -1 && Front.instance.selected[change] > -1)
            {
                if (s != null)
                {
                    if (Forground) s.sharedMaterial.renderQueue = 3100;
                    else s.sharedMaterial.renderQueue = 3000;
                    if (File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selected[change]), s.sharedMaterial.mainTexture.name + ".png")))
                        s.sharedMaterial.mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Front.instance.selected[change]), s.sharedMaterial.mainTexture.name + ".png"));
                }
                try
                {
                    foreach (Spine.Slot s2 in __instance.Skeleton.Slots)
                    {
                        try
                        {
                            if (Forground) s2.Attachment.GetMaterial().renderQueue = 3100;
                            else s2.Attachment.GetMaterial().renderQueue = 3000;
                            if (File.Exists(Path.Combine(Back.GetSkin(change, Front.instance.selected[change]), s2.Attachment.GetMaterial().mainTexture.name + ".png")))
                                s2.Attachment.GetMaterial().mainTexture = Back.GetTexture(Path.Combine(Back.GetSkin(change, Front.instance.selected[change]), s2.Attachment.GetMaterial().mainTexture.name + ".png"));
                        }
                        catch { }

                    }
                }
                catch (Exception e) { ModLogger.AddLog("Skinchanger", "", e.Message + "\n" + e.StackTrace); }
            }
        }
        private static void PreRefreshExpression(ref int ___m_CharacterIdx)
        {
            //Force refresh id
            ___m_CharacterIdx = -1;
        }
        private static void RefreshExpression(ref List<CharacterExpression.Expression> ___m_Expressions, int ___m_CharacterIdx)
        {
            if (Front.extract)
            {
                List<Expression> expressions = new List<Expression>();
                foreach (var expression in ___m_Expressions)
                {
                    expressions.Add(new Expression(expression.animName, expression.audioNames, expression.texts, expression.weight));
                }
                File.WriteAllText(Combine(Front.CurrentDirectory, "Skins", Singleton<ConfigManager>.instance["character_English"][___m_CharacterIdx]["cosName"].ToObject<string>(), "Default", "Expressions.json"), Newtonsoft.Json.JsonConvert.SerializeObject(expressions, Newtonsoft.Json.Formatting.Indented));
            }
            string skin = GetSkin(___m_CharacterIdx, Front.instance.selected[___m_CharacterIdx]);
            if (File.Exists(Path.Combine(skin, "Expressions.json")))
            {
                List<Expression> expression = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Expression>>(File.ReadAllText(Path.Combine(skin, "Expressions.json")));
                List<CharacterExpression.Expression> expressions = new List<CharacterExpression.Expression>();
                foreach (var v in expression)
                {
                    expressions.Add(new CharacterExpression.Expression()
                    {
                        animName = v.animName,
                        audioNames = v.audioNames,
                        texts = v.texts,
                        weight = v.weight
                    });
                }
                ___m_Expressions.Clear();
                ___m_Expressions.AddRange(expressions);
            }
        }
        private static void CharacterApplyApply(CharacterApply __instance, int ___m_Index)
        {
            try
            {
                if (!CharacterApplyToSkeleton.ContainsKey(__instance))
                {
                    CharacterApplyToSkeleton.Add(__instance, __instance.gameObject.GetComponent<SkeletonMecanim>().skeleton);
                }
                Spine.Skeleton rend = CharacterApplyToSkeleton[__instance];
                //Extract
                if (Front.extract)
                {
                    foreach (Spine.Slot s in rend.Slots)
                    {
                        try
                        {
                            if (s.Attachment.GetMaterial().mainTexture.name != "")
                                Extract(s?.Attachment?.GetMaterial()?.mainTexture as Texture2D, ___m_Index);
                        }
                        catch { }
                    }
                }
                //Reset To Default Skin
                if (___m_Index > -1 && Front.instance.selected[___m_Index] == -1)
                {
                    foreach (Spine.Slot slot in rend.Slots)
                    {
                        try
                        {
                            if (!TextureNameDictionary.ContainsKey(___m_Index))
                                TextureNameDictionary.Add(___m_Index, slot.Attachment.GetMaterial().mainTexture.name + ".png");

                            if (!CharacterApplyDefaultTextures.ContainsKey(___m_Index) && !IsModded((Texture2D)slot.Attachment.GetMaterial().mainTexture))
                                CharacterApplyDefaultTextures.Add(___m_Index, (Texture2D)slot.Attachment.GetMaterial().mainTexture);
                            if (Forground) slot.Attachment.GetMaterial().renderQueue = 3100;
                            else slot.Attachment.GetMaterial().renderQueue = 3000;
                            slot.Attachment.GetMaterial().mainTexture = CharacterApplyDefaultTextures[___m_Index];
                        }
                        catch { }
                    }
                }
                //Set Modded Skin
                if (___m_Index > -1 && Front.instance.selected[___m_Index] > -1)
                {
                    foreach (Spine.Slot slot in rend.Slots)
                    {
                        try
                        {
                            if (CharacterApplyBackupMaterial == null)
                                CharacterApplyBackupMaterial = new Material(slot.Attachment.GetMaterial());
                            if (slot.Attachment is Spine.MeshAttachment)
                            {
                                try
                                {
                                    ((Spine.AtlasRegion)((Spine.MeshAttachment)slot.Attachment).RendererObject).page.rendererObject = new Material(CharacterApplyBackupMaterial);
                                }
                                catch { }
                            }
                            if (!TextureNameDictionary.ContainsKey(___m_Index))
                                TextureNameDictionary.Add(___m_Index, slot.Attachment.GetMaterial().mainTexture.name + ".png");
                            if (File.Exists(Path.Combine(GetSkin(___m_Index, Front.instance.selected[___m_Index]), TextureNameDictionary[___m_Index])))
                            {
                                string skin = GetSkin(___m_Index, Front.instance.selected[___m_Index]);
                                string dir = Path.Combine(skin, TextureNameDictionary[___m_Index]);
                                Texture2D texture = GetTexture(dir);
                                if (!CharacterApplyDefaultTextures.ContainsKey(___m_Index) && !IsModded((Texture2D)slot.Attachment.GetMaterial().mainTexture))
                                    CharacterApplyDefaultTextures.Add(___m_Index, (Texture2D)slot.Attachment.GetMaterial().mainTexture);
                                slot.Attachment.GetMaterial().mainTexture = texture;

                                if (Forground) slot.Attachment.GetMaterial().renderQueue = 3100;
                                else slot.Attachment.GetMaterial().renderQueue = 3000;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
        #endregion
        static int Scan(GameObject obj, bool elfin = false)
        {
            int change = -1;
            for (int i = 0; i < Singleton<ConfigManager>.instance.GetJson(elfin ? "elfin" : "character", false).Count; i++)
            {
                if (elfin && obj.name.Replace("(Clone)", "").Contains(Singleton<ConfigManager>.instance["elfin"][i]["prefab"].ToObject<string>().ToLower()))
                    change = i;
                else
                    foreach (string s in shows)
                    {
                        if (obj.name.Replace("(Clone)", "").Contains(Singleton<ConfigManager>.instance["character"][i]["cosName"].ToObject<string>().ToLower()))
                            change = i;
                        if (obj.name.Replace("(Clone)", "") == Singleton<ConfigManager>.instance.GetJson("character", false)[i][s].ToObject<string>())
                            change = i;
                    }
            }
            return change;
        }
        static void Extract(Texture2D tex, int num, bool elfin = false)
        {

            Directory.CreateDirectory(Combine(Front.CurrentDirectory, "Skins"));
            Directory.CreateDirectory(Combine(Front.CurrentDirectory, "Skins", Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][elfin ? "prefab" : "cosName"].ToObject<string>()));
            Directory.CreateDirectory(Combine(Front.CurrentDirectory, "Skins", Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][elfin ? "prefab" : "cosName"].ToObject<string>(), "Default"));

            string file = Combine(Front.CurrentDirectory,
                    "Skins\\" + Singleton<ConfigManager>.instance[elfin ? "elfin_English" : "character_English"][num][elfin ? "prefab" : "cosName"].ToObject<string>() + "\\Default",
                    tex.name + ".png") ;

            if (!File.Exists(file))
                File.WriteAllBytes(file, MakeReadable(tex).EncodeToPNG());
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
