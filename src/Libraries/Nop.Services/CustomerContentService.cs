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
using System.Linq;
using Nop.Data;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core;

namespace Nop.Services
{
    /// <summary>
    /// Customer content service
    /// </summary>
    public partial class CustomerContentService : ICustomerContentService
    {
        #region Fields

        private readonly IRepository<CustomerContent> _contentRespository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="contentRespository">Customer content repository</param>
        public CustomerContentService(ICacheManager cacheManager,
            IRepository<CustomerContent> contentRespository)
        {
            this._cacheManager = cacheManager;
            this._contentRespository = contentRespository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a customer content
        /// </summary>
        /// <param name="content">Customer content</param>
        public void DeleteLanguage(CustomerContent content)
        {
            if (content == null)
                return;

            _contentRespository.Delete(content);
        }

        /// <summary>
        /// Gets all customer content
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param>
        /// <returns>Customer content</returns>
        public List<CustomerContent> GetAllCustomerContent(int customerId, bool? approved)
        {
            var query = from c in _contentRespository.Table
                        orderby c.CreatedOnUtc
                        where !approved.HasValue || c.IsApproved == approved &&
                        (customerId == 0 || c.CustomerId == customerId)
                        select c;
            var content = query.ToList();
            return content;
        }

        /// <summary>
        /// Gets all customer content
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param>
        /// <returns>Customer content</returns>
        public List<T> GetAllCustomerContent<T>(int customerId, bool? approved) where T: CustomerContent
        {
            var query = from c in _contentRespository.Table
                        orderby c.CreatedOnUtc
                        where !approved.HasValue || c.IsApproved == approved &&
                        (customerId == 0 || c.CustomerId == customerId)
                        select c;
            var content = query.OfType<T>().ToList();
            return content;
        }

        /// <summary>
        /// Gets a customer content
        /// </summary>
        /// <param name="contentId">Customer content identifier</param>
        /// <returns>Customer content</returns>
        public CustomerContent GetCustomerContentById(int contentId)
        {
            if (contentId == 0)
                return null;

            return _contentRespository.GetById(contentId);
                                          
        }

        /// <summary>
        /// Inserts a customer content
        /// </summary>
        /// <param name="content">Customer content</param>
        public void InsertCustomerContent(CustomerContent content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            _contentRespository.Insert(content);
        }

        /// <summary>
        /// Updates a customer content
        /// </summary>
        /// <param name="content">Customer content</param>
        public void UpdateCustomerContente(CustomerContent content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            _contentRespository.Update(content);
        }

        #endregion
    }
}
