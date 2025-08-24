using System;
using System.Collections.Generic;

namespace Blogio.Blazor.Models;

public class PagedResult<T>
{
    public long TotalCount { get; set; }
    public List<T> Items { get; set; } = new();
}

public class GetBlogPostListDto
{
    public string? Filter { get; set; }
    public bool? IsPublished { get; set; }
    public Guid? AuthorId { get; set; }
    public List<Guid>? TagIds { get; set; }
    public DateTime? CreationTimeStart { get; set; }
    public DateTime? CreationTimeEnd { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; } = 10;
    public string? Sorting { get; set; } = "CreationTime desc";
}

public class BlogPostListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsPublished { get; set; }
    public Guid AuthorId { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
}

public class BlogPostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public bool IsPublished { get; set; }
    public Guid AuthorId { get; set; }
    public int LikeCount { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public List<TagDto> BlogPostTags { get; set; } = new();
}

public class CreateUpdateBlogPostDto
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public Guid AuthorId { get; set; }
    public List<Guid> TagIds { get; set; } = new();
    public bool? IsPublished { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = "";
    public Guid BlogPostId { get; set; }
    // Audited alanlar (server JSON'uyla bire bir)
    public Guid? CreatorId { get; set; }
    public DateTime? CreationTime { get; set; }

    // Senin eklediğin ekstra alan
    public string? CreatorUserName { get; set; }
}

public class CreateUpdateCommentDto
{
    public string Text { get; set; } = "";
    public Guid BlogPostId { get; set; }
}

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public class CreateUpdateTagDto
{
    public string Name { get; set; } = "";
}

public class AuthorDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Email { get; set; }
    public int PostCount { get; set; }
}


