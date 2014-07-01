using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Nereid
{
   namespace FinalFrontier
   {
      public interface Filter<T>
      {
         bool Accept(T x);
      }
   }
}
