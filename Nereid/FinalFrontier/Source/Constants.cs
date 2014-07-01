using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nereid
{
   namespace FinalFrontier
   {
      static class Constants
      {
         public static readonly int WINDOW_ID_HALLOFFAMEBROWSER = 18142;
         public static readonly int WINDOW_ID_RIBBONBROWSER = 18143;
         public static readonly int WINDOW_ID_DISPLAY = 18144;
         public static readonly int WINDOW_ID_CONFIG = 18145;
         public static readonly int WINDOW_ID_ABOUT = 18146;
         public static readonly int WINDOW_ID_CODEBROWSER = 18147;
         public static readonly int WINDOW_ID_MISSION_SUMMARY = 18148;

         public static readonly int MISSIONCOUNT_FOR_BADASS = 2;

         public static readonly int MAX_RIBBONS_PER_AREA = 12;
         public static readonly int MAX_RIBBONS_PER_EXPANDED_AREA = 60;

         public static readonly double MIN_DISTANCE_FOR_MOVING_VEHICLE_ON_SURFACE = 0.3;
         public static readonly double MIN_HEIGT_FOR_MOVING_VEHICLE_ON_SURFACE = 0.1;

         public static readonly int SECONDS_PER_MINUTE = 60;
         public static readonly int SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE;
         public static readonly int HOURS_PER_EARTH_DAY = 24;
         public static readonly int HOURS_PER_KERBIN_DAY = 6;
         public static readonly int SECONDS_PER_EARTH_DAY = HOURS_PER_EARTH_DAY * SECONDS_PER_HOUR;
         public static readonly int SECONDS_PER_KERBIN_DAY = HOURS_PER_KERBIN_DAY * SECONDS_PER_HOUR;

      }
   }
}
