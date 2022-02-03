﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAggregator.API.Core
{
    public interface IEditorService
    {
        public IList<Insert> GetInserts();
        public void SetInserts(IList<Insert> inserts);
    }
}
