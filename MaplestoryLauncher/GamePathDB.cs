using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaplestoryLauncher
{
    class GamePathDB
    {
        Dictionary<string, string> alias = new Dictionary<string, string>()
            {
                {"新楓之谷", "MapleStory.exe"},
                {"艾爾之光", "elsword.exe"},
                {"跑跑", "KartRider.exe"},
                {"mabinogi", "mabinogi.exe" },
            };

        Dictionary<string, string> db;
        public GamePathDB()
        {
            try
            {
                db = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Settings.Default.gamePathDB);
            }
            catch
            {
                db = new Dictionary<string, string>();
            }
            finally
            {
                string gamePath = Properties.Settings.Default.defaultGamePath;
                if (gamePath != "")
                {
                    string programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                    if (programFilesPath == null)
                        programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles");
                    if (!gamePath.StartsWith(programFilesPath))
                        gamePath = programFilesPath + "\\" + gamePath;
                    Set("新楓之谷", gamePath);
                    Properties.Settings.Default.defaultGamePath = "";
                    Save();
                }
            }
        }

        public string GetAlias(string key)
        {
            foreach (string k in alias.Keys)
            {
                if (key.Contains(k))
                {
                    return alias[k];
                }
            }

            return key;
        }

        public string Get(string key)
        {
            string val = "";
            return db.TryGetValue(GetAlias(key), out val) == true ? val : "";
        }

        public void Set(string key, string val)
        {

            db[GetAlias(key)] = val;
        }

        public void Save()
        {
            Properties.Settings.Default.gamePathDB = JsonConvert.SerializeObject(db);
            Properties.Settings.Default.Save();
        }
    }
}
