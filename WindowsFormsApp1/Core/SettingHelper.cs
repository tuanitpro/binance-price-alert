using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.Core
{
    public class SymbolItem
    {
        public string Symbol { get; set; }
        public decimal Above { get; set; }
        public decimal Below { get; set; }
        public decimal ChangePrice { get; set; } = 0;
        public decimal LastPrice { get; set; } = 0;
    }
    public class Setting
    {
        public string FilePath { get; set; }
        public List<SymbolItem> ListPrices { get; set; }
    }

    public class SettingHelper
    {
        public Setting Setting { get; set; }
        public List<SymbolItem> ListPrices
        {
            get
            {
                return Setting.ListPrices;
            }
        }
        public SettingHelper()
        {
            Setting = GetSetting();
        }

        public Setting GetSetting()
        {
            var setting = new Setting();
            var filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}data\data.ini";
            string text = System.IO.File.ReadAllText(filePath);
            if (!string.IsNullOrEmpty(text))
            {
                setting = JsonConvert.DeserializeObject<Setting>(text);
            }
            else
            {
                setting = new Setting
                {
                    FilePath = $@"{AppDomain.CurrentDomain.BaseDirectory}data\alarm.wav",
                    ListPrices = new List<SymbolItem>()
                };
            }
            return setting;
        }

        public static void Save(Setting setting)
        {
            string text = JsonConvert.SerializeObject(setting);
            var filePath = $@"{AppDomain.CurrentDomain.BaseDirectory}data\data.ini";
            System.IO.File.WriteAllText(filePath, text);
        }
    }
    public class SettingSingleton
    {
        static object _syncLock = new object();
        static SettingHelper _instance;
        public static SettingHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncLock)
                    {
                        if (_instance == null) _instance = new SettingHelper();
                    }
                }
                return _instance;
            }
        }
    }
}