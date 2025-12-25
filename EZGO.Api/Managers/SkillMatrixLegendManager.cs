using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EZGO.Api.Models.Skills;
using EZGO.Api.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Managers
{
    /// <summary>
    /// Manager for Skills Matrix Legend business logic operations
    /// </summary>
    public class SkillMatrixLegendManager : ISkillMatrixLegendManager
    {
        private readonly ISkillMatrixLegendRepository _repository;
        private readonly ILogger<SkillMatrixLegendManager> _logger;

        // Regex pattern for validating 6-digit HEX color codes
        private static readonly Regex HexColorPattern = new Regex(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public SkillMatrixLegendManager(
            ISkillMatrixLegendRepository repository,
            ILogger<SkillMatrixLegendManager> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> GetConfigurationAsync(int companyId)
        {
            _logger.LogDebug("Getting legend configuration for company {CompanyId}", companyId);

            var configuration = await _repository.GetByCompanyIdAsync(companyId);

            if (configuration == null)
            {
                _logger.LogDebug("No configuration found for company {CompanyId}, returning default", companyId);
                return SkillMatrixLegendConfiguration.CreateDefault(companyId);
            }

            return configuration;
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> SaveConfigurationAsync(
            SkillMatrixLegendConfiguration configuration,
            int userId)
        {
            _logger.LogInformation("Saving legend configuration for company {CompanyId} by user {UserId}",
                configuration.CompanyId, userId);

            var validation = ValidateConfiguration(configuration);
            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    $"Invalid configuration: {string.Join(", ", validation.Errors)}");
            }

            var exists = await _repository.ExistsAsync(configuration.CompanyId);

            if (exists)
            {
                var currentVersion = await _repository.GetVersionAsync(configuration.CompanyId);
                configuration.Version = currentVersion + 1;
                configuration.UpdatedBy = userId;
                return await _repository.UpdateAsync(configuration);
            }
            else
            {
                configuration.Version = 1;
                configuration.CreatedBy = userId;
                return await _repository.CreateAsync(configuration);
            }
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendItem> UpdateItemAsync(int companyId, SkillMatrixLegendItem item)
        {
            _logger.LogInformation("Updating legend item for company {CompanyId}, skill level {SkillLevelId}",
                companyId, item.SkillLevelId);

            var validation = ValidateItem(item);
            if (!validation.IsValid)
            {
                throw new ArgumentException(
                    $"Invalid item: {string.Join(", ", validation.Errors)}");
            }

            // Ensure configuration exists
            var exists = await _repository.ExistsAsync(companyId);
            if (!exists)
            {
                // Create default configuration first
                var defaultConfig = SkillMatrixLegendConfiguration.CreateDefault(companyId);
                await _repository.CreateAsync(defaultConfig);
            }

            return await _repository.UpdateItemAsync(companyId, item);
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> ResetToDefaultAsync(int companyId, int userId)
        {
            _logger.LogInformation("Resetting legend configuration to default for company {CompanyId} by user {UserId}",
                companyId, userId);

            var defaultConfig = SkillMatrixLegendConfiguration.CreateDefault(companyId);

            var exists = await _repository.ExistsAsync(companyId);

            if (exists)
            {
                var currentVersion = await _repository.GetVersionAsync(companyId);
                defaultConfig.Version = currentVersion + 1;
                defaultConfig.UpdatedBy = userId;
                return await _repository.UpdateAsync(defaultConfig);
            }
            else
            {
                defaultConfig.Version = 1;
                defaultConfig.CreatedBy = userId;
                return await _repository.CreateAsync(defaultConfig);
            }
        }

        /// <inheritdoc/>
        public (bool IsValid, string[] Errors) ValidateConfiguration(SkillMatrixLegendConfiguration configuration)
        {
            var errors = new List<string>();

            if (configuration == null)
            {
                return (false, new[] { "Configuration is required" });
            }

            if (configuration.CompanyId <= 0)
            {
                errors.Add("Company ID must be a positive integer");
            }

            // Validate mandatory skills
            if (configuration.MandatorySkills != null)
            {
                var mandatoryOrders = new HashSet<int>();
                var mandatoryIds = new HashSet<int>();

                foreach (var item in configuration.MandatorySkills)
                {
                    var itemValidation = ValidateItem(item);
                    if (!itemValidation.IsValid)
                    {
                        errors.AddRange(itemValidation.Errors);
                    }

                    if (!mandatoryOrders.Add(item.Order))
                    {
                        errors.Add($"Duplicate order value {item.Order} in mandatory skills");
                    }

                    if (!mandatoryIds.Add(item.SkillLevelId))
                    {
                        errors.Add($"Duplicate skill level ID {item.SkillLevelId} in mandatory skills");
                    }
                }
            }

            // Validate operational skills
            if (configuration.OperationalSkills != null)
            {
                var operationalOrders = new HashSet<int>();
                var operationalIds = new HashSet<int>();

                foreach (var item in configuration.OperationalSkills)
                {
                    var itemValidation = ValidateItem(item);
                    if (!itemValidation.IsValid)
                    {
                        errors.AddRange(itemValidation.Errors);
                    }

                    if (!operationalOrders.Add(item.Order))
                    {
                        errors.Add($"Duplicate order value {item.Order} in operational skills");
                    }

                    if (!operationalIds.Add(item.SkillLevelId))
                    {
                        errors.Add($"Duplicate skill level ID {item.SkillLevelId} in operational skills");
                    }
                }
            }

            return (errors.Count == 0, errors.ToArray());
        }

        /// <inheritdoc/>
        public (bool IsValid, string[] Errors) ValidateItem(SkillMatrixLegendItem item)
        {
            var errors = new List<string>();

            if (item == null)
            {
                return (false, new[] { "Legend item is required" });
            }

            // Label validation
            if (string.IsNullOrWhiteSpace(item.Label))
            {
                errors.Add($"Label is required for skill level {item.SkillLevelId}");
            }

            // Icon color validation
            if (!IsValidHexColor(item.IconColor))
            {
                errors.Add($"Invalid icon color format for skill level {item.SkillLevelId}. Must be a valid 6-digit HEX code (e.g., #FF8800)");
            }

            // Background color validation
            if (!IsValidHexColor(item.BackgroundColor))
            {
                errors.Add($"Invalid background color format for skill level {item.SkillLevelId}. Must be a valid 6-digit HEX code (e.g., #FFFFFF)");
            }

            // Order validation
            if (item.Order <= 0)
            {
                errors.Add($"Order must be a positive integer for skill level {item.SkillLevelId}");
            }

            // Skill type validation
            if (string.IsNullOrWhiteSpace(item.SkillType) ||
                (item.SkillType.ToLower() != "mandatory" && item.SkillType.ToLower() != "operational"))
            {
                errors.Add($"Skill type must be 'mandatory' or 'operational' for skill level {item.SkillLevelId}");
            }

            // Score value validation for operational skills
            if (item.SkillType?.ToLower() == "operational" &&
                (!item.ScoreValue.HasValue || item.ScoreValue < 1 || item.ScoreValue > 5))
            {
                errors.Add($"Operational skill level {item.SkillLevelId} must have a score value between 1 and 5");
            }

            return (errors.Count == 0, errors.ToArray());
        }

        /// <summary>
        /// Validates that a string is a valid 6-digit HEX color code
        /// </summary>
        private static bool IsValidHexColor(string color)
        {
            return !string.IsNullOrWhiteSpace(color) && HexColorPattern.IsMatch(color);
        }
    }
}
