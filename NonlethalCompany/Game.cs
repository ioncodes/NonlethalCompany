using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

using GameNetcodeStuff;

namespace NonlethalCompany
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.AllowPlayerDeath))]
    class GodModePatch
    {
        static bool Prefix()
        {
            Console.WriteLine("[Unlethal Company][God Mode] Calling GameNetcodeStuff::PlayerControllerB::AllowPlayerDeath() -> false");
            return false;
        }
    }

    public static class Game
    {
        public static void Initialize()
        {
            AllocConsole();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Application.logMessageReceivedThreaded += (string condition, string stackTrace, LogType type) =>
            {
                //Console.WriteLine($"[Lethal Company][Log] {condition} {stackTrace}");
            };

            Console.WriteLine("[Unlethal Company] DLL loaded!");

            var harmony = new Harmony("me.layle.nonlethalcompany");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [DllImport("kernel32")]
        static extern bool AllocConsole();
    }
}