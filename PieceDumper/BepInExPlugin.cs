// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using JetBrains.Annotations;
using BepInEx.Logging;
using UnityEngine;
using BepInEx;

namespace PieceDumper
{
    [
        BepInPlugin(
            "EnvyGeeks.PieceDumper", "Piece Dumper", "0.1.0"
        )
    ]
    public class BepInExPlugin: BaseUnityPlugin
    {
        /**
         * Harmony
         * Provides a way for us to
         * conditionally patch everything
         * so tha we don't create unecessary
         * patches and issues
         */
        private Harmony harmony;

        /**
         * Config
         */
        private static ConfigEntry<bool> fireplaces;
        private static ConfigEntry<bool> cookingStations;
        private static ConfigEntry<bool> smelters;
        private static ConfigEntry<bool> pieces;

        /**
         * Logger
         */
        private static readonly ManualLogSource Log =
            BepInEx.Logging.Logger.CreateLogSource(
                "PieceDumper"
            );

        private void Awake()
        {
            harmony = new Harmony("EnvyGeeks.PieceDumper");
            fireplaces = Config.Bind("General", "fireplaces", true, "");
            cookingStations = Config.Bind("General", "cooking_stations", true, "");
            smelters = Config.Bind("General", "smelters", true, "");
            pieces = Config.Bind("General", "pieces", false, "");
            ConditionallyPatch();
        }

        private void ConditionallyPatch()
        {
            if (pieces.Value)
            {
                Log.LogDebug("Patching Piece");
                harmony.Patch(
                    original: AccessTools.Method(typeof(Piece), "Awake"),
                    postfix: new HarmonyMethod(
                        typeof(PieceAwakePatch),
                        "Postfix"
                    )
                );
            }

            if (fireplaces.Value)
            {
                Log.LogDebug("Patching Fireplace");
                var tFireplace = typeof(Fireplace);
                var tFireplacePatch = typeof(
                    FireplaceUpdatePatch
                );

                harmony.Patch(
                    original: AccessTools.Method(tFireplace, "UpdateFireplace"),
                    postfix: new HarmonyMethod(
                        tFireplacePatch, "Postfix"
                    )
                );
            }

            if (cookingStations.Value)
            {
                Log.LogDebug("Patching CookingStation");
                var tCooking = typeof(CookingStation);
                var tCookingPatch = typeof(
                    CookingStationUpdatePatch
                );

                harmony.Patch(
                    original: AccessTools.Method(tCooking, "UpdateCooking"),
                    postfix: new HarmonyMethod(
                        tCookingPatch, "Postfix"
                    )
                );
            }

            if (smelters.Value)
            {
                Log.LogDebug("Patching Smelters");
                var tSmelter = typeof(Smelter);
                var tSmelterPatch = typeof(
                    SmelterUpdatePatch
                );

                harmony.Patch(
                    original: AccessTools.Method(tSmelter, "UpdateSmelter"),
                    postfix: new HarmonyMethod(
                        tSmelterPatch, "Postfix"
                    )
                );
            }
        }

        private static void LogPiece(IPiece obj)
        {
            var internalName = obj.m_name;
            var type = obj.WrappedType().ToString();
            var name = obj.name;

            Log.LogDebug(
                "Found piece\n" +
                $"\tname: {name}\n" +
                $"\tm_name: {internalName}\n" +
                $"\ttype: {type}"
            );
        }

        [HarmonyPatch]
        public static class PieceAwakePatch
        {
            [UsedImplicitly]
            private static void Postfix(Piece __instance)
            {
                if (pieces.Value)
                {
                    LogPiece(
                        new PieceWrapper<Piece>(
                            __instance
                        )
                    );
                }
            }
        }

        [HarmonyPatch]
        static class FireplaceUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                Fireplace __instance, ZNetView ___m_nview
            ) {
                if (fireplaces.Value) {
                    LogPiece(
                        new PieceWrapper<Fireplace>(
                            __instance
                        )
                    );
                }
            }
        }

        [HarmonyPatch]
        static class CookingStationUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                CookingStation __instance, ZNetView ___m_nview
            ) {
                if (cookingStations.Value)
                {
                    LogPiece(
                        new PieceWrapper<CookingStation>(
                            __instance
                        )
                    );
                }
            }
        }

        [HarmonyPatch]
        static class SmelterUpdatePatch
        {
            [HarmonyPatch]
            [UsedImplicitly]
            private static void Postfix(
                Smelter __instance, ZNetView ___m_nview
            ) {
                if (smelters.Value) {
                    LogPiece(
                        new PieceWrapper<Smelter>(
                            __instance
                        )
                    );
                }
            }
        }
    }
}
