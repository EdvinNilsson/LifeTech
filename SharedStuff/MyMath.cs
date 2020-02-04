using System.Collections.Generic;

namespace SharedStuff
{
    public static class MyMath
    {
        public static float Map(this short value, short fromLow, short fromHigh, float toLow, float toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        public static double Map(this short value, short fromLow, short fromHigh, double toLow, double toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;

        public static TValue GetValueCreateNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new() {
            try {
                return dictionary[key];
            } catch (KeyNotFoundException) {
                dictionary.Add(key, new TValue());
                return dictionary[key];
            }
        }
    }
}