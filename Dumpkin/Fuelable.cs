// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable CheckNamespace

using HarmonyLib;
using JetBrains.Annotations;
using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using BepInEx;

namespace Dumpkin.Fuelable
{
    [
        BepInPlugin(
            "dumpkin.fuelable", "Dumpkin Fuelable", "0.1.0"
        )
    ]
    public class Fuelable: BaseUnityPlugin
    {
        /**
         * Config
         */
        private static ConfigEntry<bool> fireplaces;
        private static readonly List<string> alreadySeen = new List<string>();
        private static ConfigEntry<bool> cookingStations;
        private new static ConfigEntry<bool> enabled;
        private static ConfigEntry<bool> smelters;

        /**
         * Internal
         */
        private static Fuelable _self;
        private static Harmony _harmony;
        private static readonly ManualLogSource _logger =
            BepInEx.Logging.Logger.CreateLogSource(
                "dumpkin.fuelable"
            );

        /**
         * Most cooking stations
         * do not take any fuel, just the
         * one that has m_fuelItem
         */
        private static readonly List<string> allowedCookingStations =
            new List<string>()
            {
                "$piece_oven"
            };

        /**
         * Not all smelters take fuel
         * e.g. the spinning wheel is a smelter,
         * but it only takes thread, same with the
         * windmill, and others
         */
        private static readonly List<string> allowedSmelters =
            new List<string>()
            {
                "$piece_smelter",
                "$piece_blastfurnace"
            };

        private void Awake()
        {
            _self = this;
            _harmony = new Harmony("dumpkin.fuelable");
            enabled = Config.Bind("General", "enabled", true);
            fireplaces = Config.Bind("General", "fireplaces", true);
            cookingStations = Config.Bind("General", "cooking_stations", true);
            smelters = Config.Bind("General", "smelters", true);
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
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchAll(
                "dumpkin.fuelable"
            );
        }

        [HarmonyPatch]
        static class CookingStationUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                CookingStation __instance, ZNetView ___m_nview
            ) {
                if (allowedCookingStations.Contains(__instance.m_name))
                    _self.StartCoroutine(
                        DumpFuel(
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
                if (allowedSmelters.Contains(__instance.m_name))
                    _self.StartCoroutine(
                        DumpFuel(
                            __instance
                        )
                    );
            }
        }

        [HarmonyPatch]
        static class FireplaceUpdatePatch
        {
            [UsedImplicitly]
            private static void Postfix(
                Smelter __instance, ZNetView ___m_nview
            ) {
                _self.StartCoroutine(
                    DumpFuel(
                        __instance
                    )
                );
            }
        }

        private static IEnumerator DumpFuel<T>(T item)
        where T: MonoBehaviour
        {
            string mName, header, fName;
            yield return new WaitForSeconds(0.1f);
            if (item is Smelter sm)
            {
                mName = sm.m_name;
                fName = sm.m_fuelItem.m_itemData
                    .m_shared.m_name;
            }
            else
            {
                if (item is CookingStation cs)
                {
                    mName = cs.m_name;
                    fName = cs.m_fuelItem.m_itemData
                        .m_shared.m_name;
                }
                else
                {
                    if (item is Fireplace fp)
                    {
                        mName = fp.m_name;
                        fName = fp.m_fuelItem.m_itemData
                            .m_shared.m_name;
                    }
                    else
                    {
                        yield break;
                    }
                }
            }

            header = $"{mName} ";
            if (alreadySeen.Contains(mName)) yield break;
            _logger.LogDebug($"{header} takes {fName} to run");
            alreadySeen.Add(
                mName
            );
        }
    }
}
