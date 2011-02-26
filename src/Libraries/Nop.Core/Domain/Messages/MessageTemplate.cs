﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents a message template
    /// </summary>
    public partial class MessageTemplate : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the BCC Email addresses
        /// </summary>
        public string BccEmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the used email account identifier
        /// </summary>
        public int EmailAccountId { get; set; }

        /// <summary>
        /// Gets the email account
        /// </summary>
        public virtual EmailAccount EmailAccount { get; set; }
    }
}
