// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using HarmonyLib;
using System.Collections;
using JetBrains.Annotations;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using BepInEx;

namespace Dumpkin.Feedable
{
    [
        BepInPlugin(
            "dumpkin.feedable", "Dumpkin Feedable", "0.1.0"
        )
    ]
    public class Feedable: BaseUnityPlugin
    {
        /**
         * Config
         */
        private new static ConfigEntry<bool> enabled;
        private static ConfigEntry<bool> cookingStations;
        private static ConfigEntry<bool> smelters;

        /**
         * Internal
         */
        private static Feedable _self;
        private static Harmony _harmony;
        private static readonly List<string> alreadySeen = new List<string>();
        private static readonly ManualLogSource _logger =
            BepInEx.Logging.Logger.CreateLogSource(
                "dumpkin.feedable"
            );

        private void Awake()
        {
            _self = this;
            _harmony = new Harmony("dumpkin.feedable");
            smelters = Config.Bind("General", "smelters", true);
            cookingStations = Config.Bind("General", "cooking_stations", true);
            enabled = Config.Bind("General", "enabled", true);
            if (enabled.Value) TryToPatchAll();
        }

        private void TryToPatchAll()
        {
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
                "dumpkin.feedable"
            );
        }

        [HarmonyPatch]
        static class CookingStationUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                CookingStation __instance, ZNetView ___m_nview
            ) {
                _self.StartCoroutine(
                    DumpFeed(
                        __instance
                    )
                );
            }
        }

        [HarmonyPatch]
        static class SmelterUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                Smelter __instance, ZNetView ___m_nview
            ) {
                _self.StartCoroutine(
                    DumpFeed(
                        __instance
                    )
                );
            }
        }

        // TODO: This can just be put in each method, this is meaningless
        private static IEnumerator DumpFeed<T>(T piece)
        where T : MonoBehaviour
        {
            yield return new WaitForSeconds(0.1f);
            if (piece is Smelter sm)
            {
                DumpConversions(sm.m_name,
                    new ItemConversionContainer(
                        sm.m_conversion
                    )
                );
            }
            else if (piece is CookingStation cs)
            {
                DumpConversions(cs.m_name,
                    new ItemConversionContainer(
                        cs.m_conversion
                    )
                );
            }
        }

        private static void DumpConversions(
            string mName, ItemConversionContainer conversions
        ) {
            string header = $"{mName} ";
            if (alreadySeen.Contains(mName)) return;
            foreach (var conversion in conversions)
            {
                var to = conversion.m_to.m_itemData.m_shared.m_name;
                var from = conversion.m_from.m_itemData.m_shared.m_name;
                _logger.LogDebug($"{header} {from} -> {to}");
            }

            alreadySeen.Add(
                mName
            );
        }
    }
}
