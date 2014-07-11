using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace Nereid
{
   namespace FinalFrontier
   {
      static class Persistence
      {
         private static readonly String ROOT_PATH = Utils.GetRootPath();
         private static readonly String SAVE_BASE_FOLDER = ROOT_PATH + "/saves/"; // suggestion/hint from Cydonian Monk
         private static readonly String FILE_NAME = "halloffame.ksp";
         private static readonly int BACKUP_CNT = 9;

         /*
          * This method is called for testign purposes only. It should never be called in a public release
          */
         public static void WriteSupersedeChain(Pool<Ribbon> ribbons)
         {
            StreamWriter file = File.CreateText(ROOT_PATH+"/GameData/Nereid/FinalFrontier/supersede.txt");
            List<Ribbon> sorted = new List<Ribbon>(ribbons);
            sorted.Sort(delegate(Ribbon left, Ribbon right)
            {
               return left.GetCode().CompareTo(right.GetCode());
            });
            try
            {
               foreach (Ribbon ribbon in sorted)
               {
                  String code = ribbon.GetCode().PadRight(20);
                  Ribbon supersede = ribbon.SupersedeRibbon();
                  file.WriteLine(code+(supersede!=null?supersede.GetCode():""));
               }
            }
            finally
            {
               file.Close();
            }
         }

         public static void Save( HallOfFame hallOfFame)
         {
            Save(HighLogic.CurrentGame, hallOfFame);
         }

         public static void Save(Game game, HallOfFame hallOfFame)
         {
            String filename = SAVE_BASE_FOLDER + HighLogic.SaveFolder + "/" + FILE_NAME;
            // create a backup of halloffame
            Utils.FileRotate(filename, BACKUP_CNT);
            //
            Log.Detail("saving hall of fame to " + filename + "(root path was " + ROOT_PATH + ")");
            hallOfFame.WriteToFile(filename);
         }

         public static void Load(Game game, HallOfFame hallOfFame)
         {
            String filename = SAVE_BASE_FOLDER + HighLogic.SaveFolder + "/" + FILE_NAME;
            Log.Detail("loading hall of fame from " + filename);
            List<LogbookEntry> logbook = LoadLogbook(filename,game.UniversalTime);
            hallOfFame.CreateFromLogbook(game,logbook);
            Log.Detail("loading hall of fame done");
         }

         public static void SaveLogbook(List<LogbookEntry> book, String filename)
         {
            Log.Detail("writing logbook to file "+filename);
            StreamWriter file = File.CreateText(filename);
            try
            {
               Log.Detail("logbook file " + filename + " created");
               foreach (LogbookEntry entry in book)
               {
                  file.WriteLine(entry.Serialize());
               }
               Log.Detail("writing of logbook finished");
            }
            finally
            {
               file.Close();
            }
         }

         public static List<LogbookEntry> LoadLogbook(String filename, double timeLimit=0)
         {
            Log.Detail("loading logbook");
            StreamReader file = null;
            List<LogbookEntry> book = new List<LogbookEntry>();
            try
            {
               if(!File.Exists(filename))
               {
                  Log.Detail("no final frontier logbook (" + filename + ") found");
                  return book;
               }

               file = File.OpenText(filename);
               String line;
               while( (line=file.ReadLine()) != null )
               {
                  Log.Trace("read line: "+line);
                  if (line.Trim().Length > 0)
                  {
                     LogbookEntry entry = LogbookEntry.Deserialize(line);
                     if(entry!=null)
                     {
                        if (timeLimit == 0 || entry.UniversalTime <= timeLimit)
                        {
                           book.Add(entry);
                        }
                        else
                        {
                           Log.Warning("entry at "+entry.UniversalTime+" ignored because of time constraint ("+timeLimit+")");
                           Log.Detail("line '" + line + "' ignored");
                        }
                     }
                     else
                     {
                        Log.Error("invalid file structure in " + filename + "; can't deserialize line '" + line + "'");
                     }
                  }
               } 
            }
            catch
            {
               Log.Error("loading hall of fame logbook failed");
            }
            finally
            {
               if(file!=null) file.Close();
            }
            return book;
         }

      }
   }
}