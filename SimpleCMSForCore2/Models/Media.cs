using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using SimpleCMSForCore2.Validations;

namespace SimpleCMSForCore2.Models
{
    [Table("T_Media")]
    public class Media
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string Filename { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [MaxLength(10)]
        public string Path { get; set; }

        [Required]
        [Range(1, 3)]
        [DefaultValue(1)]
        public byte Type { get; set; }

        [Required]
        [DefaultDateTimeValue("Now")]
        public DateTime? Uploaded { get; set; }

        [Required]
        [DefaultValue(0)]
        public int Size { get; set; }

    }

    public enum MediaType
    {
        Image = 1,
        Audio = 2,
        Video = 3
    }

}