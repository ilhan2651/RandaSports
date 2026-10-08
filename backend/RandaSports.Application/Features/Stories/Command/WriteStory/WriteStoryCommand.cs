using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Stories.Command.WriteStory;

/// <summary>Konunun tüm kaynaklarından tek bir haber metni yazdırır.</summary>
public sealed record WriteStoryCommand(Guid StoryId) : IRequest<Result<bool>>;
