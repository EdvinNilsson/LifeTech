using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharedStuff
{
    public static class MyMath
    {
        public static float Map(this short value, short fromLow, short fromHigh, float toLow, float toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        public static double Map(this short value, short fromLow, short fromHigh, double toLow, double toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;

        public static int Min(int a, int b) => a < b ? a : b;
        public static int Max(int a, int b) => a > b ? a : b;

        public static TValue GetValueCreateNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new() {
            try {
                return dictionary[key];
            } catch (KeyNotFoundException) {
                dictionary.Add(key, new TValue());
                return dictionary[key];
            }
        }
        
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType) {
            return new TimeSpan(
                Convert.ToInt64(Math.Round(
                    time.Ticks / (decimal)roundingInterval.Ticks,
                    roundingType
                )) * roundingInterval.Ticks
            );
        }
        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval) => Round(time, roundingInterval, MidpointRounding.ToEven);
        public static DateTime Round(this DateTime datetime, TimeSpan roundingInterval) => new DateTime((datetime - DateTime.MinValue).Round(roundingInterval).Ticks);
    }
}