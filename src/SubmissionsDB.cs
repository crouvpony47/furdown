using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace furdown
{
    [Serializable]
    class SubmissionsDB
    {
        private SortedSet<uint> database;

        [OptionalField(VersionAdded=2)]
        private Dictionary<uint, uint> fileids; // used for determining the updated content

        [NonSerialized]
        public static SubmissionsDB DB;

        [OnDeserialized()]
        public void DbDeserialized(StreamingContext context)
        {
            if (fileids == null)
            {
                Console.WriteLine("Note :: Upgrading database format: v.1 -> v.2");
                fileids = new Dictionary<uint, uint>();
            }
        }

        public SubmissionsDB()
        {
            database = new SortedSet<uint>();
            fileids = new Dictionary<uint, uint>();
        }

        public bool Exists(uint sub)
        {
            return database.Contains(sub);
        }

        public uint GetFileId(uint sub)
        {
            if (!fileids.ContainsKey(sub))
            {
                return 0;
            }
            return fileids[sub];
        }

        public bool AddSubmission(uint sub)
        {
            return database.Add(sub);
        }

        public bool AddSubmissionWithFileId(uint sub, uint fileid)
        {
            fileids[sub] = fileid;
            return database.Add(sub);
        }

        public bool RemoveSubmission(uint sub)
        {
            if (fileids.ContainsKey(sub))
            {
                fileids.Remove(sub);
            }
            return database.Remove(sub);
        }

        public void Clear()
        {
            DB.database.Clear();
            Save();
        }
        
        public static void Save()
        {
            try
            {
                string dbFN = Path.Combine(GlobalSettings.Settings.systemPath, "furdown.db");
                using (Stream stream = File.Open(dbFN, FileMode.Create))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bformatter.Serialize(stream, DB);
                }
            }
            catch
            {
                Console.WriteLine("[Error] Failed to write database file!");
                System.Windows.Forms.MessageBox.Show("Failed to write database file!");
            }
        }

        public static void Load()
        {
            try
            {
                string dbFN = Path.Combine(GlobalSettings.Settings.systemPath, "furdown.db");
                using (Stream stream = File.Open(dbFN, FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    try
                    {
                        DB = (SubmissionsDB)bformatter.Deserialize(stream);
                    }
                    // DB file is present, but failed to be loaded
                    catch
                    {
                        Console.WriteLine("Failed to load DB, using empty one.");
                    }
                }
            }
            catch
            {
                Console.WriteLine("No DB found.");
            }
        }
    }
}
