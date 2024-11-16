// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Local
// ReSharper disable CheckNamespace

using HarmonyLib;
using System.Collections;
using BepInEx.Configuration;
using UnityUtils = Aides.Valheim.Utils.Unity;
using System.Collections.Generic;
using JetBrains.Annotations;
using BepInEx.Logging;
using UnityEngine;
using BepInEx;

namespace Dumpkin.Pieces
{
    [
        BepInPlugin(
            "dumpkin.pieces", "Dumpkin.Pieces", "0.1.0"
        )
    ]
    public class Pieces: BaseUnityPlugin
    {
        /**
         * Config
         */
        private static ConfigEntry<bool> fireplaces;
        private new static ConfigEntry<bool> enabled;
        private static ConfigEntry<bool> cookingStations;
        private static ConfigEntry<bool> uniqueOnly;
        private static ConfigEntry<bool> smelters;
        private static ConfigEntry<bool> pieces;

        /**
         * Internal
         */
        private static Pieces _self;
        private static Harmony _harmony;
        private static readonly List<string> pieceCache = new List<string>();
        private static readonly ManualLogSource Log =
            BepInEx.Logging.Logger.CreateLogSource(
                "dumpkin.pieces"
            );

        private void Awake()
        {
            _self = this;
            _harmony = new Harmony("dumpkin.pieces");
            enabled = Config.Bind("General", "enabled", true);
            fireplaces = Config.Bind("General", "fireplaces", true);
            cookingStations = Config.Bind("General", "cooking_stations", true);
            uniqueOnly = Config.Bind("General", "unique_only", true);
            smelters = Config.Bind("General", "smelters", true);
            pieces = Config.Bind("General", "pieces", false);
            if (enabled.Value) TryToPatchAll();
        }

        private void TryToPatchAll()
        {
            // --
            // Patch
            // --
            if (pieces.Value)
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Piece), "Awake"),
                    postfix: new HarmonyMethod(
                        typeof(PieceAwakePatch),
                        "Postfix"
                    )
                );

            // --
            // Patch
            // --
            if (fireplaces.Value)
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Fireplace), "UpdateFireplace"),
                    postfix: new HarmonyMethod(
                        typeof(FireplaceUpdatePatch),
                        "Postfix"
                    )
                );

            // --
            // Patch
            // --
            if (cookingStations.Value)
                _harmony.Patch(
                    original: AccessTools.Method(typeof(CookingStation), "UpdateCooking"),
                    postfix: new HarmonyMethod(
                        typeof(CookingStationUpdatePatch),
                        "Postfix"
                    )
                );

            // --
            // Patch
            // --
            if (smelters.Value)
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Smelter), "UpdateSmelter"),
                    postfix: new HarmonyMethod(
                        typeof(SmelterUpdatePatch),
                        "Postfix"
                    )
                );
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchAll(
                "dumpkin.pieces"
            );
        }

        private static void LogPiece(Object obj)
        {
            var m_name = UnityUtils.GetValue<string>(obj, "m_name");
            if (m_name != null)
            {
                if (uniqueOnly.Value)
                {
                    if (pieceCache.Contains(m_name)) return;
                    pieceCache.Add(
                        m_name
                    );
                }

                var instanceId = obj.GetInstanceID();
                var type = obj.ToString();
                Log.LogDebug(
                    $"Found {m_name}({instanceId}) of type {type}"
                );
            }
        }

        [HarmonyPatch]
        public static class PieceAwakePatch
        {
            [UsedImplicitly]
            private static void Postfix(Piece __instance)
            {
                if (!pieces.Value) return;
                LogPiece(
                    __instance
                );
            }
        }

        [HarmonyPatch]
        static class FireplaceUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                Fireplace __instance, ZNetView ___m_nview
            ) {
                if (!fireplaces.Value) return;
                Pieces._self.StartCoroutine(
                    LogCoroutine(
                        __instance, ___m_nview
                    )
                );
            }
        }

        [HarmonyPatch]
        static class CookingStationUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                CookingStation __instance, ZNetView ___m_nview
            ) {
                if (!cookingStations.Value) return;
                Pieces._self.StartCoroutine(
                    LogCoroutine(
                        __instance, ___m_nview
                    )
                );
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
                if (!smelters.Value) return;
                Pieces._self.StartCoroutine(
                    LogCoroutine(__instance, ___m_nview)
                );
            }
        }

        private static IEnumerator LogCoroutine<T>(T instance, ZNetView mNView)
        where T: MonoBehaviour
        {
            yield return new WaitForSeconds(1f);
            switch (instance)
            {
                case Fireplace _ when !fireplaces.Value:
                case CookingStation _ when !cookingStations.Value:
                case Smelter _ when !smelters.Value:
                    yield break;
                default:
                    LogPiece(
                        instance
                    );
                    break;
            }
        }
    }
}
