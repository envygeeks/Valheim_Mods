using UnityEngine;

namespace Aides.Valheim
{
    public class Helpers
    {
        public static bool IsFuelable<T>(T instance)
        where T: MonoBehaviour
        {
            return
                // There is (right now) one cooking and smelter
                instance is Fireplace fp && fp.m_fuelItem != null ||
                instance is CookingStation cs && cs.m_fuelItem != null ||
                instance is Smelter sm && sm.m_fuelItem != null;
        }

        public static bool IsFeedable<T>(T instance)
        where T: MonoBehaviour
        {
            return
                // Really only smelters and cooking stations can be fed
                instance is Smelter sm && sm.m_conversion != null && sm.m_conversion.Count > 0 ||
                instance is CookingStation cs && cs.m_conversion != null
                    && cs.m_conversion.Count > 0;
        }
    }
}
