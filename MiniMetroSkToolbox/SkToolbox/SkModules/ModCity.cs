using SkToolbox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkToolbox.Utility;
using UnityEngine;

namespace SkToolbox.SkModules
{
    class ModCity : SkBaseModule, IModule
    {
        Game game;
        GameController gameController;

        private Rect StatsWindow = new Rect(5, 100, 150, 50);
        private Rect LinesWindow = new Rect(325, 5, 450, 50);
        private bool IsGUIVisibleStation = false;
        private bool IsGUIVisibleLine = false;
        private bool IsEditMode = false;

        public ModCity() : base()
        {
            base.ModuleName = "City";
            base.Loading();
        }

        public void Start()
        {
            BeginMenu();
            base.CallerEntry = new SkMenuItem("City Menu   \t►", () => base.SkMC.RequestSubMenu(base.FlushMenu()));
            base.Ready(); // Must be called when the module has completed initialization. // End of Start

        }

        public void BeginMenu()
        {
            SkMenu GenericMenu = new SkMenu();
            GenericMenu.AddItem("Set Gamemode ►", new Action(BeginSelectGamemode));
            GenericMenu.AddItemToggle("Toggle Edit Mode", ref IsEditMode, new Action(BeginEdit));
            GenericMenu.AddItemToggle("Toggle Station Information", ref IsGUIVisibleStation, new Action(ToggleStationGUI));
            GenericMenu.AddItemToggle("Toggle Line Information", ref IsGUIVisibleLine, new Action(ToggleLineGUI));
            MenuOptions = GenericMenu;
        }

        public void BeginSelectGamemode()
        {
            GetObjects();
            SkMenu skMenu = new SkMenu();
            foreach (string gameMode in Enum.GetNames(typeof(GameMode)))
            {
                if (!gameMode.Equals("None") && !gameMode.Equals("Count"))
                {
                    skMenu.AddItem(gameMode, new Action<string>(SetGamemode), "Current: " + game?.Mode.ToString());
                }
            }

            base.RequestMenu(skMenu);
        }

        public void BeginEdit()
        {
            GetObjects();
            IsEditMode = !IsEditMode;

            if(IsEditMode)
            {
                game.City.CityGrid.GenerateStationCircles(game.City, true);
                SkUtilities.InvokePrivateMethod(game.HudScreen, "AddSandboxToggleButton", new object[] { });
                game?.HudScreen?.ShowEditModeButton();
            } else
            {
                game?.HudScreen?.HideEditModeButton();
                SkUtilities.InvokePrivateMethod(game.HudScreen, "RemoveSandboxToggleButton", new object[] { });
            }
            SkUtilities.Logz("Edit mode set to: " + IsEditMode.ToString());

            BeginMenu();
        }

        public void SetGamemode(string pGameMode)
        {
            GetObjects();

            GameMode gameMode = GameMode.CLASSIC;

            Enum.TryParse(pGameMode, out gameMode);

            if (game != null)
            {
                game.Mode = gameMode;
                game?.HudScreen?.HandleGameModeChanged();
                SkUtilities.Logz("Game mode set to: " + gameMode);
            }
        }

        public void OnGUI()
        {
            if (IsGUIVisibleStation && game?.City?.StationCount > 0)
            {
                StatsWindow = GUILayout.Window(39999, StatsWindow, ProcessStats, "Station Information");
            }
            if (IsGUIVisibleLine && game?.City?.LineCount > 0)
            {
                LinesWindow = GUILayout.Window(39908, LinesWindow, ProcessLines, "Line Information");
            }
        }

        public void ToggleStationGUI()
        {
            IsGUIVisibleStation = !IsGUIVisibleStation;
            if (IsGUIVisibleStation)
            {
                GetObjects();
            }
            BeginMenu();
        }

        public void ToggleLineGUI()
        {
            IsGUIVisibleLine = !IsGUIVisibleLine;
            if (IsGUIVisibleLine)
            {
                GetObjects();
            }
            BeginMenu();
        }

        public void Update()
        {
            if ((IsGUIVisibleStation || IsGUIVisibleLine || IsEditMode) && game?.IsOver == true)
            {
                IsGUIVisibleStation = false;
                IsGUIVisibleLine = false;
                StatsWindow = new Rect(5, 100, 150, 50);
                LinesWindow = new Rect(325, 5, 450, 50);

                if (IsEditMode)
                {
                    IsEditMode = false;
                    game?.HudScreen?.HideEditModeButton();
                    SkUtilities.InvokePrivateMethod(game.HudScreen, "RemoveSandboxToggleButton", new object[] { });
                }

                BeginMenu();
            }
        }

