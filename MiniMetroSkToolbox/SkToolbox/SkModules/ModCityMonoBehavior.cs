using SkToolbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SkToolboxMiniMetro
{
    public class ModCityMonoBehavior : MonoBehaviour
    {
        public ModCity parentClass;

        public ModCityMonoBehavior()
        {
            gameObject.name = "ModCityGameObject";
            base.transform.SetParent(SkToolbox.Loaders.SkLoader._SkGameObject.transform);

        }

        public void Update()
        {
            if (parentClass.game == null || parentClass.gameController == null)
            {
                parentClass.GetObjects();
            }
            if ((parentClass.IsGUIVisibleStation || parentClass.IsGUIVisibleLine || parentClass.IsNextStationVisible) && parentClass.game?.IsOver == true)
            {
                parentClass.IsGUIVisibleStation = false;
                parentClass.IsGUIVisibleLine = false;
                parentClass.IsNextStationVisible = false;
                parentClass.StatsWindow = new Rect(5, 100, 150, 50);
                parentClass.LinesWindow = new Rect(325, 5, 500, 50);

                parentClass.WeekLabel = null;
                parentClass.Week = 0;

                parentClass.BeginMenu();
                parentClass.game = null;
                parentClass.gameController = null;

            }

            if (parentClass.game != null && !parentClass.game.IsOver && parentClass.Week != parentClass.game.City.Clock.Week && parentClass.WeekLabel != null)
            {
                parentClass.Week = parentClass.game.City.Clock.Week;
                parentClass.WeekLabel.SetText(new LocalizedString(LocaleDatabase.Instance.CurrentLocale, "Week " + (parentClass.Week + 1)));
            }

            if (parentClass.game != null && !parentClass.game.IsOver && parentClass.IsNextStationVisible)
            {
                if (parentClass.NextStationIndex != (parentClass.game.City.StationCount + 1))
                {
                    parentClass.game.City.CityLayer.StationLayer.container.RemoveChild(parentClass.NextStationSprite);
                    parentClass.SetNextStation();
                }
            }
        }

        private void ProcessStats(int WindowID)
        {
            GUILayout.BeginVertical();
            GUIStyle displayStyle = new GUIStyle(GUI.skin.box);
            displayStyle.richText = true;
            foreach (string stationtype in Enum.GetNames(typeof(StationType)))
            {
                GUI.color = ThemeDatabase.Instance.MenuTheme.GetLineColor(((int)Enum.Parse(typeof(StationType), stationtype) % ThemeDatabase.Instance.MenuTheme.LineCount));
                //GUI.backgroundColor = ThemeDatabase.Instance.MenuTheme.GetLineColor(((int)Enum.Parse(typeof(StationType), stationtype) % ThemeDatabase.Instance.MenuTheme.LineCount));
                GUILayout.BeginHorizontal();
                List<Station> stationList = parentClass.game.City.GetStationsOfType((StationType)Enum.Parse(typeof(StationType), stationtype));
                int? stationCount = stationList.Count;
                int stationCapacity = 0;
                int stationPeeps = 0;
                int stationOver = 0;
                foreach (Station station in stationList)
                {
                    stationCapacity += station.PeepCapacity;
                    stationPeeps += station.PeepCount;
                    if (station.PeepCount > station.PeepCapacity)
                    {
                        stationOver += 1;
                    }
                }

                if (stationCount > 0)
                {
                    GUILayout.Button("<b>" + stationCount + " " + (stationtype.Substring(0, 1) + stationtype.Substring(1).ToLower() + (stationCount > 1 ? "s" : "")).ToString().PadLeft(9, ' ') + "</b>", displayStyle);
                    GUILayout.Button("<b>Shapes Waiting: " + stationPeeps + "</b>", displayStyle);
                    if (stationOver > 0)
                    {
                        GUI.color = Color.red;
                        GUILayout.Button("<b>CROWDED: " + stationOver + "</b>", displayStyle);
                        GUI.color = ThemeDatabase.Instance.MenuTheme.GetLineColor(((int)Enum.Parse(typeof(StationType), stationtype) % ThemeDatabase.Instance.MenuTheme.LineCount));
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUI.color = Color.white;
            GUILayout.Button("<b>Total Stations: " + parentClass.game?.City?.StationCount.ToString() + "</b>", displayStyle);
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void ProcessLines(int WindowID)
        {
            int totalLinks = 0;
            //LinesWindow.height = 47;
            GUIStyle displayStyle = new GUIStyle(GUI.skin.box);
            GUILayout.BeginVertical();
            int averageLength = Mathf.RoundToInt(parentClass.game.City.TrackLength) / parentClass.game.City.LineCount;
            foreach (Line line in SkUtilities.GetPrivateField<List<Line>>(parentClass.game.City, "lines"))
            {
                try
                {
                    if (line != null)
                    {
                        totalLinks += line.LiveLinkCount + (line.IsLooping ? 0 : 1);
                        GUI.color = line.PeepColor;
                        GUI.backgroundColor = line.Color;
                        displayStyle.richText = true;

                        GUILayout.BeginHorizontal();
                        if (line.IsSelected || line.IsBeingEdited)
                        {
                            GUILayout.Label("<b>►</b>", displayStyle);
                        }

                        GUILayout.Button("<b>In Transit: " + line.PeepCount.ToString().PadLeft(2, '0') + " / "
                            + ((line.TrainCount * 6) + (line.CarriageCount * 6)).ToString().PadLeft(2, '0')
                            + " (" + Math.Round((double)line.PeepCount / ((line.TrainCount * 6) + (line.CarriageCount * 6)) * 100, 0).ToString().PadLeft(2, '0') + "%)"
                            + "</b>", displayStyle);
                        GUILayout.Button("<b>Stations: " + (line.LiveLinkCount + 1).ToString().PadLeft(2, '0') + "</b>", displayStyle);
                        GUILayout.Button("<b>Length: " + Mathf.RoundToInt(line.Length).ToString().PadLeft(4, '0')
                            + " (" + (Mathf.RoundToInt(line.Length - averageLength)).ToString().PadLeft(3, '0') + ")" + "</b>", displayStyle);
                        if (line.LiveLinkCount > 0)
                        {
                            GUILayout.Button("<b>Avg: " + Mathf.RoundToInt((line.Length / (line.LiveLinkCount + (line.IsLooping ? 0 : 1)))) + "</b>", displayStyle);
                            GUILayout.Button("<b>Eff: " + Mathf.RoundToInt(Audio.GetLineEfficiency(line.Index) * 100) + "%</b>", displayStyle);
                        }
                        if (line.TrainCount < 1)
                        {
                            GUI.color = Color.red;
                        }
                        GUILayout.Button("<b>Tr: " + line.TrainCount + "</b>", displayStyle);
                        GUI.color = line.PeepColor;
                        GUILayout.Button("<b>Ca: " + line.CarriageCount + "</b>", displayStyle);
                        GUILayout.EndHorizontal();
                    }
                }
                catch (Exception)
                {

                }
            }
            GUI.color = Color.white;
            GUILayout.BeginHorizontal();
            //GUILayout.Label("Peeps: " + game.City.PassengersInTransit + " / " + game.City.TotalCapacity);
            GUILayout.Button("<b>Total Links: " + totalLinks + "</b>", displayStyle);
            GUILayout.Button("<b>Total Length: " + Mathf.RoundToInt(parentClass.game.City.TrackLength)
                + " (Avg: " + (averageLength).ToString().PadLeft(3, '0') + ")</b>", displayStyle);
            GUILayout.Button("<b>Trip Count: " + parentClass.game.City.TripCount + "</b>", displayStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        public void OnGUI()
        {
            if (parentClass.IsGUIVisibleStation && parentClass.game?.City?.StationCount > 0)
            {
                parentClass.StatsWindow = GUILayout.Window(39999, parentClass.StatsWindow, ProcessStats, "Station Information");
            }
            if (parentClass.IsGUIVisibleLine && parentClass.game?.City?.LineCount > 0)
            {
                parentClass.LinesWindow = GUILayout.Window(39908, parentClass.LinesWindow, ProcessLines, "Line Information");
            }
        }
    }
}
