using System;
using UnityEngine;
using KSP.IO;

namespace Nereid
{
   namespace FinalFrontier
   {
      public abstract class AbstractWindow 
      {
         public static readonly int AUTO_HEIGHT = -1;
         private static readonly int DEFAULT_WIDTH = 400;
         //
         private readonly int id;
         private string title;
         private bool visible = false;
         protected Rect bounds = new Rect();
         protected Vector2 mousePosition;
         private Vector2 tmp = new Vector2(0,0);

         private String tooltip;

         public AbstractWindow(int id, string title) 
         {
            Log.Detail("creating window "+id+" with title '"+title+"'");
            this.id = id;
            this.title = title;

            try
            {
               RenderingManager.AddToPostDrawQueue(0, OnDraw);
            }
            catch
            {
               Log.Error("error creating window "+id+" "+title);
            }
         }

         public int GetWindowId()
         {
            return id;
         }

         protected virtual void OnOpen()
         {
         }

         protected virtual void OnClose()
         {
         }

         protected void DragWindow()
         {
            GUI.DragWindow();
         }

         private void OnDraw()
         {
            if (visible)
            {
               if (GetInitialHeight() == AUTO_HEIGHT)
               {
                  bounds = GUILayout.Window(id, bounds, OnWindowInternal, title, FFStyles.STYLE_WINDOW, GUILayout.Width(GetInitialWidth()));
               }
               else
               {
                  bounds = GUILayout.Window(id, bounds, OnWindowInternal, title, FFStyles.STYLE_WINDOW, GUILayout.Width(GetInitialWidth()), GUILayout.Height(GetInitialHeight()));
               }
            }
         }

         private void OnWindowInternal(int id)
         {
            if (Log.IsLogable(Log.LEVEL.TRACE)) Log.Trace("OnWindowInternal for ID "+id+" called; x="+bounds.x+", y="+bounds.y);
            mousePosition.x = Input.mousePosition.x - bounds.x;
            mousePosition.y = (Screen.height - Input.mousePosition.y) - bounds.y - FFStyles.STYLE_WINDOW.border.top;
            OnWindow(id);
            DrawTooltip();
            OnDrawFinished(id);
            CheckBounds();
         }

         private void CheckBounds()
         {
            const float MARGIN = 5;
            if (bounds.x > Screen.width - MARGIN)
            {
               Log.Warning("WINDOW "+id+" OUT OF SCREEN (x=" + bounds.x+"); resetting x coordinates");
               bounds.x = Screen.width - bounds.width;
            }
            if (bounds.x + bounds.width < MARGIN)
            {
               Log.Warning("WINDOW " + id + " OUT OF SCREEN (x=" + bounds.x + "); resetting x coordinates");
               bounds.x = 0;
            }

            if (bounds.y > Screen.height - MARGIN)
            {
               Log.Warning("WINDOW " + id + " OUT OF SCREEN (y=" + bounds.y + "); resetting y coordinates");
               bounds.y = Screen.height - bounds.height;
            }
            if (bounds.y + bounds.height < MARGIN)
            {
               Log.Warning("WINDOW " + id + " OUT OF SCREEN (y=" + bounds.y + "); resetting y coordinates");
               bounds.y = 0;
            }

         }

         protected virtual void OnDrawFinished(int id)
         {
         }

         protected void DrawTooltip()
         {
            tooltip = GUI.tooltip;
            if (tooltip != null && tooltip.Trim().Length > 0)
            {
               Vector2 size = FFStyles.STYLE_TOOLTIP.CalcSize(new GUIContent(tooltip));
               float x = (mousePosition.x + size.x > bounds.width) ? (bounds.width - size.x) : mousePosition.x;
               float y = mousePosition.y + 32;
               GUI.Label(new Rect(x-1, y-1, size.x, size.y), tooltip, FFStyles.STYLE_BG_TOOLTIP);
               GUI.Label(new Rect(x, y, size.x, size.y), tooltip, FFStyles.STYLE_TOOLTIP);
            }
         }

         protected abstract void OnWindow(int id);


         public void SetVisible(bool visible, float x, float y)
         {
            bounds.x = x;
            bounds.y = y;
            SetVisible(visible);
         }


         public void SetVisible(bool visible)
         {
            if (!this.visible && visible) OnOpen();
            if (this.visible && !visible) OnClose();
            this.visible = visible;
            if (Log.IsLogable(Log.LEVEL.TRACE) && visible) Log.Trace("set window ID "+id+" to visible");
         }

         public bool IsVisible()
         {
            return this.visible;
         }

         // TODO: make protected and add GetWidth(), GetHeight()
         public virtual int GetInitialWidth()
         {
            return DEFAULT_WIDTH;
         }

         // TODO: make protected and add GetWidth(), GetHeight()
         protected virtual int GetInitialHeight()
         {
            return AUTO_HEIGHT;
         }

         public int GetX()
         {
            return (int)bounds.xMin;
         }

         public int GetY()
         {
            return (int)bounds.yMin;
         }

         protected void MoveWindowAside(AbstractWindow window)
         {
            int x = GetX() + GetInitialWidth() + 5;
            if (x + window.GetInitialWidth() > Screen.width) x = GetX() - window.GetInitialWidth() - 5;
            window.SetPosition(x, GetY());
         }

         protected bool MouseOver(float dx=0, float dy=0)
         {
            tmp.x = mousePosition.x + dx;
            tmp.y = mousePosition.y + dy;
            return GUILayoutUtility.GetLastRect().Contains(tmp);
         }

         public Vector2 GetMousePositionInWindow()
         {
            return mousePosition;
         }

         public void SetPosition(int x, int y)
         {
            Log.Trace("moving window "+id+" to "+x+"/"+y);
            bounds.Set(x, y, bounds.width, bounds.height);
         }

         public void SetTitle(String title)
         {
            this.title = title;
         }

         public void CenterWindow()
         {
            if (!visible) return;
            int x = (Screen.width - (int)bounds.width) / 2;
            int y = (Screen.height - (int)bounds.height) / 2;
            SetPosition(x, y);
         }
      }

   }
}
