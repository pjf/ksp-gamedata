using System;
using UnityEngine;
using KSP.IO;
using System.Collections.Generic;


namespace Nereid
{
   namespace FinalFrontier
   {

      class RibbonPool : Pool<Ribbon>
      {
         public static readonly RibbonPool instance = new RibbonPool();

         // custom ribbons
         private readonly List<Ribbon> custom = new List<Ribbon>();

         // flag if ribbons already created
         private bool ribbonsCreated = false;

         private RibbonPool()
         {
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
         }

         protected override string CodeOf(Ribbon x)
         {
            return x.GetCode();
         }

         public Ribbon GetRibbonForCode(String code)
         {
            return base.GetElementForCode(code);
         }

         private void CreateRibbons()
         {
            Log.Info("creating ribbons in pool");
            Clear();
            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
               string bodyName = body.GetName();
               int basePrestige = body.BasePrestige();

               Log.Detail("creating ribbons for " + bodyName + ", base prestige is " + basePrestige);

               Achievement soi = new SphereOfInfluenceAchievement(body, basePrestige);
               Ribbon soiRibbon = new Ribbon(bodyName + "/SphereOfInfluence", soi);
               Add(soiRibbon);

               Ribbon evaOrbitRibbon = null;
               Ribbon evaRibbon = null;
               Ribbon orbitRibbon = null;
               Ribbon landingRibbon = null;
               Ribbon evagroundRibbon = null;
               Ribbon orbitDockedRibbon = null;
               Ribbon flagRibbon = null;
               Ribbon roverRibbon = null;
               Ribbon atmosphereRibbon = null;
               for (int i = 1; i <= 2; i++)
               {
                  bool first = (i == 2);
                  String prefix = first ? "/First" : "/";
                  Achievement orbit = new OrbitAchievement(body, basePrestige + 10 + i, first);
                  Achievement atmosphere = new EnteringAtmosphereAchievement(body, basePrestige + 15 + i, first);
                  Achievement landing = new LandingAchievement(body, basePrestige + 20 + i, first);
                  Achievement flag = new PlantFlagAchievement(body, basePrestige + 25 + i, first);
                  Achievement eva = new EvaAchievement(body, basePrestige + 30 + i, first);
                  Achievement rover = new RoverAchievement(body, basePrestige + 35 + i, first);
                  Achievement evaorbit = new EvaOrbitAchievement(body, basePrestige + 40 + i, first);
                  Achievement evaground = new EvaGroundAchievement(body, basePrestige + 50 + i, first);
                  Achievement docked = new DockingAchievement(body, basePrestige + 60 + i, first);

                  Add(orbitRibbon = new Ribbon(bodyName + prefix + "OrbitCapsule", orbit, first ? orbitRibbon : soiRibbon));
                  Add(evaRibbon = new Ribbon(bodyName + prefix + "EvaSpace", eva, first ? evaRibbon : soiRibbon));
                  Add(evaOrbitRibbon = new Ribbon(bodyName + prefix + "EvaOrbit", evaorbit, first ? evaOrbitRibbon : orbitRibbon));
                  Add(orbitDockedRibbon = new Ribbon(bodyName + prefix + "OrbitCapsuleDocked", docked, first ? orbitDockedRibbon : orbitRibbon));

                  // some achievements are impossible on the sun and other bodies
                  if (!bodyName.Equals("Sun"))
                  {
                     Add(landingRibbon = new Ribbon(bodyName + prefix + "Landing", landing,  first ? landingRibbon : soiRibbon));
                     Add(evagroundRibbon = new Ribbon(bodyName + prefix + "EvaGround", evaground, first ? evagroundRibbon : landingRibbon));
                     // no flag or rover on Jool
                     if (!bodyName.Equals("Jool"))
                     {
                        Add(flagRibbon = new Ribbon(bodyName + prefix + "PlantFlag", flag, first ? flagRibbon : evagroundRibbon ));
                        Add(roverRibbon = new Ribbon(bodyName + prefix + "Rover", rover, first ? roverRibbon : evagroundRibbon));
                     }
                  }
                  // some archivement are impossible without atmosphere
                  if(body.atmosphere)
                  {
                     // no atmosphere ribbon for Kerbin
                     if (!bodyName.Equals("Kerbin"))
                     {
                        Add(atmosphereRibbon = new Ribbon(bodyName + prefix + "Atmosphere", atmosphere, first ? atmosphereRibbon : landingRibbon));
                     }
                  }
               }
            }
            // special celestial body ribbons
            //
            Ribbon sunOrbitRibbon = GetRibbonForCode("O:Sun");
            Ribbon firstSunOrbitRibbon = GetRibbonForCode("O1:Sun");
            Ribbon ribbonCloserSolarOrbit = new Ribbon("Sun/CloserSolarOrbit",  new CloserSolarOrbitAchievement(50500, false), sunOrbitRibbon);
            Ribbon ribbonFirstCloserSolarOrbit = new Ribbon("Sun/FirstCloserSolarOrbit",  new CloserSolarOrbitAchievement(50550, true), firstSunOrbitRibbon);
            Add(ribbonCloserSolarOrbit);
            Add(ribbonFirstCloserSolarOrbit);
            // 
            // Jool deep athmosphere
            CelestialBody jool = Utils.GetCelestialBody("Jool");
            Ribbon soiJoolRibbon = GetRibbonForCode("I:Jool");
            if (jool != null)
            {
               Add(new Ribbon("Jool/DeepAtmosphere", new DeepAtmosphereArchievement(jool, jool.BasePrestige() + 90), soiJoolRibbon));
            }

