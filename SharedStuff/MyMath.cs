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

        public static void IndexCreateNew<T>(this IList<T> list, int index, T value) {
            try {
                list[index] = value;
            } catch (ArgumentOutOfRangeException) {
                for (int i = list.Count; i <= index; ++i) {
                    list.Add(default);
                }
                list[index] = value;
            }
        }

        public static void FillToIndex<T>(this IList<T> list, int index) {
            for (int i = list.Count; i <= index; ++i) {
                list.Add(default);
            }
        }

        public static TValue GetValueCreateDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : struct {
            try {
                return dictionary[key];
            } catch (KeyNotFoundException) {
                dictionary.Add(key, default);
                return dictionary[key];
            }
        }

        public static TimeSpan Round(this TimeSpan time, TimeSpan roundingInterval) {
            return new TimeSpan(
                Convert.ToInt64(Math.Round(
                    time.Ticks / (decimal)roundingInterval.Ticks
                )) * roundingInterval.Ticks
            );
        }

        public static TimeSpan Floor(this TimeSpan time, TimeSpan roundingInterval) {
            return new TimeSpan(
                Convert.ToInt64(Math.Floor(
                    time.Ticks / (decimal)roundingInterval.Ticks
                )) * roundingInterval.Ticks
            );
        }

        public static DateTime Round(this DateTime datetime, TimeSpan roundingInterval) => new DateTime((datetime - DateTime.MinValue).Round(roundingInterval).Ticks);
        public static DateTime Floor(this DateTime datetime, TimeSpan roundingInterval) => new DateTime((datetime - DateTime.MinValue).Floor(roundingInterval).Ticks);
    }
}