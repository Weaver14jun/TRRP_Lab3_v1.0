using System;
using System.Collections.Generic;
using System.Text;

namespace Normalizer
{
    public class PCNormModel
    {
        public int Id { get; set; }
        public int RamId { get; set; }
        public int ProcId { get; set; }
        public int MotherboardId { get; set; }
        public int GraphCardId { get; set; }
        public int OSId { get; set; }
    }
}
