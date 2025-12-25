using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EZGO.Api.Entities
{
    /// <summary>
    /// Database entity for Skills Matrix Legend Item
    /// Uses PostgreSQL naming conventions (snake_case)
    /// </summary>
    [Table("skill_matrix_legend_item")]
    public class SkillMatrixLegendItemEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("configuration_id")]
        public int ConfigurationId { get; set; }

        [Required]
        [Column("skill_level_id")]
        public int SkillLevelId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("skill_type")]
        public string SkillType { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("label")]
        public string Label { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [MaxLength(7)]
        [Column("icon_color")]
        public string IconColor { get; set; }

        [Required]
        [MaxLength(7)]
        [Column("background_color")]
        public string BackgroundColor { get; set; }

        [Required]
        [Column("sort_order")]
        public int Order { get; set; }

        [Column("score_value")]
        public int? ScoreValue { get; set; }

        [MaxLength(50)]
        [Column("icon_class")]
        public string IconClass { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property for configuration
        /// </summary>
        [ForeignKey("ConfigurationId")]
        public virtual SkillMatrixLegendConfigurationEntity Configuration { get; set; }
    }
}
