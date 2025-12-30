using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using EZGO.Api.Controllers.Base;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Security.Interfaces;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Json;
using EZGO.Api.Utils.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EZGO.Api.Controllers.General
{
    [Route(Settings.ApiSettings.VERSION_V1_BASE_API_ROUTE)]
    [ApiController]
    public class MediaController : BaseController<MediaController>
    {
        public readonly IMediaUploader _mediauploader;
        #region - constructor(s) -
        public MediaController(IMediaUploader mediauploader, ILogger<MediaController> logger, IApplicationUser applicationUser, IConfigurationHelper configurationHelper) : base(logger, applicationUser, configurationHelper)
        {
            _mediauploader = mediauploader;
        }
        #endregion


        [Route("media/image/upload/{type}/{itemid}")]
        [RequestSizeLimit(2147483648)] // 2 GB
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file, MediaStorageTypeEnum type, int itemid, [FromQuery] bool includebaseurlonreturn = false)
        {

            if (!FileValidator.CheckImageFormat(file.FileName)) {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_IS_NOT_A_SUPPORTED_IMAGE.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            Agent.Tracer.CurrentTransaction.SetLabel("filename", file.FileName);

            var result = await _mediauploader.UploadFileAsync(file: file, storageType: StorageTypeEnum.AwsS3AndDisk, mediaType: MediaTypeEnum.Image, mediaStorageType: type, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: itemid, includeBaseUrlOnReturn: includebaseurlonreturn);

            if (!string.IsNullOrEmpty(result)) Agent.Tracer.CurrentTransaction.SetLabel("filename done", result);
            Agent.Tracer.CurrentSpan.End();

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.UnprocessableEntity, ("Can not process uploaded file.").ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("media/docs/upload/{type}/{itemid}")]
        [RequestSizeLimit(2147483648)] // 2 GB
        [HttpPost]
        public async Task<IActionResult> UploadDocs(IFormFile file, MediaStorageTypeEnum type, int itemid, [FromQuery] bool includebaseurlonreturn = false)
        {

            if (!FileValidator.CheckDocsFormat(file.FileName))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_IS_NOT_A_SUPPORTED_DOC.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            Agent.Tracer.CurrentTransaction.SetLabel("filename", file.FileName);

            var result = await _mediauploader.UploadFileAsync(file: file, storageType: StorageTypeEnum.AwsS3AndDisk, mediaType: MediaTypeEnum.Docs, mediaStorageType: type, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: itemid, includeBaseUrlOnReturn: includebaseurlonreturn);

            if(!string.IsNullOrEmpty(result)) Agent.Tracer.CurrentTransaction.SetLabel("filename done", result);
            Agent.Tracer.CurrentSpan.End();

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.UnprocessableEntity, ("Can not process uploaded file.").ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("media/video/upload/{type}/{itemid}")]
        [RequestSizeLimit(2147483648)] // 2 GB
        [HttpPost]
        public async Task<IActionResult> UploadVideo(IFormFile file, MediaStorageTypeEnum type, int itemid, [FromQuery] bool includebaseurlonreturn = false)
        {
            if (!FileValidator.CheckVideoFormat(file.FileName))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_IS_NOT_A_SUPPORTED_VIDEO.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            Agent.Tracer.CurrentTransaction.SetLabel("filename", file.FileName);

            var result = await _mediauploader.UploadFileAsync(file: file, storageType: StorageTypeEnum.AwsS3, mediaType: MediaTypeEnum.Video, mediaStorageType: type, companyId:  await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: itemid, includeBaseUrlOnReturn: includebaseurlonreturn);

            if (!string.IsNullOrEmpty(result)) Agent.Tracer.CurrentTransaction.SetLabel("filename done", result);
            Agent.Tracer.CurrentSpan.End();

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.UnprocessableEntity, ("Can not process uploaded file.").ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("media/upload/video")]
        [HttpPost]
        public async Task<IActionResult> AddVideo(IFormFile file)
        {
            var result = "/video/sample";
            await Task.CompletedTask;
            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("media/upload/aws")]
        [RequestSizeLimit(2147483648)] // 2 GB
        [HttpPost]
        public async Task<IActionResult> AddUploadAWS(IFormFile file, [FromQuery] bool includebaseurlonreturn = false)
        {
            if (!FileValidator.CheckImageFormat(file.FileName))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_IS_NOT_A_SUPPORTED_IMAGE.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            Agent.Tracer.CurrentTransaction.SetLabel("filename", file.FileName);

            var result = await _mediauploader.UploadFileAsync(file: file, storageType: StorageTypeEnum.AwsS3, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.Company, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: 0, includeBaseUrlOnReturn: includebaseurlonreturn);

            if (!string.IsNullOrEmpty(result)) Agent.Tracer.CurrentTransaction.SetLabel("filename done", result);
            Agent.Tracer.CurrentSpan.End();

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.UnprocessableEntity, ("Can not process uploaded file.").ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("media/upload/disk")]
        [RequestSizeLimit(2147483648)] // 2 GB
        [HttpPost]
        public async Task<IActionResult> AddUploadDisk(IFormFile file, [FromQuery] bool includebaseurlonreturn = false)
        {
            if (!FileValidator.CheckImageFormat(file.FileName))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_IS_NOT_A_SUPPORTED_IMAGE.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            Agent.Tracer.CurrentTransaction.SetLabel("filename", file.FileName);

            var result = await _mediauploader.UploadFileAsync(file: file, storageType: StorageTypeEnum.Disk, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.Company, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: 0, includeBaseUrlOnReturn: includebaseurlonreturn);

            if (!string.IsNullOrEmpty(result)) Agent.Tracer.CurrentTransaction.SetLabel("filename done", result);
            Agent.Tracer.CurrentSpan.End();

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.UnprocessableEntity, ("Can not process uploaded file.").ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }

        [Route("media/upload/awsdisk")]
        [RequestSizeLimit(2147483648)] // 2 GB
        [HttpPost]
        public async Task<IActionResult> AddUploadAwsDisk(IFormFile file, [FromQuery] bool includebaseurlonreturn = false)
        {
            if (!FileValidator.CheckImageFormat(file.FileName))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, FileValidator.MESSAGE_FILE_IS_NOT_A_SUPPORTED_IMAGE.ToJsonFromObject());
            }

            Agent.Tracer.CurrentTransaction.StartSpan("logic.execution", ApiConstants.ActionExec);
            Agent.Tracer.CurrentTransaction.SetLabel("filename", file.FileName);

            var result = await _mediauploader.UploadFileAsync(file: file, storageType: StorageTypeEnum.AwsS3AndDisk, mediaType: MediaTypeEnum.Image, mediaStorageType: MediaStorageTypeEnum.Company, companyId: await this.CurrentApplicationUser.GetAndSetCompanyIdAsync(), objectId: 0, includeBaseUrlOnReturn: includebaseurlonreturn);

            if (!string.IsNullOrEmpty(result)) Agent.Tracer.CurrentTransaction.SetLabel("filename done", result);
            Agent.Tracer.CurrentSpan.End();

            if (string.IsNullOrEmpty(result))
            {
                return StatusCode((int)HttpStatusCode.UnprocessableEntity, ("Can not process uploaded file.").ToJsonFromObject());
            }

            return StatusCode((int)HttpStatusCode.OK, (result).ToJsonFromObject());
        }
    }
}


