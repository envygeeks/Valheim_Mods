// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using HarmonyLib;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using BepInEx;

namespace ContainerDumper
{
    [
        BepInPlugin(
            "EnvyGeeks.BreakCheckYourself", "BreakCheckYourself", "0.1.0"
        )
    ]
    public class Plugin: BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(
                Assembly.GetExecutingAssembly()
            );
        }
    }
}
