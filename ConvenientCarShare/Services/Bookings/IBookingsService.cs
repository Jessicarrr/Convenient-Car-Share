using System;
using System.Threading.Tasks;
using ConvenientCarShare.Areas.Identity.Data;
using ConvenientCarShare.Models;
using ConvenientCarShare.Views.Customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ConvenientCarShare.Services
{
    public interface IBookingsService
    {
        Task<ManageTrips> CancelBookingAsync(int cancelBookingId, ApplicationUser currentUser);
        Task<ManageTrips> ResendActivationCodeAsync(int bookingId, ApplicationUser currentUser, IUrlHelper urlHelper, string scheme);
        Task<ManageTrips> GetManageTripsModelAsync(ApplicationUser currentUser);
        Task<BookModel> GetBookModelAsync(int carId, string startTime, string endTime, ITempDataDictionary tempData);
        Task<PaymentModel> GetPaymentModelAsync(int carId, DateTime startDate, DateTime endDate, decimal price, ApplicationUser currentUser, ITempDataDictionary tempData);
        Task<PaymentModel> SubmitPaymentAsync(
            int carId, decimal price, DateTime startDate, DateTime endDate,
            string fullName, string creditCardNumber, string cvv, DateTime expiryDate,
            ApplicationUser currentUser, IUrlHelper urlHelper, string scheme, ITempDataDictionary tempData);
    }
}
