using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nereid
{
   namespace FinalFrontier
   {
      public class Pair<T, U>
      {
         public T first { get; set; }
         public U second { get; set; }

         public Pair()
         {
         }

         public Pair(T first, U second)
         {
            this.first = first;
            this.second = second;
         }

         public override String ToString()
         {
            return "(" + first + "," + second + ")";
         }

         public override bool Equals(System.Object right)
         {
            if (right == null) return false;
            Pair<T,U> cmp = right as Pair<T,U>;
            if (cmp == null) return false;
            return first.Equals(cmp.first) && second.Equals(cmp.second);
         }


         public override int GetHashCode()
         {
            return first.GetHashCode() + 7 * second.GetHashCode();
         }
      };

      static class Extensions
      {

         public static String Envelope(this String s)
         {
            if (s.Trim().Length == 0) return " ";
            return " " + s.Trim() + " ";
         }

         public static bool isInStableOrbit(this Vessel vessel)
         {
            Orbit orbit = vessel.orbit;
            float atmosphereAltitude = Math.Max(vessel.mainBody.maxAtmosphereAltitude, 0f);
            return (orbit.ApA > 0) && (orbit.PeA > 0) && (orbit.PeA > atmosphereAltitude) && (orbit.ApR<vessel.mainBody.sphereOfInfluence);
         }

         public static bool InCrewOfActiveFlight(this ProtoCrewMember kerbal)
         {
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null) return false;
            foreach(ProtoCrewMember member in vessel.GetVesselCrew())
            {
               if (kerbal.Equals(member)) return true;
            }
            return false;
         }

         public static bool IsSolidFuel ( this PartResource resource)
         {
            return resource.resourceName.Equals("SolidFuel");
         }

         public static bool IsInOrbit(this Vessel vessel)
         {
            Orbit orbit = vessel.orbit;
            return (orbit.ApA > 0) && (orbit.PeA > 0);
         }


         public static bool IsLanded(this Vessel vessel)
         {
            if (vessel.situation == Vessel.Situations.LANDED) return true;
            if (vessel.situation == Vessel.Situations.PRELAUNCH) return true;
            return false;
         }

         public static bool IsInAtmosphere(this Vessel vessel)
         {
            if(vessel.mainBody==null) return false;
            if(vessel.altitude<=vessel.mainBody.maxAtmosphereAltitude) return true;
            return false;
         }

         public static bool IsInAtmosphereWithOxygen(this Vessel vessel)
         {
            if (vessel.mainBody == null) return false;
            if (vessel.altitude > vessel.mainBody.maxAtmosphereAltitude) return false;
            return vessel.mainBody.atmosphereContainsOxygen;
         }

         public static bool IsFlag(this Vessel vessel)
         {
            if (vessel == null) return false;
            if (vessel.vesselType != VesselType.Flag) return false;
            return true;
         }

         public static ProtoCrewMember GetCrewMember(this Vessel vessel, String name)
         {
            foreach (ProtoCrewMember member in vessel.GetVesselCrew())
            {
               if (member.name.Equals(name))
               {
                  return member;
               }
            }
            return null;
         }

         public static double MachNumber(this Vessel vessel)
         {
            if (vessel == null) return 0.0;
            if (vessel.mainBody == null) return 0.0;
            return GameUtils.ConvertSpeedToMachNumber(vessel.mainBody, vessel.atmDensity, vessel.altitude, vessel.srf_velocity);
         }

         public static double MachNumberHorizontal(this Vessel vessel)
         {
            Vector3d velocity = new Vector3d(vessel.horizontalSrfSpeed,0,0);
            return GameUtils.ConvertSpeedToMachNumber(vessel.mainBody, vessel.atmDensity, vessel.altitude, velocity);
         }

         public static bool IsKerbin(this CelestialBody body)
         {
            return body.GetName().Equals("Kerbin");
         }

         public static String ToString(this Vector3d v, String format)
         {
            return "("+v[0].ToString(format)+","+v[1].ToString(format)+","+v[2].ToString(format)+")";
         }

         public static Vessel GetVessel(this ProtoCrewMember kerbal)
         {
            Log.Detail("get vessel for kerbal " + kerbal.name);
            foreach (Vessel vessel in FlightGlobals.fetch.vessels)
            {
               foreach(ProtoCrewMember member in vessel.GetVesselCrew())
               {
                  if (kerbal.name.Equals(member.name)) return vessel;
               }
            }
            return null;
         }


         public static int BasePrestige(this CelestialBody body)
         {
            switch (body.GetName())
            {
               case "Kerbin": return 100;
               case "Mun": return 300;
               case "Minmus": return 500;
               case "Gilly": return 900;
               case "Ike": return 800;
               case "Duna": return 1000;
               case "Eve": return 2000;
               case "Moho": return 3000;
               case "Dres": return 3500;
               case "Jool": return 5000;
               case "Vall": return 5100;
               case "Tylo": return 5200;
               case "Bop": return 5300;
               case "Pol": return 5500;
               case "Laythe": return 6000;
               case "Eeloo": return 20000;
               case "Sun": return 50000;
               default: Log.Warning("no base prestige for celestial body " + body.GetName());
                  return 99000;
            }
         }

         public static void Write(this BinaryWriter writer, HallOfFameBrowser.HallOfFameFilter filter)
         {
            writer.Write((Int64)filter.GetScene());
            writer.Write(filter.showDead);
            writer.Write(filter.showAssigned);
            writer.Write(filter.showAvailable);
            writer.Write(filter.showUndecorated);
            writer.Write(filter.showFlightOnly);
         }

         public static HallOfFameBrowser.HallOfFameFilter ReadFilter(this BinaryReader reader)
         {
            long sceneCode = reader.ReadInt64();
            GameScenes scene = (GameScenes)sceneCode;
            HallOfFameBrowser.HallOfFameFilter filter = new HallOfFameBrowser.HallOfFameFilter(scene);
            filter.showDead = reader.ReadBoolean();
            filter.showAssigned = reader.ReadBoolean();
            filter.showAvailable = reader.ReadBoolean();
            filter.showUndecorated = reader.ReadBoolean();
            filter.showFlightOnly = reader.ReadBoolean();
            return filter;
         }

         public static void Write(this BinaryWriter writer, HallOfFameBrowser.HallOfFameSorter sorter)
         {
            writer.Write((Int64)sorter.GetScene());
            writer.Write((Int32)sorter.GetDirection());
            writer.Write((Int32)sorter.GetSortPredicate());
         }

         public static HallOfFameBrowser.HallOfFameSorter ReadSorter(this BinaryReader reader)
         {
            long sceneCode = reader.ReadInt64();
            HallOfFameBrowser.HallOfFameSorter.DIRECTION direction = (HallOfFameBrowser.HallOfFameSorter.DIRECTION)reader.ReadInt32();
            HallOfFameBrowser.HallOfFameSorter.SORT_BY predicate = (HallOfFameBrowser.HallOfFameSorter.SORT_BY)reader.ReadInt32();
            GameScenes scene = (GameScenes)sceneCode;
            HallOfFameBrowser.HallOfFameSorter sorter = new HallOfFameBrowser.HallOfFameSorter(scene, direction, predicate);
            return sorter;
         }


         /*public static String GetPath(this Game game)
         {
            String title = game.Title;
            int index = title.LastIndexOf(" (");
            String name = title.Remove(index);

            return "saves/" + name;
         }*/
      }
   }
}