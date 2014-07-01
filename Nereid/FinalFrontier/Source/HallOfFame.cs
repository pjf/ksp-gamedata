using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace Nereid
{
   namespace FinalFrontier
   {
      public class HallOfFame : IEnumerable<HallOfFameEntry>
      {
         public static readonly HallOfFame instance = new HallOfFame();

         class DefaultSorter : Sorter<HallOfFameEntry>
         {
            public void Sort(List<HallOfFameEntry> list)
            {
                list.Sort(delegate(HallOfFameEntry left, HallOfFameEntry right)
                {
                   return left.GetName().CompareTo(right.GetName());
                });
            }
         }

         private static DefaultSorter DEFAULT_SORTER = new DefaultSorter();

         private Sorter<HallOfFameEntry> sorter = DEFAULT_SORTER; 

         // all accomplished achiements so far 
         private readonly HashSet<Achievement> accomplished;
         // current achievements given to crew in a single transaction
         private readonly HashSet<Achievement> currentTransaction;
         // all events
         private readonly List<LogbookEntry> logbook;

         private double currentTransactionTime=0;

         // the hall of fame of all kerbals
         private readonly List<HallOfFameEntry> entries;
         // map of known kerbals by name
         private readonly Dictionary<String, ProtoCrewMember> mapOfKerbals;

         private HallOfFame()
         {
            Log.Info("creating hall of fame");
            this.entries = new List<HallOfFameEntry>();
            this.currentTransaction = new HashSet<Achievement>();
            this.accomplished = new HashSet<Achievement>();
            this.logbook = new List<LogbookEntry>();
            this.mapOfKerbals = new Dictionary<String, ProtoCrewMember>();
         }

         /**
          * Just for debug: dump hall of fame to log.
          */
         public void DumpToLog()
         {
            Log.Info("Hall of Fame: ");
            foreach (HallOfFameEntry entry in entries)
            {
               Log.Info(entry.GetName() + ": " + entry.MissionsFlown + " missions");
            }
         }

         private HallOfFameEntry CreateEntry(ProtoCrewMember kerbal)
         {
            HallOfFameEntry entry = new HallOfFameEntry(kerbal);
            entries.Add(entry);
            if (!mapOfKerbals.ContainsKey(kerbal.name))
            {
               Log.Detail("mapping new kerbal " + kerbal.name);
               mapOfKerbals.Add(kerbal.name, kerbal);
            }
            return entry;
         }


         public HallOfFameEntry GetEntry(ProtoCrewMember kerbal)
         {
            // search linear
            // perhaps someday I will improve this
            foreach (HallOfFameEntry entry in entries)
            {
               if (entry.GetName().Equals(kerbal.name))
               {
                  // kerbal instances change during runtime
                  if (!entry.GetKerbal().Equals(kerbal))
                  {
                     Log.Detail("different kerbals with same name for " + kerbal.name);
                     // set the new kerbal
                     entry.SetKerbal(kerbal);
                  }
                  return entry;
               }
            }
            // no entry found, create a new one
            Log.Trace("no Hall of Fame entry found for kerbal " + kerbal.name);
            HallOfFameEntry newentry = CreateEntry(kerbal);
            // sort entries again
            Sort();
            return newentry;
         }

         private LogbookEntry TakeLog(double time, String code, String name, String text = "")
         {
            if (time == 0.0) time = Planetarium.GetUniversalTime();
            LogbookEntry lbentry = new LogbookEntry(time, code, name, text);
            logbook.Add(lbentry);
            return lbentry;
         }


         private void TakeLog(double time, String code, HallOfFameEntry entry)
         {
            LogbookEntry lbentry = TakeLog(time, code, entry.GetName());
            entry.AddLogRef(lbentry);
         }

         private void TakeLog(String code, HallOfFameEntry entry)
         {
            double time = Planetarium.GetUniversalTime();
            TakeLog(time, code, entry);
         }

         public void RecordMissionFinished(ProtoCrewMember kerbal)
         {
            HallOfFameEntry entry = GetEntry(kerbal);
            Log.Detail("mission end recorded for kerbal " + kerbal.name + ": " + entry.GetName() + " at " + Utils.ConvertToKerbinTime(entry.TimeOfLastLaunch));
            double now = Planetarium.GetUniversalTime();
            Action action = ActionPool.ACTION_RECOVER;
            if (ActionPool.ACTION_RECOVER.DoAction(now, entry))
            {
               TakeLog(now, action.GetCode(), entry);
            }
         }

         public void RecordLaunch(ProtoCrewMember kerbal)
         {
            HallOfFameEntry entry = GetEntry(kerbal);
            Log.Detail("launch recorded for kerbal " + kerbal.name + ": " + entry.GetName() + " at " + Utils.ConvertToKerbinTime(entry.TimeOfLastLaunch));
            double now = Planetarium.GetUniversalTime();
            Action action = ActionPool.ACTION_LAUNCH;
            if (action.DoAction(now, entry))
            {
               TakeLog(now, action.GetCode(), entry);
            }
         }

         public void RecordEva(ProtoCrewMember kerbal, Vessel fromVessel)
         {
            HallOfFameEntry entry = GetEntry(kerbal);
            Log.Detail("EVA recorded for kerbal " + kerbal.name + ": " + entry.GetName() + " at " + Utils.ConvertToKerbinTime(entry.TimeOfLastEva));
            double now = Planetarium.GetUniversalTime();
            EvaAction action = Action.GetEvaAction(kerbal, fromVessel);
            if (action.DoAction(now, entry))
            {
               entry.evaAction = action;
               TakeLog(now, action.GetCode(), entry);
            }
         }

         public void RecordBoarding(ProtoCrewMember kerbal)
         {
            HallOfFameEntry entry = GetEntry(kerbal);
            double now = Planetarium.GetUniversalTime();
            Log.Detail("vessel boarding recorded for kerbal " + kerbal.name + ": " + entry.GetName() + " at " + now + ", eva time was " + (now - entry.TimeOfLastEva));
            Action action = ActionPool.ACTION_BOARDING;
            if (action.DoAction(now, entry))
            {
               entry.evaAction = null;
               TakeLog(now, action.GetCode(), entry);
            }
         }

         public void RecordDocking(ProtoCrewMember kerbal)
         {
            HallOfFameEntry entry = GetEntry(kerbal);
            double now = Planetarium.GetUniversalTime();
            Log.Detail("docking recorded for kerbal " + kerbal.name + ": " + entry.GetName() + " at " + Utils.ConvertToKerbinTime(entry.TimeOfLastLaunch));
            Action action = ActionPool.ACTION_DOCKING;
            if (action.DoAction(now, entry))
            {
               TakeLog(now, action.GetCode(), entry);
            }
         }

         public void Record(ProtoCrewMember kerbal, Ribbon ribbon)
         {
            Log.Detail("Record ribbon "+ribbon.GetName());
            HallOfFameEntry entry = GetEntry(kerbal);
            Achievement achievement = ribbon.GetAchievement();
            double time = currentTransactionTime > 0 ? currentTransactionTime : Planetarium.GetUniversalTime();
            if (!achievement.HasToBeFirst() || !accomplished.Contains(achievement))
            {
               Log.Detail("ribbon " + ribbon.GetName() + " awarded to " + kerbal.name + " at " + Utils.ConvertToEarthTime(currentTransactionTime) + "(" + currentTransactionTime+")");
               if(entry.Award(ribbon))
               {
                  TakeLog(time, ribbon.GetCode(), entry);
               }
            }
            currentTransaction.Add(achievement);
         }

         public void RecordCustomRibbon(Ribbon ribbon)
         {
            CustomAchievement achievement = ribbon.GetAchievement() as CustomAchievement;
            if(achievement!=null)
            {
               int nr = achievement.GetNr();
               double now = HighLogic.CurrentGame.UniversalTime;
               Log.Detail("new or changed custom ribbon " + ribbon.GetName() + " recorded  at " + Utils.ConvertToKerbinTime(Planetarium.GetUniversalTime()));
               String code = DataChange.DATACHANGE_CUSTOMRIBBON.GetCode() + nr;
               TakeLog(now, code, achievement.GetName(), achievement.GetText());
            }
            else
            {
               Log.Error("invalid custom ribbon achievement");
            }
         }

         public void BeginArwardOfRibbons()
         {
            Log.Detail("begin award of ribbon transaction");
            // just in case the last award wasn't finished...
            currentTransaction.Clear();
            currentTransactionTime = Planetarium.GetUniversalTime();
         }

         public void BeginArwardOfRibbons(double time)
         {
            Log.Detail("begin award of ribbon transaction at time " + time);
            // just in case the last award wasn't finished...
            currentTransaction.Clear();
            currentTransactionTime = time;
         }

         public void EndArwardOfRibbons()
         {
            foreach (Achievement achievement in currentTransaction)
            {
               accomplished.Add(achievement);
            }
            currentTransaction.Clear();
            currentTransactionTime = 0.0;
            Log.Detail("end award of ribbon transaction");
         }


         /**
          * Remova a ribbon
          */
         public void Revocation(ProtoCrewMember kerbal, Ribbon ribbon, bool removeSuperseded = false)
         {
            Log.Detail("revocation of ribbon " + ribbon.GetName() + " for kerbal " + kerbal.name);
            HallOfFameEntry entry = GetEntry(kerbal);
            if(entry!=null)
            {
               bool successs = entry.Revocation(ribbon);
               if (!successs) return;
               //
               LogbookEntry logEntry;
               while ( (logEntry=logbook.Find(x => x.Name.Equals(kerbal.name) && x.Code.Equals(ribbon.GetCode())))!=null)
               {
                  logbook.Remove(logEntry);
               }
               // remove superseeded ribbons?
               if(removeSuperseded)
               {
                  Ribbon superseded = ribbon.SupersedeRibbon();
                  while(superseded !=null)
                  {
                     Revocation(kerbal, superseded, true);
                     superseded = superseded.SupersedeRibbon();
                  }
               }
            }
            else
            {
               Log.Warning("no entry for revocation from kerbal "+kerbal.name);
            }

         }

         public void Sort()
         {
            Log.Detail("sorting hall of fame");
            if(sorter!=null)
            {
               sorter.Sort(entries);
            }
            else
            {
               DEFAULT_SORTER.Sort(entries);
            }
         }

         public void SetSorter(Sorter<HallOfFameEntry> sorter)
         {
            this.sorter = sorter;
            Sort();
         }

         public void Refresh()
         {
            if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.CrewRoster == null)
            {
               Clear();
               return;
            }
            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster)
            {
               HallOfFameEntry entry = GetEntry(kerbal);
               {
                  entry.SetKerbal(kerbal);
               }
            }
            Sort();
            Log.Info("hall of fame refreshed");
         }

         public void Clear()
         {
            Log.Detail("emptying Final Frontier hall of fame");
            entries.Clear();
            logbook.Clear();
            mapOfKerbals.Clear();
            accomplished.Clear();
            currentTransaction.Clear();
            PrefetchKerbals(HighLogic.CurrentGame);
         }

         public bool Contains(ProtoCrewMember kerbal)
         {
            foreach (HallOfFameEntry entry in entries)
            {
               if (kerbal.name.Equals(entry.GetName())) return true;
            }
            return false;
         }


         public System.Collections.IEnumerator GetEnumerator()
         {
            return entries.GetEnumerator();
         }

         IEnumerator<HallOfFameEntry> IEnumerable<HallOfFameEntry>.GetEnumerator()
         {
            return entries.GetEnumerator();
         }

         public void WriteToFile(String filename)
         {
            try
            {
               Persistence.SaveLogbook(this.logbook, filename);
            }
            catch
            {
               Log.Error("writing hall of fame failed");
            }
         }

         private void PrefetchKerbals(Game game)
         {
            Log.Trace("adding known kerbals");
            if (game == null) return;
            foreach (ProtoCrewMember kerbal in game.CrewRoster)
            {
               Log.Trace("add known kerbal " + kerbal.name);
               if (!mapOfKerbals.ContainsKey(kerbal.name))
               {
                  mapOfKerbals.Add(kerbal.name, kerbal);
               }
               // just make sure a hall of fame entry existsts
               CreateEntry(kerbal);
            }
         }

         private void addLogbookEntry(LogbookEntry lbentry, HallOfFameEntry entry)
         {
            logbook.Add(lbentry);
            entry.AddLogRef(lbentry);
         }

         private void changeCustomRibbon(LogbookEntry lbentry)
         {
            String code = "X" + lbentry.Code.Substring(2);
            Log.Trace("changing custom ribbon for code "+code);

            Ribbon ribbon = RibbonPool.instance.GetRibbonForCode(code);
            if(ribbon==null)
            {
               Log.Error("invalid custom ribbon code: " + code);
               return;
            }
            //
            CustomAchievement achievement = ribbon.GetAchievement() as CustomAchievement;
            if(achievement==null)
            {
               Log.Error("invalid custom ribbon achievement");
               return;
            }
            achievement.SetName(lbentry.Name);
            achievement.SetText(lbentry.Text);
            Log.Trace("xustom ribbonchanged");
         }

         public void CreateFromLogbook(Game game, List<LogbookEntry> book)
         {
            Log.Detail("creating hall of fame from logbook");
            Clear();

            if(book.Count==0)
            {
               Log.Detail("no logbook entries");
               return;
            }

            Log.Detail("resolving logbook");

            LogbookEntry lastEntry = null;

            foreach (LogbookEntry lbentry in book)
            {
               Log.Trace("processing logbook entry "+lbentry.UniversalTime+": "+lbentry.Code+" "+lbentry.Name);
               try
               {
                  // this is a custom ribbon entry
                  if (lbentry.Code.StartsWith(DataChange.DATACHANGE_CUSTOMRIBBON.GetCode()))
                  {
                     changeCustomRibbon(lbentry);
                     //
                     Log.Detail("adding custom ribbon " + lbentry.Code + ": " + lbentry.Name);
                     logbook.Add(lbentry);
                     //
                     continue;
                  }
                  ProtoCrewMember kerbal = mapOfKerbals[lbentry.Name];
                  HallOfFameEntry entry = GetEntry(kerbal);
                  Action action = ActionPool.instance.GetActionForCode(lbentry.Code);
                  if(action!=null)
                  {
                     action.DoAction(lbentry.UniversalTime, entry);
                     addLogbookEntry(lbentry, entry);
                  }
                  else
                  {
                     Ribbon ribbon = RibbonPool.instance.GetRibbonForCode(lbentry.Code);
                     if (ribbon != null)
                     {
                        Achievement achievement = ribbon.GetAchievement();
                        bool sameTransaction =  InSameTransaction(lbentry,lastEntry);
                        if (!achievement.HasToBeFirst() || !accomplished.Contains(achievement) || sameTransaction)
                        {
                           // make sure old transactions have same timestamps in each entry
                           if (sameTransaction) lbentry.UniversalTime = lastEntry.UniversalTime;
                           //
                           entry.Award(ribbon);
                           addLogbookEntry(lbentry, entry);
                           // to prevent multiple first ribbon awards
                           accomplished.Add(ribbon.GetAchievement());
                        }
                     }
                     else
                     {
                        Log.Warning("no ribbon for code " + lbentry.Code + " found");
                     }

                  }
               }
               catch (KeyNotFoundException)
               {
                  Log.Error("kerbal " + lbentry.Name + " not found");
               }
               lastEntry = lbentry;

            } // end for
            Log.Detail("new hall of fame created");
         }

         private bool InSameTransaction(LogbookEntry a, LogbookEntry b)
         {
            // maximal diffence of timestamps in seconds
            double MAX_TIME_DIFF = 1.0; 
            if (a == null || b == null) return false;
            if (!a.Code.Equals(b.Code)) return false;
            if (a.UniversalTime > b.UniversalTime + MAX_TIME_DIFF || a.UniversalTime < b.UniversalTime - MAX_TIME_DIFF) return false;
            if (b.UniversalTime > a.UniversalTime + MAX_TIME_DIFF || b.UniversalTime < a.UniversalTime - MAX_TIME_DIFF) return false;
            return true;
         }


         public List<Ribbon> GetRibbonsOfLatestMission(ProtoCrewMember kerbal)
         {
            List<Ribbon> result = new List<Ribbon>();
            HashSet<Ribbon> ignored = new HashSet<Ribbon>();
            HallOfFameEntry entry = GetEntry(kerbal);
            List<LogbookEntry> log = new List<LogbookEntry>(entry.GetLogRefs());
            log.Reverse();
            bool start = false;
            String codeLaunch=ActionPool.ACTION_LAUNCH.GetCode();
            String codeRecover=ActionPool.ACTION_RECOVER.GetCode();
            foreach(LogbookEntry logentry in log)
            {
               String code = logentry.Code;
               if (code.Equals(codeRecover))
               {
                  start = true;
               }
               else if (code.Equals(codeLaunch))
               {
                  break;
               }
               else if(start)
               {
                  Ribbon ribbon = RibbonPool.instance.GetRibbonForCode(code);
                  if(ribbon!=null)
                  {
                     // add this ribbon if not already taken or superseded
                     if (!ignored.Contains(ribbon))
                     {
                        result.Add(ribbon);                       
                        // add the new ribbon and all superseded ribbons to the ignore set
                        while(ribbon!=null)
                        {
                           ignored.Add(ribbon);
                           ribbon = ribbon.SupersedeRibbon();
                        }
                     }
                  }
               }

            }
            return result;
         }
      }


   }
}