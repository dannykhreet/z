using System;
using System.Threading.Tasks;
using EZGO.Api.Managers;
using EZGO.Api.Models.Skills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.V1
{
    /// <summary>
    /// API Controller for Skills Matrix Legend Configuration management
    /// </summary>
    [ApiController]
    [Route("v1/company/{companyId}/skillmatrixlegend")]
    [Authorize]
    public class SkillMatrixLegendController : ControllerBase
    {
        private readonly ISkillMatrixLegendManager _legendManager;
        private readonly ILogger<SkillMatrixLegendController> _logger;

        public SkillMatrixLegendController(
            ISkillMatrixLegendManager legendManager,
            ILogger<SkillMatrixLegendController> logger)
        {
            _legendManager = legendManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets the Skills Matrix Legend configuration for a company
        /// Returns default configuration if none exists
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>The legend configuration</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SkillMatrixLegendConfiguration), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetConfiguration(int companyId)
        {
            try
            {
                // Verify user has access to this company
                if (!HasCompanyAccess(companyId))
                {
                    return Forbid();
                }

                var configuration = await _legendManager.GetConfigurationAsync(companyId);
                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting legend configuration for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "An error occurred while retrieving the configuration" });
            }
        }

        /// <summary>
        /// Saves or updates the Skills Matrix Legend configuration for a company
        /// Only team leaders and managers can edit
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="configuration">The configuration to save</param>
        /// <returns>The saved configuration with updated version</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SkillMatrixLegendConfiguration), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SaveConfiguration(int companyId, [FromBody] SkillMatrixLegendConfiguration configuration)
        {
            try
            {
                // Verify user has access to this company
                if (!HasCompanyAccess(companyId))
                {
                    return Forbid();
                }

                // Verify user has permission to edit (team leader or manager)
                if (!CanEditLegend())
                {
                    return StatusCode(403, new { error = "Only team leaders and managers can edit the legend configuration" });
                }

                // Validate configuration
                var validation = _legendManager.ValidateConfiguration(configuration);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                configuration.CompanyId = companyId;
                var userId = GetCurrentUserId();

                var savedConfig = await _legendManager.SaveConfigurationAsync(configuration, userId);
                return Ok(savedConfig);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error saving legend configuration for company {CompanyId}", companyId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving legend configuration for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "An error occurred while saving the configuration" });
            }
        }

        /// <summary>
        /// Updates a single legend item
        /// Only team leaders and managers can edit
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="item">The legend item to update</param>
        /// <returns>The updated item</returns>
        [HttpPost("item")]
        [ProducesResponseType(typeof(SkillMatrixLegendItem), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateItem(int companyId, [FromBody] SkillMatrixLegendItem item)
        {
            try
            {
                // Verify user has access to this company
                if (!HasCompanyAccess(companyId))
                {
                    return Forbid();
                }

                // Verify user has permission to edit (team leader or manager)
                if (!CanEditLegend())
                {
                    return StatusCode(403, new { error = "Only team leaders and managers can edit the legend configuration" });
                }

                // Validate item
                var validation = _legendManager.ValidateItem(item);
                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var updatedItem = await _legendManager.UpdateItemAsync(companyId, item);
                return Ok(updatedItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating legend item for company {CompanyId}", companyId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating legend item for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "An error occurred while updating the item" });
            }
        }

        /// <summary>
        /// Resets the legend configuration to default values
        /// Only team leaders and managers can reset
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>The reset configuration</returns>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(SkillMatrixLegendConfiguration), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ResetConfiguration(int companyId)
        {
            try
            {
                // Verify user has access to this company
                if (!HasCompanyAccess(companyId))
                {
                    return Forbid();
                }

                // Verify user has permission to edit (team leader or manager)
                if (!CanEditLegend())
                {
                    return StatusCode(403, new { error = "Only team leaders and managers can reset the legend configuration" });
                }

                var userId = GetCurrentUserId();
                var configuration = await _legendManager.ResetToDefaultAsync(companyId, userId);
                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting legend configuration for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "An error occurred while resetting the configuration" });
            }
        }

        /// <summary>
        /// Deletes the legend configuration for a company (restores to system default)
        /// Only team leaders and managers can delete
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>Success status</returns>
        [HttpDelete]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteConfiguration(int companyId)
        {
            try
            {
                // Verify user has access to this company
                if (!HasCompanyAccess(companyId))
                {
                    return Forbid();
                }

                // Verify user has permission to edit (team leader or manager)
                if (!CanEditLegend())
                {
                    return StatusCode(403, new { error = "Only team leaders and managers can delete the legend configuration" });
                }

                var userId = GetCurrentUserId();

                // Reset to default is equivalent to delete since we return default when no config exists
                await _legendManager.ResetToDefaultAsync(companyId, userId);
                return Ok(new { success = true, message = "Configuration reset to default" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting legend configuration for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "An error occurred while deleting the configuration" });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Checks if the current user has access to the specified company
        /// </summary>
        private bool HasCompanyAccess(int companyId)
        {
            // TODO: Implement actual company access check based on user claims/profile
            // This should verify the user belongs to or has access to the company
            var userCompanyId = GetUserCompanyId();
            return userCompanyId == companyId;
        }

        /// <summary>
        /// Checks if the current user can edit the legend (team leader or manager)
        /// </summary>
        private bool CanEditLegend()
        {
            // TODO: Implement actual role check based on user claims/profile
            // Should check if user role is "manager" or "teamleader"
            var userRole = GetUserRole()?.ToLower();
            return userRole == "manager" || userRole == "teamleader";
        }

        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        private int GetCurrentUserId()
        {
            // TODO: Implement actual user ID retrieval from claims
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Gets the current user's company ID from claims
        /// </summary>
        private int GetUserCompanyId()
        {
            // TODO: Implement actual company ID retrieval from claims
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            return int.TryParse(companyIdClaim, out var companyId) ? companyId : 0;
        }

        /// <summary>
        /// Gets the current user's role from claims
        /// </summary>
        private string GetUserRole()
        {
            // TODO: Implement actual role retrieval from claims
            return User.FindFirst("role")?.Value;
        }

        #endregion
    }
}
