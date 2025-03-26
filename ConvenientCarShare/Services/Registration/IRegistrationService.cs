using ConvenientCarShare.DataTransferObjects;
using System.Threading.Tasks;

namespace ConvenientCarShare.Services.Registration
{
    public interface IRegistrationService
    {
        Task<RegistrationResult> RegisterUserAsync(RegistrationDto registrationDto);
    }

}
