//------------------------------------------------------------------------------
// The contents of this file are subject to the nopCommerce Public License Version 1.0 ("License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at  http://www.nopCommerce.com/License.aspx. 
// 
// Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
// See the License for the specific language governing rights and limitations under the License.
// 
// The Original Code is nopCommerce.
// The Initial Developer of the Original Code is NopSolutions.
// All Rights Reserved.
// 
// Contributor(s): _______. 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using NopSolutions.NopCommerce.BusinessLogic.Caching;
using NopSolutions.NopCommerce.BusinessLogic.Configuration.Settings;
using NopSolutions.NopCommerce.BusinessLogic.Data;
using NopSolutions.NopCommerce.Common.Utils;


namespace NopSolutions.NopCommerce.BusinessLogic.Directory
{
    /// <summary>
    /// Country manager
    /// </summary>
    public partial class CountryManager
    {
        #region Constants
        private const string COUNTRIES_ALL_KEY = "Nop.country.all-{0}";
        private const string COUNTRIES_REGISTRATION_KEY = "Nop.country.registration-{0}";
        private const string COUNTRIES_BILLING_KEY = "Nop.country.billing-{0}";
        private const string COUNTRIES_SHIPPING_KEY = "Nop.country.shipping-{0}";
        private const string COUNTRIES_BY_ID_KEY = "Nop.country.id-{0}";
        private const string COUNTRIES_PATTERN_KEY = "Nop.country.";
        #endregion

        #region Methods
        /// <summary>
        /// Deletes a country
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        public static void DeleteCountry(int countryId)
        {
            var country = GetCountryById(countryId);
            if (country == null)
                return;

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(country))
                context.Countries.Attach(country);
            context.DeleteObject(country);
            context.SaveChanges();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(COUNTRIES_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Gets all countries
        /// </summary>
        /// <returns>Country collection</returns>
        public static List<Country> GetAllCountries()
        {
            bool showHidden = NopContext.Current.IsAdmin;
            string key = string.Format(COUNTRIES_ALL_KEY, showHidden);
            object obj2 = NopRequestCache.Get(key);
            if (CountryManager.CacheEnabled && (obj2 != null))
            {
                return (List<Country>)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from c in context.Countries
                        orderby c.DisplayOrder, c.Name
                        where showHidden || c.Published
                        select c;
            var countryCollection = query.ToList();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.Add(key, countryCollection);
            }
            return countryCollection;
        }

        /// <summary>
        /// Gets all countries that allow registration
        /// </summary>
        /// <returns>Country collection</returns>
        public static List<Country> GetAllCountriesForRegistration()
        {
            bool showHidden = NopContext.Current.IsAdmin;
            string key = string.Format(COUNTRIES_REGISTRATION_KEY, showHidden);
            object obj2 = NopRequestCache.Get(key);
            if (CountryManager.CacheEnabled && (obj2 != null))
            {
                return (List<Country>)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from c in context.Countries
                        orderby c.DisplayOrder, c.Name
                        where (showHidden || c.Published) && c.AllowsRegistration
                        select c;
            var countryCollection = query.ToList();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.Add(key, countryCollection);
            }
            return countryCollection;
        }

        /// <summary>
        /// Gets all countries that allow billing
        /// </summary>
        /// <returns>Country collection</returns>
        public static List<Country> GetAllCountriesForBilling()
        {
            bool showHidden = NopContext.Current.IsAdmin;
            string key = string.Format(COUNTRIES_BILLING_KEY, showHidden);
            object obj2 = NopRequestCache.Get(key);
            if (CountryManager.CacheEnabled && (obj2 != null))
            {
                return (List<Country>)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from c in context.Countries
                        orderby c.DisplayOrder, c.Name
                        where (showHidden || c.Published) && c.AllowsBilling
                        select c;
            var countryCollection = query.ToList();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.Add(key, countryCollection);
            }
            return countryCollection;
        }

        /// <summary>
        /// Gets all countries that allow shipping
        /// </summary>
        /// <returns>Country collection</returns>
        public static List<Country> GetAllCountriesForShipping()
        {

            bool showHidden = NopContext.Current.IsAdmin;
            string key = string.Format(COUNTRIES_SHIPPING_KEY, showHidden);
            object obj2 = NopRequestCache.Get(key);
            if (CountryManager.CacheEnabled && (obj2 != null))
            {
                return (List<Country>)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from c in context.Countries
                        orderby c.DisplayOrder, c.Name
                        where (showHidden || c.Published) && c.AllowsShipping
                        select c;
            var countryCollection = query.ToList();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.Add(key, countryCollection);
            }
            return countryCollection;
        }

        /// <summary>
        /// Gets a country 
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Country</returns>
        public static Country GetCountryById(int countryId)
        {
            if (countryId == 0)
                return null;

            string key = string.Format(COUNTRIES_BY_ID_KEY, countryId);
            object obj2 = NopRequestCache.Get(key);
            if (CountryManager.CacheEnabled && (obj2 != null))
            {
                return (Country)obj2;
            }

            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from c in context.Countries
                        where c.CountryId == countryId
                        select c;
            var country = query.SingleOrDefault();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.Add(key, country);
            }
            return country;
        }

