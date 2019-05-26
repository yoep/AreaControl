using AreaControl.AbstractionLayer;
using AreaControl.Actions.Model;
using AreaControl.Duties;
using AreaControl.Instances;
using AreaControl.Menu;
using AreaControl.Menu.Response;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI.Elements;

namespace AreaControl.Actions.CrimeScene
{
    public class CrimeSceneImpl : AbstractCrimeScene, ICrimeScene
    {
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private readonly IEntityManager _entityManager;
        private readonly IResponseManager _responseManager;
        private readonly IDutyManager _dutyManager;

        private CrimeSceneSlot _crimeSceneSlot;
        private ACVehicle _policeVehicle;

        public CrimeSceneImpl(IRage rage, ILogger logger, IEntityManager entityManager, IResponseManager responseManager, IDutyManager dutyManager)
        {
            _rage = rage;
            _logger = logger;
            _entityManager = entityManager;
            _responseManager = responseManager;
            _dutyManager = dutyManager;
        }

        #region IMenuComponent

        /// <inheritdoc />
        public UIMenuItem MenuItem { get; } = new UIMenuItem(AreaControl.CreateCrimeScene, AreaControl.CrimeSceneDescription);

        /// <inheritdoc />
        public MenuType Type => MenuType.AREA_CONTROL;

        /// <inheritdoc />
        public bool IsAutoClosed => true;

        public void OnMenuActivation(IMenu sender)
        {
            if (IsActive)
            {
                RemoveCrimeScene();
            }
            else
            {
                CreateCrimeScene();
            }
        }

        #endregion

        #region Functions

        private void RemoveCrimeScene()
        {
            MenuItem.Text = AreaControl.CreateCrimeScene;
            Functions.PlayScannerAudio("WE_ARE_CODE_4");
            _dutyManager.DismissDuties();
            _entityManager.Dismiss();
            ClearBarriers();
            IsActive = false;
        }

        private void ClearBarriers()
        {
            
        }

        private void CreateCrimeScene()
        {
            IsActive = true;
            MenuItem.Text = AreaControl.RemoveCrimeScene;
            _rage.NewSafeFiber(() =>
            {
                var position = Game.LocalPlayer.Character.Position;
                _crimeSceneSlot = DetermineCrimeSceneSlot();
                
                _rage.DisplayNotification("Requesting dispatch to ~b~create crime scene~s~ " + World.GetStreetName(position) + "...");
                Functions.PlayScannerAudioUsingPosition("WE_HAVE OFFICER_IN_NEED_OF_ASSISTANCE IN_OR_ON_POSITION UNITS_RESPOND_CODE_03", position);
                GameFiber.Sleep(5000);
                
                
            }, "CrimeSceneImpl.CreateCrimeScene");
        }

        private void SpawnVehicles()
        {
            
        }

        #endregion
    }
}