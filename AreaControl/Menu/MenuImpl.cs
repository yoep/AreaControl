using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using AreaControl.AbstractionLayer;
using AreaControl.Settings;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace AreaControl.Menu
{
    public class MenuImpl : IMenu
    {
        private static readonly MenuPool MenuPool = new MenuPool();
        private static readonly UIMenu NormalMenu = CreateMenu();
        private static readonly UIMenu DebugMenu = CreateMenu();
        private static readonly UIMenuSwitchMenusItem MenuSwitcher = CreateMenuSwitcher();
        private static readonly List<IMenuComponent> MenuItems = new List<IMenuComponent>();
        private readonly IRage _rage;
        private readonly ISettingsManager _settingsManager;

        #region Constructors

        public MenuImpl(IRage rage, ISettingsManager settingsManager)
        {
            _rage = rage;
            _settingsManager = settingsManager;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsMenuInitialized { get; private set; }

        /// <inheritdoc />
        public bool IsShown { get; private set; }

        /// <inheritdoc />
        public int TotalItems => MenuItems.Count;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void RegisterComponent(IMenuComponent component)
        {
            Assert.NotNull(component, "component cannot be null");

            if (component.IsDebug)
            {
                DebugMenu.AddItem(component.MenuItem);
                DebugMenu.RefreshIndex();
            }
            else
            {
                NormalMenu.AddItem(component.MenuItem);
                NormalMenu.RefreshIndex();
            }

            MenuItems.Add(component);
        }

        #endregion

        #region Functions

        [IoC.PostConstruct]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Init()
        {
            try
            {
                _rage.LogTrivialDebug("Initializing submenus...");
                NormalMenu.AddItem(MenuSwitcher, 0);
                DebugMenu.AddItem(MenuSwitcher, 0);
                NormalMenu.RefreshIndex();
                DebugMenu.RefreshIndex();
                _rage.LogTrivialDebug("Submenus initialized");
                
                _rage.LogTrivialDebug("Adding MenuImpl.Process to FrameRender handler...");
                Game.FrameRender += Process;
                _rage.LogTrivialDebug("MenuImpl.Process added to FrameRender handler");
                
                _rage.LogTrivialDebug("Adding Menu handlers...");
                NormalMenu.OnItemSelect += ItemSelectionHandler;
                NormalMenu.OnIndexChange += ItemChangeHandler;
                DebugMenu.OnItemSelect += ItemSelectionHandler;
                DebugMenu.OnIndexChange += ItemChangeHandler;
                _rage.LogTrivialDebug("Menu handlers added");

                IsMenuInitialized = true;
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("*** An unexpected error occurred while initializing the menu ***" +
                                 Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.DisplayPluginNotification("an unexpected error occurred");
            }
        }

        private void Process(object sender, GraphicsEventArgs e)
        {
            try
            {
                if (IsMenuKeyPressed())
                {
                    MenuSwitcher.CurrentMenu.Visible = !MenuSwitcher.CurrentMenu.Visible;
                    IsShown = MenuSwitcher.CurrentMenu.Visible;
                }

                MenuPool.ProcessMenus();
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("*** An unexpected error occurred while processing the menu ***" +
                                 Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.DisplayPluginNotification("an unexpected error occurred");
            }
        }

        private bool IsMenuKeyPressed()
        {
            var generalSettings = _settingsManager.GeneralSettings;
            var secondKey = generalSettings.OpenMenuModifierKey;
            var secondKeyDown = secondKey == Keys.None;

            if (!secondKeyDown && secondKey == Keys.ShiftKey && Game.IsShiftKeyDownRightNow)
                secondKeyDown = true;

            if (!secondKeyDown && secondKey == Keys.ControlKey && Game.IsControlKeyDownRightNow)
                secondKeyDown = true;

            return Game.IsKeyDown(generalSettings.OpenMenuKey) && secondKeyDown;
        }

        private void ItemSelectionHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            var menuComponent = MenuItems.FirstOrDefault(x => x.MenuItem == selectedItem);

            try
            {
                if (menuComponent == null)
                    throw new MenuException("No menu item action found for the selected menu item", selectedItem);

                menuComponent.OnMenuActivation(this);

                if (menuComponent.IsAutoClosed)
                    CloseMenu();
            }
            catch (MenuException ex)
            {
                _rage.LogTrivial(ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.DisplayPluginNotification("could not invoke menu item, see log files for more info");
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("*** An unexpected error occurred while activating the menu item ***" +
                                 Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.DisplayPluginNotification("an unexpected error occurred while invoking the menu action");
            }
        }

        private void ItemChangeHandler(UIMenu sender, int newIndex)
        {
            MenuItems.FirstOrDefault(x => x.MenuItem == sender.MenuItems[newIndex])?.OnMenuHighlighted(this);
        }

        private static void CloseMenu()
        {
            MenuSwitcher.CurrentMenu.Visible = false;
        }

        private static UIMenu CreateMenu()
        {
            var menu = new UIMenu("Area Control", "~b~CONTROL YOUR SURROUNDING");
            MenuPool.Add(menu);
            return menu;
        }

        private static UIMenuSwitchMenusItem CreateMenuSwitcher()
        {
            return new UIMenuSwitchMenusItem("Type", null,
                new DisplayItem(NormalMenu, "Street Control"),
                new DisplayItem(DebugMenu, "Debug"));
        }

        #endregion
    }
}