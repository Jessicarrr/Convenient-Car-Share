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

- **Backend:** ASP.NET with .NET 9.0
- **Database:** SQLite with Entity Framework
- **Authentication:** ASP.NET Identity
- **Frontend:** Google Maps API integration
- **Hosting:** AWS EC2 (with auto scaling) and Route 53 for DNS management
- **CI/CD Pipeline:** GitHub Actions for automated build, test, and deployment
- **SSL/TLS:** Managed via Certbot with Nginx as a reverse proxy

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

4. **Run the Application Locally:**

   You can run the application locally using Visual Studio or via the CLI:

   ```bash
   dotnet run
   ```

## Deployment

### CI/CD Pipeline

The CI/CD pipeline is fully automated using GitHub Actions. It performs the following steps on every push or on-demand (via workflow_dispatch):

- **Build & Test:**  
  The pipeline checks out your code, sets up the .NET environment, restores NuGet packages, builds the solution, and runs all unit tests.

- **Deployment:**  
  After a successful build and test, the pipeline deploys your ASP.NET application to an AWS EC2 instance:
  - **Code Deployment:** The repository is cloned or updated in the target folder (e.g., `/var/app/Convenient-Car-Share/`).
  - **Publishing:** The application is published using `dotnet publish` and deployed to the server.
  - **Service Management:** A systemd service (e.g., `Convenient-Car-Share-Service`) is restarted if available.
  - **Health Check:** The pipeline makes an HTTP request to the health check endpoint (e.g., `http://ccs.jessica-roscher.com/health`) to ensure the deployment succeeded.

### Hosting Environment

- **EC2 & Auto Scaling:**  
  The application is hosted on an AWS EC2 instance that is part of an auto scaling group. A custom AMI is used so that new instances come pre-configured with the deploy user and environment settings.

- **DNS Management:**  
  Domain names and subdomains are managed via AWS Route 53. For example, `ccs.jessica-roscher.com` is configured to route to the EC2 instance.

- **Reverse Proxy & SSL:**  
  Nginx is configured on the EC2 instance to:
  - Redirect HTTP to HTTPS for your primary domains.
  - Proxy incoming HTTPS requests to the correct application port (e.g., port 5001 for the new ASP.NET app).
  - Certbot is used to obtain and renew SSL certificates for the domains.

## Configuration

The application relies on several environment variables and secure settings:

- **Email Credentials:**
  - `EmailSender__UserName`
  - `EmailSender__Password`

These are configured in your deployment environment and managed via GitHub Secrets in the CI/CD pipeline.

- **Port Configuration:**  
  The new ASP.NET app is configured to run on port 5001. Nginx proxies traffic from your domain to this port.

## Usage

1. **Initial Landing:**  
   When you first navigate to the site, you'll see a disclaimer page. Click "Agree and Continue" to proceed.

2. **Dashboard:**  
   Once accepted, you'll see a map dashboard displaying available parking spots.

3. **Registration & Email Verification:**  
   - Click "Register" in the top right corner and fill in your details.
   - Check your email for a verification link, then click it to verify your account.

4. **Booking a Car:**  
   - Click on a parking spot on the map and then click "Book."
   - Enter your booking details (note that credit card info is simulated).

5. **Manage Bookings:**  
   - Use the "Manage Bookings" option to view, extend, or cancel your active bookings.
   - Start your booking by clicking the activation code link sent via email.

## Contributing

This project is primarily for demonstration and portfolio purposes. Contributions are currently not being actively accepted.

## License & Credits

- **License:**  
  This project is not open sourced and is provided "as is" with all rights reserved.
  
- **Credits:**  
  I would like to acknowledge my former university teammates who collaborated on the original project.

## Contact

For any questions or inquiries, please contact me at:  
**Email:** [jwp01993@gmail.com](mailto:jwp01993@gmail.com)
