using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nereid
{

   namespace FinalFrontier
   {

      class EventObserver
      {
         private readonly AchievementRecorder recorder;

         // previous active vessel State
         private volatile VesselState previousVesselState;

         // custom events
         private bool orbitClosed = false;
         private bool deepAthmosphere = false;
         private long updateCycle = 0;
         //
         private Vector3d lastVesselSurfacePosition;
         private bool landedVesselHasMoved = false;
         //
         private readonly GeeForceInspector geeForceInspector = new GeeForceInspector();
         private readonly MachNumberInspector machInspector = new MachNumberInspector();
         private readonly AltitudeInspector altitudeInspector = new AltitudeInspector();
         private readonly AtmosphereInspector atmosphereInspector = new AtmosphereInspector();

         //
         //private MissionSummaryWindow missionSummaryWindow; 


         public EventObserver()
         {
            Log.Info("EventObserver:: registering events");
            this.recorder = new AchievementRecorder();
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            GameEvents.onLaunch.Add(this.OnLaunch);
            GameEvents.onGamePause.Add(this.OnGamePause);
            GameEvents.onVesselGoOnRails.Add(this.OnVesselGoOnRails);
            GameEvents.onVesselSOIChanged.Add(this.OnVesselSOIChanged);
            GameEvents.onVesselSituationChange.Add(this.OnVesselSituationChange);
            GameEvents.onVesselChange.Add(this.OnVesselChange);
            GameEvents.onVesselRecovered.Add(this.OnVesselRecovered);
            GameEvents.onVesselOrbitClosed.Add(this.OnVesselOrbitClosed); // wont work in 0.23
            GameEvents.onCrewOnEva.Add(this.OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(this.OnCrewBoardVessel);
            GameEvents.onStageActivate.Add(this.OnStageActivate);
            GameEvents.onJointBreak.Add(this.OnJointBreak);
            GameEvents.onGameStateSaved.Add(this.OnGameStateSaved);
            GameEvents.onGameSceneLoadRequested.Add(this.OnGameSceneLoadRequested);
            GameEvents.onCollision.Add(this.OnCollision);
            GameEvents.onPartCouple.Add(this.OnPartCouple);
            GameEvents.onPartAttach.Add(this.OnPartAttach);
            GameEvents.onVesselWasModified.Add(this.OnVesselWasModified);
         }

         public void OnAwake()
         {
            // not used
            // reserved
         }  


         private void OnVesselWasModified(Vessel vessel)
         {
            // not used
         }

         private void OnGamePause()
         {
            // not used
         }         

         private void OnPartActionUICreate(Part part)
         {
            // not used
         }

         private void OnSceneChange()
         {
            // not used
         } 

         private void OnPartAttach(GameEvents.HostTargetAction<Part,Part> action)
         {
            // not used
         }

         private void OnPartCouple(GameEvents.FromToAction<Part, Part> action)
         {
            // we are just checking flights
            if (!HighLogic.LoadedSceneIsFlight) return;
            Part from = action.from;
            Part to = action.to;
            Log.Info("part couple event");
            // eva wont count as docking
            if (from == null || from.vessel == null || from.vessel.isEVA) return;
            Log.Info("from vessel " + from.vessel);
            if (to == null || to.vessel == null || to.vessel.isEVA) return;
            Log.Info("to vessel " + to.vessel);
            Vessel vessel = action.from.vessel.isActiveVessel?action.from.vessel:action.to.vessel;
            if (vessel != null && vessel.isActiveVessel)
            {
               Log.Info("docking vessel "+vessel.name);
               VesselState vesselState = new VesselState(vessel);
               CheckAchievements(vesselState.Docked());
               recorder.RecordDocking(vessel);
            }
         }

         public void OnVesselOrbitClosed(Vessel vessel)
         {
            Log.Detail("EventObserver:: OnVesselOrbitClosed " + vessel.GetName());
            if(vessel.isActiveVessel)
            {
               CheckAchievements(vessel);
            }
         }

         public void OnEnteringDeepAthmosphere(Vessel vessel)
         {
            Log.Detail("EventObserver:: OnEnteringDeepAthmosphere " + vessel.GetName() );
            if (vessel.isActiveVessel)
            {
               CheckAchievements(vessel);
            }
         }

         public void OnLandedVesselMove(Vessel vessel)
         {
            Log.Detail("EventObserver:: OnLandedVesselMove " + vessel.GetName());
            if (vessel.isActiveVessel)
            {
               VesselState vesselState = new VesselState(vessel);
               CheckAchievements(vesselState.MovedOnSurface());
            }
         }

         
         
         private void OnCollision(EventReport report)
         {
            // just for safety
            if (report.origin != null && report.origin.vessel!=null)
            {
               Vessel vessel = report.origin.vessel;
               if (vessel.isActiveVessel)
               {
                  Log.Info("EventObserver:: collision detected for active vessel " + vessel.GetName());
                  CheckAchievements(vessel, report);
               }
            }
         }

         private void OnGameStateSaved(Game game)
         {
            Log.Info("EventObserver:: OnGameStateSaved: game saved " + game.Title);
            Persistence.Save(game, HallOfFame.instance);
         }

         private void OnGameSceneLoadRequested(GameScenes scene)
         {
            Log.Info("EventObserver:: OnGameSceneLoadRequested: "+scene+" current="+HighLogic.LoadedScene);
            this.previousVesselState = null;
         }

         private void OnJointBreak(EventReport report)
         {
            // not used
         }

         private void OnStageActivate(int stage)
         {
            // not used
         }


         private void  OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> action)
         {
            Log.Detail("EventObserver:: crew board vessel "+action.from.GetType());
            if (action.from == null || action.from.vessel == null) return;
            Part from = action.from;
            if (action.to == null || action.to.vessel == null) return;
            // boarding crew is still the active vessel
            Vessel eva = action.from.vessel;
            Vessel vessel = action.to.vessel;
            String nameOfKerbalOnEva = eva.vesselName;
            // find kerbal that return from eva in new crew
            ProtoCrewMember member = vessel.GetCrewMember(nameOfKerbalOnEva);
            if (member!=null)
            {
               Log.Detail(member.name + " returns from EVA to " + vessel.name);
               recorder.RecordBoarding(member);
               CheckAchievements(member);
            }
            else
            {
               Log.Warning("boarding crew member " + nameOfKerbalOnEva+" not found in vesssel "+vessel.name);
            }
         }

         private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> action)
         {
            Log.Detail("EventObserver:: crew on EVA");
            if (action.from == null || action.from.vessel == null) return;
            if (action.to == null || action.to.vessel == null) return;
            Vessel vessel = action.from.vessel;
            Vessel crew = action.to.vessel;
            // record EVA
            foreach(ProtoCrewMember member in crew.GetVesselCrew())
            {
               recorder.RecordEva(member, vessel);
            }
            // the previous vessel shoud be previous
            this.previousVesselState = new VesselState(vessel);
            // current vessel is crew
            CheckAchievements(crew);
         }

         private void OnGameStateCreated(Game game)
         {
            ResetInspectors();
            Log.Info("EventObserver:: OnGameStateCreated " + game.UniversalTime+", game status: "+game.Status+", scene "+HighLogic.LoadedScene);
            Persistence.Load(game, HallOfFame.instance);
         }


         private void OnVesselRecovered(ProtoVessel vessel)
         {
            Log.Info("EventObserver:: OnVesselRecovered " + vessel.vesselName);
            recorder.RecordVesselRecovered(vessel);
            // check for kerbal specific achiements
            HallOfFame.instance.BeginArwardOfRibbons();
            foreach (ProtoCrewMember member in vessel.GetVesselCrew())
            {
               CheckAchievements(member);
            }
            HallOfFame.instance.EndArwardOfRibbons();
            //
            // ------ MissionSummary ------
            if(HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
               if (FinalFrontier.configuration.IsMissionSummaryEnabled())
               {
                  double technicalMissionEndTime = Planetarium.GetUniversalTime();
                  MissionSummaryWindow missionSummaryWindow = new MissionSummaryWindow();
                  missionSummaryWindow.SetSummaryForVessel(vessel, technicalMissionEndTime);
                  missionSummaryWindow.SetVisible(true);
               }
            }
            // 
            // refresh roster status
            HallOfFame.instance.Refresh();
         }


         private void OnLaunch(EventReport report)
         {
            ResetInspectors();
            //
            Vessel vessel = FlightGlobals.ActiveVessel;
            if(vessel!=null)
            {
               Log.Detail("EventObserver:: OnLaunch: " + vessel.name);
               if (!vessel.isActiveVessel) return;
               //
               ResetLandedVesselHasMovedFlag();
               // Mission is started (=first launch)?
               // comparing to 0.01 to prevent undedected launches; a second launch after 0.01 seconds should be impossible
               if(vessel.missionTime<=0.01)
               {
                  recorder.RecordLaunch(vessel);
               }
               //
               VesselState vesselState = VesselState.CreateLaunchFromVessel(vessel);
               CheckAchievements(vesselState);
            }
         }

         private void OnVesselGoOnRails(Vessel vessel)
         {
            // not used
         }

         private void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> e)
         {
            Vessel vessel = e.host;
            if (vessel.isActiveVessel)
            {
               if (vessel.situation != Vessel.Situations.LANDED) ResetLandedVesselHasMovedFlag();
               //
               Log.Detail("situation change for active vessel");
               CheckAchievements(vessel);
            }
            else
            {
               if (vessel != null && vessel.IsFlag() && vessel.situation==Vessel.Situations.LANDED)
               {
                  Log.Detail("situation change for flag");
                  Vessel active = FlightGlobals.ActiveVessel;
                  if (active != null && active.isEVA)
                  {
                     VesselState vesselState = VesselState.CreateFlagPlantedFromVessel(active);
                     CheckAchievements(vesselState);
                     return;
                  }
               }
            }
         }

         private void OnVesselChange(Vessel vessel)
         {
            ResetInspectors();

            ResetLandedVesselHasMovedFlag();
            //
            Log.Info("EventObserver:: onVesselChange " + vessel.GetName());
            if (!vessel.isActiveVessel) return;
            //
            this.previousVesselState = null;
            CheckAchievements(vessel);
         }

         private void OnVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> e)
         {
            // not used
         }

         private void checkAchievements(VesselState previous, VesselState current, EventReport report, bool hasToBeFirst)
         {
            foreach (Ribbon ribbon in RibbonPool.instance)
            {
               Achievement achievement = ribbon.GetAchievement();
               if (achievement.HasToBeFirst() == hasToBeFirst)
               {
                  Vessel vessel = current.Origin;
                  // check situationchanges
                  if (achievement.Check(previous,current))
                  {
                     recorder.Record(ribbon, vessel);
                  }
                  // check events
                  if(report!=null && achievement.Check(report))
                  {
                     recorder.Record(ribbon, vessel);
                  }
               }
            }
         }

         private void checkAchievements(VesselState previous, VesselState current, EventReport report)
         {
            // first check all first achievements
            checkAchievements(previous, current, report, true);
            // now check the rest
            checkAchievements(previous, current, report, false);
         }


         private void CheckAchievements(VesselState currentVesselState, EventReport report = null)
         {
            Log.Info("EventObserver:: checkArchivements for vessel state");
            //
            //
            checkAchievements(previousVesselState, currentVesselState, report);
            //
            this.previousVesselState = currentVesselState;
            Log.Info("EventObserver:: checkArchivements done");
         }

         private void CheckAchievements(Vessel vessel, EventReport report=null)
         {
            // just delegate
            CheckAchievements(new VesselState(vessel), report);
         }

         private void CheckAchievements(ProtoCrewMember kerbal, bool hasToBeFirst)
         {
            if (kerbal == null) return;
            HallOfFameEntry entry = HallOfFame.instance.GetEntry(kerbal);
            if (entry != null)
            {
               foreach (Ribbon ribbon in RibbonPool.instance)
               {
                  Achievement achievement = ribbon.GetAchievement();
                  if (achievement.HasToBeFirst() == hasToBeFirst)
                  {
                     if (achievement.Check(entry))
                     {
                        recorder.Record(ribbon, kerbal);
                     }
                  }
               }
            }
            else
            {
               Log.Warning("no entry for kerbal " + kerbal.name + " in hall of fame");
            }
         }

         private void CheckAchievements(ProtoCrewMember kerbal)
         {
            // just for safety
            if (kerbal == null) return;
            Log.Detail("EventObserver:: checkArchivements for kerbal "+kerbal.name);
            // first check all first achievements
            CheckAchievements(kerbal, true);
            // now check the rest
            CheckAchievements(kerbal, false);
            Log.Detail("EventObserver:: checkArchivements done");
         }

         // TODO: move to EventObserver
         private void FireCustomEvents()
         {
            // detect events only in flight
            if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;
            //
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel != null)
            {
               // Orbit closed
               bool inOrbit = vessel.isInStableOrbit();
               if (inOrbit && !orbitClosed)
               {
                  Log.Info("orbit closed detected for vessel " + vessel.name);
                  OnVesselOrbitClosed(vessel);
               }
               orbitClosed = inOrbit;
               //
               // deep athmosphere
               double atmDensity = vessel.atmDensity;
               if (!deepAthmosphere && atmDensity >= 10.0)
               {
                  Log.Trace("vessel entering deep athmosphere");
                  deepAthmosphere = true;
                  OnEnteringDeepAthmosphere(vessel);
               }
               else if (deepAthmosphere && atmDensity < 10.0)
               {
                  deepAthmosphere = false;
               }
            }
            else
            {
               orbitClosed = false;
               deepAthmosphere = false;
            }
            //
            // G-force increased
            if(geeForceInspector.StateHasChanged())
            {
               CheckAchievements(vessel);
            }
            // Mach increased
            if (machInspector.StateHasChanged())
            {
               CheckAchievements(vessel);
            }
            //AtmosphereChanged
            if (atmosphereInspector.StateHasChanged())
            {
               CheckAchievements(vessel); 
            }
            //

         }

         private void ResetLandedVesselHasMovedFlag()
         {
            Log.Detail("reset of LandedVesselHasMovedFlag");
            landedVesselHasMoved = false;
            lastVesselSurfacePosition = Vector3d.zero;
         }

         private bool checkIfLandedVesselHasMoved(Vessel vessel)
         {
            if(vessel != null)
            {
               // no rover, no driving vehilce, sorry
               if (vessel.vesselType != VesselType.Rover) return false;
               //
               if (vessel.situation == Vessel.Situations.LANDED)
               {
                  Vector3d currentVesselPosition = vessel.GetWorldPos3D();
                  double distance = Vector3d.Distance(currentVesselPosition, this.lastVesselSurfacePosition);
                  if(distance > Constants.MIN_DISTANCE_FOR_MOVING_VEHICLE_ON_SURFACE)
                  {
                     if (this.lastVesselSurfacePosition != Vector3d.zero )
                     {
                        return true;
                     }
                     this.lastVesselSurfacePosition = currentVesselPosition;
                  }
               }
               else
               {
                  // not landed, so ignore this position
                  this.lastVesselSurfacePosition = Vector3d.zero;
               }
            }
            else
            {
               // no vessel, so ignore this position
               this.lastVesselSurfacePosition = Vector3d.zero;
            }
            return false;
         }


         private void ResetInspectors()
         {
            if (Log.IsLogable(Log.LEVEL.DETAIL)) Log.Detail("rest of inspectors");
            machInspector.Reset();
            altitudeInspector.Reset();
            geeForceInspector.Reset();
            atmosphereInspector.Reset();
         }

         public void Update()
         {
            // game eventy occur in FLIGHT only
            if ( !HighLogic.LoadedScene.Equals(GameScenes.FLIGHT) ) return;

            updateCycle++;
            // test custom events every fifth update
            if (updateCycle % 5 == 0)
            {
               Vessel vessel = FlightGlobals.ActiveVessel;
               if (vessel != null)
               {
                  if (!landedVesselHasMoved && checkIfLandedVesselHasMoved(vessel))
                  {
                     landedVesselHasMoved = true;
                     OnLandedVesselMove(vessel);
                  }

                  geeForceInspector.Inspect(vessel);

                  altitudeInspector.Inspect(vessel);
                  if(altitudeInspector.StateHasChanged())
                  {
                     machInspector.Reset();
                     altitudeInspector.Reset();
                  }

                  machInspector.Inspect(vessel);

                  atmosphereInspector.Inspect(vessel);
               }
            }

            // test for custom events each second
            if (updateCycle % 60 == 0)
            {
               FireCustomEvents();
               ResetInspectors();
            }
         }

      }
   }
}