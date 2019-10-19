using AreaControl.Duties;
using AreaControl.Utils;
using Rage;

namespace AreaControl.Instances.Scenery
{
    /// <inheritdoc />
    /// <summary>
    /// Object item that is part of an action's scenery.
    /// </summary>
    public abstract class AbstractPlaceableSceneryItem : ISceneryItem
    {
        private PlaceObjectsDuty.PlaceObject _placeObject;
        private Object _previewObject;

        #region Constructors

        protected AbstractPlaceableSceneryItem(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Vector3 Position { get; }

        /// <inheritdoc />
        public float Heading { get; }

        /// <inheritdoc />
        public PlaceObjectsDuty.PlaceObject Object => GetPlaceObjectInstance();

        #endregion

        #region IPreviewSupport

        /// <inheritdoc />
        public bool IsPreviewActive => _previewObject != null;

        /// <inheritdoc />
        public void CreatePreview()
        {
            if (IsPreviewActive)
                return;

            _previewObject = CreateItemInstance();
            PreviewUtils.TransformToPreview(_previewObject);
            PropUtils.PlaceCorrectlyOnGround(_previewObject);
        }

        /// <inheritdoc />
        public void DeletePreview()
        {
            if (!IsPreviewActive)
                return;

            PropUtils.Remove(_previewObject);
            _previewObject = null;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Create an instance of the scenery item.
        /// </summary>
        /// <returns>Returns the scenery item instance.</returns>
        protected abstract Object CreateItemInstance();

        private PlaceObjectsDuty.PlaceObject GetPlaceObjectInstance()
        {
            return _placeObject ?? (_placeObject = new PlaceObjectsDuty.PlaceObject(Position, Heading, CreateItemInstance));
        }

        #endregion
    }
}