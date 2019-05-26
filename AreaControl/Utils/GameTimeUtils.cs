using Rage;

namespace AreaControl.Utils
{
    public static class GameTimeUtils
    {
        /// <summary>
        /// Get the current hour of the game.
        /// </summary>
        /// <returns>Returns the hour of the game.</returns>
        public static int GetCurrentHour()
        {
            var time = Game.GameTime;

            return (int) (time / 3600000);
        }

        /// <summary>
        /// Get the current time period of the game.
        /// </summary>
        public static TimePeriod TimePeriod
        {
            get
            {
                var hour = GetCurrentHour();

                if (hour >= 6 && hour < 8)
                    return TimePeriod.Morning;

                if (hour >= 8 && hour < 12)
                    return TimePeriod.Forenoon;

                if (hour >= 12 && hour < 18)
                    return TimePeriod.Afternoon;

                if (hour >= 18 && hour < 20)
                    return TimePeriod.Evening;

                // otherwise, it's night
                return TimePeriod.Night;
            }
        }
    }
}