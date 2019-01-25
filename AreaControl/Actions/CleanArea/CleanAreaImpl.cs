using System.Linq;
using AreaControl.AbstractionLayer;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CleanArea
{
    public class CleanAreaImpl : ICleanArea
    {
        private const float SearchPedRadius = 100f;

        private readonly IRage _rage;
        private readonly IEntityManager _entityManager;
        private readonly IDutyManager _dutyManager;

        public CleanAreaImpl(IRage rage, IEntityManager entityManager, IDutyManager dutyManager)
        {
            _rage = rage;
            _entityManager = entityManager;
            _dutyManager = dutyManager;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.ClearArea);

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        /// <inheritdoc />
        public bool IsVisible => true;

        /// <inheritdoc />
        public void OnMenuActivation(IMenu sender)
        {
            Execute();
        }

        /// <inheritdoc />
        public void OnMenuHighlighted(IMenu sender)
        {
            //no-op
        }

        #endregion

        #region Functions

        private void Execute()
        {
            _rage.NewSafeFiber(() =>
            {
                var peds = _entityManager.FindPedsWithin(Game.LocalPlayer.Character.Position, SearchPedRadius)
                    .Where(x => !x.IsBusy)
                    .ToList();

                if (peds.Count == 0)
                {
                    _rage.DisplayNotification("~b~Clear surrounding area~r~\nNo available cops within the surrounding area");
                    return;
                }

                foreach (var ped in peds)
                {
                    var duty = _dutyManager.NextAvailableDuty(ped);

                    if (duty != null)
                    {
                        _rage.LogTrivialDebug("Activating clear area duty " + duty);
                        duty.OnCompletion += (sender, args) => ped.ReturnToLspdfrDuty();
                    }
                    else
                    {
                        _rage.LogTrivialDebug("Couldn't find an available clear area duty for " + ped);
                    }
                }
            }, "CleanAreaImpl");
        }

        #endregion
    }
}