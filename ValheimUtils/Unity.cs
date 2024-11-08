using UnityEngine;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace ValheimUtils
{
    public static class Unity
    {
        /**
         * A simple wrapper to get
         * the Field Value using Reflection,
         * it just makes life easier
         */
        [UsedImplicitly]
        public static T GetValueOf<T>(
            System.Object obj, string field
        )
        {
            // The reality is, I just hate long str
            return (T)obj.GetType().GetField(field)?.
                GetValue(
                    obj
                );
        }

        /**
         * Get an object by ID
         * @return Fireplace|CookingStation|Smelter
         * @param id int
         */
        [UsedImplicitly]
        public static T GetObjectById<T>(int id) where T : Component
        {
            return Object.FindObjectsOfType<T>().FirstOrDefault(
                obj => obj.GetInstanceID() == id
            );
        }
}
