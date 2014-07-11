using System;
using UnityEngine;
using Toolbar;
using System.Collections.Generic;


namespace Nereid
{
   namespace FinalFrontier
   {
      public class HallOfFameBrowser : PositionableWindow
      {
         private static readonly int KERBAL_BUTTON_WIDTH = 140;
         private static readonly int KERBAL_AREA_WIDTH = 450;
         private static readonly int KERBAL_AREA_HEIGHT = 65;
         private readonly Texture2D TEXTURE_AVAILABLE;
         private readonly Texture2D TEXTURE_ASSIGNED;
         private readonly Texture2D TEXTURE_KILLED;
         private GUIStyle STYLE_KERBAL_BUTTON;
         private GUIStyle STYLE_KERBAL_STATUS;
         private GUIStyle STYLE_KERBAL_AREA;
         private GUIStyle STYLE_KERBAL_AREA_EXPANDED;
         private GUIStyle STYLE_RIBBON_AREA;
         private static readonly int STYLE_SCROLLVIEW_HEIGHT = 450;

         private RibbonBrowser ribbonBrowser;
         private DisplayWindow display;
         private AboutWindow about;
         private ConfigWindow configWindow;

         private Vector2 scrollPosition = Vector2.zero;

         private IButton toolbarButton;
         private String toolbarButtonTextureOn;
         private String toolbarButtonTextureOff;

         // Filter
         HallOfFameFilter filter;
         // Sorter
         HallOfFameSorter sorter;

         public class GameSceneBased
         {
            private GameScenes scene;

            protected GameSceneBased(GameScenes scene)
            {
               this.scene = scene;
            }

            public GameScenes GetScene()
            {
               return scene;
            }

            public override bool Equals(System.Object right)
            {
               if (right == null) return false;
               GameSceneBased cmp = right as GameSceneBased;
               if (cmp == null) return false;
               return scene.Equals(cmp.scene);
            }

            public override int GetHashCode()
            {
               return scene.GetHashCode();
            }
         }

         public class HallOfFameFilter : GameSceneBased,Filter<HallOfFameEntry>
         {
            public bool showDead { get; set; }
            public bool showAssigned { get; set; }
            public bool showAvailable { get; set; }
            public bool showUndecorated { get; set; }
            public bool showFlightOnly { get; set; }

            public HallOfFameFilter(GameScenes scene, bool showDead = true, bool showAssigned = true, bool showAvailable = true, bool showUndecorated = true, bool showFlightOnly = true)
               : base(scene)
            {
               this.showDead = true;
               this.showAssigned = showAssigned;
               this.showAvailable = showAvailable;
               this.showUndecorated = showUndecorated;
               this.showFlightOnly = showFlightOnly;
            }

