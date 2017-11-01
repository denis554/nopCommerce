﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Nop.Core;

namespace Nop.Web.Framework.Themes
{
    /// <summary>
    /// Represents a default theme provider implementation
    /// </summary>
    public partial class ThemeProvider : IThemeProvider
    {
        #region Constants

        private const string ThemesPath = "~/Themes/";
        private const string ThemeDescriptionFileName = "theme.json";

        #endregion

        #region Fields

        private IList<ThemeDescriptor> _themeDescriptors;

        #endregion

        #region Utilities

        /// <summary>
        /// Get theme descriptor from the file
        /// </summary>
        /// <param name="filePath">Path to the description file</param>
        /// <returns>Theme descriptor</returns>
        protected virtual ThemeDescriptor GeThemeDescriptor(string filePath)
        {
            var text = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(text))
                return new ThemeDescriptor();

            //get theme description from the JSON file
            var themeDescriptor = JsonConvert.DeserializeObject<ThemeDescriptor>(text);

            //some validation
            if (string.IsNullOrEmpty(themeDescriptor?.SystemName))
                throw new Exception($"A theme descriptor '{filePath}' has no system name");

            if (_themeDescriptors?.Any(descriptor => descriptor.SystemName.Equals(themeDescriptor.SystemName, StringComparison.InvariantCultureIgnoreCase)) ?? false)
                throw new Exception($"A theme with '{themeDescriptor.SystemName}' system name is already defined");

            return themeDescriptor;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all theme descriptors
        /// </summary>
        /// <returns>List of the theme descriptor</returns>
        public IList<ThemeDescriptor> GetThemeDescriptors()
        {
            if (_themeDescriptors == null)
            {
                //load all theme descriptors
                var themeFolder = new DirectoryInfo(CommonHelper.MapPath(ThemesPath));
                _themeDescriptors = new List<ThemeDescriptor>();
                foreach (var descriptionFile in themeFolder.GetFiles(ThemeDescriptionFileName, SearchOption.AllDirectories))
                {
                    _themeDescriptors.Add(GeThemeDescriptor(descriptionFile.FullName));
                }
            }

            return _themeDescriptors;
        }

        /// <summary>
        /// Get theme descriptor by theme system name
        /// </summary>
        /// <param name="systemName">Theme system name</param>
        /// <returns>Theme descriptor</returns>
        public ThemeDescriptor GetThemeDescriptorBySystemName(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return null;

            return GetThemeDescriptors().SingleOrDefault(descriptor => descriptor.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Check whether theme descriptor with specified system name exists
        /// </summary>
        /// <param name="systemName">Theme system name</param>
        /// <returns>True if theme descriptor exists; otherwise false</returns>
        public bool ThemeDescriptorExists(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return false;

            return GetThemeDescriptors().Any(descriptor => descriptor.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion
    }
}