            // Ribbons without a celestial body
            //
            // Multiple Missions
            Achievement flownFiveOrMoreMissions = new MissionsFlownAchievement(5, 86);
            Achievement flownTwentyOrMoreMissions = new MissionsFlownAchievement(20, 87);
            Achievement flownFiftyOrMoreMissions = new MissionsFlownAchievement(50, 88);
            Achievement flown100OrMoreMissions = new MissionsFlownAchievement(100, 89);
            Achievement flown200OrMoreMissions = new MissionsFlownAchievement(200, 90);
            Ribbon ribbonFiveOrMoreMissions = new Ribbon("Missions5", flownFiveOrMoreMissions);
            Ribbon ribbonTwentyOrMoreMissions = new Ribbon("Missions20", flownTwentyOrMoreMissions, ribbonFiveOrMoreMissions);
            Ribbon ribbonFiftyOrMoreMissions = new Ribbon("Missions50", flownFiftyOrMoreMissions, ribbonTwentyOrMoreMissions);
            Ribbon ribbon100OrMoreMissions = new Ribbon("Missions100", flown100OrMoreMissions, ribbonFiftyOrMoreMissions);
            Ribbon ribbon200OrMoreMissions = new Ribbon("Missions200", flown200OrMoreMissions, ribbon100OrMoreMissions);
            Add(ribbonFiveOrMoreMissions);
            Add(ribbonTwentyOrMoreMissions);
            Add(ribbonFiftyOrMoreMissions);
            Add(ribbon100OrMoreMissions);
            Add(ribbon200OrMoreMissions);
            //
            // Dangerous EVA
            Achievement dangerouseEva = new DangerousEvaAchievement(100001);
            Ribbon ribbonDangerouseEva = new Ribbon("DangerousEva", dangerouseEva);
            Add(ribbonDangerouseEva);
            //
            // Fast Orbit
            Achievement fastOrbit1 = new FastOrbitAchievement(250, 3101);
            Achievement fastOrbit2 = new FastOrbitAchievement(200, 3102);
            Achievement fastOrbit3 = new FastOrbitAchievement(150, 3103);
            Achievement fastOrbit4 = new FastOrbitAchievement(120, 3104);
            Ribbon ribbonFastOrbit1 = new Ribbon("FastOrbit1", fastOrbit1);
            Ribbon ribbonFastOrbit2 = new Ribbon("FastOrbit2", fastOrbit2, ribbonFastOrbit1);
            Ribbon ribbonFastOrbit3 = new Ribbon("FastOrbit3", fastOrbit3, ribbonFastOrbit2);
            Ribbon ribbonFastOrbit4 = new Ribbon("FastOrbit5", fastOrbit4, ribbonFastOrbit3); // FastOrbit4 skipped, because of upper/lower case problems in file name
            Add(ribbonFastOrbit1);
            Add(ribbonFastOrbit2);
            Add(ribbonFastOrbit3);
            Add(ribbonFastOrbit4);
            //
            // Mission Time
            Achievement missionTime5days = new MissionTimeAchievement(Utils.ConvertDaysToSeconds(5), 4901);
            Achievement missionTime20days = new MissionTimeAchievement(Utils.ConvertDaysToSeconds(20), 4902);
            Achievement missionTime50days = new MissionTimeAchievement(Utils.ConvertDaysToSeconds(50), 4903);
            Achievement missionTime100days = new MissionTimeAchievement(Utils.ConvertDaysToSeconds(100), 4904);
            Achievement missionTime500days = new MissionTimeAchievement(Utils.ConvertDaysToSeconds(500), 4905);
            Achievement missionTime2000days = new MissionTimeAchievement(Utils.ConvertDaysToSeconds(2000), 4906);
            Achievement missionTime5000days = new MissionTimeAchievement(Utils.ConvertDaysToSeconds(5000), 4907);
            Ribbon ribbonMissionTime5days = new Ribbon("LongMissionTime1", missionTime5days);
            Ribbon ribbonMissionTime20days = new Ribbon("LongMissionTime2", missionTime20days, ribbonMissionTime5days);
            Ribbon ribbonMissionTime50days = new Ribbon("LongMissionTime3", missionTime50days, ribbonMissionTime20days);
            Ribbon ribbonMissionTime100days = new Ribbon("LongMissionTime4", missionTime100days, ribbonMissionTime50days);
            Ribbon ribbonMissionTime500days = new Ribbon("LongMissionTime5", missionTime500days, ribbonMissionTime100days);
            Ribbon ribbonMissionTime2000days = new Ribbon("LongMissionTime6", missionTime2000days, ribbonMissionTime500days);
            Ribbon ribbonMissionTime5000days = new Ribbon("LongMissionTime7", missionTime5000days, ribbonMissionTime2000days);
            Add(ribbonMissionTime5days);
            Add(ribbonMissionTime20days);
            Add(ribbonMissionTime50days);
            Add(ribbonMissionTime100days);
            Add(ribbonMissionTime500days);
            Add(ribbonMissionTime2000days);
            Add(ribbonMissionTime5000days);
            //
            // Endurance
            Achievement endurance20days = new SingleMissionTimeAchievement(Utils.ConvertDaysToSeconds(20), 4951);
            Achievement endurance50days = new SingleMissionTimeAchievement(Utils.ConvertDaysToSeconds(50), 4952);
            Achievement endurance125days = new SingleMissionTimeAchievement(Utils.ConvertDaysToSeconds(125), 4953);
            Achievement endurance500days = new SingleMissionTimeAchievement(Utils.ConvertDaysToSeconds(500), 4954);
            Achievement endurance2000days = new SingleMissionTimeAchievement(Utils.ConvertDaysToSeconds(2000), 4955);
            Ribbon ribbonEndurance20days = new Ribbon("SingleMissionTime1", endurance20days);
            Ribbon ribbonEndurance50days = new Ribbon("SingleMissionTime2", endurance50days, ribbonEndurance20days);
            Ribbon ribbonEndurance125days = new Ribbon("SingleMissionTime3", endurance125days, ribbonEndurance50days);
            Ribbon ribbonEndurance500days = new Ribbon("SingleMissionTime4", endurance500days, ribbonEndurance125days);
            Ribbon ribbonEndurance2000days = new Ribbon("SingleMissionTime5", endurance2000days, ribbonEndurance500days);
            Add(ribbonEndurance20days);
            Add(ribbonEndurance50days);
            Add(ribbonEndurance125days);
            Add(ribbonEndurance500days);
            Add(ribbonEndurance2000days);
            //
            // Splashdown
            Add( new Ribbon("Splashdown", new SplashDownAchievement(85)) );
            //
            // Collision
            Add(new Ribbon("Collision", new CollisionAchievement(0)));
            //
            // First in Space
            Ribbon ribbonFirstInSpace = new Ribbon("FirstInSpace", new InSpaceAchievement(999000));
            Add(ribbonFirstInSpace);
            //
            // First EVA in Space
            Ribbon ribbonFirstEvaInSpace = new Ribbon("FirstEvaInSpace", new FirstEvaInSpaceAchievement(899000));
            Add(ribbonFirstEvaInSpace);
            //
            // Solid Fuel Booster Launch
            Ribbon solidFuelLaunch10;
            Ribbon solidFuelLaunch20;
            Add(solidFuelLaunch10 = new Ribbon("SolidFuelBooster10", new SolidFuelLaunchAchievement(10, 710)));
            Add(solidFuelLaunch20 = new Ribbon("SolidFuelBooster20", new SolidFuelLaunchAchievement(20, 720), solidFuelLaunch10));
            Add(new Ribbon("SolidFuelBooster30", new SolidFuelLaunchAchievement(30, 730), solidFuelLaunch20));

