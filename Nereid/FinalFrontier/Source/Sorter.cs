using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Nereid
{
   namespace FinalFrontier
   {
      public interface Sorter<T>
      {
         void Sort(List<T> list);
      }
   }
}
