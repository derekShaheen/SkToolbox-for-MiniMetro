
using SkToolbox;
using SkToolbox.SkModules;
using SkToolbox.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkToolboxMiniMetro
{
    public class ModCity : IModule
    {
        public Game game;
        public GameController gameController;

        public Rect StatsWindow = new Rect(5, 100, 150, 50);
        public Rect LinesWindow = new Rect(325, 5, 500, 50);
        public bool IsGUIVisibleStation = false;
        public bool IsGUIVisibleLine = false;

        public bool IsNextStationVisible = false;
        public int NextStationIndex = 0;
        public FInstancedGeoSprite NextStationSprite;
        public Station NextStation;
        public FLabel WeekLabel;
        public int Week = 0;

        private string moduleName = "City";

        private ModCityMonoBehavior modCityGameobject;

        #region Standard Methods
        /// <summary>
        /// These methods will be used in every menu module. Copy / Paste this entire Standard Methods region to new modules as they are created.
        /// These are placed here instead of a base class due to Activator.CreateInstance() not being able to call subclass constructors. 
        /// 
        /// !! This region generally does not need to be modified.
        /// </summary>
        public SkMenu MenuOptions { get; set; } = new SkMenu();
        public SkMenuItem CallerEntry { get; set; } = new SkMenuItem();
        public SkUtilities.Status ModuleStatus { get; set; } = SkUtilities.Status.Initialized;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (IsEnabled && ModuleStatus != SkUtilities.Status.Ready) // If the module is ready, then it is loaded and running.
                {   // To disable, set the status to "Unload" so it properly unloads.
                    isEnabled = value;
                }
                if (!IsEnabled)
                {
                    IsEnabled = value;
                }
            }
        }

        public string ModuleName { get => moduleName; set => moduleName = value; }


        private bool isEnabled = true;

        internal bool conWriteToFile = false;

        public List<SkMenuItem> FlushMenu()
        {
            return MenuOptions.FlushMenu();
        }

        public void RequestMenu()
        {
            SkToolbox.Loaders.SkLoader.MenuController.RequestSubMenu(MenuOptions.FlushMenu());
        }

        public void RequestMenu(SkMenu Menu)
        {
            SkToolbox.Loaders.SkLoader.MenuController.RequestSubMenu(Menu);
        }

        public void RemoveModule()
        {

            throw new NotImplementedException();
            //Destroy(this);
        }

        public void Ready()
        {
            ModuleStatus = SkUtilities.Status.Ready;
        }
        public void Loading()
        {
            ModuleStatus = SkUtilities.Status.Loading;
        }
        public void Error()
        {
            ModuleStatus = SkUtilities.Status.Error;
        }
        public void Unload()
        {
            ModuleStatus = SkUtilities.Status.Unload;
        }
        #endregion Standard Methods


        #region Required but Individual
        public ModCity()
        {
            Loading();
            Start();
        }

        public void Start()
        {
            BeginMenu();
            CallerEntry = new SkMenuItem("City Menu   \t►", () => SkToolbox.Loaders.SkLoader.MenuController.RequestSubMenu(FlushMenu()));
            Ready(); // Must be called when the module has completed initialization. // End of Start

        }

        public void BeginMenu()
        {
            SkMenu GenericMenu = new SkMenu();

            GenericMenu.AddItemToggle("Station Information", ref IsGUIVisibleStation, new Action(ToggleStationGUI), "View station information");
            GenericMenu.AddItemToggle("Line Information", ref IsGUIVisibleLine, new Action(ToggleLineGUI), "View line information");
            GenericMenu.AddItemToggle("Next Station", ref IsNextStationVisible, new Action(ToggleNextStationViewer), "Identifies where the next station will appear");
            GenericMenu.AddItem("Show Week Number", new Action(AddWeekTime), "Adds the Week # to the UI");
            MenuOptions = GenericMenu;
        }
        #endregion Required but Individual

        public void AddWeekTime()
        {
            GetObjects();

            Font clockLabel = game?.HudScreen?.ClockWidget.DayFont;
            if (WeekLabel == null)
            {
                WeekLabel = new FLabel(clockLabel.GetFFont(LocaleDatabase.Instance.CurrentLocale), "Week " + game?.City.Clock.Week.ToString()
                    , LocaleDatabase.Instance.CurrentLocale.TextDirection, Main.Instance.Scene.MenuDetailPartitionId, 0, null);
                WeekLabel.alignToPixel = true;
                WeekLabel.x = game.HudScreen.ClockWidget.AdjustedX - 5;
                WeekLabel.y = 15;
                WeekLabel.scale = Main.AspectRatioDelta;
                WeekLabel.anchorX = 1f;
                WeekLabel.anchorY = 0.6f;

                game?.HudScreen?.ClockWidget.container.AddChild(WeekLabel);
            }
            else
            {
                WeekLabel.SetText(new LocalizedString(LocaleDatabase.Instance.CurrentLocale, "Week " + game?.City.Clock.Week.ToString()));
            }

        }

        public void ToggleStationGUI()
        {
            if (modCityGameobject == null)
            {
                modCityGameobject = SkToolbox.Loaders.SkLoader._SkGameObject.AddComponent<ModCityMonoBehavior>();
                modCityGameobject.parentClass = this;

            }
            IsGUIVisibleStation = !IsGUIVisibleStation;
            if (IsGUIVisibleStation)
            {
                GetObjects();
                StatsWindow = new Rect(5, 100, 150, 50);
            }
            BeginMenu();
        }

        public void ToggleNextStationViewer()
        {
            if (modCityGameobject == null)
            {
                modCityGameobject = SkToolbox.Loaders.SkLoader._SkGameObject.AddComponent<ModCityMonoBehavior>();
            }
            IsNextStationVisible = !IsNextStationVisible;
            if (IsNextStationVisible)
            {
                SetNextStation();
            }
            else
            {
                if (NextStationSprite != null)
                {
                    game.City.CityLayer.StationLayer.container.RemoveChild(NextStationSprite);
                }
            }
            BeginMenu();
        }

        public void SetNextStation()
        {
            if (modCityGameobject == null)
            {
                modCityGameobject = SkToolbox.Loaders.SkLoader._SkGameObject.AddComponent<ModCityMonoBehavior>();
            }
            GetObjects();

            NextStationIndex = game.City.StationCount + 1;
            NextStation = SkUtilities.GetPrivateField<Station[]>(game?.City, "stations")[NextStationIndex];
            if (NextStation != null)
            {
                NextStationSprite = GeoFactory.Instance.GetCross(10, Color.yellow, Main.Instance.Scene.MenuDetailPartitionId, 0); //GeoFactory.Instance.GetPlus(10, Color.white, Main.Instance.Scene.MenuDetailPartitionId, 0);
                NextStationSprite.SetPosition(NextStation.Position);
                game.City.CityLayer.StationLayer.container.AddChild(NextStationSprite);
            }
        }

        public void ToggleLineGUI()
        {
            IsGUIVisibleLine = !IsGUIVisibleLine;
            if (IsGUIVisibleLine)
            {
                GetObjects();
                LinesWindow = new Rect(325, 5, 500, 50);
            }
            BeginMenu();
        }

        public void GetObjects()
        {
            if (Main.Instance != null)
            {
                try
                {
                    gameController = SkUtilities.GetPrivateField<GameController>(Main.Instance, "controller");
                    if (gameController == null)
                    {
                        SkUtilities.Logz("Could not find game controller.");
                    }

                    game = SkUtilities.GetPrivateField<Game>(gameController, "game");
                    if (gameController == null)
                    {
                        SkUtilities.Logz("Could not find game.");
                    }
                }
                catch (InvalidCastException)
                {
                    //
                }
            }
        }
    }
}
