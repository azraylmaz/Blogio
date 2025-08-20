using Blogio.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Blogio.Blazor.Services;

public class BlogApi
{
    private readonly HttpClient _http;

    public BlogApi(HttpClient http) => _http = http;

    // ---- Blog ----

    public async Task<PagedResult<BlogPostListItemDto>> GetListAsync(GetBlogPostListDto input)
    {
        // ABP paging/sorting konvansiyonu
        var qs = new List<string>
        {
            $"SkipCount={input.SkipCount}",
            $"MaxResultCount={input.MaxResultCount}",
            $"Sorting={Uri.EscapeDataString(input.Sorting ?? "")}"
        };
        if (!string.IsNullOrWhiteSpace(input.Filter)) qs.Add($"Filter={Uri.EscapeDataString(input.Filter)}");
        if (input.IsPublished.HasValue) qs.Add($"IsPublished={input.IsPublished.Value}");
        if (input.AuthorId.HasValue) qs.Add($"AuthorId={input.AuthorId.Value}");
        if (input.CreationTimeStart.HasValue) qs.Add($"CreationTimeStart={input.CreationTimeStart:O}");
        if (input.CreationTimeEnd.HasValue) qs.Add($"CreationTimeEnd={input.CreationTimeEnd:O}");
        if (input.TagIds is { Count: > 0 }) foreach (var t in input.TagIds) qs.Add($"TagIds={t}");

        var url = $"/api/app/blog?{string.Join("&", qs)}";
        return await _http.GetFromJsonAsync<PagedResult<BlogPostListItemDto>>(url)
               ?? new PagedResult<BlogPostListItemDto>();
    }

    public Task<BlogPostDto?> GetAsync(Guid id)
        => _http.GetFromJsonAsync<BlogPostDto>($"/api/app/blog/{id}");

    public async Task<BlogPostDto?> CreateAsync(CreateUpdateBlogPostDto dto)
    {
        var res = await _http.PostAsJsonAsync("/api/app/blog", dto);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<BlogPostDto>();
    }

    public async Task<BlogPostDto?> UpdateAsync(Guid id, CreateUpdateBlogPostDto dto)
    {
        var res = await _http.PutAsJsonAsync($"/api/app/blog/{id}", dto);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<BlogPostDto>();
    }

    public async Task DeleteAsync(Guid id)
    {
        var res = await _http.DeleteAsync($"/api/app/blog/{id}");
        res.EnsureSuccessStatusCode();
    }

    public async Task PublishAsync(Guid id)
        => (await _http.PostAsync($"/api/app/blog/{id}/publish", null)).EnsureSuccessStatusCode();

    public async Task UnpublishAsync(Guid id)
        => (await _http.PostAsync($"/api/app/blog/{id}/unpublish", null)).EnsureSuccessStatusCode();

    public async Task<int> LikeAsync(Guid id)
    {
        var res = await _http.PostAsync($"/api/app/blog/{id}/like", null);
        res.EnsureSuccessStatusCode();

        // Bazı servisler application/json int döner
        if ((res.Content.Headers.ContentType?.MediaType ?? "").Contains("json"))
            return await res.Content.ReadFromJsonAsync<int>();

        // İçerik boşsa sayımı post’u tazeleyerek al
        return (await GetAsync(id))?.LikeCount ?? 0;
    }

    public async Task<int> UnlikeAsync(Guid id)
    {
        var res = await _http.PostAsync($"/api/app/blog/{id}/unlike", null);
        res.EnsureSuccessStatusCode();
        if ((res.Content.Headers.ContentType?.MediaType ?? "").Contains("json"))
            return await res.Content.ReadFromJsonAsync<int>();
        return (await GetAsync(id))?.LikeCount ?? 0;
    }


    // ---- Comments ----

    public async Task<List<CommentDto>> GetCommentsAsync(Guid postId)
    {
        var url = $"/api/app/blog/comments/{postId}?SkipCount=0&MaxResultCount=100";

        // ABP default: ListResultDto
        try
        {
            var list = await _http.GetFromJsonAsync<ListResult<CommentDto>>(url);
            if (list?.Items is { Count: > 0 }) return list.Items;
        }
        catch { /* fallback */ }

        // Yine de farklı dönerse:
        try
        {
            var paged = await _http.GetFromJsonAsync<PagedResult<CommentDto>>(url);
            if (paged?.Items is { Count: > 0 }) return paged.Items;
        }
        catch { }

        try
        {
            return await _http.GetFromJsonAsync<List<CommentDto>>(url) ?? new();
        }
        catch { return new(); }
    }
    public async Task<CommentDto?> AddCommentAsync(CreateUpdateCommentDto dto)
    {
        var res = await _http.PostAsJsonAsync("/api/app/blog/comment", dto);
        res.EnsureSuccessStatusCode();

        if (res.Content == null || (res.Content.Headers.ContentLength ?? 0) == 0)
            return null;

        return await res.Content.ReadFromJsonAsync<CommentDto>();
    }

    public async Task DeleteCommentAsync(Guid id)
        => (await _http.DeleteAsync($"/api/app/blog/comment/{id}")).EnsureSuccessStatusCode();

    // ---- Tags ----

    public async Task<List<TagDto>> GetAllTagsAsync()
    {
        var res = await _http.GetFromJsonAsync<ListResult<TagDto>>("/api/app/blog/tags");
        return res?.Items ?? new List<TagDto>();
    }
    public async Task<TagDto?> CreateTagAsync(CreateUpdateTagDto dto)
    {
        var res = await _http.PostAsJsonAsync("/api/app/blog/tag", dto);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TagDto>();
    }

    public async Task<TagDto?> UpdateTagAsync(Guid id, CreateUpdateTagDto dto)
    {
        var res = await _http.PutAsJsonAsync($"/api/app/blog/{id}/tag", dto);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TagDto>();
    }

    public async Task DeleteTagAsync(Guid id)
    => (await _http.DeleteAsync($"/api/app/blog/{id}/tag")).EnsureSuccessStatusCode();

    // ---- Authors ----

    public async Task<List<AuthorDto>> GetAuthorsAsync()
    {
        var res = await _http.GetFromJsonAsync<ListResult<AuthorDto>>("/api/app/blog/authors");
        return res?.Items ?? new List<AuthorDto>();
    }
}