            //
            // G-Force
            Ribbon geeForce3;
            Ribbon geeForce4;
            Ribbon geeForce5;
            Add(geeForce3 = new Ribbon("HighGeeForce3", new HighGeeForceAchievement(3, 91)));
            Add(geeForce4 = new Ribbon("HighGeeForce4", new HighGeeForceAchievement(4, 92), geeForce3));
            Add(geeForce5 = new Ribbon("HighGeeForce5", new HighGeeForceAchievement(5, 93), geeForce4));
            Add(new Ribbon("HighGeeForce6", new HighGeeForceAchievement(6, 94), geeForce5));
            //
            // Heavy Vehicle
            Ribbon heavyVehicle1 = new Ribbon("HeavyVehicle1",new HeavyVehicleAchievement(250, 401));
            Ribbon heavyVehicle2 = new Ribbon("HeavyVehicle2", new HeavyVehicleAchievement(500, 402),  heavyVehicle1);
            Ribbon heavyVehicle3 = new Ribbon("HeavyVehicle3", new HeavyVehicleAchievement(750, 403),  heavyVehicle2);
            Ribbon heavyVehicle4 = new Ribbon("HeavyVehicle4", new HeavyVehicleAchievement(1000, 404), heavyVehicle3);
            Ribbon heavyVehicle5 = new Ribbon("HeavyVehicle5", new HeavyVehicleAchievement(1500, 405), heavyVehicle4);
            Ribbon heavyVehicle6 = new Ribbon("HeavyVehicle6", new HeavyVehicleAchievement(2000, 406), heavyVehicle5);
            Ribbon heavyVehicle7 = new Ribbon("HeavyVehicle7", new HeavyVehicleAchievement(4000, 407), heavyVehicle6);
            Add(heavyVehicle1);
            Add(heavyVehicle2);
            Add(heavyVehicle3);
            Add(heavyVehicle4);
            Add(heavyVehicle5);
            Add(heavyVehicle6);
            Add(heavyVehicle7);
            //
            // Heavy Vehicle Landing
            Ribbon heavyVehicleLanding1 = new Ribbon("HeavyVehicleLanding1", new HeavyVehicleLandAchievement(250, 421));
            Ribbon heavyVehicleLanding2 = new Ribbon("HeavyVehicleLanding2", new HeavyVehicleLandAchievement(500, 422), heavyVehicleLanding1);
            Ribbon heavyVehicleLanding3 = new Ribbon("HeavyVehicleLanding3", new HeavyVehicleLandAchievement(750, 423), heavyVehicleLanding2);
            Ribbon heavyVehicleLanding4 = new Ribbon("HeavyVehicleLanding4", new HeavyVehicleLandAchievement(1000, 424), heavyVehicleLanding3);
            Ribbon heavyVehicleLanding5 = new Ribbon("HeavyVehicleLanding5", new HeavyVehicleLandAchievement(1500, 425), heavyVehicleLanding4);
            Ribbon heavyVehicleLanding6 = new Ribbon("HeavyVehicleLanding6", new HeavyVehicleLandAchievement(2000, 426), heavyVehicleLanding5);
            Ribbon heavyVehicleLanding7 = new Ribbon("HeavyVehicleLanding7", new HeavyVehicleLandAchievement(4000, 427), heavyVehicleLanding6);
            Add(heavyVehicleLanding1);
            Add(heavyVehicleLanding2);
            Add(heavyVehicleLanding3);
            Add(heavyVehicleLanding4);
            Add(heavyVehicleLanding5);
            Add(heavyVehicleLanding6);
            Add(heavyVehicleLanding7);
            //
            // Heavy Vehicle Launch
            Ribbon heavyVehicleLaunch1 = new Ribbon("HeavyVehicleLaunch1", new HeavyVehicleLaunchAchievement(250,  411));
            Ribbon heavyVehicleLaunch2 = new Ribbon("HeavyVehicleLaunch2", new HeavyVehicleLaunchAchievement(500, 412), heavyVehicleLaunch1);
            Ribbon heavyVehicleLaunch3 = new Ribbon("HeavyVehicleLaunch3", new HeavyVehicleLaunchAchievement(750, 413), heavyVehicleLaunch2);
            Ribbon heavyVehicleLaunch4 = new Ribbon("HeavyVehicleLaunch4", new HeavyVehicleLaunchAchievement(1000, 414), heavyVehicleLaunch3);
            Ribbon heavyVehicleLaunch5 = new Ribbon("HeavyVehicleLaunch5", new HeavyVehicleLaunchAchievement(1500, 415), heavyVehicleLaunch4);
            Ribbon heavyVehicleLaunch6 = new Ribbon("HeavyVehicleLaunch6", new HeavyVehicleLaunchAchievement(2000, 416), heavyVehicleLaunch5);
            Ribbon heavyVehicleLaunch7 = new Ribbon("HeavyVehicleLaunch7", new HeavyVehicleLaunchAchievement(4000, 417), heavyVehicleLaunch6);
            Add(heavyVehicleLaunch1);
            Add(heavyVehicleLaunch2);
            Add(heavyVehicleLaunch3);
            Add(heavyVehicleLaunch4);
            Add(heavyVehicleLaunch5);
            Add(heavyVehicleLaunch6);
            Add(heavyVehicleLaunch7);
            //
            // Eva Time
            Ribbon evaTime1 = new Ribbon("TotalEva1", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(1),   471));
            Ribbon evaTime2 = new Ribbon("TotalEva2", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(2),   472), evaTime1);
            Ribbon evaTime3 = new Ribbon("TotalEva3", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(6),   473), evaTime2);
            Ribbon evaTime4 = new Ribbon("TotalEva4", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(12),  474), evaTime3);
            Ribbon evaTime5 = new Ribbon("TotalEva5", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(24),  475), evaTime4);
            Ribbon evaTime6 = new Ribbon("TotalEva6", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(48),  476), evaTime5);
            Ribbon evaTime7 = new Ribbon("TotalEva7", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(96),  477), evaTime6);
            Ribbon evaTime8 = new Ribbon("TotalEva8", new EvaTotalTimeAchievement(Utils.ConvertHoursToSeconds(192), 478), evaTime7);
            Add(evaTime1);
            Add(evaTime2);
            Add(evaTime3);
            Add(evaTime4);
            Add(evaTime5);
            Add(evaTime6);
            Add(evaTime7);
            Add(evaTime8);
            //
            // EVA Endurance 
            Ribbon evaEndurance1 = new Ribbon("Eva1", new EvaTimeAchievement(2 * Utils.ConvertHoursToSeconds(1) / 6, 451));
            Ribbon evaEndurance2 = new Ribbon("Eva2", new EvaTimeAchievement(3 * Utils.ConvertHoursToSeconds(1) / 6, 452), evaEndurance1);
            Ribbon evaEndurance3 = new Ribbon("Eva3", new EvaTimeAchievement(4 * Utils.ConvertHoursToSeconds(1) / 6, 453), evaEndurance2);
            Ribbon evaEndurance4 = new Ribbon("Eva4", new EvaTimeAchievement(5 * Utils.ConvertHoursToSeconds(1) / 6, 454), evaEndurance3);
            Ribbon evaEndurance5 = new Ribbon("Eva5", new EvaTimeAchievement(1 * Utils.ConvertHoursToSeconds(1),     455), evaEndurance4);
            Ribbon evaEndurance6 = new Ribbon("Eva6", new EvaTimeAchievement(3 * Utils.ConvertHoursToSeconds(1) / 2, 456), evaEndurance5);
            Ribbon evaEndurance7 = new Ribbon("Eva7", new EvaTimeAchievement(1 * Utils.ConvertHoursToSeconds(2),     457), evaEndurance6);
            Ribbon evaEndurance8 = new Ribbon("Eva8", new EvaTimeAchievement(1 * Utils.ConvertHoursToSeconds(3),     458), evaEndurance7);
            Ribbon evaEndurance9 = new Ribbon("Eva9", new EvaTimeAchievement(1 * Utils.ConvertHoursToSeconds(4),     459), evaEndurance8);
            Ribbon evaEndurance10 = new Ribbon("Eva10", new EvaTimeAchievement(1 * Utils.ConvertHoursToSeconds(5),   460), evaEndurance9);
            Add(evaEndurance1);
            Add(evaEndurance2);
            Add(evaEndurance3);
            Add(evaEndurance4);
            Add(evaEndurance5);
            Add(evaEndurance6);
            Add(evaEndurance7);
            Add(evaEndurance8);
            Add(evaEndurance9);
            Add(evaEndurance10);
            //
            // Mach
            Ribbon mach1 = new Ribbon("Mach1", new MachNumberAchievement(1, 481));
            Ribbon mach2 = new Ribbon("Mach2", new MachNumberAchievement(2, 482),  mach1);
            Ribbon mach3 = new Ribbon("Mach3", new MachNumberAchievement(3, 483),  mach2);
            Ribbon mach4 = new Ribbon("Mach4", new MachNumberAchievement(4, 484),  mach3);
            Ribbon mach5 = new Ribbon("Mach5", new MachNumberAchievement(5, 485),  mach4);
            Ribbon mach6 = new Ribbon("Mach6", new MachNumberAchievement(6, 486),  mach5);
            Ribbon mach7 = new Ribbon("Mach7", new MachNumberAchievement(8, 487),  mach6);
            Ribbon mach8 = new Ribbon("Mach8", new MachNumberAchievement(10, 488), mach7);
            Add(mach1);
            Add(mach2);
            Add(mach3);
            Add(mach4);
            Add(mach5);
            Add(mach6);
            Add(mach7);
            Add(mach8);
            //
            // dont know how to detect yet
            // Achievement missionAbort = new MissionAbortedAchievement(55);
            // Add(new Ribbon("MissionAborted", missionAbort));

            Sort();

            //Persistence.WriteSupersedeChain(this);

            Log.Info("ribbon pool created");
         }

         private void CreateCustomRibbons()
         {
            Log.Info("creating custom ribbons");
            // custom ribbons provided by nothke
            CreateCustomRibbon(0, "Diamond", "Diamond", "The highest honor awarded for exceptional courage, unselfishness and valor");
            CreateCustomRibbon(1, "InterSidera", "Inter Sidera", "\"Among the Stars\" - A commemorative ribbon for fallen kerbonauts");
            CreateCustomRibbon(2, "Kerbalkind", "Kerbalkind", "Honorary retirement award for service to Kerbalkind");
            // --------------- 3: not working
            CreateCustomRibbon(4, "Station", "Station", "custom station ribbon");
            CreateCustomRibbon(5, "Spaceplane", "Spaceplane", "custom spaceplane ribbon");
            CreateCustomRibbon(6, "CertifiedBadass", "Certified Badass", "Awarded for ludicrous, near-impossible and brave endeavor");
            // custom ribbons provided by SmarterThanMe
            CreateCustomRibbon(21, "STM01", "Test Pilot", "Awarded for courage in flying in experimental craft");
            CreateCustomRibbon(22, "STM02", "Expeditionary Command", "Awarded for being in command of a significant expedition, station or base");
            CreateCustomRibbon(23, "STM03", "Mission Command", "Awarded for being in command of a small scale mission");
            // --------------- 24: used
            // --------------- 25: used
            CreateCustomRibbon(26, "STM06", "Space Search & Rescue", "Awarded for being involved in a search and rescue mission in space");
            CreateCustomRibbon(27, "STM07", "Wings", "Awarded for piloting a plane in atmosphere");
            CreateCustomRibbon(28, "STM08", "Space Wings", "Awarded for piloting a spaceplane in and out of atmosphere");
            // --------------- 29: used
            CreateCustomRibbon(30, "STM10", "Arrow", "");
            CreateCustomRibbon(31, "STM11", "Qualified Scientist", "Awarded for having the necessary skills and knowledge, and completing the space mission training program");
            CreateCustomRibbon(32, "STM12", "Qualified Operations", "Awarded for having the necessary skills and knowledge, and completing the space mission training program");
            CreateCustomRibbon(33, "STM13", "Qualified Engineering", "Awarded for having the necessary skills and knowledge, and completing the space mission training program");
            CreateCustomRibbon(34, "STM14", "Specialist Scientist", "Awarded for developing and demonstrating specialist aptitude in field activities for the Kerbal Space Program");
            CreateCustomRibbon(35, "STM15", "Specialist Operations", "Awarded for developing and demonstrating specialist aptitude in field activities for the Kerbal Space Program");
            CreateCustomRibbon(36, "STM16", "Specialist Engineering", "Awarded for developing and demonstrating specialist aptitude in field activities for the Kerbal Space Program");
            CreateCustomRibbon(37, "STM17", "Senior Scientist", "Awarded for leadership, significant experience and outstanding technical aptitude in field activities for the Kerbal Space Program");
            CreateCustomRibbon(38, "STM18", "Senior Operations", "Awarded for leadership, significant experience and outstanding technical aptitude in field activities for the Kerbal Space Program");
            CreateCustomRibbon(39, "STM19", "Senior Engineering", "Awarded for leadership, significant experience and outstanding technical aptitude in field activities for the Kerbal Space Program");
            CreateCustomRibbon(24, "STM04", "Chief Scientist", "Awarded for being a Lead Scientist on a significant expedition, or a mission with a major scientific task");
            CreateCustomRibbon(29, "STM09", "Chief Operations", "Awarded for being the lead officer in charge of operations on a significant expedition or mission with a major piloting task or series of tasks");
            CreateCustomRibbon(25, "STM05", "Chief Engineering", "Awarded for being the Lead Engineer on a significant expedition or mission with a major engineering task");


            // generic custom ribbons
            int CUSTOM_BASE_INDEX = 100;
            for (int i = 0; i < 20; i++)
            {
               CustomAchievement achievement = new CustomAchievement(CUSTOM_BASE_INDEX + i, -1000 + i);
               int nr = i + 1;
               String ss = nr.ToString("00");
               achievement.SetName(ss + " Custom");
               achievement.SetText(ss + " Custom");
               Ribbon ribbon = new Ribbon("Custom"+ss,achievement);
               custom.Add(ribbon);
               Add(ribbon);
            }
            Log.Info("custom ribbons created ("+custom.Count+" custom ribbons)");
         }

         public void CreateCustomRibbon(int index, String filename, String name, String text)
         {
            Log.Detail("creating custom ribbon " + name + " (#" + index + ")");
            CustomAchievement achievement = new CustomAchievement(index, -1000 + index);
            //String ss = index.ToString("00");
            Ribbon ribbon = new Ribbon(filename, achievement);
            achievement.SetName(name);
            achievement.SetText(text);
            custom.Add(ribbon);
            Add(ribbon);
         }

         public List<Ribbon> GetCustomRibbons()
         {
            return custom;
         }

         private void OnGameStateCreated(Game game)
         {
            // we wont load ribbons twice
            if (ribbonsCreated) return;
            // create ribbons
            CreateRibbons();
            CreateCustomRibbons();
            // and guard this
            ribbonsCreated = true;
         }

      }
   }
}
