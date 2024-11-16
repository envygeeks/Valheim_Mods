// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.Collections;

namespace Dumpkin.Feedable
{
    /**
     * <summary>
     *     Represents a wrapper
     *     for item conversions, containing the
     *     source item (<see cref="m_from"/>) and the
     *     resulting item (<see cref="m_to"/>)
     * </summary>
     */
    internal class ItemConversionWrapper
    {
        public ItemDrop m_to { get; }
        public ItemDrop m_from
        {
            get;
        }

        /**
         * <summary>
         *     Initializes a new instance
         *     of the <see cref="ItemConversionWrapper"/>
         *     class, encapsulating a conversion
         *     from one item to another
         * </summary>
         */
        public ItemConversionWrapper(ItemDrop from, ItemDrop to)
        {
            (m_from, m_to) = (
                from, to
            );
        }
    }

    /**
     * <summary>
     *     A container for handling
     *     multiple item conversions, supporting
     *     conversions from <see cref="Smelter.ItemConversion"/>
     *     and <see cref="CookingStation.ItemConversion"/>
     * </summary>
     */
    internal class ItemConversionContainer:
    IEnumerable<ItemConversionWrapper>
    {
        private readonly IEnumerable<object> _conversions;

        /**
         * <summary>
         *     Initializes a new instance
         *     of the <see cref="ItemConversionContainer"/>
         *     class with the specified collection of
         *     item conversions.
         * </summary>
         */
        public ItemConversionContainer(
            IEnumerable<object> conversions
        ) {
            _conversions = conversions;
        }

        /**
         * <summary>
         *     Returns an enumerator
         *     that iterates through the collection
         *     of <see cref="ItemConversionWrapper"/>
         *     objects, wrapping supported item
         *     conversion types.
         * </summary>
         */
        public IEnumerator<ItemConversionWrapper> GetEnumerator()
        {
            foreach (var conversion in _conversions)
                switch (conversion)
                {
                    case Smelter.ItemConversion sm_ic:
                        yield return new ItemConversionWrapper(
                            sm_ic.m_from, sm_ic.m_to
                        );
                        break;
                    case CookingStation.ItemConversion cs_ic:
                        yield return new ItemConversionWrapper(
                            cs_ic.m_from, cs_ic.m_to
                        );
                        break;
                    default:
                        throw new System.InvalidOperationException(
                            "Unsupported ItemConversion type."
                        );
                }
        }

        /**
         * <summary>
         *     Returns an enumerator that
         *     iterates through the collection
         *     of <see cref="ItemConversionWrapper"/>
         *     objects
         * </summary>
         */
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
