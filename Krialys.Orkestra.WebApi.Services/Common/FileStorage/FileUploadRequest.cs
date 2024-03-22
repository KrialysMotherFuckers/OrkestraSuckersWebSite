using Krialys.Common.Interfaces;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.WebApi.Infrastructure.Validation;

namespace Krialys.Orkestra.WebApi.Infrastructure.Common.FileStorage;

public class FileUploadRequest
{
    public string Name { get; set; } = default!;
    public string Extension { get; set; } = default!;
    public string Data { get; set; } = default!;
}

public interface IFileStorageService : ITransientService
{
    public Task<string> UploadAsync<T>(FileUploadRequest request, FileType supportedFileType, CancellationToken cancellationToken = default)
    where T : class;

    public void Remove(string path);
}

public class FileUploadRequestValidator : CustomValidator<FileUploadRequest>
{
    public FileUploadRequestValidator(IStringLocalizer<FileUploadRequestValidator> T)
    {
        RuleFor(p => p.Name)
            .NotEmpty()
                .WithMessage(T["Image Name cannot be empty!"])
            .MaximumLength(150);

        RuleFor(p => p.Extension)
            .NotEmpty()
                .WithMessage(T["Image Extension cannot be empty!"])
            .MaximumLength(5);

        RuleFor(p => p.Data)
            .NotEmpty()
                .WithMessage(T["Image Data cannot be empty!"]);
    }
}