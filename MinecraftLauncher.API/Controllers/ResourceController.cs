using Microsoft.AspNetCore.Mvc;
using MinecraftLauncher.Core.Domain.Entities;
using MinecraftLauncher.Core.Domain.Enums;
using MinecraftLauncher.Core.Domain.Interfaces;
using MinecraftLauncher.Core.DTOs.Common;
using MinecraftLauncher.Core.DTOs.Resource;

namespace MinecraftLauncher.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly IResourceService _resourceService;
    private readonly IResourceUploadService _uploadService;
    private readonly IDownloadService _downloadService;

    public ResourceController(
        IResourceService resourceService,
        IResourceUploadService uploadService,
        IDownloadService downloadService)
    {
        _resourceService = resourceService;
        _uploadService = uploadService;
        _downloadService = downloadService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ResourceListResult>>> GetResources(
        [FromQuery] ResourceType? type,
        [FromQuery] string? gameVersion,
        [FromQuery] LoaderType? loader,
        [FromQuery] string? keyword,
        [FromQuery] SortBy sortBy = SortBy.Newest,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ResourceBrowseQuery
        {
            Type = type,
            GameVersion = gameVersion,
            Loader = loader,
            Keyword = keyword,
            SortBy = sortBy,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _resourceService.GetResources(query);
        return Ok(ApiResponse<ResourceListResult>.Ok(result));
    }

    [HttpGet("{resourceId}")]
    public async Task<ActionResult<ApiResponse<ResourceDetail>>> GetResourceDetail(Guid resourceId)
    {
        try
        {
            var result = await _resourceService.GetResourceDetail(resourceId);
            return Ok(ApiResponse<ResourceDetail>.Ok(result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<ResourceDetail>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> CreateResource([FromBody] CreateResourceRequest request)
    {
        try
        {
            var resource = await _resourceService.CreateResource(request, Guid.Empty);
            return Ok(ApiResponse.Ok("资源创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{resourceId}")]
    public async Task<ActionResult<ApiResponse>> UpdateResource(Guid resourceId, [FromBody] UpdateResourceRequest request)
    {
        try
        {
            var resource = await _resourceService.UpdateResource(resourceId, request, Guid.Empty);
            return Ok(ApiResponse.Ok("资源更新成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("{resourceId}")]
    public async Task<ActionResult<ApiResponse>> DeleteResource(Guid resourceId)
    {
        var success = await _resourceService.DeleteResource(resourceId, Guid.Empty);

        if (!success)
        {
            return NotFound(ApiResponse.Fail("资源不存在"));
        }

        return Ok(ApiResponse.Ok("资源删除成功"));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Resource>>>> GetMyResources()
    {
        var resources = await _resourceService.GetMyResources(Guid.Empty);
        return Ok(ApiResponse<IEnumerable<Resource>>.Ok(resources));
    }

    [HttpPost("upload/init")]
    public async Task<ActionResult<ApiResponse<UploadInitResult>>> InitializeUpload([FromBody] UploadRequest request)
    {
        var result = await _uploadService.InitializeUpload(request);
        return Ok(ApiResponse<UploadInitResult>.Ok(result));
    }

    [HttpGet("upload/{uploadId}/progress")]
    public async Task<ActionResult<ApiResponse<UploadProgress>>> GetUploadProgress(string uploadId)
    {
        var progress = await _uploadService.GetUploadProgress(uploadId);
        return Ok(ApiResponse<UploadProgress>.Ok(progress));
    }

    [HttpPost("upload/{uploadId}/complete")]
    public async Task<ActionResult<ApiResponse<UploadCompleteResult>>> CompleteUpload(string uploadId)
    {
        var result = await _uploadService.CompleteUpload(uploadId);
        return Ok(ApiResponse<UploadCompleteResult>.Ok(result));
    }

    [HttpPost("upload/{uploadId}/cancel")]
    public async Task<ActionResult<ApiResponse>> CancelUpload(string uploadId)
    {
        var success = await _uploadService.CancelUpload(uploadId);
        return Ok(ApiResponse.Ok("上传已取消"));
    }

    [HttpGet("{resourceId}/download")]
    public async Task<ActionResult<ApiResponse<DownloadInfo>>> GetDownloadInfo(Guid resourceId)
    {
        var info = await _downloadService.GetDownloadInfo(resourceId);
        return Ok(ApiResponse<DownloadInfo>.Ok(info));
    }

    [HttpPost("{resourceId}/download")]
    public async Task<ActionResult<ApiResponse<DownloadResult>>> DownloadResource(
        Guid resourceId,
        [FromQuery] string targetPath)
    {
        var result = await _downloadService.DownloadResource(resourceId, Guid.Empty, targetPath);
        return Ok(ApiResponse<DownloadResult>.Ok(result));
    }

    [HttpPost("{resourceId}/install")]
    public async Task<ActionResult<ApiResponse<InstallResult>>> InstallResource(
        Guid resourceId,
        [FromQuery] string gameRootPath)
    {
        var result = await _downloadService.InstallResource(resourceId, Guid.Empty, gameRootPath);
        return Ok(ApiResponse<InstallResult>.Ok(result));
    }
}
