using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Nereid
{
   namespace FinalFrontier
   {
      class ActivityPool : Pool<Activity>
      {
         public static readonly ActivityPool instance = new ActivityPool();

         private static readonly Mutex mutex = new Mutex();

         private ActivityPool()
         {

         }

         protected override string CodeOf(Activity x)
         {
            return x.GetCode();
         }

         public void RegisterActivity(Activity activity)
         {
            try
            {
               Log.Info("registering activity "+activity.GetCode()+": "+activity.GetName());
               mutex.WaitOne();
               Add(activity);
            }
            finally
            {
               mutex.ReleaseMutex();
            }
         }
      }
   }
}
