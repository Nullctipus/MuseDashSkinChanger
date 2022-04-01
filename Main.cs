using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeroTools;
using PeroTools2;
using MelonLoader;
using Logger = SkinChangerRewrite.Logger;

namespace SkinChangerRewrite
{
    public class Main : MelonMod
    {
        public override void OnApplicationStart()
        {
            Logger.Log("Hello World", ConsoleColor.Magenta);
            Logger.Error("Hello World", ConsoleColor.Magenta);
            Logger.Warn("Hello World", ConsoleColor.Magenta);
        }
    }
}
