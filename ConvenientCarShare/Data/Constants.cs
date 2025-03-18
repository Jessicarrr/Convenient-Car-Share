namespace ConvenientCarShare.Data
{
    public static class Constants
    {
        public const string MessageBroadcast = "Broadcast";
        public const string MessageSingleUser = "SingleUser";
        public const string AdministratorRole = "Administrator";
        public const string CustomerRole = "Customer";

        /// <summary>
        /// The amount of time (in hours) that must be reserved between each booking.
        /// e.g. If this variable == 1, and a booking's end time is at 4:00PM, the next available time
        /// is at 5:00PM.
        /// </summary>
        public const int MinimumTimeBetweenBookings = 1; // 1 hour

        // filter, time, hours, "hours before next booking to fitler out cars"
        /// <summary>
        /// The maximum amount of hours between two bookings before the booked car is filtered
        /// from the map.
        /// </summary>
        public const int HoursBetweenBookingsBeforeFiltering = MinimumTimeBetweenBookings + 1;

        public const string statusBooked = "Booked";
        public const string statusDriving = "Driving";
        public const string statusFinished = "Finished";
        public const string statusCancelled = "Cancelled";





    }
}
