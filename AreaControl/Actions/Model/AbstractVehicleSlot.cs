using AreaControl.Instances;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Actions.Model
{
    public abstract class AbstractVehicleSlot : IPreviewSupport
    {
        private Entity _previewObject;

        protected AbstractVehicleSlot(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        #region Properties

        /// <summary>
        /// Get the position of the vehicle.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Get the heading of the vehicle.
        /// </summary>
        public float Heading { get; }

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => _previewObject != null;

        /// <inheritdoc />
        public virtual void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            _previewObject = new Vehicle(GetModelInstance(), Position, Heading);
            PreviewUtils.TransformToPreview(_previewObject);
        }

        /// <inheritdoc />
        public virtual void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            EntityUtils.Remove(_previewObject);
            _previewObject = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear the vehicle slot from any cars/debris.
        /// </summary>
        public void ClearSlotFromTraffic()
        {
            EntityUtils.CleanArea(Position, 5f, true);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Get the vehicle model instance to create.
        /// </summary>
        /// <returns></returns>
        protected abstract Rage.Model GetModelInstance();

        #endregion
    }
}