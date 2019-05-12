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
        private static readonly IDictionary<MenuType, UIMenu> Menus = new Dictionary<MenuType, UIMenu>();
        private static readonly List<IMenuComponent> MenuItems = new List<IMenuComponent>();
        private readonly IRage _rage;
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;

        private UIMenuSwitchMenusItem _menuSwitcher;

        #region Constructors

        public MenuImpl(IRage rage, ISettingsManager settingsManager, ILogger logger)
        {
            _rage = rage;
            _logger = logger;
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

        #region IDisposable

        public void Dispose()
        {
            Game.FrameRender -= Process;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void RegisterComponent(IMenuComponent component)
        {
            Assert.NotNull(component, "component cannot be null");
            var uiMenu = Menus[component.Type];

            uiMenu.AddItem(component.MenuItem);
            uiMenu.RefreshIndex();

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
                _logger.Debug("Creating submenus...");
                Menus.Add(MenuType.AREA_CONTROL, CreateMenu());
                Menus.Add(MenuType.STREET_CONTROL, CreateMenu());
                Menus.Add(MenuType.DEBUG, CreateMenu());

                _logger.Debug("Creating menu switcher...");
                CreateMenuSwitcher();

                _logger.Debug("Initializing submenus...");
                foreach (var menu in Menus)
                {
                    menu.Value.AddItem(_menuSwitcher, 0);
                    menu.Value.RefreshIndex();
                    menu.Value.OnItemSelect += ItemSelectionHandler;
                }

                _logger.Debug("Adding MenuImpl.Process to FrameRender handler...");
                Game.FrameRender += Process;
                _logger.Debug("MenuImpl.Process added to FrameRender handler");

                IsMenuInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while initializing the menu with error {ex.Message}", ex);
                _rage.DisplayPluginNotification("an unexpected error occurred");
            }
        }

        private void Process(object sender, GraphicsEventArgs e)
        {
            try
            {
                if (IsMenuKeyPressed())
                {
                    _menuSwitcher.CurrentMenu.Visible = !_menuSwitcher.CurrentMenu.Visible;
                    IsShown = _menuSwitcher.CurrentMenu.Visible;
                }

                MenuPool.ProcessMenus();
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while processing the menu with error {ex.Message}", ex);
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
                _logger.Error(ex.Message, ex);
                _rage.DisplayPluginNotification("could not invoke menu item, see log files for more info");
            }
            catch (Exception ex)
            {
                _logger.Error($"An unexpected error occurred while activating the menu item {ex.Message}", ex);
                _rage.DisplayPluginNotification("an unexpected error occurred while invoking the menu action");
            }
        }

        private void CloseMenu()
        {
            _menuSwitcher.CurrentMenu.Visible = false;
        }

        private void CreateMenuSwitcher()
        {
            _menuSwitcher = new UIMenuSwitchMenusItem("Type", null,
                new DisplayItem(Menus[MenuType.AREA_CONTROL], "Area Control"),
                new DisplayItem(Menus[MenuType.STREET_CONTROL], "Street Control"),
                new DisplayItem(Menus[MenuType.DEBUG], "Debug"));
        }

        private static UIMenu CreateMenu()
        {
            var menu = new UIMenu("Area Control", "~b~CONTROL YOUR SURROUNDING");
            MenuPool.Add(menu);
            return menu;
        }

        #endregion
    }
}