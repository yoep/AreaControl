using System.Diagnostics.CodeAnalysis;
using Rage;

namespace AreaControl.Instances
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class AbstractACInstance<TType> : IACEntity where TType : Entity
    {
        private readonly float _blipScale;

        #region Constructors

        protected AbstractACInstance(TType instance, long id, float blipScale)
        {
            _blipScale = blipScale;
            Instance = instance;
            Id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the game instance.
        /// </summary>
        public TType Instance { get; }

        /// <summary>
        /// Get the active blip for this instance.
        /// </summary>
        public Blip Blip { get; private set; }

        #endregion

        #region IACEntity

        /// <inheritdoc />
        public long Id { get; }

        /// <inheritdoc />
        public bool IsValid => Instance != null && Instance.IsValid();

        /// <inheritdoc />
        public bool IsInvalid => !IsValid;

        /// <inheritdoc />
        public void CreateBlip()
        {
            if (Blip != null)
                return;

            Blip = new Blip(Instance)
            {
                IsRouteEnabled = false,
                IsFriendly = true,
                Scale = _blipScale
            };
        }

        /// <inheritdoc />
        public void DeleteBlip()
        {
            if (Blip == null || !Blip.IsValid())
                return;

            Blip.Delete();
            Blip = null;
        }

        #endregion

        #region IDeletable

        /// <inheritdoc />
        public virtual void Delete()
        {
            DeleteBlip();

            if (Instance.IsValid())
                Instance.Delete();
        }

        #endregion
    }
}