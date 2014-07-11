using System;
using UnityEngine;
using KSP.IO;
using Toolbar;

namespace Nereid
{
   namespace FinalFrontier
   {

      [KSPAddonFixed(KSPAddon.Startup.EveryScene, false, typeof(FinalFrontier))]
      public class FinalFrontier : MonoBehaviour
      {
         private static int wokenupCount = 0;

         private static EventObserver observer = new EventObserver();

         public static readonly String RESOURCE_PATH =  "Nereid/FinalFrontier/Resource/";

         public static readonly Configuration configuration = new Configuration();

         private IButton button;
         private HallOfFameBrowser browser;

         // just to make sure that all pool instances existst
         private RibbonPool ribbons = RibbonPool.instance;
         private ActionPool actions = ActionPool.instance;

         private volatile bool keyAltPressed = false;
         private volatile bool keyCtrlPressed = false;

         public void Awake()
         {
            wokenupCount++;
            // first awake?
            if (IsFirstAwake())
            {
               configuration.Load();
               Log.SetLevel(configuration.GetLogLevel());
               Log.Info("log level is " + configuration.GetLogLevel());
            }
            Log.Info("log level is " + configuration.GetLogLevel());

            observer.OnAwake();
         }

         private bool IsFirstAwake()
         {
            return wokenupCount == 1;
         }

         public void Start()
         {
            AddToolbarButtons();
         }


         public void Update()
         {
            //
            if (Input.GetKeyDown(KeyCode.LeftAlt)) keyAltPressed = true;
            if (Input.GetKeyUp(KeyCode.LeftAlt)) keyAltPressed = false;
            if (Input.GetKeyDown(KeyCode.LeftControl)) keyCtrlPressed = true;
            if (Input.GetKeyUp(KeyCode.LeftControl)) keyCtrlPressed = false;
            if (configuration.IsHotkeyEnabled() && keyAltPressed && Input.GetKeyDown(KeyCode.F))
            {
               Log.Info("hotkey ALT-F detected");
               switch (HighLogic.LoadedScene)
               {
                  case GameScenes.EDITOR:
                  case GameScenes.FLIGHT:
                  case GameScenes.SPACECENTER:
                  case GameScenes.TRACKSTATION:
                  case GameScenes.SPH:
                     if (!keyCtrlPressed)
                     {
                        Log.Info("hotkey hall of fame browser");
                        createBrowserOnce();
                        browser.SetVisible(!browser.IsVisible());
                     }
                     else
                     {
                        Log.Info("hotkey reset window positions");
                        PositionableWindow.ResetAllWindowPositions();
                     }
                     break;
                  default:
                     Log.Info("cant open/close hall of fame in game scene " + HighLogic.LoadedScene);
                     break;
               }
            }

            if (observer != null)
            {
               observer.Update();
            }

         }



         private void AddToolbarButtons()
         {
            Log.Info("adding toolbar buttons");
            String iconOn = RESOURCE_PATH + "IconOn_24";
            String iconOff = RESOURCE_PATH + "IconOff_24";
            button = ToolbarManager.Instance.add("FinalFrontier", "button");
            if (button != null)
            {
               button.TexturePath = iconOff;
               button.ToolTip = "Open Final Frontier";
               button.OnClick += (e) =>
                   {
                      createBrowserOnce();
                      if (browser != null) browser.registerToolbarButton(button, iconOn, iconOff);
                      toggleBrowserVisibility();
                   };

               button.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION, GameScenes.SPH);
            }
            else
            {
               Log.Error("toolbar button was null");
            }
         }

         private void toggleBrowserVisibility()
         {
            browser.SetVisible(!browser.IsVisible());
         }

         private void createBrowserOnce()
         {
            if (browser == null)
            {
               browser = new HallOfFameBrowser();
            }
         }

         internal void OnDestroy()
         {
            Log.Info("destroying Final Frontier");
            Log.Info("log level is " + Log.GetLogLevel());
            configuration.Save();
            button.Destroy();
         }

      }
   }
}
