using System.Collections.Generic;
using System.Windows.Forms;
using AreaControl.Rage;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AreaControl.Menu
{
    public class MenuImpl : IMenu
    {
        private static readonly MenuPool MenuPool = new MenuPool();
        private static readonly UIMenu AreaControlMenu = new UIMenu("Area Control", "~b~CONTROL YOUR SURROUNDING");
        private static readonly Dictionary<UIMenuItem, IMenuComponent> MenuItems = new Dictionary<UIMenuItem, IMenuComponent>();
        private readonly IRage _rage;

        #region Constructors

        public MenuImpl(IRage rage)
        {
            _rage = rage;
        }

        #endregion

        #region Properties

        public bool IsMenuInitialized { get; private set; }

        #endregion

        #region Methods

        public void RegisterItem(UIMenuItem item, IMenuComponent component)
        {
            Assert.NotNull(item, "item cannot be null");
            Assert.NotNull(component, "component cannot be null");

            MenuItems.Add(item, component);
            AreaControlMenu.AddItem(item);
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        // ReSharper disable once UnusedMember.Local
        private void Init()
        {
            _rage.LogTrivialDebug("Adding AreaControlMenu to the MenuPool...");
            MenuPool.Add(AreaControlMenu);
            _rage.LogTrivialDebug("AreaControlMenu added to MenuPool");
            _rage.LogTrivialDebug("Adding MenuImpl.Process to FrameRender handler...");
            Game.FrameRender += Process;
            _rage.LogTrivialDebug("MenuImpl.Process added to FrameRender handler");
            AreaControlMenu.OnItemSelect += ItemSelectionHandler;

            IsMenuInitialized = true;
        }

        private static void Process(object sender, GraphicsEventArgs e)
        {
            if (Game.IsKeyDown(Keys.T))
                AreaControlMenu.Visible = !AreaControlMenu.Visible;

            MenuPool.ProcessMenus();
        }

        private static void ItemSelectionHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (!MenuItems.ContainsKey(selectedItem))
                throw new MenuException("No menu item action found for the selected menu item", selectedItem);

            MenuItems[selectedItem].OnMenuActivation();
            CloseMenu();
        }

        private static void CloseMenu()
        {
            AreaControlMenu.Visible = false;
        }

        #endregion
    }
}