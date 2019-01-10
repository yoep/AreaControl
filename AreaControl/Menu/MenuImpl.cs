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

        /// <inheritdoc />
        public bool IsMenuInitialized { get; private set; }

        /// <inheritdoc />
        public int TotalItems => MenuItems.Count;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void RegisterComponent(IMenuComponent component)
        {
            Assert.NotNull(component, "component cannot be null");

            MenuItems.Add(component);
            AreaControlMenu.AddItem(component.Item);
        }

        /// <inheritdoc />
        public void ReplaceComponent(IMenuComponent originalComponent, IMenuComponent newComponent)
        {
            if (!MenuItems.Contains(originalComponent))
                return;

            var index = AreaControlMenu.MenuItems.IndexOf(originalComponent.Item);
            _rage.LogTrivialDebug("Replacing menu item at index " + index);

            AreaControlMenu.MenuItems.Insert(index, newComponent.Item);
            MenuItems.Remove(originalComponent);
            MenuItems.Add(newComponent);
            RemoveItemFromMenu(originalComponent);
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

        private void Process(object sender, GraphicsEventArgs e)
        {
            if (Game.IsKeyDown(Keys.T))
            {
                foreach (var component in MenuItems)
                {
                    if (component.IsVisible && !IsShownInMenu(component))
                        AreaControlMenu.AddItem(component.Item);
                    if (!component.IsVisible && IsShownInMenu(component))
                        RemoveItemFromMenu(component);
                }

                AreaControlMenu.Visible = !AreaControlMenu.Visible;
            }

            MenuPool.ProcessMenus();
        }

        private static bool IsShownInMenu(IMenuComponent component)
        {
            return AreaControlMenu.MenuItems.Contains(component.Item);
        }

        private void ItemSelectionHandler(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            var menuComponent = MenuItems.FirstOrDefault(x => x.Item == selectedItem);

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
                _rage.DisplayNotification("could not invoke menu item, see log files for more info");
            }
            catch (Exception ex)
            {
                _rage.LogTrivial("*** An unexpected error occurred while activating the menu item ***" +
                                 Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                _rage.DisplayNotification("an unexpected error occurred while invoking the menu action");
            }
        }

        private static void RemoveItemFromMenu(IMenuComponent component)
        {
            if (IsShownInMenu(component))
                AreaControlMenu.RemoveItemAt(AreaControlMenu.MenuItems.IndexOf(component.Item));
            
            AreaControlMenu.RefreshIndex();
        }

        private static void CloseMenu()
        {
            AreaControlMenu.Visible = false;
        }

        #endregion
    }
}