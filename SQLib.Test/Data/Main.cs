using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLib.Test.Data
{
    public class Main
    {
        public DateTime CreationTime { get; set; }
        public int Integer { get; set; }
        public double Real { get; set; }

        [Column("Text")]
        public string Plain { get; set; }

        public byte[] Blob { get; set; }
    }
}
