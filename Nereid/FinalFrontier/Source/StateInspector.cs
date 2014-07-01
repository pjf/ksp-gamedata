using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nereid
{
   namespace FinalFrontier
   {
      // class for inspecting simple states, such as change in vessel altitute, mach number  or change in Gee-Force
      public abstract class StateInspector<T>
      {
         private bool changed = false;
         //private Action method = null;

         public abstract void Inspect(T x);

         public void Reset()
         {
            this.changed = false;
            ResetState();
         }

         protected abstract void ResetState();

         public void Change()
         {
            this.changed = true;
         }

         public bool StateHasChanged()
         {
            return this.changed;
         }
      }

      public class MachNumberInspector : StateInspector<Vessel>
      {
         private int lastMachNumber = 0;

         public override void Inspect(Vessel vessel)
         {
            if (vessel == null) return;
            if(vessel.situation != Vessel.Situations.FLYING) return;

            double mach = vessel.MachNumber();
            int machWholeNumber = (int)Math.Truncate(mach);
            if (machWholeNumber > lastMachNumber)
            {
               Log.Info("mach number increasing to " + machWholeNumber);
               this.lastMachNumber = machWholeNumber;
               Change();
            }
            else if (machWholeNumber < lastMachNumber)
            {
               Log.Info("mach number decreasing to " + machWholeNumber);
               this.lastMachNumber = machWholeNumber;
               Change();
            }
         }

         protected override void ResetState()
         {
            this.lastMachNumber = 0;
         }
      }

      public class AltitudeInspector : StateInspector<Vessel>
      {
         private long lastAltitudeAsMultipleOf1k = 0;

         public override void Inspect(Vessel vessel)
         {
            if (vessel == null) return;
            if (vessel.situation != Vessel.Situations.FLYING) return;

            double altitide = vessel.altitude;
            int alt1000k = 1000*(int)Math.Truncate(altitide/1000);
            if (alt1000k > lastAltitudeAsMultipleOf1k)
            {
               Log.Info("alt increasing to " + alt1000k);
               this.lastAltitudeAsMultipleOf1k = alt1000k;
               Change();
            }
            else if (alt1000k < lastAltitudeAsMultipleOf1k)
            {
               Log.Info("alt decreasing to " + alt1000k);
               this.lastAltitudeAsMultipleOf1k = alt1000k;
               Change();
            }
         }

         protected override void ResetState()
         {

         }
      }

      public class GeeForceInspector : StateInspector<Vessel>
      {
         private double maxGeeForce = 1.0;

         public override void Inspect(Vessel vessel)
         {
            double gForce = vessel.geeForce;
            if (gForce > maxGeeForce)
            {
               this.maxGeeForce = gForce;
               StateHasChanged();
            }
         }

         protected override void ResetState()
         {
            this.maxGeeForce = 1.0;
         }
      }

      public class AtmosphereInspector : StateInspector<Vessel>
      {
         private bool inAtmosphere = true;

         public override void Inspect(Vessel vessel)
         {
            bool inAtmosphere = vessel.IsInAtmosphere();
            if (this.inAtmosphere != inAtmosphere)
            {
               Log.Detail("AtmosphereInspector:: atmosphere " + (inAtmosphere ? "entered" : "leaved"));
               this.inAtmosphere = inAtmosphere;
               StateHasChanged();
            }
         }

         protected override void ResetState()
         {
            Vessel vessel = FlightGlobals.ActiveVessel;
            if(vessel!=null)
            {
               inAtmosphere = vessel.IsInAtmosphere();
            }
            else
            {
               inAtmosphere = false;
            }
         }
      }

   } // end of FinalFrontier namespace */
}
