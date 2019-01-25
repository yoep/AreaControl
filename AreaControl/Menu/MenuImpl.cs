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
        private static readonly UIMenu AreaControlMenu = new UIMenu("Area Control", "~b~CONTROL YOUR SURROUNDING");
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

            MenuItems.Add(component);
            AreaControlMenu.AddItem(component.MenuItem);
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
                AreaControlMenu.OnIndexChange += ItemChangeHandler;

                IsMenuInitialized = true;
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("*** An unexpected error occurred while initializing the menu ***" +
                                 Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.LogTrivial("an unexpected error occurred");
            }
        }

        private void Process(object sender, GraphicsEventArgs e)
        {
            if (IsMenuKeyPressed())
            {
                foreach (var component in MenuItems)
                {
                    if (component.IsVisible && !IsShownInMenu(component))
                        AreaControlMenu.AddItem(component.MenuItem);
                    if (!component.IsVisible && IsShownInMenu(component))
                        RemoveItemFromMenu(component);
                }

                AreaControlMenu.Visible = !AreaControlMenu.Visible;
                IsShown = AreaControlMenu.Visible;
            }

            MenuPool.ProcessMenus();
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

        private static bool IsShownInMenu(IMenuComponent component)
        {
            return AreaControlMenu.MenuItems.Contains(component.MenuItem);
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

        private void ItemChangeHandler(UIMenu sender, int newindex)
        {
            MenuItems.FirstOrDefault(x => x.MenuItem == sender.MenuItems[newindex])?.OnMenuHighlighted(this);
        }

        private static void RemoveItemFromMenu(IMenuComponent component)
        {
            if (IsShownInMenu(component))
                AreaControlMenu.RemoveItemAt(AreaControlMenu.MenuItems.IndexOf(component.MenuItem));

            AreaControlMenu.RefreshIndex();
        }

        private static void CloseMenu()
        {
            AreaControlMenu.Visible = false;
        }

        #endregion
    }
}