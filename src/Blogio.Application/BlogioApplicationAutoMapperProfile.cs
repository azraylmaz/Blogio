using AutoMapper;
using Blogio.Blog;
using System.Linq;

namespace Blogio;

public class BlogioApplicationAutoMapperProfile : Profile
{
    public BlogioApplicationAutoMapperProfile()
    {
        // ----- Tag -----
        CreateMap<Tag, TagDto>();

        // Create/Update -> Entity: validasyon gevşek (unmapped dest. members sorun olmaz)
        CreateMap<CreateUpdateTagDto, Tag>(MemberList.None);

        // ----- BlogPost -----
        CreateMap<BlogPost, BlogPostListItemDto>()
            .ForMember(d => d.CommentCount, opt => opt.MapFrom(s => s.Comments.Count));

        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(d => d.BlogPostTags, opt => opt.MapFrom(s => s.BlogPostTags.Select(x => x.Tag)))
            .ForMember(d => d.Comments, opt => opt.MapFrom(s => s.Comments));

        CreateMap<Comment, CommentDto>()
            .ForMember(d => d.CreatorUserName, opt => opt.Ignore());

        CreateMap<BlogPostDraft, BlogPostDraftDto>()
                .ForMember(d => d.Tags, cfg => cfg.MapFrom(s => s.Tags.Select(t => t.Tag)));

        CreateMap<BlogPostVersion, BlogPostVersionDto>()
            .ForMember(d => d.Tags, cfg => cfg.MapFrom(s => s.BlogPostVersionTags.Select(t => t.Tag)));
    }
}

