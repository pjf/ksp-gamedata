
using System;
using UnityEngine;


namespace Nereid
{
   namespace FinalFrontier
   {
      public class GameUtils
      {
         public static void SetPermadeathEnabled(bool enabled)
         {
            if (HighLogic.CurrentGame == null) return;
            if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn == enabled)
            {
               HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn = !enabled;
               HighLogic.CurrentGame.Updated();
               Log.Detail("permadeath " + (enabled ? "enabled" : "disabled"));
            }
         }

         public static bool IsPermadeathEnabled()
         {
            if (HighLogic.CurrentGame == null) return false;
            return !HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn;
         }

         public static void SetKerbinTimeEnabled(bool enabled)
         {
            
            if (GameSettings.KERBIN_TIME != enabled)
            {
               GameSettings.KERBIN_TIME = enabled;
               Log.Detail("kerbin time " + (enabled ? "enabled" : "disabled"));
            }
         }

         public static bool IsKerbinTimeEnabled()
         {
            return GameSettings.KERBIN_TIME;
         }

         public static CelestialBody GetCelestialBody(String name)
         {
            CelestialBody body = PSystemManager.Instance.localBodies.Find(x => x.name.Equals(name));
            if (body == null) Log.Warning("celestial body '"+name+"' not found");
            return body;
         }

         public static double ConvertSpeedToMachNumber(CelestialBody body, double atmDensity, double altitude, Vector3 velocity)
         {
            return External.ConvertSpeedToMachNumber(body, atmDensity, altitude, velocity);
         }

         public static double ApproximateMachNumber(CelestialBody body, double atmDensity, double altitude, Vector3 velocity)
         {
            // a technical constant for speed of sound appromixation 
            // experimental resolved; feel free to make better suggestions
            double c1 = (altitude / 16000);
            double c2 = (altitude / 39000);
            double c = 1.05 + (altitude / 15000) * (1 + altitude / 10000) + Math.Pow(c1, 4.15) + Math.Pow(c2, 5.58);

            return velocity.magnitude/(300 * Math.Sqrt(atmDensity))/c;
         }

      }
   }
}
