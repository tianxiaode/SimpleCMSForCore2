using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCMSForCore2.Models
{ 

    [Table("T_Tag")]
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        //[Index("IX_TAG_TAGNAME", IsUnique = true)]
        public string Name { get; set; }

        public virtual ICollection<ContentTag> ContentTags { get; set; }
    }
}