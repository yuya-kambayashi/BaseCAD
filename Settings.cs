﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCAD
{
    public class Settings
    {
        private Dictionary<string, Setting> settings = new Dictionary<string, Setting>();

        public class Setting<T> : Setting
        {
            public string Name { get; private set; }
            public T Value { get; set; }

            public Setting(string name, T value)
            {
                Name = name;
                Value = value;
            }
        }

        public abstract class Setting
        {

        }

        public void Set<T>(string name, T value)
        {
            if (settings.TryGetValue(name, out Setting s))
            {
                ((Setting<T>)s).Value = value;
            }
            else
            {
                settings.Add(name, new Setting<T>(name, value));
            }
        }

        public T Get<T>(string name)
        {
            Setting s = settings[name];
            return ((Setting<T>)s).Value;
        }

        public Settings()
        {
            Set("DisplayPrecision", 2);
        }
    }
}
