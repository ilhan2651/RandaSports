namespace RandaSports.Application.Common.Filtering;

public interface IArticleFilter
{
    bool ShouldSkip(string title, string url, out string reason);
}
