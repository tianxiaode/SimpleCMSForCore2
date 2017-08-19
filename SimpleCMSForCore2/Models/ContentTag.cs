using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleCMSForCore2.Models
{
    [Table("T_ContentTag")]
    public class ContentTag
    {
        public int ContentId { get; set; }
        public Content Content { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
