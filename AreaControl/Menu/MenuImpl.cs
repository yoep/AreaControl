using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using AreaControl.AbstractionLayer;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AreaControl.Menu
{
    public class MenuImpl : IMenu
    {
        private static readonly MenuPool MenuPool = new MenuPool();
        private static readonly UIMenu AreaControlMenu = new UIMenu("Area Control", "~b~CONTROL YOUR SURROUNDING");
        private static readonly List<IMenuComponent> MenuItems = new List<IMenuComponent>();
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

        public void RegisterComponent(IMenuComponent component)
        {
            Assert.NotNull(component, "component cannot be null");

            MenuItems.Add(component);
            AreaControlMenu.AddItem(component.Item);
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            try
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
            catch (Exception ex)
            {
                _rage.LogTrivial("*** An unexpected error occurred while initializing the menu ***" +
                                 Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.LogTrivial("an unexpected error occurred");
            }
        }

        private static void Process(object sender, GraphicsEventArgs e)
        {
            if (Game.IsKeyDown(Keys.T))
                AreaControlMenu.Visible = !AreaControlMenu.Visible;

            MenuPool.ProcessMenus();
        }

        private void ItemSelectionHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            var menuComponent = MenuItems.FirstOrDefault(x => x.Item == selectedItem);

            try
            {
                if (menuComponent == null)
                    throw new MenuException("No menu item action found for the selected menu item", selectedItem);
                
                menuComponent.OnMenuActivation();
                
                if (menuComponent.IsAutoClosed)
                    CloseMenu();
            }
            catch (MenuException ex)
            {
                _rage.LogTrivial(ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.DisplayNotification("could not invoke menu item, see log files for more info");
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("*** An unexpected error occurred while activating the menu item ***" +
                                 Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.DisplayNotification("an unexpected error occurred while invoking the menu action");
            }
        }

        private static void CloseMenu()
        {
            AreaControlMenu.Visible = false;
        }

        #endregion
    }
}