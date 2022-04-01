using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SkinChangerRewrite
{
    public class HarmonyUser
    {
        public static HarmonyUser _instance;
        public HarmonyLib.Harmony harmony;
        public static void Start()
        {
            _instance = new HarmonyUser();
        }
        public static void Stop()
        {
            _instance.harmony.UnpatchSelf();
        }
        static HarmonyMethod GetMethod(string method)
        {
            return new HarmonyMethod(typeof(HarmonyUser).GetMethod(method));
        }
        public HarmonyUser()
        {
            harmony = new HarmonyLib.Harmony("Apotheosis.MuseDash.SkinChanger");

        }
    }
}
