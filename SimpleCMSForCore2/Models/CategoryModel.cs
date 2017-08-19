﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SimpleCMSForCore2.LocalResources;

namespace SimpleCMSForCore2.Models
{
    public class CategoryModel
    {
        public int? Id { get; set; }

        [Display(Name = "CategoryParentId", ResourceType = typeof(Message))]
        public int? ParentId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Message))]
        [Display(Name = "CategoryTitle", ResourceType = typeof(Message))]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(255)]
        [Display(Name = "CategoryImage", ResourceType = typeof(Message))]
        public string Image { get; set; }

        [MaxLength(4000)]
        [Display(Name = "CategoryContent", ResourceType = typeof(Message))]
        public string Content { get; set; }

        [DefaultValue(0)]
        [Display(Name = "CategorySortOrder", ResourceType = typeof(Message))]
        public int SortOrder { get; set; }
    }
}