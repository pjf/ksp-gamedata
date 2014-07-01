
using System;
using UnityEngine;


namespace Nereid
{
   namespace FinalFrontier
   {
      public static class External
      {
         public static volatile bool FAR_ENABLED = false;

         public static double ConvertSpeedToMachNumber(CelestialBody body, double atmDensity, double altitude, Vector3 velocity)
         {
            return GameUtils.ApproximateMachNumber(body, atmDensity, altitude, velocity);
         }
      }
   }
}