        private void ProcessStats(int WindowID)
        {
                //StatsWindow.height = 47;
                GUILayout.BeginVertical();
                GUIStyle displayStyle = new GUIStyle(GUI.skin.box);
                displayStyle.richText = true;
                foreach (string stationtype in Enum.GetNames(typeof(StationType)))
                {
                    GUI.color = ThemeDatabase.Instance.MenuTheme.GetLineColor(((int)Enum.Parse(typeof(StationType), stationtype) % ThemeDatabase.Instance.MenuTheme.LineCount));
                    //GUI.backgroundColor = ThemeDatabase.Instance.MenuTheme.GetLineColor(((int)Enum.Parse(typeof(StationType), stationtype) % ThemeDatabase.Instance.MenuTheme.LineCount));
                    GUILayout.BeginHorizontal();
                    List<Station> stationList = game.City.GetStationsOfType((StationType)Enum.Parse(typeof(StationType), stationtype));
                    int? stationCount = stationList.Count;
                    int stationCapacity = 0;
                    int stationPeeps = 0;
                    int stationOver = 0;
                    foreach (Station station in stationList) {
                        stationCapacity += station.PeepCapacity;
                        stationPeeps += station.PeepCount;
                        if(station.PeepCount > station.PeepCapacity)
                        {
                            stationOver += 1;
                        }
                    }

                    if (stationCount > 0)
                    {
                        GUILayout.Button("<b>" + stationCount + " " + stationtype.Substring(0, 1) + stationtype.Substring(1).ToLower() + (stationCount > 1 ? "s" : "")  + "</b>", displayStyle);
                        GUILayout.Button("<b>Peeps Waiting: " + stationPeeps + "</b>", displayStyle);
                        if(stationOver > 0)
                        {
                            GUI.color = Color.red;
                            GUILayout.Button("<b>CROWDED: " + stationOver + "</b>", displayStyle);
                            GUI.color = ThemeDatabase.Instance.MenuTheme.GetLineColor(((int)Enum.Parse(typeof(StationType), stationtype) % ThemeDatabase.Instance.MenuTheme.LineCount));
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUI.color = Color.white;
                GUILayout.Button("<b>Total Stations: " + game?.City?.StationCount.ToString() + "</b>", displayStyle);
                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void ProcessLines(int WindowID)
        {
            int totalLinks = 0;
                //LinesWindow.height = 47;
                GUIStyle displayStyle = new GUIStyle(GUI.skin.box);
                GUILayout.BeginVertical();
                foreach (Line line in SkUtilities.GetPrivateField<List<Line>>(game.City, "lines"))
                {
                    totalLinks += line.LiveLinkCount + 1;
                    GUI.color = line.PeepColor;
                    GUI.backgroundColor = line.Color;
                    displayStyle.richText = true;

                    GUILayout.BeginHorizontal();
                    if (line.IsSelected || line.IsBeingEdited)
                    {
                        GUILayout.Label("<b>►</b>", displayStyle);
                    }
                    GUILayout.Button("<b>In Transit: " + line.PeepCount.ToString().PadLeft(2, '0') + "</b>", displayStyle);
                    GUILayout.Button("<b>Stations: " + (line.LiveLinkCount + 1).ToString().PadLeft(2, '0') + "</b>", displayStyle);
                    GUILayout.Button("<b>Length: " + Mathf.RoundToInt(line.Length).ToString().PadLeft(4, '0') + "</b>", displayStyle);
                    if (line.LiveLinkCount > 0)
                    {
                        GUILayout.Button("<b>Avg: " + Mathf.RoundToInt(line.Length / (line.LiveLinkCount + 1)).ToString().PadLeft(3, '0') + "</b>", displayStyle);
                    }
                    GUILayout.Button("<b>Tr: " + line.TrainCount + "</b>", displayStyle);
                    GUILayout.Button("<b>Ca: " + line.CarriageCount + "</b>", displayStyle);
                    GUILayout.EndHorizontal();
                }
                GUI.color = Color.white;
                GUILayout.BeginHorizontal();
                //GUILayout.Label("Peeps: " + game.City.PassengersInTransit + " / " + game.City.TotalCapacity);
                GUILayout.Button("<b>Total Links: " + totalLinks + "</b>", displayStyle);
                GUILayout.Button("<b>Total Length: " + Mathf.RoundToInt(game.City.TrackLength) 
                    + " (Avg: " + (Mathf.RoundToInt(game.City.TrackLength) / game.City.LineCount).ToString().PadLeft(3, '0') + ")</b>", displayStyle);
                GUILayout.Button("<b>Trip Count: " + game.City.TripCount + "</b>", displayStyle);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        public void GetObjects()
        {
            if (Main.Instance != null)
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
        }
    }
}
