using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Nereid
{
   namespace FinalFrontier
   {
      public abstract class Action : Activity
      {
         public Action(String code, String name)
            : base (code, name)
         {
         }

         public abstract bool DoAction(double timeOfAction, HallOfFameEntry entry);

         public static EvaAction GetEvaAction(ProtoCrewMember kerbal, Vessel fromVessel)
         {
            if (fromVessel != null)
            {
               bool atmosphere = fromVessel.IsInAtmosphere();
               bool oxygen = fromVessel.IsInAtmosphereWithOxygen();
               Log.Detail("creating EVA action for kerbal "+kerbal.name+" in atmosphere:"+atmosphere+", oxygen:"+oxygen);
               if (atmosphere && oxygen)
               {
                  return ActionPool.ACTION_EVA_OXYGEN;                     
               }
               else if (atmosphere && ! oxygen)
               {
                  return ActionPool.ACTION_EVA_INATM;                     
               }
               else if (!atmosphere)
               {
                  return ActionPool.ACTION_EVA_NOATM;                     
               }
               else
               {
                  Log.Warning("unexpected EVA situation");
                  return ActionPool.ACTION_EVA_NOATM;
               }
            }
            else
            {
               Log.Warning("no vessel for kerbal "+kerbal.name+" on EVA");
               return ActionPool.ACTION_EVA_NOATM;
            }
         }
      }

      public class BoardingAction : Action
      {
         public BoardingAction() : base("B+", "Kerbal Boarding Vessel") { }

         public override bool DoAction(double timeOfAction, HallOfFameEntry entry)
         {
            ProtoCrewMember kerbal = entry.GetKerbal();
            try
            {
               if (entry.TimeOfLastEva >= 0)
               {
                  Log.Detail("end of EVA for kerbal " + kerbal.name + ": " + entry.TotalEvaTime + " eva time");
                  if (entry.TotalEvaTime >= 0)
                  {
                     entry.LastEvaDuration = timeOfAction - entry.TimeOfLastEva;
                     entry.TotalEvaTime += entry.LastEvaDuration;
                     // specific EVA actions
                     if(entry.evaAction!=null)
                     {
                        entry.evaAction.OnBoardingVessel(timeOfAction, entry);
                     }
                     else
                     {
                        Log.Detail("boarding vessel ingored: no EVA action defined for " + kerbal.name + " at " + timeOfAction);
                     }
                  }
                  return true;
               }
               else
               {
                  Log.Warning("return from EVA ignored for "+kerbal.name+" at "+timeOfAction);
                  return false;
               }
            }
            finally
            {
               Log.Detail("EVA for kerbal " + kerbal.name +" has ended");
               entry.IsOnEva = false;
               entry.evaAction = null;
            }
         }

         public override string CreateLogBookEntry(LogbookEntry entry)
         {
            return entry.Name + " returns from EVA";
         }  
      }

      public class DockingAction : Action
      {
         public DockingAction() : base("D+", "Vessel docked") { }

         public override bool DoAction(double timeOfAction, HallOfFameEntry entry)
         {
            entry.Dockings++;
            return true;
         }

         public override string CreateLogBookEntry(LogbookEntry entry)
         {
            return entry.Name + " has docked on another spacecraft";
         }   
      }

      public class LaunchAction : Action
      {
         public LaunchAction() : base("L+", "Launching Vessel") { }

         public override bool DoAction(double timeOfAction, HallOfFameEntry entry)
         {
            entry.TimeOfLastLaunch = timeOfAction;
            return true;
         }

         public override string CreateLogBookEntry(LogbookEntry entry)
         {
            return entry.Name + " launched a mission";
         }
      }

      public class RecoverAction : Action
      {
         public RecoverAction() : base("M+", "Vessel recovered") { }

         public override bool DoAction(double timeOfAction, HallOfFameEntry entry)
         {
            ProtoCrewMember kerbal = entry.GetKerbal();
            entry.MissionsFlown++;
            Log.Detail("mission recover recorded for kerbal " + kerbal.name + ": " + entry.MissionsFlown + " missions flown");
            try
            {
               if (entry.TimeOfLastLaunch >= 0)
               {
                  entry.TotalMissionTime += (timeOfAction - entry.TimeOfLastLaunch);
                  return true;
               }
               return false;
            }
            finally
            {
               Log.Detail("kerbal " + kerbal.name + " no longer on mission");
               entry.TimeOfLastLaunch = -1;
               entry.IsOnEva = false;
            }
         }

         public override string CreateLogBookEntry(LogbookEntry entry)
         {
            return entry.Name + " has returned from a mission";
         }
      }

      public abstract class EvaAction : Action
      {
         public EvaAction(String code, String name) : base(code, name) { }

         public abstract void OnBoardingVessel(double timeOfAction, HallOfFameEntry entry);

      }

      public class EvaNoAtmosphereAction : EvaAction
      {
         public EvaNoAtmosphereAction() : base("E+", "Kerbal on Eva in zero atmosphere") { }

         public override bool DoAction(double timeOfAction, HallOfFameEntry entry)
         {
            entry.TimeOfLastEva = timeOfAction;
            return true;
         }

         public override string CreateLogBookEntry(LogbookEntry entry)
         {
            return entry.Name + " begins EVA in zero atmosphere";
         }

         public override void OnBoardingVessel(double timeOfAction, HallOfFameEntry entry)
         {
            if (entry.TimeOfLastEva <= 0) return;
            entry.TotalTimeInEvaWithoutOxygen += timeOfAction - entry.TimeOfLastEva;
         }
      }




      public class EvaWithOxygen : EvaAction
      {
         public EvaWithOxygen() : base("EX+", "Kerbal on Eva in atmosphere with oxygen") { }

         public override bool DoAction(double timeOfAction, HallOfFameEntry entry)
         {
            entry.TimeOfLastEva = timeOfAction;
            return true;
         }

         public override string CreateLogBookEntry(LogbookEntry entry)
         {
            return entry.Name + " begins EVA in atmosphere";
         }

         public override void OnBoardingVessel(double timeOfAction, HallOfFameEntry entry)
         {
            if (entry.TimeOfLastEva <= 0) return;
            entry.TotalTimeInEvaWithOxygen += timeOfAction - entry.TimeOfLastEva;
         }
      }

      public class EvaInAtmosphereAction : EvaAction
      {
         public EvaInAtmosphereAction() : base("EA+", "Kerbal on Eva in toxic atmosphere without oxygen") { }

         public override bool DoAction(double timeOfAction, HallOfFameEntry entry)
         {
            entry.TimeOfLastEva = timeOfAction;
            return true;
         }

         public override string CreateLogBookEntry(LogbookEntry entry)
         {
            return entry.Name + " begins EVA in toxic atmosphere";
         }

         public override void OnBoardingVessel(double timeOfAction, HallOfFameEntry entry)
         {
            if (entry.TimeOfLastEva <= 0) return;
            entry.TotalTimeInEvaWithoutOxygen += timeOfAction - entry.TimeOfLastEva;
         }
      }
   }
}
