﻿using System.Collections;

namespace Nop.Web.Framework.Kendoui
{
    public class DataSourceResult
    {
        public IEnumerable Data { get; set; }

        public object Errors { get; set; }

        public int Total { get; set; }
    }
}
