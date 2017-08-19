using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SimpleCMSForCore2.Models
{
    [Table("T_UserProfile")]
    public class UserProfile
    {
        [Key]
        public int UserProfileId { get; set; }

        [DefaultValue(1)]
        [Range(1, 3)]
        public byte UserProfileType { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Keyword { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Value { get; set; }

    }

    public enum UserProfileType
    {
        State = 1
    }
}