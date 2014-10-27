namespace Utilities
{
    public static class MiscUtils
    {
        public static bool AreEqual(object firstValue, object secondValue)
        {
            if (firstValue == null && secondValue == null)
                return true;
            if (firstValue == null || secondValue == null)
                return false;
            return firstValue.Equals(secondValue);
        }
    }
}
