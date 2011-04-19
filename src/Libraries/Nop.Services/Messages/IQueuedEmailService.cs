﻿using System;
using System.Collections.Generic;

using Nop.Core.Domain.Messages;
using Nop.Core;

namespace Nop.Services.Messages
{
    public partial interface IQueuedEmailService
    {
        /// <summary>
        /// Inserts a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        void InsertQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Updates a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        void UpdateQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="queuedEmail">Queued email</param>
        void DeleteQueuedEmail(QueuedEmail queuedEmail);

        /// <summary>
        /// Gets a queued email by identifier
        /// </summary>
        /// <param name="queuedEmailId">Queued email identifier</param>
        /// <returns>Queued email</returns>
        QueuedEmail GetQueuedEmailById(int queuedEmailId);

        /// <summary>
        /// Gets all queued emails
        /// </summary>
        /// <param name="queuedEmailCount">Email item count. 0 if you want to get all items</param>
        /// <param name="loadNotSentItemsOnly">A value indicating whether to load only not sent emails</param>
        /// <param name="maxSendTries">Maximum send tries</param>
        /// <returns>Queued email collection</returns>
        IList<QueuedEmail> GetAllQueuedEmails(int queuedEmailCount,
            bool loadNotSentItemsOnly, int maxSendTries);

        /// <summary>
        /// Search queued emails
        /// </summary>
        /// <param name="fromEmail">From Email</param>
        /// <param name="toEmail">To Email</param>
        /// <param name="startTime">The start time</param>
        /// <param name="endTime">The end time</param>
        /// <param name="queuedEmailCount">Email item count. 0 if you want to get all items</param>
        /// <param name="loadNotSentItemsOnly">A value indicating whether to load only not sent emails</param>
        /// <param name="maxSendTries">Maximum send tries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Email item collection</returns>
        IPagedList<QueuedEmail> SearchEmails(string fromEmail,
            string toEmail, DateTime? startTime, DateTime? endTime,
            int queuedEmailCount, bool loadNotSentItemsOnly, int maxSendTries, int pageIndex, int pageSize);
    }
}
