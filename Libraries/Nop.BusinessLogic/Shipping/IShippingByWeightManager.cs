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
using NopSolutions.NopCommerce.BusinessLogic.IoC;

namespace NopSolutions.NopCommerce.BusinessLogic.Shipping
{
    /// <summary>
    /// "ShippingByWeight" manager interface
    /// </summary>
    public partial interface IShippingByWeightManager
    {
        /// <summary>
        /// Gets a ShippingByWeight
        /// </summary>
        /// <param name="shippingByWeightId">ShippingByWeight identifier</param>
        /// <returns>ShippingByWeight</returns>
        ShippingByWeight GetById(int shippingByWeightId);

        /// <summary>
        /// Deletes a ShippingByWeight
        /// </summary>
        /// <param name="shippingByWeightId">ShippingByWeight identifier</param>
        void DeleteShippingByWeight(int shippingByWeightId);

        /// <summary>
        /// Gets all ShippingByWeights
        /// </summary>
        /// <returns>ShippingByWeight collection</returns>
        List<ShippingByWeight> GetAll();

        /// <summary>
        /// Inserts a ShippingByWeight
        /// </summary>
        /// <param name="shippingByWeight">ShippingByWeight</param>
        void InsertShippingByWeight(ShippingByWeight shippingByWeight);

        /// <summary>
        /// Updates the ShippingByWeight
        /// </summary>
        /// <param name="shippingByWeight">ShippingByWeight</param>
        void UpdateShippingByWeight(ShippingByWeight shippingByWeight);

        /// <summary>
        /// Gets all ShippingByWeights by shipping method identifier
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>ShippingByWeight collection</returns>
        List<ShippingByWeight> GetAllByShippingMethodId(int shippingMethodId);

         /// <summary>
        /// Gets or sets a value indicating whether to calculate per weight unit (e.g. per lb)
        /// </summary>
        bool CalculatePerWeightUnit { get; set; }
    }
}
