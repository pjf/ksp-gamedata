using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;


namespace Nereid
{
   namespace FinalFrontier
   {

      public class HallOfFameEntry : IComparable<HallOfFameEntry>
      {
         private ProtoCrewMember kerbal;
         // currently awarded ribbons
         private List<Ribbon> ribbons = new List<Ribbon>();
         // previously awarded ribbons , superseded by other ribbons
         private HashSet<Ribbon> supersededRibbons = new HashSet<Ribbon>();
         // the logbook entries of this kerbal
         private List<LogbookEntry> logbook = new List<LogbookEntry>();
         // the logbook as text
         private StringBuilder logtext = new StringBuilder("");

         public int MissionsFlown { get; set; }
         public int Dockings { get; set; }
         public double TimeOfLastLaunch { get; set; }
         public double TimeOfLastEva { get; set; }
         public double TotalMissionTime { get; set; }
         public double TotalEvaTime { get; set; }
         public double LastEvaDuration { get; set; }
         // Kerbal currently on Eva
         public bool IsOnEva { get; set; }
         // Action for specific ongoing EVA
         public EvaAction evaAction { get; set; }
         // special EVA times
         public double TotalTimeInEvaWithoutAtmosphere { get; set; }
         public double TotalTimeInEvaWithoutOxygen { get; set; }
         public double TotalTimeInEvaWithOxygen { get; set; }

         public HallOfFameEntry(ProtoCrewMember kerbal)
         {
            this.kerbal = kerbal;
            this.IsOnEva = false;
            this.TimeOfLastLaunch = -1;
            this.TimeOfLastEva = -1;
            this.TotalEvaTime = 0;
            this.LastEvaDuration = 0;
            this.evaAction = null;
            //
            TotalTimeInEvaWithoutAtmosphere = 0;
            TotalTimeInEvaWithoutOxygen = 0;
            TotalTimeInEvaWithOxygen = 0;
         }

         int System.IComparable<HallOfFameEntry>.CompareTo(HallOfFameEntry right)
         {
            return kerbal.name.CompareTo(right.GetName());
         }

         public override bool Equals(System.Object right)
         {
            if (right == null) return false;
            HallOfFameEntry cmp = right as HallOfFameEntry;
            if (cmp == null) return false;
            return kerbal.name.Equals(kerbal.name);
         }

         public override int GetHashCode()
         {
            return kerbal.name.GetHashCode();
         }

         public bool HasRibbon(Ribbon ribbon)
         {
            return ribbons.Contains(ribbon);
         }

         /**
          * Add a reference to the corresponding logbook entry. This creates a personal logbook for this kerbal.
          * */
         public void AddLogRef(LogbookEntry lbentry)
         {
            logbook.Add(lbentry);
            if(logtext.Length>0) logtext.Append("\n");
            logtext.Append(lbentry.ToString());
         }

         /**
           * Returns the personal logbook for this kerbal.
           * */
         public List<LogbookEntry> GetLogRefs()
         {
            return logbook; //.AsReadOnly();
         }

         /**
          * Returns logbook in textform
          * */
         public String GetLogText()
         {
            return logtext.ToString();
         }


         /**
          * Returns the Kerbal for this entry.
          */
         public ProtoCrewMember GetKerbal()
         {
            return kerbal;
         }

         /**
          * Set the Kerbal for this entry.
          */
         public void SetKerbal(ProtoCrewMember kerbal)
         {
            if(kerbal==null)
            {
               Log.Error("can't change hall of fame entry to no kerbal (from=" + this.kerbal.name+")");
            }
            if(this.kerbal.name.Equals(kerbal.name))
            {
               this.kerbal = kerbal;
            }
            else
            {
               Log.Error("can't change hall of fame entry to different kerbal (from="+this.kerbal.name+",to="+kerbal.name+")");
            }
         }

         /**
          * Returns the name of the Kerbal for this entry.
          */
         public String GetName()
         {
            return kerbal.name;
         }

         /**
          * Returns all ribbons for this entry.
          */
         public List<Ribbon> GetRibbons()
         {
            return ribbons;
         }

         /**
          * Remova a ribbon
          */
         public bool Revocation(Ribbon ribbon)
         {
            bool result = ribbons.Remove(ribbon);
            Ribbon superseded = ribbon.SupersedeRibbon();
            if(supersededRibbons.Contains(superseded))
            {
               supersededRibbons.Remove(superseded);
               ribbons.Add(superseded);
            }
            return result;
         }


         /**
          * Tries to award a ribbon. Returns true if successful, false otherwise.
          */
         public bool Award(Ribbon ribbon)
         {
            Log.Detail("awarding ribbon " + ribbon.GetCode()+" to "+kerbal.name);
            // not currently awarded and not superseded by another ribbon
            if (!ribbons.Contains(ribbon) && !supersededRibbons.Contains(ribbon))
            {
               Log.Detail("new ribbon for kerbal " + kerbal.name + ": " + ribbon.GetName());
               ribbons.Add(ribbon);
               // if this ribbon supersedes another, then remove the other ribbon
               Ribbon supersede = ribbon.SupersedeRibbon();
               while (supersede != null)
               {
                  supersededRibbons.Add(supersede);
                  if (ribbons.Remove(supersede))
                  {
                     Log.Detail("ribbon supersedes " + supersede.GetName());
                  }
                  supersede = supersede.SupersedeRibbon();
               }
               // sort ribbons by prestige
               ribbons.Sort();
               return true;
            }
            else if(Log.GetLevel()>=Log.LEVEL.DETAIL)
            {
               Log.Detail("ribbon not awarded because...");
               if (ribbons.Contains(ribbon)) Log.Detail("ribbon was already awarded");
               if (supersededRibbons.Contains(ribbon))
               {
                  Log.Detail("ribbon superseeded by another ribbon");
                  foreach(Ribbon s in supersededRibbons)
                  {
                     Log.Detail("already superseeded: " + ribbon.GetCode());                     
                  }
               }
            }
            return false;
         }
      }
   }
}