﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestWebAppCoolName.Models
{
    public class ImageFile
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
    }
}