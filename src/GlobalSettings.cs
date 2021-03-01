﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace furdown
{
    [Serializable]
    public class GlobalSettings
    {
        public string systemPath;
        public string downloadPath;
        public string filenameTemplate;
        public string descrFilenameTemplate;
        public bool downloadOnlyOnce;

        // %SCRAPS% substitute when handling scraps galleries
        [OptionalField(VersionAdded = 2)]
        public string scrapsTemplateActive;
        // %SCRAPS% substitute when handling non-scraps galleries and loose submissions
        [OptionalField(VersionAdded = 2)]
        public string scrapsTemplatePassive;

        [NonSerialized]
        public static GlobalSettings Settings;

        [NonSerialized]
        private static string appDataPath;
        [NonSerialized]
        private static string settingsFN;

        [OnDeserialized()]
        public void DbDeserialized(StreamingContext context)
        {
            if (scrapsTemplateActive == null) scrapsTemplateActive = ".scraps";
            if (scrapsTemplatePassive == null) scrapsTemplatePassive = "";
        }

        /// <summary>
        /// Creates a singleton settings object and loads parameters from file, if it exists.
        /// </summary>
        public static void GlobalSettingsInit()
        {
            Settings = new GlobalSettings();
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            settingsFN = Path.Combine(appDataPath, "furdown\\furdown.conf");
            // true portable mode
            if (File.Exists("./furdown-portable.conf"))
            {
                settingsFN = "./furdown-portable.conf";
            }
            // if settings file exists, load it
            bool needToSetDefaults = false;
            if (File.Exists(settingsFN))
            {
                using (Stream stream = File.Open(settingsFN, FileMode.Open))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    try
                    {
                        Settings = (GlobalSettings)bformatter.Deserialize(stream);
                    }
                    // settings file is present, but failed to be loaded
                    catch
                    {
                        needToSetDefaults = true;
                        Console.WriteLine("Settings file is incompatible or corrupted, restoring defaults.");
                    }
                }
            }
            else
            {
                needToSetDefaults = true;
            }
            // set defaults, if settings file does not exist or otherwise not loaded
            if (needToSetDefaults)
            {
                Settings.downloadPath = Path.Combine(appDataPath, "furdown\\downloads");
                Settings.systemPath = Path.Combine(appDataPath, "furdown\\system");
                Settings.filenameTemplate = "%ARTIST%%SCRAPS%\\%SUBMID%.%FILEPART%";
                Settings.descrFilenameTemplate = "%ARTIST%%SCRAPS%\\%SUBMID%.%FILEPART%.dsc.htm";
                Settings.downloadOnlyOnce = true;
                Settings.scrapsTemplateActive = ".scraps";
                Settings.scrapsTemplateActive = "";
                try
                {
                    Directory.CreateDirectory(Settings.downloadPath);
                    Directory.CreateDirectory(Settings.systemPath);
                }
                catch
                {
                    Console.WriteLine("[Error] Default paths are invalid!");
                }
            }
            else
            {
                if (!Directory.Exists(Settings.downloadPath))
                    Directory.CreateDirectory(Settings.downloadPath);
                if (!Directory.Exists(Settings.systemPath))
                    Directory.CreateDirectory(Settings.systemPath);
            }
            SubmissionsDB.Load();
        }

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        public static void GlobalSettingsSave()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFN));
                using (Stream stream = File.Open(settingsFN, FileMode.Create))
                {
                    var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bformatter.Serialize(stream, Settings);
                }
                SubmissionsDB.Save();
            }
            catch
            {
                Console.WriteLine("[Error] Failed to write settings file!");
                System.Windows.Forms.MessageBox.Show("Failed to write settings file!");
            }
        }
    }
}
