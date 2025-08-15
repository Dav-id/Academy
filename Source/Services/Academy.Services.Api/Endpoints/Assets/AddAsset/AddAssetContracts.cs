using Academy.Shared.Localisation;

using FluentValidation;

namespace Academy.Services.Api.Endpoints.Assets.AddAsset
{
    public static class AddAssetContracts
    {
        public record AddAssetRequest(IFormFile File);

        public record AddAssetResponse(Guid Id,
                                       string Path);
    }

    public sealed class AddAssetValidator : AbstractValidator<AddAssetContracts.AddAssetRequest>
    {
        public AddAssetValidator()
        {
            RuleFor(x => x.File)
                .NotEmpty().WithMessage(_ => ModelTranslation.Global__Field__Required);
        }
    }
}
