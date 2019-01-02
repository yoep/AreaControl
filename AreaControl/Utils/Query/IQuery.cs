using Rage;

namespace AreaControl.Utils.Query
{
    public interface IQuery<out T> where T : Entity
    {
        /// <summary>
        /// Get the closest <typeparamref name="T"/> to the given vector.
        /// </summary>
        /// <param name="position">Set the position to search around.</param>
        /// <param name="radius">Set the radius to search in.</param>
        /// <returns>Returns the closest <typeparamref name="T"/> if any is nearby, else null.</returns>
        T FindWithin(Vector3 position, float radius);
    }
}