using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EZGO.Api.Models.Skills;
using EZGO.Api.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Repository.Implementations
{
    /// <summary>
    /// Repository implementation for Skills Matrix Legend Configuration operations
    /// </summary>
    public class SkillMatrixLegendRepository : ISkillMatrixLegendRepository
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<SkillMatrixLegendRepository> _logger;

        public SkillMatrixLegendRepository(IDbConnection connection, ILogger<SkillMatrixLegendRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> GetByCompanyIdAsync(int companyId)
        {
            try
            {
                const string configSql = @"
                    SELECT Id, CompanyId, Version, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
                    FROM SkillMatrixLegendConfiguration
                    WHERE CompanyId = @CompanyId";

                var configuration = await _connection.QueryFirstOrDefaultAsync<SkillMatrixLegendConfiguration>(
                    configSql, new { CompanyId = companyId });

                if (configuration == null)
                {
                    return null;
                }

                const string itemsSql = @"
                    SELECT Id, ConfigurationId, SkillLevelId, SkillType, Label, Description,
                           IconColor, BackgroundColor, [Order], ScoreValue, IconClass, IsDefault,
                           CreatedAt, UpdatedAt
                    FROM SkillMatrixLegendItem
                    WHERE ConfigurationId = @ConfigurationId
                    ORDER BY SkillType, [Order]";

                var items = await _connection.QueryAsync<SkillMatrixLegendItem>(
                    itemsSql, new { ConfigurationId = configuration.Id });

                var itemsList = items.ToList();
                configuration.MandatorySkills = itemsList.Where(i => i.SkillType == "mandatory").ToList();
                configuration.OperationalSkills = itemsList.Where(i => i.SkillType == "operational").ToList();

                return configuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting legend configuration for company {CompanyId}", companyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> CreateAsync(SkillMatrixLegendConfiguration configuration)
        {
            try
            {
                const string insertConfigSql = @"
                    INSERT INTO SkillMatrixLegendConfiguration
                        (CompanyId, Version, CreatedAt, CreatedBy)
                    VALUES
                        (@CompanyId, @Version, @CreatedAt, @CreatedBy);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                configuration.CreatedAt = DateTime.UtcNow;
                configuration.Id = await _connection.ExecuteScalarAsync<int>(insertConfigSql, new
                {
                    configuration.CompanyId,
                    configuration.Version,
                    configuration.CreatedAt,
                    configuration.CreatedBy
                });

                // Insert all items
                await InsertItemsAsync(configuration);

                return configuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating legend configuration for company {CompanyId}", configuration.CompanyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendConfiguration> UpdateAsync(SkillMatrixLegendConfiguration configuration)
        {
            try
            {
                const string updateConfigSql = @"
                    UPDATE SkillMatrixLegendConfiguration
                    SET Version = @Version,
                        UpdatedAt = @UpdatedAt,
                        UpdatedBy = @UpdatedBy
                    WHERE CompanyId = @CompanyId";

                configuration.UpdatedAt = DateTime.UtcNow;
                await _connection.ExecuteAsync(updateConfigSql, new
                {
                    configuration.CompanyId,
                    configuration.Version,
                    configuration.UpdatedAt,
                    configuration.UpdatedBy
                });

                // Delete existing items and insert new ones
                const string deleteItemsSql = @"
                    DELETE FROM SkillMatrixLegendItem
                    WHERE ConfigurationId = (
                        SELECT Id FROM SkillMatrixLegendConfiguration WHERE CompanyId = @CompanyId
                    )";

                await _connection.ExecuteAsync(deleteItemsSql, new { configuration.CompanyId });

                // Get the configuration ID
                const string getIdSql = "SELECT Id FROM SkillMatrixLegendConfiguration WHERE CompanyId = @CompanyId";
                configuration.Id = await _connection.ExecuteScalarAsync<int>(getIdSql, new { configuration.CompanyId });

                await InsertItemsAsync(configuration);

                return configuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legend configuration for company {CompanyId}", configuration.CompanyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<SkillMatrixLegendItem> UpdateItemAsync(int companyId, SkillMatrixLegendItem item)
        {
            try
            {
                const string updateItemSql = @"
                    UPDATE i
                    SET i.Label = @Label,
                        i.Description = @Description,
                        i.IconColor = @IconColor,
                        i.BackgroundColor = @BackgroundColor,
                        i.[Order] = @Order,
                        i.ScoreValue = @ScoreValue,
                        i.IconClass = @IconClass,
                        i.IsDefault = 0,
                        i.UpdatedAt = @UpdatedAt
                    FROM SkillMatrixLegendItem i
                    INNER JOIN SkillMatrixLegendConfiguration c ON i.ConfigurationId = c.Id
                    WHERE c.CompanyId = @CompanyId
                      AND i.SkillLevelId = @SkillLevelId
                      AND i.SkillType = @SkillType";

                item.UpdatedAt = DateTime.UtcNow;
                item.IsDefault = false;

                await _connection.ExecuteAsync(updateItemSql, new
                {
                    CompanyId = companyId,
                    item.SkillLevelId,
                    item.SkillType,
                    item.Label,
                    item.Description,
                    item.IconColor,
                    item.BackgroundColor,
                    item.Order,
                    item.ScoreValue,
                    item.IconClass,
                    item.UpdatedAt
                });

                // Update configuration version
                const string updateVersionSql = @"
                    UPDATE SkillMatrixLegendConfiguration
                    SET Version = Version + 1, UpdatedAt = @UpdatedAt
                    WHERE CompanyId = @CompanyId";

                await _connection.ExecuteAsync(updateVersionSql, new { CompanyId = companyId, item.UpdatedAt });

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legend item for company {CompanyId}", companyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByCompanyIdAsync(int companyId)
        {
            try
            {
                const string deleteItemsSql = @"
                    DELETE FROM SkillMatrixLegendItem
                    WHERE ConfigurationId = (
                        SELECT Id FROM SkillMatrixLegendConfiguration WHERE CompanyId = @CompanyId
                    )";

                const string deleteConfigSql = @"
                    DELETE FROM SkillMatrixLegendConfiguration
                    WHERE CompanyId = @CompanyId";

                await _connection.ExecuteAsync(deleteItemsSql, new { CompanyId = companyId });
                var rows = await _connection.ExecuteAsync(deleteConfigSql, new { CompanyId = companyId });

                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting legend configuration for company {CompanyId}", companyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(int companyId)
        {
            const string sql = "SELECT COUNT(1) FROM SkillMatrixLegendConfiguration WHERE CompanyId = @CompanyId";
            var count = await _connection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
            return count > 0;
        }

        /// <inheritdoc/>
        public async Task<int> GetVersionAsync(int companyId)
        {
            const string sql = "SELECT ISNULL(Version, 0) FROM SkillMatrixLegendConfiguration WHERE CompanyId = @CompanyId";
            return await _connection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
        }

        /// <summary>
        /// Inserts legend items for a configuration
        /// </summary>
        private async Task InsertItemsAsync(SkillMatrixLegendConfiguration configuration)
        {
            const string insertItemSql = @"
                INSERT INTO SkillMatrixLegendItem
                    (ConfigurationId, SkillLevelId, SkillType, Label, Description,
                     IconColor, BackgroundColor, [Order], ScoreValue, IconClass, IsDefault, CreatedAt)
                VALUES
                    (@ConfigurationId, @SkillLevelId, @SkillType, @Label, @Description,
                     @IconColor, @BackgroundColor, @Order, @ScoreValue, @IconClass, @IsDefault, @CreatedAt)";

            var allItems = new List<SkillMatrixLegendItem>();
            if (configuration.MandatorySkills != null)
            {
                allItems.AddRange(configuration.MandatorySkills);
            }
            if (configuration.OperationalSkills != null)
            {
                allItems.AddRange(configuration.OperationalSkills);
            }

            foreach (var item in allItems)
            {
                item.ConfigurationId = configuration.Id;
                item.CreatedAt = DateTime.UtcNow;

                await _connection.ExecuteAsync(insertItemSql, new
                {
                    item.ConfigurationId,
                    item.SkillLevelId,
                    item.SkillType,
                    item.Label,
                    item.Description,
                    item.IconColor,
                    item.BackgroundColor,
                    item.Order,
                    item.ScoreValue,
                    item.IconClass,
                    item.IsDefault,
                    item.CreatedAt
                });
            }
        }
    }
}
