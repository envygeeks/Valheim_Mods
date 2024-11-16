// ReSharper disable Unity.PreferNonAllocApi
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable CheckNamespace

using System;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using JetBrains.Annotations;
using System.Reflection;
using System.Linq;

namespace Aides.Valheim.Utils
{
    public static class Extensions
    {
        /**
         * <summary>
         *     Invokes the specified
         *     <see cref="Action{T}"/> on
         *     the object and returns it, enabling
         *     fluent chaining.
         * </summary>
         * <example>
         *     <code>
         *         var obj = new GameObject().Tap(
         *           g => g.name = "New Name"
         *         );
         *     </code>
         * </example>
         */
        public static T Tap<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }

    public static class Unity
    {

        /**
         * necessary because of the way
         * we operate from _within_ the methods
         * we are patching
         */
        private const BindingFlags Bindings =
            BindingFlags.Instance | BindingFlags.NonPublic |
            BindingFlags.Public;

        /**
         * <summary>
         *     Retrieves the value of a
         *     specified property on an object
         *     using reflection with internal
         *     and public binding flags
         * </summary>
         * <example>
         *     <code>
         *         using UnityUtils = Aides.Valheim.Utils.Unity;
         *         var name = UnityUtils.GetProperty&lt;string&gt;(
         *             obj, "PropertyName"
         *         );
         *     </code>
         * </example>
         */
        [PublicAPI]
        public static T GetProperty<T>(
            object obj, string property
        ) {
            return (T)obj.GetType().
                GetProperty(property, Bindings)?.
                GetValue(
                    obj
                );
        }

        /**
         * <summary>
         *     Retrieves the value of
         *     a specified field on an object
         *     using reflection with internal and
         *     public binding flags.
         * </summary>
         * <example>
         *     <code>
         *         using UnityUtils = Aides.Valheim.Utils.Unity;
         *         var val = UnityUtils.GetValue&lt;int&gt;(
         *             obj, "FieldName"
         *         );
         *     </code>
         * </example>
         */
        [PublicAPI]
        public static T GetValue<T>(
            object obj, string field
        ) {
            return (T)obj.GetType().
                GetField(field, Bindings)?.
                GetValue(
                    obj
                );
        }

        /**
         * <summary>
         *     Invokes a specified method
         *     on an object using reflection and
         *     returns the result as type
         *     <typeparamref name="T"/>.
         * </summary>
         * <example>
         *     <code>
         *         using UnityUtils = Aides.Valheim.Utils.Unity;
         *         var result = UnityUtils.GetReturn&lt;bool&gt;(
         *             obj, "MethodName"
         *         );
         *     </code>
         * </example>
         */
        [PublicAPI]
        public static T GetReturn<T>(
            object obj, string method
        ) {
            return (T)obj.GetType().
                GetMethod(method, Bindings)?.
                Invoke(
                    obj, null
                );
        }

        /**
         * <summary>
         *     Finds an object of
         *     type <typeparamref name="T"/>
         *     by its instance ID
         * </summary>
         * <example>
         *     <code>
         *         using UnityUtils = Aides.Valheim.Utils.Unity;
         *         var fireplace = UnityUtils.GetObjectById&lt;Fireplace&gt;(
         *                 instanceId
         *             );
         *     </code>
         * </example>
         */
        [PublicAPI]
        public static T GetObjectById<T>(int id) where T : Component
        {
            return Object.FindObjectsOfType<T>().FirstOrDefault(
                obj => obj.GetInstanceID() == id
            );
        }

        /**
         * <summary>
         *     Recursively retrieves the
         *     closest valid <see cref="Container"/>
         *     component from any given
         *     <see cref="Transform"/>
         * </summary>
         * <example>
         *     <code>
         *         using UnityUtils = Aides.Valheim.Utils.Unity;
         *         var container = UnityUtils.GetContainer(
         *             transform
         *         );
         *     </code>
         * </example>
         */
        [PublicAPI]
        public static Container GetContainer(Transform transform)
        {
            if (!transform) return null;
            var container = transform.GetComponent<Container>();
            if (container && container.GetComponent<ZNetView>()?.IsValid() == true)
                if (container.GetInventory() != null) return container;
            transform = transform.parent;
            return GetContainer(
                transform
            );
        }

        /**
         * <summary>
         *     Converts a <see cref="Collider"/>
         *     into a <see cref="Transform"/> and then
         *     retrieves the associated container
         * </summary>
         */
        [PublicAPI]
        public static Container GetContainer(Collider collider)
        {
            return GetContainer(
                collider.transform
            );
        }

        #if NON_ALLOC_WIP
        [PublicAPI]
        public static List<Cache.Item<Container>> GetContainers<T>(
            T instance, ref int lastKnownHit, Vector3 center, int range,
            Collider[] colliderCache, Cache cache
        ) where T: MonoBehaviour {
            var containerCache = cache.OfType<T>();
            var hit = Physics.OverlapSphereNonAlloc(
                center + Vector3.up, range, colliderCache,
                LayerMask.GetMask(
                    "piece"
                )
            );

            if (hit == lastKnownHit)
            {
                var list = containerCache.Values.ToList();
                return list as List<Cache.Item<Container>>;
            }

            lastKnownHit = hit;
            containerCache.Clear();
            for (var i = 0; i < hit; i++)
            {
                var container = GetContainer(colliderCache[i]);
                if (container)
                    containerCache.Add(
                        container
                    );
            }

            return containerCache;
        }
        #endif

        /**
         * TODO: Port Config from AutoFuel into here
         * TODO: Swap range off of Feedable.cs Awake patches to here
         *
         * <summary>
         *     Finds and caches all containers
         *     within a specified range of a center
         *     point for a given <see cref="MonoBehaviour"/>
         *     instance. This method focuses on efficient
         *     memory use and avoids frequent
         *     reallocation of colliders
         * </summary>
         * <example>
         *     <code>
         *         Fireplace fp;
         *         using UnityUtils = Aides.Valheim.Utils.Unity;
         *         Unity.GetContainers(
         *           fp, center, range, cache
         *         );
         *     </code>
         * </example>
         * <remarks>
         *     Instead of using `NonAlloc` for this
         *     once-in-a-while query, this method leverages
         *     the existing overlap detection in Unity to retrieve
         *     containers within a specified range. This design choice
         *     minimizes memory usage by caching nearby containers
         *     rather than frequently reallocating a list of
         *     colliders, especially since the target
         *     objects are static and don't move
         *     without being destroyed.
         *
         *     By pre-caching nearby containers
         *     and managing them through destroy and
         *     awake events, this method reduces garbage
         *     collection overhead and improves performance
         *     over time. While `NonAlloc` could be considered
         *     if recalculating every frame, it’s unnecessary
         *     here since these containers are static
         * </remarks>
         */
        [PublicAPI]
        public static IEnumerator CacheContainers<T>(
            T instance, ZNetView mNView, int range, Cache cache
        ) where T: MonoBehaviour {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
            var item = cache.GetOrAdd(instance).Tap(i => i.ClearNearby());
            if (!instance || !mNView || !mNView.IsValid()) yield break;
            var center = instance.transform.position;
            var processedCount = 0;

            foreach (var collider in Physics.OverlapSphere(
                center + Vector3.up, range, LayerMask.GetMask(
                    "piece"
                )
            )) {
                var container = GetContainer(collider);
                if (container)
                {
                    item.AddNearby(
                        container
                    );
                }

                processedCount++;
                if (processedCount % 10 == 0)
                    yield return null;
            }
        }
    }
}
