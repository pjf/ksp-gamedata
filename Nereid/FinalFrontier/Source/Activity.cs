using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nereid
{
   namespace FinalFrontier
   {
      public abstract class Activity 
      {
         private static ActivityPool ACTIVITY_POOL = ActivityPool.instance;

         private readonly String code;
         private String name;

         public Activity(String code, String name)
         {
            this.code = code;
            this.name = name;
            ACTIVITY_POOL.RegisterActivity(this);
         }

         // name of the activity
         public virtual String GetName() 
         {
            return name;
         }

         // unique code of the activity
         public String GetCode()
         {
            return code;
         }

         public override bool Equals(System.Object right)
         {
            //Log.Test("Activity::Equals");
            if (right == null) return false;
            Activity cmp = right as Activity;
            //Log.Test("Activity::Equals instance");
            if (cmp == null) return false;
            //Log.Test("Activity::Equals codes "+GetCode()+" ? "+cmp.GetCode());
            //Log.Test("Activity::Equals compare "+GetCode().Equals(cmp.code));
            return GetCode().Equals(cmp.code);
         }

         public override int GetHashCode()
         {
            return code.GetHashCode();
         }

         public abstract String CreateLogBookEntry(LogbookEntry entry);

         public override string ToString()
         {
            return code;
         }

         protected void Rename(String name)
         {
            this.name = name;
         }
      }
   }
}
