﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinChangerRewrite
{
    public static class Logger
    {
        
        static string PrintTime()
        {
            return DateTime.Now.ToString("hh:mm:ss.fff");
        }
        static void Outline(string text,ConsoleColor color,bool space = true)
        {
            Console.Write((space ? " " : "") +"[");
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
            Console.Write("]"+(space ? " " : ""));
        }
        static void WritePrep()
        {
            Outline(PrintTime(), ConsoleColor.Green,false);
            Outline("SkinChanger", ConsoleColor.Cyan);
        }
        public static void Log(string log,ConsoleColor color = ConsoleColor.White)
        {
            WritePrep();
            Console.ForegroundColor = color;
            Console.WriteLine($"{log}");
            Console.ResetColor();
        }
        public static void Error(string log, ConsoleColor color = ConsoleColor.Red)
        {
            WritePrep();
            Outline("Error", ConsoleColor.Red,false);
            Console.ForegroundColor = color;
            Console.WriteLine($" {log}");
            Console.ResetColor();
        }
        public static void Error(object obj, bool parse = false, bool stacktrace = false)
        {
            if (parse)
            {
                Error(Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented)+" "+(stacktrace ? new System.Diagnostics.StackTrace().ToString() : ""));
                return;
            }
            if (stacktrace) {
                Error($"{obj} {new System.Diagnostics.StackTrace()}");
                return;
            }
            Error(obj.ToString());
        }
        public static void Warn(string log, ConsoleColor color = ConsoleColor.Yellow)
        {
            WritePrep();
            Outline("Warning", ConsoleColor.Yellow,false);
            Console.ForegroundColor = color;
            Console.WriteLine($" {log}");
            Console.ResetColor();
        }
    }
}
