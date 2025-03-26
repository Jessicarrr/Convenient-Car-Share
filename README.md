# Convenient Car Share

**Convenient Car Share** is a demo car sharing platform created as my university capstone project. It has since been updated to incorporate improved security practices, bug fixes, and unit testing. The project lets users browse cars on a map, book a car for a fee (with fake payments), and manage their booking through features like cancellation, extension, and early return.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technologies](#technologies)
- [Installation & Setup](#installation--setup)
- [Deployment](#deployment)
- [Configuration](#configuration)
- [Usage](#usage)
- [Contributing](#contributing)
- [License & Credits](#license--credits)
- [Contact](#contact)

## Overview

Convenient Car Share provides users with a demo experience where they can:
- View cars on a map using the Google Maps API.
- Select a car, book it, and pay (all fake transactions).
- Manage bookings including cancelling, extending (1-2 hours), and returning a car.
- Register and verify their account via email.

## Features

- **User Registration & Email Verification:**  
  Users can register, verify their email addresses, and update their profile.
  
- **Booking Management:**  
  The platform supports booking a car, managing active bookings, cancelling, and extending bookings.
  
- **Car Booking Extension:**  
  Users can extend their booking for an additional 1-2 hours.
  
- **Return Process:**  
  Return the car by selecting a parking spot on the map.
  
- **Messaging System:**  
  For demo purposes, the system provides notifications and booking details.
  
> **Note:** All payment information is fake and no real transactions occur. This project is for demonstration and portfolio purposes only.

## Technologies

- **Backend:** ASP.NET with .NET 9.0 (as defined in the csproj)
- **Database:** SQLite with Entity Framework
- **Authentication:** ASP.NET Identity
- **Frontend:** Google Maps API integration
- **Hosting (Future):** AWS EC2 with auto scaling, managed via Route 53
- **CI/CD Pipeline (Future):** GitHub Actions

## Installation & Setup

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) or later
- .NET 9.0
- SQLite (or use the in-memory SQLite database for testing)
- Git

### Steps

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/Jessicarrr/convenient-car-share.git
   cd convenient-car-share
   ```

2. **Restore Dependencies and Build:**

   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run Unit Tests:**

   ```bash
   dotnet test
   ```

4. **Run the Application:**

   You can run the application locally using Visual Studio or via the CLI:

   ```bash
   dotnet run
   ```

## Deployment

### Planned Deployment

- **CI/CD Pipeline:**  
  A CI/CD pipeline will be set up using GitHub Actions to automate building, testing, and deploying the project.

- **Hosting:**  
  The application is planned to be hosted on AWS EC2 (using auto scaling) with Route 53 handling domain name resolution.

### Deployment Environment

- The deployment will utilize environment variables and GitHub Secrets for sensitive configuration, such as:
  - `EmailSender__UserName`
  - `EmailSender__Password`
  
- A new AMI will be created once the CI/CD pipeline is stable, ensuring that auto-scaling instances launch with the latest configuration.

## Configuration

The application relies on certain environment variables for secure operations:

- **Email Credentials:**
  - `EmailSender__UserName`
  - `EmailSender__Password`
  
These should be set in your deployment environment (or via GitHub Secrets in your CI/CD pipeline).

## Usage

1. **Initial Landing:**  
   When you first navigate to the site, you'll see a disclaimer page. Click "Agree and Continue" to proceed.

2. **Dashboard:**  
   Once accepted, you'll see a map dashboard displaying available parking spots.

3. **Registration & Email Verification:**  
   - Click "Register" in the top right corner and fill in your details (license number is optional).
   - Check your email (including spam) for a verification link, then click it to verify your account.

4. **Booking a Car:**  
   - Click on a parking spot on the map and then click "Book."
   - Enter your booking details (credit card info is simulated and not stored).

5. **Manage Bookings:**  
   - Use the "Manage Bookings" option in the navigation bar to view active bookings.
   - Start your booking by clicking the email link containing the activation code.
   - Once active, you can extend or return your booking from the management page.

## Contributing

This project is primarily for portfolio purposes and demonstration. Contributions are not being actively accepted at this time.

## License & Credits

- **License:**  
  This project is not open sourced and is provided "as is" with all rights reserved.
  
- **Credits:**  
  I would like to acknowledge my former university teammates who collaborated on the original project.

## Contact

For any questions or inquiries, please contact me at:  
**Email:** [jwp01993@gmail.com](mailto:jwp01993@gmail.com)
