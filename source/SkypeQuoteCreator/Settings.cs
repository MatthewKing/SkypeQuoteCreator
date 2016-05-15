using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SkypeQuoteCreator
{
    /// <summary>
    /// Represents the application settings file.
    /// </summary>
    internal sealed class Settings
    {
        /// <summary>
        /// Gets the default directory for the settings file.
        /// </summary>
        public static string DefaultDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkypeQuoteCreator");

        /// <summary>
        /// Gets the default settings file.
        /// </summary>
        public static Settings Default { get; } = new Settings(Path.Combine(DefaultDirectory, "settings.xml"));

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets a set containing the names that have been used.
        /// </summary>
        public ISet<string> NameHistory { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Settings file path.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Initializes a new instance of the Settings class.
        /// </summary>
        /// <param name="path">Settings file path.</param>
        private Settings(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Saves the settings to the settings file.
        /// </summary>
        public void Save()
        {
            try
            {
                string xml = new XDocument(new XElement("Settings",
                    new XElement("UserId", UserId),
                    new XElement("NameHistory", NameHistory.Select(name => new XElement("Name", name)).ToArray())))
                        .ToString(SaveOptions.None);

                File.WriteAllText(_path, xml);
            }
            catch
            {
                // Swallow all exceptions.
            }
        }

        /// <summary>
        /// Populates the settings from the settings file.
        /// </summary>
        public void Load()
        {
            try
            {
                string xml = File.ReadAllText(_path);
                XDocument document = XDocument.Parse(xml);

                UserId = document.Root.Element("UserId")?.Value;
                foreach (string name in document.Root.Element("NameHistory").Elements("Name").Select(element => element.Value))
                {
                    NameHistory.Add(name);
                }
            }
            catch
            {
                // Swallow all exceptions.
            }
        }
    }
}
