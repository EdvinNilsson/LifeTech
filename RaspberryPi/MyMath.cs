namespace RaspberryPi
{
    public static class MyMath
    {
        public static float Map(this short value, short fromLow, short fromHigh, float toLow, float toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        public static double Map(this short value, short fromLow, short fromHigh, double toLow, double toHigh) => (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}