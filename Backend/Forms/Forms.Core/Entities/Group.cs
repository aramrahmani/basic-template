﻿using System;
using System.Collections.Generic;

namespace Forms.Core
{
    public class Group
    {
        public string ProjectId { get; set; }
        public int Id { get; set; }

        public string Title { get; set; }

        [IgnoreMap]
        public ICollection<Grid> Tables { get; set; }
    }
}