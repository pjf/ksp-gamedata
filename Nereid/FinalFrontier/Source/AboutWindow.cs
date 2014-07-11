using System;
using UnityEngine;
using Toolbar;

namespace Nereid
{
   namespace FinalFrontier
   {
      class AboutWindow : PositionableWindow
      {
         public AboutWindow()
            : base(Constants.WINDOW_ID_ABOUT, "About")
         {

         }

         protected override void OnWindow(int id)
         {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(FFStyles.STYLE_RIBBON_DESCRIPTION);
            GUILayout.Label("Final Frontier - written by Nereid (A.Kolster)",FFStyles.STYLE_LABEL);
            GUILayout.Label("  some ribbons and graphics are inspired and/or created by Unistrut", FFStyles.STYLE_LABEL);
            GUILayout.Label("  the toolbar was created by blizzy78", FFStyles.STYLE_LABEL);
            GUILayout.Label("  some custom ribbons are created/provided by nothke and SmarterThanMe", FFStyles.STYLE_LABEL);
            GUILayout.Label("  special thanks to Unistrut for giving permissions to use his ribbon graphics", FFStyles.STYLE_LABEL);
            GUILayout.Label("");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Utils.ConvertToEarthTime(Planetarium.GetUniversalTime()), FFStyles.STYLE_LABEL);
            GUILayout.EndHorizontal();
            if (FlightGlobals.ActiveVessel != null)
            {
               /*Vessel vessel = FlightGlobals.ActiveVessel;
               GUILayout.Label("atm den: " + vessel.atmDensity.ToString("0.000000000"));
               GUILayout.Label("max atm: " +vessel.mainBody.maxAtmosphereAltitude);
               GUILayout.Label("alt:     " + vessel.altitude);
               GUILayout.Label("hspd: " + FlightGlobals.ActiveVessel.MachNumberHorizontal().ToString("0.000"));
               GUILayout.Label("status: " + FlightGlobals.ActiveVessel.situation);*/
               /*double machFAR = FlightGlobals.ActiveVessel.MachNumber();
               double machAPX = FlightGlobals.ActiveVessel.ApproxMachNumber();
               double f = machAPX / machFAR;
               double a = FlightGlobals.ActiveVessel.altitude;
               //double c = 1.1 + (a/1500)*0.1 * (1 + Math.Pow(a / 10000,0.95));
               double c1 = (a / 16000);
               double c2 = (a / 39000);
               double c = 1.05 + (a / 15000) * (1 + a / 10000) + Math.Pow(c1, 4.15) + Math.Pow(c2, 5.58);
               GUILayout.Label("velocity srf:    " + FlightGlobals.ActiveVessel.srf_velocity.ToString("0.0000"));
               GUILayout.Label("speed:  " + FlightGlobals.ActiveVessel.RevealSpeed().ToString("0.00000"));
               GUILayout.Label("speed srf:  " + FlightGlobals.ActiveVessel.srfSpeed);
               GUILayout.Label("mach FAR: " + machFAR.ToString("0.000"));
               GUILayout.Label("mach APX: " + machAPX.ToString("0.000"));
               GUILayout.Label("mach F: " + f.ToString("0.00"));
               GUILayout.Label("c: " + c.ToString("0.00"));
               GUILayout.Label("corect: " + (machAPX/c).ToString("0.00"));
               GUILayout.Label("atm:      " + FlightGlobals.ActiveVessel.atmDensity.ToString("0.0000"));*/
            }
            GUILayout.EndVertical();
            if (GUILayout.Button("Close", FFStyles.STYLE_BUTTON)) SetVisible(false);
            GUILayout.EndHorizontal();
            DragWindow();
         }
      }


   }
}
