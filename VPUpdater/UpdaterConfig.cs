#region Copyright

// -----------------------------------------------------------------------
//  <copyright file="VirtualParadiseConfig.cs" company="VPUpdater">
//      (C) 2019 Oliver Booth. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

#endregion

namespace VPUpdater
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    #endregion

    /// <summary>
    /// Represents a serialized version of the file VPUpdater.cfg
    /// </summary>
    [Serializable]
    public class UpdaterConfig
    {
        #region Fields

        /// <summary>
        /// The filename of the configuration.
        /// </summary>
        private const string ConfigFile = @"VPUpdater.cfg";

        /// <summary>
        /// Config dictionary.
        /// </summary>
        private readonly Dictionary<string, object> configValues;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterConfig"/> class.
        /// </summary>
        /// <param name="configValues">The config dictionary.</param>
        private UpdaterConfig(Dictionary<string, object> configValues)
        {
            this.configValues = configValues;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a config value.
        /// </summary>
        /// <param name="key">The key to fetch.</param>
        /// <param name="defaultValue">The default value to return, should <paramref name="key"/> not be found.</param>
        /// <returns>Returns the value.</returns>
        public object this[string key, object defaultValue = default]
        {
            get => this.configValues.ContainsKey(key)
                ? Convert.ChangeType(this.configValues[key], defaultValue?.GetType() ?? typeof(object))
                : defaultValue;
            set
            {
                if (this.configValues.ContainsKey(key))
                {
                    this.configValues[key] = value;
                }
                else
                {
                    this.configValues.Add(key, value);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the configuration file and returns an instance of <see cref="UpdaterConfig"/> that represents it.
        /// </summary>
        /// <returns>Returns an instance of <see cref="UpdaterConfig"/>.</returns>
        public static async Task<UpdaterConfig> Load()
        {
            if (!File.Exists(ConfigFile))
            {
                return new UpdaterConfig(new Dictionary<string, object>());
            }

            using (StreamReader reader = new StreamReader(ConfigFile, Encoding.UTF8))
            {
                Dictionary<string, object> configValues = new Dictionary<string, object>();
                string                     contents     = await reader.ReadToEndAsync();
                string[]                   lines        = contents.Split(Environment.NewLine.ToCharArray());

                foreach (string line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] split = line.Split(new[] {'='}, 2);

                    if (configValues.ContainsKey(split[0]))
                    {
                        configValues[split[0]] = split[1];
                    }
                    else
                    {
                        configValues.Add(split[0], split[1]);
                    }
                }

                UpdaterConfig config = new UpdaterConfig(configValues);
                return config;
            }
        }

        /// <summary>
        /// Loads the default configuration.
        /// </summary>
        public async Task LoadDefaults()
        {
            this["stable_only"] = this["stable_only", 1];

            await this.Save();
        }

        /// <summary>
        /// Saves the configuration to file.
        /// </summary>
        public async Task Save()
        {
            using (StreamWriter writer = new StreamWriter(ConfigFile, false, Encoding.UTF8))
            {
                foreach (KeyValuePair<string, object> pair in this.configValues)
                {
                    await writer.WriteLineAsync($"{pair.Key}={pair.Value}");
                }
            }
        }

        #endregion
    }
}
