namespace AreaControl.Instances
{
    /// <summary>
    /// Defines movement speeds for peds.
    /// </summary>
    public class MovementSpeed
    {
        public static readonly MovementSpeed Walk = new MovementSpeed(1f);
        public static readonly MovementSpeed Run = new MovementSpeed(2f);

        private MovementSpeed(float value)
        {
            Value = value;
        }

        /// <summary>
        /// Get the value for the movement speed.
        /// </summary>
        public float Value { get; }
    }
}