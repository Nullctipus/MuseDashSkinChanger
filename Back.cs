using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;

namespace SkinChanger
{
    public static class Back
    {
        public static Dictionary<string, List<string>> skins = new Dictionary<string, List<string>>();
        public static void Reload()
        {
            textures.Clear();
            skins.Clear();
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Skins"));
            foreach(string s in Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "Skins")))
            {
                List<string> skin = new List<string>();
                foreach (string s2 in Directory.GetDirectories(s))
                {
                    skin.Add(s2);
                }
                skins.Add(new DirectoryInfo(s).Name, skin);
            }
        }
        public static string GetSkin(int character,int selected)
        {
            string name = Singleton<ConfigManager>.instance.GetConfigStringValue("character_English", character, "cosName");
            //string name = Singleton<ConfigManager>.instance["character_English"][character]["cosName"].ToObject<string>();
            try
            {
                return skins[name][selected];
            }
            catch
            {
                return "";
            }

        }
        public static Texture2D GetTexture(string path)
        {
            if (!textures.ContainsKey(path))
            {
                textures.Add(path, new Texture2D(1024,2048,TextureFormat.RGBA32,false,true));
                textures[path].LoadImage(File.ReadAllBytes(path));
                textures[path].filterMode = FilterMode.Bilinear;
                textures[path].mipMapBias = 0;
                textures[path].Apply();
                Console.WriteLine(textures[path].height + ", " + textures[path].width);
            }
            return textures[path];
        }
        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    }
}
