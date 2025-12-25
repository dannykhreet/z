using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EZGO.Api.Entities
{
    /// <summary>
    /// Database entity for Skills Matrix Legend Configuration
    /// Uses PostgreSQL naming conventions (snake_case)
    /// </summary>
    [Table("skill_matrix_legend_configuration")]
    public class SkillMatrixLegendConfigurationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [Column("version")]
        public int Version { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
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
