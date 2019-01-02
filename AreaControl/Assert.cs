using System;

namespace AreaControl
{
    public static class Assert
    {
        public static void NotNull(object value, string message)
        {
            if (value == null)
                throw new ArgumentException(message, (Exception) null);
        }

        public static void HasText(string value, string message)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(message);
        }

        public static void IsPositive(float value, string message)
        {
            if (value < 0)
                throw new ArgumentException(message);
        }
    }
}