            public bool Accept(HallOfFameEntry x)
            {
               ProtoCrewMember kerbal = x.GetKerbal();
               if (kerbal == null) return false;
               if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.DEAD && !showDead) return false;
               if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.MISSING && !showDead) return false;
               if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.ASSIGNED && !showAssigned) return false;
               if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.AVAILABLE && !showAvailable) return false;
               //
               if (showFlightOnly && FlightGlobals.ActiveVessel != null && !kerbal.InCrewOfActiveFlight()) return false;
               //
               if (x.GetRibbons().Count == 0 && !showUndecorated) return false;
               return true;

            }

            public override string ToString()
            {
               return GetScene() + ": dead=" + showDead + ", assigned=" + showAssigned + ", available=" + showAvailable + ", undecorated=" + showUndecorated + ", flight only=" + showFlightOnly;
            }
         }

         public class HallOfFameSorter : GameSceneBased, Sorter<HallOfFameEntry>
         {
            public enum DIRECTION { ASCENDING = 1, DESCENDING = 2 };
            public enum SORT_BY { NAME = 1, MISSIONS = 2, MISSIONTIME = 3, RIBBONS = 4, DOCKINGS = 5, EVA = 6, STATE = 7 }
            //
            private DIRECTION direction;
            private SORT_BY sort_by;

            public HallOfFameSorter(GameScenes scene, DIRECTION direction = DIRECTION.ASCENDING, SORT_BY predicate = SORT_BY.NAME)
             :  base(scene)
            {
               this.direction = direction;
               this.sort_by = predicate;
            }

            public String PredicateAsString()
            {
               switch (sort_by)
               {
                  case SORT_BY.NAME: return "Name";
                  case SORT_BY.MISSIONS: return "Missions";
                  case SORT_BY.MISSIONTIME: return "Mission Time";
                  case SORT_BY.RIBBONS: return "Ribbons";
                  case SORT_BY.DOCKINGS: return "Dockings";
                  case SORT_BY.EVA: return "Eva";
                  case SORT_BY.STATE: return "State";
               }
               return "Unknown";
            }

            public String DirectionAsString()
            {
               switch (direction)
               {
                  case DIRECTION.ASCENDING: return "ASCENDING";
                  case DIRECTION.DESCENDING: return "DESCENDING";
               }
               return "UNKNOWN";
            }


            public void NextPredicate()
            {
               sort_by++;
               if ((int)sort_by > 7) sort_by = SORT_BY.NAME;
               HallOfFame.instance.Sort();
            }

            public void NextDirection()
            {
               direction++;
               if ((int)direction > 2) direction = DIRECTION.ASCENDING;
               HallOfFame.instance.Sort();
            }

            public void Sort(List<HallOfFameEntry> list)
            {
               if (list == null) return;
               list.Sort(delegate(HallOfFameEntry left, HallOfFameEntry right)
               {
                  int sign = direction == DIRECTION.ASCENDING ? 1 : -1;
                  int cmp;
                  switch (sort_by)
                  {
                     case SORT_BY.NAME:
                        return sign * (left.GetName().CompareTo(right.GetName()));
                     case SORT_BY.MISSIONS:
                        cmp = sign * (left.MissionsFlown - right.MissionsFlown);
                        if (cmp != 0) return cmp;
                        return left.GetName().CompareTo(right.GetName());
                     case SORT_BY.MISSIONTIME:
                        cmp = (int)(sign * (left.TotalMissionTime - right.TotalMissionTime));
                        if (cmp != 0) return cmp;
                        return left.GetName().CompareTo(right.GetName());
                     case SORT_BY.RIBBONS:
                        cmp = sign * (left.GetRibbons().Count - right.GetRibbons().Count);
                        if (cmp != 0) return cmp;
                        return left.GetName().CompareTo(right.GetName());
                     case SORT_BY.STATE:
                        cmp = sign * (left.GetKerbal().rosterStatus.CompareTo(right.GetKerbal().rosterStatus));
                        if (cmp != 0) return cmp;
                        return left.GetName().CompareTo(right.GetName());
                     case SORT_BY.DOCKINGS:
                        cmp = sign * (left.Dockings - right.Dockings);
                        if (cmp != 0) return cmp;
                        return left.GetName().CompareTo(right.GetName());
                     case SORT_BY.EVA:
                        cmp = sign * (int)(left.TotalEvaTime - right.TotalEvaTime);
                        if (cmp != 0) return cmp;
                        return left.GetName().CompareTo(right.GetName());
                     default:
                        Log.Error("unknown sorting method");
                        return 0;
                  }
               });
            }

            public void SetDirection(DIRECTION direction)
            {
               this.direction = direction;
               HallOfFame.instance.Sort();
            }

            public void SetSortPredicate(SORT_BY predicate)
            {
               this.sort_by = predicate;
               HallOfFame.instance.Sort();
            }

            public DIRECTION GetDirection()
            {
               return direction;
            }

            public SORT_BY GetSortPredicate()
            {
               return sort_by;
            }

            public override string ToString()
            {
               return GetScene() + ": sort by " + sort_by + " " + direction;
            }
         }

         // expanded ribbon area (-1: none)
         private int expandedRibbonAreaIndex = -1;


         public HallOfFameBrowser()
            : base(Constants.WINDOW_ID_HALLOFFAMEBROWSER, FinalFrontier.configuration.GetHallOfFameWindowTitle())
         {
            STYLE_KERBAL_BUTTON = new GUIStyle(HighLogic.Skin.button);
            STYLE_KERBAL_BUTTON.fixedWidth = KERBAL_BUTTON_WIDTH;
            STYLE_KERBAL_BUTTON.clipping = TextClipping.Clip;
            STYLE_KERBAL_STATUS = new GUIStyle(HighLogic.Skin.button);
            STYLE_KERBAL_STATUS.fixedWidth = 20;
            STYLE_KERBAL_AREA = new GUIStyle(HighLogic.Skin.box);
            STYLE_KERBAL_AREA.fixedWidth = KERBAL_AREA_WIDTH;
            STYLE_KERBAL_AREA.fixedHeight = KERBAL_AREA_HEIGHT;
            STYLE_KERBAL_AREA.clipping = TextClipping.Clip;
            STYLE_KERBAL_AREA_EXPANDED = new GUIStyle(HighLogic.Skin.box);
            STYLE_KERBAL_AREA_EXPANDED.fixedWidth = KERBAL_AREA_WIDTH;
            STYLE_KERBAL_AREA_EXPANDED.stretchHeight = true;
            STYLE_KERBAL_AREA_EXPANDED.clipping = TextClipping.Clip;
            STYLE_RIBBON_AREA = new GUIStyle(HighLogic.Skin.label);
            STYLE_RIBBON_AREA.stretchHeight = true;
            STYLE_RIBBON_AREA.stretchWidth = true;
            STYLE_RIBBON_AREA.padding = new RectOffset(10, 10, 2, 2);

            TEXTURE_AVAILABLE = ImageLoader.GetTexture(FinalFrontier.RESOURCE_PATH + "active");
            TEXTURE_ASSIGNED = ImageLoader.GetTexture(FinalFrontier.RESOURCE_PATH + "assigned");
            TEXTURE_KILLED = ImageLoader.GetTexture(FinalFrontier.RESOURCE_PATH + "killed");

            ribbonBrowser = new RibbonBrowser();
            display = new DisplayWindow();
            about = new AboutWindow();
            configWindow = new ConfigWindow();
         }


         protected override void OnWindow(int id)
         {
            
            if (HallOfFame.instance == null) return;
            //
            // persistent filter for displaying kerbals
            if(filter==null || filter.GetScene()!=HighLogic.LoadedScene) this.filter = FinalFrontier.configuration.GetDisplayFilterForSzene(HighLogic.LoadedScene);
            //
            // persistent sorter for displaying kerbals
            if (sorter == null || sorter.GetScene() != HighLogic.LoadedScene)
            {
               // sorter has changed
               this.sorter = FinalFrontier.configuration.GetHallOfFameSorterForScene(HighLogic.LoadedScene);
               HallOfFame.instance.SetSorter(this.sorter);
            }
            //
            GUILayout.BeginHorizontal();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(490), GUILayout.Height(STYLE_SCROLLVIEW_HEIGHT));
            GUILayout.BeginVertical();
            int index = 0;
            bool expandDetected = false;
            bool autoexpandEnabled = FinalFrontier.configuration.IsAutoExpandEnabled();
            foreach (HallOfFameEntry entry in HallOfFame.instance)
            {
               // autoexpand this entry on mouse hover?
               bool expandedEntry = autoexpandEnabled && (expandedRibbonAreaIndex == index) && (entry.GetRibbons().Count > Constants.MAX_RIBBONS_PER_AREA);
               //
               ProtoCrewMember kerbal = entry.GetKerbal();
               String info = GetInfo(entry);
               String missionTimeInDays = Utils.GameTimeInDaysAsString(entry.TotalMissionTime) + (GameUtils.IsKerbinTimeEnabled() ? " kerbin" : "");
               if (kerbal != null && filter.Accept(entry))
               {
                  String buttonTooltip = kerbal.name + ": " + entry.MissionsFlown + " missions, " + missionTimeInDays + " days mission time";
                  GUILayout.BeginHorizontal(STYLE_KERBAL_AREA_EXPANDED); //expandedEntry ? STYLE_KERBAL_AREA_EXPANDED : STYLE_KERBAL_AREA);          
                  GUILayout.BeginVertical();
                  GUILayout.BeginHorizontal();
                  // butto to open decoration board
                  if(GUILayout.Button(new GUIContent(entry.GetName(), buttonTooltip), STYLE_KERBAL_BUTTON))
                  {
                     Log.Detail("opening decoration board for kerbal " + entry.GetName());
                     display.SetEntry(entry);
                     display.SetVisible(true);
                  }
                  DrawStatus(kerbal);
                  GUILayout.EndHorizontal();
                  GUILayout.Label(info, FFStyles.STYLE_LABEL);
                  GUILayout.EndVertical();
                  DrawRibbons(entry, expandedEntry ? Constants.MAX_RIBBONS_PER_EXPANDED_AREA : Constants.MAX_RIBBONS_PER_AREA);
                  GUILayout.EndHorizontal();
                  //
                  if(Event.current.type == EventType.Repaint)
                  {
                     if (MouseOver(0.0f,scrollPosition.y))
                     {
                        expandedRibbonAreaIndex = index;
                        expandDetected = true;
                     }
                  }
               }
               index++;
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Close", FFStyles.STYLE_BUTTON))
            {
               SetVisible(false);
               ribbonBrowser.SetVisible(false);
               display.SetVisible(false);
            }
            GUILayout.Label("", FFStyles.STYLE_LABEL);
            if (GUILayout.Button("Ribbons", FFStyles.STYLE_BUTTON))
            {
               if(!ribbonBrowser.IsVisible())
               {
                  OpenRibbonBrowser();
               }
               else
               {
                  ribbonBrowser.SetVisible(false);
               }
            }
            GUILayout.Label("", FFStyles.STYLE_LABEL);
            GUILayout.Label("Filter:", FFStyles.STYLE_LABEL);
            if (GUILayout.Toggle(filter.showDead, "dead", FFStyles.STYLE_TOGGLE)) filter.showDead = true; else filter.showDead = false;
            if (GUILayout.Toggle(filter.showAssigned, "active", FFStyles.STYLE_TOGGLE)) filter.showAssigned = true; else filter.showAssigned = false;
            if (GUILayout.Toggle(filter.showAvailable, "available", FFStyles.STYLE_TOGGLE)) filter.showAvailable = true; else filter.showAvailable = false;
            if (GUILayout.Toggle(filter.showUndecorated, "undecorated", FFStyles.STYLE_TOGGLE)) filter.showUndecorated = true; else filter.showUndecorated = false;
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
               if (GUILayout.Toggle(filter.showFlightOnly, "flight only", FFStyles.STYLE_TOGGLE)) filter.showFlightOnly = true; else filter.showFlightOnly = false;
            }
            GUILayout.Label("", FFStyles.STYLE_LABEL); // fixed space
            
            // sorter
            GUILayout.FlexibleSpace();
            GUILayout.Label("Sort by:", FFStyles.STYLE_LABEL);
            DrawSorterButtons();
            GUILayout.Label("", FFStyles.STYLE_LABEL); // fixed space

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Config", FFStyles.STYLE_BUTTON))
            {
               if (!configWindow.IsVisible()) MoveWindowAside(configWindow);
               configWindow.SetVisible(!configWindow.IsVisible());
            }
            if (GUILayout.Button("About", FFStyles.STYLE_BUTTON)) about.SetVisible(!about.IsVisible());
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (Event.current.type == EventType.Repaint && !expandDetected)
            {
               expandedRibbonAreaIndex = -1;
            }
            
            DragWindow();

         }

         private String GetInfo(HallOfFameEntry entry)
         {
            switch(sorter.GetSortPredicate())
            {
               default:
                  return entry.MissionsFlown + " missions";
               case HallOfFameSorter.SORT_BY.EVA:
                  return Utils.GameTimeAsString(entry.TotalEvaTime) + " in EVA";
               case HallOfFameSorter.SORT_BY.DOCKINGS:
                  return entry.Dockings + " docking operations";
               case HallOfFameSorter.SORT_BY.MISSIONTIME:
                  return Utils.GameTimeInDaysAsString(entry.TotalMissionTime) + " days in missions";
            }
         }

         private void DrawSorterButtons()
         {
            if (GUILayout.Button(sorter.PredicateAsString(), FFStyles.STYLE_BUTTON))
            {
               sorter.NextPredicate();
            }
            if (GUILayout.Button(sorter.DirectionAsString(), FFStyles.STYLE_NARROW_BUTTON))
            {
               sorter.NextDirection();
            }
         }


         private void DrawStatus(ProtoCrewMember kerbal)
         {
            ProtoCrewMember.RosterStatus status = kerbal.rosterStatus;
            String tooltip;
            switch(status)
            {
               case ProtoCrewMember.RosterStatus.DEAD:
               case ProtoCrewMember.RosterStatus.MISSING:
                  tooltip = kerbal.name + " is dead";
                  GUILayout.Label(new GUIContent(TEXTURE_KILLED, tooltip), STYLE_KERBAL_STATUS);
                  break;
               case ProtoCrewMember.RosterStatus.ASSIGNED:
                  tooltip = kerbal.name + " is currently on a mission";
                  GUILayout.Label(new GUIContent(TEXTURE_ASSIGNED, tooltip), STYLE_KERBAL_STATUS);
                  break;
               default:
                  tooltip = kerbal.name + " is available for next mission";
                  GUILayout.Label(new GUIContent(TEXTURE_AVAILABLE, tooltip), STYLE_KERBAL_STATUS);
                  break;
            }
         }

         private void OpenRibbonBrowser()
         {
            float x = bounds.x + bounds.width + 8;
            float y = bounds.y;
            if (x + RibbonBrowser.WIDTH > Screen.width)
            {
               x = Screen.width - RibbonBrowser.WIDTH;
            }
            ribbonBrowser.SetVisible(true, x, y);
         }


         private void DrawRibbons(HallOfFameEntry entry, int max)
         {
            ProtoCrewMember kerbal = entry.GetKerbal();
            List<Ribbon> ribbons = entry.GetRibbons();

            GUILayout.BeginVertical(STYLE_RIBBON_AREA);
            int n = 0;
            int RIBBONS_PER_LINE = 4;
            foreach (Ribbon ribbon in ribbons)
            {
               if (n % RIBBONS_PER_LINE == 0) GUILayout.BeginHorizontal();
               String tooltip = ribbon.GetName() + "\n" + ribbon.GetText();
               GUILayout.Button(new GUIContent(ribbon.GetTexture(), tooltip), FFStyles.STYLE_RIBBON);
               n++;
               if (n % RIBBONS_PER_LINE == 0) GUILayout.EndHorizontal();
               if (n >= max) break;
            }
            if (n % RIBBONS_PER_LINE != 0) GUILayout.EndHorizontal();
            GUILayout.EndVertical();
         }

         protected void DrawRibbon(int x, int y, Ribbon ribbon, int scale = 1)
         {
            Rect rect = new Rect(x, y, ribbon.GetWidth() / scale, ribbon.GetHeight() / scale);
            GUI.DrawTexture(rect, ribbon.GetTexture());
         }

         protected override void OnOpen()
         {
            base.OnOpen();
            HallOfFame.instance.Refresh();
            if (toolbarButton != null)
            {
               toolbarButton.TexturePath = toolbarButtonTextureOn;
            }
         }

         protected override void OnClose()
         {
            base.OnClose();
            if (toolbarButton != null)
            {
               toolbarButton.TexturePath = toolbarButtonTextureOff;
            }
         }



         public override int GetInitialWidth()
         {
            return 650;
         }

         public void registerToolbarButton(IButton button, String textureOn, String textureOff)
         {
            this.toolbarButton = button;
            this.toolbarButtonTextureOn = textureOn;
            this.toolbarButtonTextureOff = textureOff;
         }

      }
   }
}


