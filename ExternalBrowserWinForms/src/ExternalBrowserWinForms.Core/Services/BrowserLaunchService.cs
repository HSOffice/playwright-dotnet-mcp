using ExternalBrowserWinForms.Core.Models;
using ExternalBrowserWinForms.Core.Validation;

namespace ExternalBrowserWinForms.Core.Services;

/// <summary>
/// Coordinates validation and process launching for browser requests.
/// </summary>
public sealed class BrowserLaunchService : IBrowserLaunchService
{
    private readonly ILaunchRequestValidator _validator;
    private readonly IBrowserProcessRunner _processRunner;

    public BrowserLaunchService(ILaunchRequestValidator validator, IBrowserProcessRunner processRunner)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    }

    public Task<BrowserLaunchResult> LaunchAsync(BrowserLaunchRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validation = _validator.Validate(request);
        if (!validation.IsValid)
        {
            return Task.FromResult(BrowserLaunchResult.Failed(validation.ErrorMessage ?? "请求不合法。"));
        }

        try
        {
            _processRunner.Launch(request);
            return Task.FromResult(BrowserLaunchResult.Successful("浏览器已启动。"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(BrowserLaunchResult.Failed(ex.Message));
        }
    }
}