        /// <summary>
        /// Gets a country by two letter ISO code
        /// </summary>
        /// <param name="twoLetterIsoCode">Country two letter ISO code</param>
        /// <returns>Country</returns>
        public static Country GetCountryByTwoLetterIsoCode(string twoLetterIsoCode)
        {
            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from c in context.Countries
                        where c.TwoLetterIsoCode == twoLetterIsoCode
                        select c;
            var country = query.FirstOrDefault();

            return country;
        }

        /// <summary>
        /// Gets a country by three letter ISO code
        /// </summary>
        /// <param name="threeLetterIsoCode">Country three letter ISO code</param>
        /// <returns>Country</returns>
        public static Country GetCountryByThreeLetterIsoCode(string threeLetterIsoCode)
        {
            var context = ObjectContextHelper.CurrentObjectContext;
            var query = from c in context.Countries
                        where c.ThreeLetterIsoCode == threeLetterIsoCode
                        select c;
            var country = query.FirstOrDefault();
            return country;
        }

        /// <summary>
        /// Inserts a country
        /// </summary>
        /// <param name="country">Country</param>
        public static void InsertCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            country.Name = CommonHelper.EnsureNotNull(country.Name);
            country.Name = CommonHelper.EnsureMaximumLength(country.Name, 100);
            country.TwoLetterIsoCode = CommonHelper.EnsureNotNull(country.TwoLetterIsoCode);
            country.TwoLetterIsoCode = CommonHelper.EnsureMaximumLength(country.TwoLetterIsoCode, 2);
            country.ThreeLetterIsoCode = CommonHelper.EnsureNotNull(country.ThreeLetterIsoCode);
            country.ThreeLetterIsoCode = CommonHelper.EnsureMaximumLength(country.ThreeLetterIsoCode, 3);

            var context = ObjectContextHelper.CurrentObjectContext;

            context.Countries.AddObject(country);
            context.SaveChanges();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(COUNTRIES_PATTERN_KEY);
            }
        }

        /// <summary>
        /// Updates the country
        /// </summary>
        /// <param name="country">Country</param>
        public static void UpdateCountry(Country country)
        {
            if (country == null)
                throw new ArgumentNullException("country");

            country.Name = CommonHelper.EnsureNotNull(country.Name);
            country.Name = CommonHelper.EnsureMaximumLength(country.Name, 100);
            country.TwoLetterIsoCode = CommonHelper.EnsureNotNull(country.TwoLetterIsoCode);
            country.TwoLetterIsoCode = CommonHelper.EnsureMaximumLength(country.TwoLetterIsoCode, 2);
            country.ThreeLetterIsoCode = CommonHelper.EnsureNotNull(country.ThreeLetterIsoCode);
            country.ThreeLetterIsoCode = CommonHelper.EnsureMaximumLength(country.ThreeLetterIsoCode, 3);

            var context = ObjectContextHelper.CurrentObjectContext;
            if (!context.IsAttached(country))
                context.Countries.Attach(country);

            context.SaveChanges();

            if (CountryManager.CacheEnabled)
            {
                NopRequestCache.RemoveByPattern(COUNTRIES_PATTERN_KEY);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether cache is enabled
        /// </summary>
        public static bool CacheEnabled
        {
            get
            {
                return SettingManager.GetSettingValueBoolean("Cache.CountryManager.CacheEnabled");
            }
        }
        #endregion
    }
}