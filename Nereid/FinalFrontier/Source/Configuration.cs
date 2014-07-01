using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Nereid
{
   namespace FinalFrontier
   {
      public class Configuration
      {
         private static readonly String ROOT_PATH = Utils.GetRootPath();
         private static readonly String CONFIG_BASE_FOLDER = ROOT_PATH + "/GameData/";
         private static readonly String FILE_NAME = "FinalFrontier.dat";
         private static readonly Int16 FILE_MARKER = 0x7A7A;
         private static readonly Int16 FILE_VERSION = 1;

         private bool customRibbonAtSpaceCenterEnabled = false;
         private Log.LEVEL logLevel = Log.LEVEL.INFO;
         private bool autoExpandEnabled = true;
         private bool hotkeyEnabled = true;
         private bool removalOfRibbonsEnabled = false;
         private bool missonSummaryPopup = true;

         private readonly Pair<int, int> ORIGIN = new Pair<int, int>(0, 0);
         private Dictionary<int, Pair<int, int>> windowPositions = new Dictionary<int, Pair<int, int>>();

         private Dictionary<GameScenes, HallOfFameBrowser.HallOfFameFilter> hallOfFameFilter = new Dictionary<GameScenes, HallOfFameBrowser.HallOfFameFilter>();
         private Dictionary<GameScenes, HallOfFameBrowser.HallOfFameSorter> hallOfFameSorter = new Dictionary<GameScenes, HallOfFameBrowser.HallOfFameSorter>();

         // configurable window titles
         private String hallOfFameWindowTitle = "Final Frontier Hall of Fame";
         private String decorationBoardWindowTitle = "Kerbal Decoration Board";
         private String missionSummaryWindowTitle = "Mission Summary";

         // FAR used?
         public bool UseFARCalculations = External.FAR_ENABLED;

         public Configuration()
         {
            // default window positions
            ResetWindowPositions();
            // 
            // Default filter/sorts
            foreach (GameScenes scene in Enum.GetValues(typeof(GameScenes)))
            {
               this.hallOfFameFilter[scene] = new HallOfFameBrowser.HallOfFameFilter(scene);
               this.hallOfFameSorter[scene] = new HallOfFameBrowser.HallOfFameSorter(scene);
            }
         }

         public String GetHallOfFameWindowTitle()
         {
            return hallOfFameWindowTitle;
         }

         public void SetHallOfFameWindowTitle(String title)
         {
            this.hallOfFameWindowTitle = title;
         }

         public String GetDecorationBoardWindowTitle()
         {
            return decorationBoardWindowTitle;
         }


         public void SetDecorationBoardWindowTitle(String title)
         {
            this.decorationBoardWindowTitle = title;
         }


         public String GetMissionSummaryWindowTitle()
         {
            return missionSummaryWindowTitle;
         }


         public void SetMissionSummaryWindowTitle(String title)
         {
            this.missionSummaryWindowTitle = title;
         }

         public HallOfFameBrowser.HallOfFameFilter GetDisplayFilterForSzene(GameScenes scene)
         {
            HallOfFameBrowser.HallOfFameFilter filter = hallOfFameFilter[scene];
            if (filter != null) return filter;
            hallOfFameFilter[scene] = new HallOfFameBrowser.HallOfFameFilter(scene);
            return hallOfFameFilter[scene];
         }

         public HallOfFameBrowser.HallOfFameSorter GetHallOfFameSorterForScene(GameScenes scene)
         {
            HallOfFameBrowser.HallOfFameSorter sorter = hallOfFameSorter[scene];
            if (sorter != null) return sorter;
            hallOfFameSorter[scene] = new HallOfFameBrowser.HallOfFameSorter(scene);
            return hallOfFameSorter[scene];
         }

         public void ResetWindowPositions()
         {
            SetWindowPosition(Constants.WINDOW_ID_HALLOFFAMEBROWSER, 150, 50);
            SetWindowPosition(Constants.WINDOW_ID_DISPLAY, 810, 50);
            SetWindowPosition(Constants.WINDOW_ID_ABOUT, 50, 50);
            SetWindowPosition(Constants.WINDOW_ID_CONFIG, 150, 50);
         }

         public Pair<int, int> GetWindowPosition(AbstractWindow window)
         {
            return GetWindowPosition(window.GetWindowId());
         }

         public Pair<int, int> GetWindowPosition(int windowId)
         {
            try
            {
               return windowPositions[windowId];
            }
            catch (KeyNotFoundException)
            {
               Log.Warning("no initial position found for window "+windowId);
               return ORIGIN;
            }
         }

         public void SetWindowPosition(AbstractWindow window)
         {
            SetWindowPosition(window.GetWindowId(), window.GetX(), window.GetY());
         }

         public void SetWindowPosition(int windowId, Pair<int,int> position)
         {
            Log.Trace("set window position for window id " + windowId + ": " + position);
            if (windowPositions.ContainsKey(windowId))
            {
               windowPositions[windowId] = position;
            }
            else
            {
               windowPositions.Add(windowId, position);
            }
         }

         public void SetWindowPosition(int windowId, int x, int y)
         {
            SetWindowPosition(windowId, new Pair<int, int>(x, y));
         }


         public bool IsCustomRibbonAtSpaceCenterEnabled()
         {
            return customRibbonAtSpaceCenterEnabled;
         }

         public void SetCustomRibbonAtSpaceCenterEnabled(bool enabled)
         {
            customRibbonAtSpaceCenterEnabled = enabled;
         }

         public bool IsAutoExpandEnabled()
         {
            return autoExpandEnabled;
         }
         
         public void SetAutoExpandEnabled(bool enabled)
         {
            autoExpandEnabled = enabled;
         }

         public bool IsHotkeyEnabled()
         {
            return hotkeyEnabled;
         }

         public void SetHotkeyEnabled(bool enabled)
         {
            hotkeyEnabled = enabled;
         }

         public bool IsMissionSummaryEnabled()
         {
            return missonSummaryPopup;
         }

         public void SetMissionSummaryEnabled(bool enabled)
         {
            missonSummaryPopup = enabled;
         }

         public bool IsRevocationOfRibbonsEnabled()
         {
            return removalOfRibbonsEnabled;
         }

         public void SetRevocationOfRibbonsEnabled(bool enabled)
         {
            removalOfRibbonsEnabled = enabled;
         }

         public Log.LEVEL GetLogLevel()
         {
            return logLevel;
         }

         public void SetLogLevel(Log.LEVEL level)
         {
            logLevel = level;
         }

         private void WriteWindowPositions(BinaryWriter writer)
         {
            Log.Info("storing window positions");
            writer.Write((Int16)windowPositions.Count);
            Log.Info("writing " + windowPositions.Count + " window positions");
            foreach (int id in windowPositions.Keys)
            {
               Pair<int, int> position = windowPositions[id];
               Log.Trace("window position for window id " + id + " written: " + position.first + "/" + position.second);
               writer.Write((Int32)id);
               writer.Write((Int16)position.first);
               writer.Write((Int16)position.second);
            }
         }

         private void ReadWindowPositions(BinaryReader reader)
         {
            Log.Detail("loading window positions");
            int count = reader.ReadInt16();
            Log.Detail("loading "+count + " window positions");
            for(int i=0; i<count; i++)
            {
               int id = reader.ReadInt32();
               int x = reader.ReadInt16();
               int y = reader.ReadInt16();
               Log.Trace("read window position for window id "+id+": "+x+"/"+y);
               SetWindowPosition(id, x, y);
            }
         }

         private void WriteHallOfFameFilter(BinaryWriter writer)
         {
            Int16 cnt = (Int16)hallOfFameFilter.Count;
            Log.Detail("writing "+cnt+" hall of fame filter to config");
            writer.Write(cnt);
            foreach (KeyValuePair<GameScenes, HallOfFameBrowser.HallOfFameFilter> entry in hallOfFameFilter)
            {
               writer.Write(entry.Value);
            }
         }

         private void WriteHallOfFameSorter(BinaryWriter writer)
         {
            Int16 cnt = (Int16)hallOfFameSorter.Count;
            Log.Detail("writing " + cnt + " hall of fame sorter to config");
            writer.Write(cnt);
            foreach (KeyValuePair<GameScenes, HallOfFameBrowser.HallOfFameSorter> entry in hallOfFameSorter)
            {
               writer.Write(entry.Value);
            }
         }


         private void ReadHallOfFameFilter(BinaryReader reader)
         {
            Log.Detail("reading hall of fame filter from config");
            Int16 cnt = (Int16)reader.ReadInt16();
            for (int i = 0; i < cnt; i++)
            {
               HallOfFameBrowser.HallOfFameFilter filter = reader.ReadFilter();
               if (filter != null)
               {
                  hallOfFameFilter[filter.GetScene()] = filter;
                  Log.Detail("hall of fame display filter loaded: "+filter);
               }
            }
         }

         private void ReadHallOfFameSorter(BinaryReader reader)
         {
            Log.Detail("reading hall of fame sorter from config");
            Int16 cnt = (Int16)reader.ReadInt16();
            for (int i = 0; i < cnt; i++)
            {
               HallOfFameBrowser.HallOfFameSorter sorter = reader.ReadSorter();
               if (sorter != null)
               {
                  hallOfFameSorter[sorter.GetScene()] = sorter;
                  Log.Detail("hall of fame display sorter loaded: " + sorter);
               }
            }
         }

         public void Save()
         {
            String filename = CONFIG_BASE_FOLDER + FILE_NAME;
            Log.Info("storing configuration in "+filename);
            Log.Detail("DETAIL");
            Log.Trace("TRACE");
            try
            {
               using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
               {
                  writer.Write((Int16)logLevel);
                  writer.Write(customRibbonAtSpaceCenterEnabled);
                  // File Version
                  writer.Write((Int16)FILE_MARKER);  // marker
                  writer.Write(FILE_VERSION);
                  //
                  WriteWindowPositions(writer);
                  //
                  writer.Write(autoExpandEnabled);
                  //
                  writer.Write(hotkeyEnabled);
                  //
                  writer.Write(removalOfRibbonsEnabled);
                  //
                  // filter
                  WriteHallOfFameFilter(writer);
                  //
                  // sorter
                  WriteHallOfFameSorter(writer);
                  //
                  // window titles
                  writer.Write(hallOfFameWindowTitle);
                  writer.Write(decorationBoardWindowTitle);
                  writer.Write(missionSummaryWindowTitle);
                  writer.Write("reserved");
                  writer.Write("reserved");
                  writer.Write("reserved");
                  //
                  // use FAR
                  writer.Write(UseFARCalculations);
                  //
                  // Popup window when recovering vessel
                  writer.Write(missonSummaryPopup);
               }
            }
            catch
            {
               Log.Error("saving configuration failed");
            }
         }

         public void Load()
         {
            String filename = CONFIG_BASE_FOLDER+FILE_NAME;
            try
            {
               if (File.Exists(filename))
               {
                  Log.Info("loading configuration from " + filename);
                  using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
                  {
                     logLevel = (Log.LEVEL) reader.ReadInt16();
                     Log.Info("log level loaded: "+logLevel);
                     customRibbonAtSpaceCenterEnabled = reader.ReadBoolean();
                     // File Version
                     Int16 marker = reader.ReadInt16();
                     if (marker != FILE_MARKER)
                     {
                        Log.Error("invalid file structure");
                        throw new IOException("invalid file structure");
                     }
                     Int16 version = reader.ReadInt16();
                     if (version != FILE_VERSION)
                     {
                        Log.Error("incompatible file version");
                        throw new IOException("incompatible file version");
                     }
                     //
                     ReadWindowPositions(reader);
                     //
                     autoExpandEnabled = reader.ReadBoolean();
                     //
                     hotkeyEnabled = reader.ReadBoolean();
                     //
                     removalOfRibbonsEnabled = reader.ReadBoolean();
                     //
                     ReadHallOfFameFilter(reader);
                     //
                     ReadHallOfFameSorter(reader);
                     //
                     hallOfFameWindowTitle = reader.ReadString();
                     decorationBoardWindowTitle = reader.ReadString();
                     missionSummaryWindowTitle = reader.ReadString();
                     reader.ReadString(); // reserved
                     reader.ReadString(); // reserved
                     reader.ReadString(); // reserved
                     //
                     // use FAR
                     UseFARCalculations = reader.ReadBoolean();
                     //
                     // Popup window when recovering vessel
                     missonSummaryPopup = reader.ReadBoolean();
                  }
               }
               else
               {
                  Log.Info("no config file: default configuration");
               }
            }
            catch
            {
               Log.Warning("loading configuration failed or incompatible file");
            }
         }
      }
   }
}
