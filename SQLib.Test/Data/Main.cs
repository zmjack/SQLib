using NStandard;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SQLib.Test.Data
{
    public class Main
    {
        public DateTime CreationTime { get; set; }
        public int Integer { get; set; }
        public double Real { get; set; }

        [Column("Text")]
        public string Plain { get; set; }
    }
}
