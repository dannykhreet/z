using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EZGO.Api.Entities
{
    /// <summary>
    /// Database entity for Skills Matrix Legend Item
    /// </summary>
    [Table("SkillMatrixLegendItem")]
    public class SkillMatrixLegendItemEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ConfigurationId { get; set; }

        [Required]
        public int SkillLevelId { get; set; }

        [Required]
        [MaxLength(20)]
        public string SkillType { get; set; }

        [Required]
        [MaxLength(255)]
        public string Label { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [MaxLength(7)]
        public string IconColor { get; set; }

        [Required]
        [MaxLength(7)]
        public string BackgroundColor { get; set; }

        [Required]
        public int Order { get; set; }

        public int? ScoreValue { get; set; }

        [MaxLength(50)]
        public string IconClass { get; set; }

        public bool IsDefault { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property for configuration
        /// </summary>
        [ForeignKey("ConfigurationId")]
        public virtual SkillMatrixLegendConfigurationEntity Configuration { get; set; }
    }
}
