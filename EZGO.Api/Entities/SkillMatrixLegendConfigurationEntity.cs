using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EZGO.Api.Entities
{
    /// <summary>
    /// Database entity for Skills Matrix Legend Configuration
    /// </summary>
    [Table("SkillMatrixLegendConfiguration")]
    public class SkillMatrixLegendConfigurationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int Version { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Navigation property for legend items
        /// </summary>
        public virtual ICollection<SkillMatrixLegendItemEntity> LegendItems { get; set; }

        public SkillMatrixLegendConfigurationEntity()
        {
            LegendItems = new List<SkillMatrixLegendItemEntity>();
        }
    }
}
