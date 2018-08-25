
namespace CMS_Entity.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CMS_Account
    {
        [StringLength(60)]
        public string Id { get; set; }

        public int Sequence { get; set; }

        public int Status { get; set; }

        [Required]
        [StringLength(100)]
        public string Account { get; set; }       

        [Required]
        [StringLength(250)]
        public string Password { get; set; }

        [StringLength(4000)]
        public string Cookies { get; set; }

        public bool IsActive { get; set; }

        [StringLength(60)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(60)]
        public string UpdatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
