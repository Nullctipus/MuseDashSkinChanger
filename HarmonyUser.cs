using System;
using System.Reflection;
using System.Collections;
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
            Logger.Log("Started Harmony!");
            _instance = new HarmonyUser();
        }
        public static void Stop()
        {
            _instance.harmony.UnpatchSelf();
        }
        static HarmonyMethod GetMethod(string method)
        {
            return new HarmonyMethod(typeof(HarmonyUser).GetMethod(method,BindingFlags.Static|BindingFlags.NonPublic));
        }
        // Initialize and Patch
        public HarmonyUser()
        {
            harmony = new HarmonyLib.Harmony("Apotheosis.MuseDash.SkinChanger");
            harmony.Patch(typeof(PnlRoleSubControl).GetMethod("Init"),null, GetMethod(nameof(PnlRoleSubControlInitPrefix)));

#if DEBUG
            /*MethodScanner(typeof(PnlRole).GetMethod("OnApplyClicked"));
            MethodScanner(typeof(PnlRole).GetMethod("OnFsvIndexChanged"));
            MethodScanner(typeof(PnlRole).GetMethod("SkinSwitchedCallback"));
            MethodScanner(typeof(PnlRoleSubControl));*/
            harmony.Patch(typeof(PnlRoleSubControl).GetMethod("CreateObject"), GetMethod(nameof(GetStack)));
            harmony.Patch(typeof(PnlRoleSubControl).GetMethod("Init"), GetMethod(nameof(GetStack)));
#endif

        }
        private static void PnlRoleSubControlInitPrefix(PnlRoleSubControl __instance)
        {
#if DEBUG
            // Get a list of skins from a path
            // Add them to m_Skin
            // might also need to set m_CharacterApplys
#endif

        }

#if DEBUG

        //Methods to exclude from MethodPrinter
        static string[] exclude = new string[]
        {
            "Assets.Scripts.UI.Panels.SelectableFancyPanel PnlRole::get_fancyPanel()",
        };
        //Types I want to look more into
        static string[] dumpfurther = new string[]
        {
            "PnlRoleSubControl",
            "Assets.Scripts.PeroTools.Nice.Variables.VariableBehaviour",
            "Assets.Scripts.UI.Controls.CharacterApply",
            "Assets.Scripts.Database.RoleSkin",
        };
        HarmonyMethod _prefix;
        HarmonyMethod Prefix
        {
            get 
            { if (_prefix == null) 
                {
                    _prefix = GetMethod(nameof(MethodPrinter));
                }
                return _prefix;
            }
        }
        //Gets all method in a type and patches them
        void MethodScanner(Type t)
        {
            foreach(var method in t.GetMethods())
            {
               
                try
                {
                    if (exclude.Contains(method.Name) || method.DeclaringType != t) continue;
                    MethodScanner(method);
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }
        void MethodScanner(MethodBase m)
        {
            harmony.Patch(m, Prefix);
        }
        // Patch
        // Print all the parameters and method of said method
        private static void MethodPrinter(MethodBase __originalMethod, object[] __args)
        {
            Logger.Log(__originalMethod.FullDescription());
            string a = "";
            ParameterInfo[] args = __originalMethod.GetParameters();
            for (int i = 0; i < __args.Length; i++)
            {
                if (dumpfurther.Contains(args[i].ParameterType.FullDescription())) DumpType(__args[i]);
                if(__args is IEnumerable) Logger.Log(args[i].Name+ ": "+String.Join(" ,", __args[i] as IEnumerable));
                a+=$"\n{args[i].Name}({args[i].ParameterType.FullDescription()}): {__args[i]}";
            }
            Logger.Log(a);
        }
        static List<object> lastUsed = new List<object>();
        // Dumps all Parameters in a type
        // Recursive
        static void DumpType(object instance)
        {
            if (instance == null || lastUsed.Contains(instance)) return;
            lastUsed.Add(instance);
            string a = instance.GetType().FullDescription();
            var props = instance.GetType().GetProperties();
            Logger.Log(instance.ToString()+" "+props.Length);
            for (int i = 0; i < props.Length; i++)
            {
                try
                {
                    if (props[i].DeclaringType.Name == instance.GetType().Name)
                    {
                        object obj = props[i].GetValue(instance);
                        if (dumpfurther.Contains(obj.GetType().FullName)) DumpType(obj);
                        if ((obj is IEnumerable || obj is Il2CppSystem.Collections.IEnumerable) && !(obj is string)) Logger.Log(props[i].Name+ ": "+String.Join(" ,", obj as IEnumerable));
                        a += $"\n{props[i].Name}: {obj}";
                    }
                }
                catch(Exception e)
                {
                    Logger.Error(e);
                }
                
            }
            Logger.Log(a);
            lastUsed.Remove(instance);
        }
        
        // Get Il2cpp Stacktrace from method
        private static void GetStack()
        {
            Logger.Log(new System.Diagnostics.StackTrace().ToString());
        }
#endif
    }
}
