using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;
using ConvenientCarShare.Services.Email;

/// <summary>
///  This service is purely for checking if email services
///  are valid on startup and erroring out if not.
///  Valid meaning whether environmental variables (OS level)
///  are set for email username and password.
/// </summary>
public class OptionsValidationHostedService : IHostedService
{
    private readonly IOptions<EmailSenderOptions> _emailOptions;

    public OptionsValidationHostedService(IOptions<EmailSenderOptions> emailOptions)
    {
        _emailOptions = emailOptions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Accessing _emailOptions.Value will trigger the validation
        var options = _emailOptions.Value;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}