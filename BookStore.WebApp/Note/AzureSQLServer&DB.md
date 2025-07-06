# Deploy the application to Azure App Service
## 1. Create Azure SQL Database
- Get in the Azure Portal and login with your account.
- Create a new SQL Database.
- In the configuration, set the server name, admin login and password.
- Set SQL server firewall to allow Azure services and resources to access the server.
## 2. Create Azure App Service
- Create a new App Service.
- In the configuration, set the runtime stack to .NET 8.0
- Set the operating system to Windows
- Set the region to the same region as the SQL Database
- Windows plan: use the suggested plan
- Pricing plan: Free F1: 60 CPU minutes/day included
=> Next: Deployment 
- Enable continuous deployment and select the GitHub repository. This helps to automatically deploy the application when changes are pushed to the repository which known as CI/CD.
- Select the branch to deploy from. We should have a branch that is used for deployment only for example main or production.
=> Next: Networking
- Set the Enable public access to On
- This will allow the application to be accessed from the internet.
=> Next: Monitoring
- Set the Application Insights to On
- This will enable monitoring of the application which helps to identify issues and improve the application.
- It will detect performance anomalies and provide insights into the application.
=> Review + create
- Details
Subscription - 2f2fa104-41c6-4c8c-8094-5fd27591e3f1
Resource Group - asp.net-LiveProject
Name - asptestapp-hnv
Publish - Code
Runtime stack - .NET 8 (LTS)

- App Service Plan (New)
Name - ASP-aspnetLiveProject-af1e
Operating System - Windows
Region - East Asia
SKU - Free
ACU - Shared infrastructure
Memory - 1 GB memory

- Monitoring (New)
Application Insights - Enabled
Name - asptestapp-hnv
Region - East Asia

- Deployment
Basic authentication - Disabled
Continuous deployment - Enabled
GitHub account - hoangvu1420
Organization - hoangvu1420
Repository - ASP.NET_Test_Web
Branch - production