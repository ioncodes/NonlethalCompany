using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

using GameNetcodeStuff;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace NonlethalCompany
{
    internal static class Extensions
    {
        internal static bool IsCurrentPlayer(this PlayerControllerB player)
        {
            return player.actualClientId == (GameNetworkManager.Instance?.localPlayerController?.actualClientId ?? ulong.MaxValue);
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.AllowPlayerDeath))]
    class GodModePatch
    {
        static bool Prefix(PlayerControllerB __instance)
        {
            if (!__instance.IsCurrentPlayer())
                return true;

            Console.WriteLine("[Unlethal Company][God Mode] Calling PlayerControllerB::AllowPlayerDeath() -> false");
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    class UnlimitedStaminaPatch
    {
        static void Prefix(PlayerControllerB __instance)
        {
            if (!__instance.IsCurrentPlayer())
                return;

            if (__instance.isExhausted || __instance.sprintMeter < 0.4f)
            {
                Console.WriteLine($"[Unlethal Company][Unlimited Stamina] Stamina is about to run out...");
                Console.WriteLine($"[Unlethal Company][Unlimited Stamina] __instance.isExhausted = {__instance.isExhausted}; __instance.sprintMeter = {__instance.sprintMeter}");
                __instance.isExhausted = false;
                __instance.sprintMeter = 1f;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "Start")]
    class PlayerSpeedPatch
    {
        static void Prefix(PlayerControllerB __instance)
        {
            if (!__instance.IsCurrentPlayer())
                return;

            //Console.WriteLine($"[Unlethal Company][Player Speed] Enabling internal speed cheat...");
            //__instance.isSpeedCheating = true;

            Console.WriteLine($"[Unlethal Company][Player Speed] Increasing player speed...");
            __instance.movementSpeed = 5f; // default is 0.5f
        }
    }

    [HarmonyPatch(typeof(GrabbableObject), "Update")]
    class UnlimitedBatteryPatch
    {
        static void Prefix(GrabbableObject __instance)
        {
            if (__instance.insertedBattery != null && (__instance.insertedBattery.empty || __instance.insertedBattery.charge < 0.4f))
            {
                Console.WriteLine($"[Unlethal Company][Unlimited Battery] Battery is about to run out...");
                Console.WriteLine($"[Unlethal Company][Unlimited Battery] __instance.insertedBattery.empty = {__instance.insertedBattery.empty}; __instance.insertedBattery.charge = {__instance.insertedBattery.charge}");
                __instance.insertedBattery.empty = false;
                __instance.insertedBattery.charge = 1f;
            }
        }
    }

    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
    class EasyModePatch
    {
        static void Prefix(RoundManager __instance)
        {
            Console.WriteLine($"[Unlethal Company][Easy Mode] Scrap is spawning... Rigging scrap multiplier...");
            Console.WriteLine($"[Unlethal Company][Easy Mode] __instance.scrapAmountMultiplier = {__instance.scrapAmountMultiplier}; __instance.scrapValueMultiplier = {__instance.scrapValueMultiplier}");
            __instance.scrapAmountMultiplier = 2f;
            __instance.scrapValueMultiplier = 2f;
        }
    }

    public static class Cheats
    {
        public static void Initialize()
        {
            AllocConsole();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Application.logMessageReceivedThreaded += (string condition, string stackTrace, LogType type) =>
            {
                //Console.WriteLine($"[Lethal Company][Log] {condition} {stackTrace}");
            };

            Console.WriteLine("[Unlethal Company] About to load patches...");

            var harmony = new Harmony("me.layle.nonlethalcompany");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            foreach (var patch in harmony.GetPatchedMethods())
                Console.WriteLine($"[Unlethal Company] Patched method: {patch.DeclaringType.Name}::{patch.Name}");

            Console.WriteLine("[Unlethal Company] Patches loaded!");
        }

        [DllImport("kernel32")]
        static extern bool AllocConsole();
    }
}