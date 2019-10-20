using Rage;
using Rage.Native;

namespace AreaControl.Utils
{
    public static class GameUtils
    {
        /// <summary>
        /// Get the current hour of the game.
        /// </summary>
        /// <returns>Returns the hour of the game.</returns>
        public static int GetCurrentHour()
        {
            return World.TimeOfDay.Hours;
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

        /// <summary>
        /// Get a new vector that is placed on the ground in the game world for the current position.
        /// </summary>
        /// <param name="position">The game world position.</param>
        /// <returns>Returns a new Vector that is placed on the ground.</returns>
        public static Vector3 GetOnTheGroundVector(Vector3 position)
        {
            var newPosition = new Vector3(position.X, position.Y, 0f);
            
            NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(position.X, position.Y, position.Z + 15f, out float newHeight, 0);
            newPosition.Z = newHeight;

            return newPosition;
        }
    }
}