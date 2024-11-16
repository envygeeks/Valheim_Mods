// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using JetBrains.Annotations;
using BepInEx.Logging;
using UnityEngine;
using BepInEx;

namespace Dumpkin.Chests
{
    [
        BepInPlugin(
            "dumpkin.chests", "Dumpkin.Chests", "0.1.0"
        )
    ]
    public class Chests: BaseUnityPlugin
    {
        /**
         * Config
         */
        private static ConfigEntry<bool> iron;
        private static ConfigEntry<bool> blackMetal;
        private static ConfigEntry<bool> mustBePlayerPlaced;
        private new static ConfigEntry<bool> enabled;
        private static ConfigEntry<bool> wood;

        /**
         * Internal
         */
        private static Chests _self;
        private static Harmony _harmony;
        private static readonly ManualLogSource _logger =
            BepInEx.Logging.Logger.CreateLogSource(
                "dumpkin.chests"
            );

        /**
         * <summary>
         *     Initializes configuration
         *     settings for iron, black metal, wood,
         *     and enables the plugin. Applies
         *     Harmony patches if the plugin
         *     is enabled
         * </summary>
         */
        private void Awake()
        {
            _self = this;
            _harmony = new Harmony("dumpkin.chests");
            iron = Config.Bind("General", "iron", true);
            enabled = Config.Bind("General", "enabled", true);
            mustBePlayerPlaced = Config.Bind("General", "player_placed_only", true);
            blackMetal = Config.Bind("General", "black_metal", true);
            wood = Config.Bind("General", "wood", true);
            if (enabled.Value) TryToPatchAll();
        }

        private void TryToPatchAll()
        {
            // --
            // Patch
            // --
            _harmony.Patch(
                original: AccessTools.Method(typeof(ZNetView), "Awake"),
                postfix: new HarmonyMethod(
                    typeof(ZNetViewAwakePatch),
                    "Postfix"
                )
            );
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchAll(
                "dumpkin.chests"
            );
        }

        /**
         * <summary>
         *     Harmony patch for the
         *     <see cref="Container"/> component's
         *     <c>Awake</c> method, invoked after the
         *     container is initialized to gather
         *     and process item data
         * </summary>
         */
        [HarmonyPatch]
        public static class ZNetViewAwakePatch
        {
            /**
             * <summary>
             *     Postfix method that runs
             *     after the <see cref="Container"/> Awake()
             *     method, attempting to retrieve the
             *     container's inventory and storing
             *     item data as needed.
             * </summary>
             */
            [UsedImplicitly]
            private static void Postfix(ZNetView __instance)
            {
                var container = __instance.GetComponent<Container>();
                if (!container)
                    return;

                var mName = container.m_name;
                if (!iron.Value && mName == "$piece_chestiron") return;
                if (!blackMetal.Value && mName == "$piece_chestblackmetal") return;
                if (!wood.Value && mName == "$piece_wood") return;
                _self.StartCoroutine(
                    DumpContainerInventory(
                        container
                    )
                );
            }

            private static IEnumerator DumpContainerInventory(
                Container container
            ) {
                yield return new WaitForSeconds(0.1f);
                var zNetView = container.GetComponent<ZNetView>();
                if (zNetView is null || zNetView.IsOwner() != true || zNetView.IsValid() != true)
                    yield break;

                // --
                // Base Vars
                // --
                var transform = container.transform;
                var piece = container.GetComponentInParent<Piece>();
                var inventory = container.GetInventory();
                var position = transform.position;

                // --
                // Disallow non-player placed
                // items unless the user specifically
                // wants them, it's messy tbh
                // --
                var playerPlaced = piece?.IsPlacedByPlayer();
                var playerPlacedStr = playerPlaced.ToString().ToLower();
                if (mustBePlayerPlaced.Value && playerPlaced != true)
                    yield break;

                // --
                // Positions so you can
                // choose to `goto x z` in the game
                // from the dev console
                // --
                var x = Math.Round(position.x);
                var z = Math.Round(
                    position.z
                );

                // --
                // The Header for all
                // the logging that we do
                // via this addon
                // --
                var mName = container.m_name;
                var instanceId = container.GetInstanceID();
                var header = $"{mName}({instanceId}):{playerPlacedStr} " +
                             $"@ {x} {z}";

                // --
                // Inventory Logging
                // --
                var items = inventory?.GetAllItems();
                if (inventory != null && items != null)
                {
                    var count = inventory.NrOfItems();
                    var stackCounts = new Dictionary<string, int>();
                    _logger.LogDebug($"{header} has {count} items");
                    foreach (var itemData in items)
                    {
                        var itemMName = itemData.m_shared.m_name;
                        if (!stackCounts.ContainsKey(mName)) stackCounts[itemMName] = 0;
                        stackCounts[itemMName] +=
                            itemData.m_stack;
                    }

                    foreach (var kvp in stackCounts)
                    {
                        _logger.LogDebug(
                            $"{header} has {kvp.Value} {kvp.Key}"
                        );
                    }
                }

                yield return null;
            }
        }
    }
}
