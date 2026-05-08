# HW2 Azure Deployment

## Deployed PaaS resources

- Azure SQL Database: `reservation_system_db` on `hw2-sql-7t3ii.database.windows.net`
- Azure App Service: `https://hw2-app.azurewebsites.net`
- Azure Storage account: `hw27t3ii`
- Public static asset container: `static`
- Private upload container: `uploads`

No virtual machines or self-managed database engines are used.

## Database connection

The app reads its EF Core connection string from:

```text
ConnectionStrings__MvcRegistrationContext
```

The Azure value is secretless and uses the App Service managed identity:

```text
Server=tcp:hw2-sql-7t3ii.database.windows.net,1433;Database=reservation_system_db;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

After Terraform creates the infrastructure, the SQL database must contain a user for the App Service identity:

```sql
CREATE USER [hw2-app] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [hw2-app];
ALTER ROLE db_datawriter ADD MEMBER [hw2-app];
ALTER ROLE db_ddladmin ADD MEMBER [hw2-app];
GO
```

The application calls `EnsureCreated()` during startup and seeds the initial Identity/users data when the database is empty.

## Static content

The application uses the `STATIC_ASSETS_BASE_URL` app setting to build URLs for CSS, JavaScript, Bootstrap, jQuery, and images.

Current value:

```text
https://hw27t3ii.blob.core.windows.net/static
```

During deployment, GitHub Actions uploads `publish/wwwroot` to the `static` Blob container. The Razor layouts reference the Blob URLs instead of serving those files from App Service compute.

## User uploads

The current reservation system does not contain a user-upload feature. The infrastructure still includes a private `uploads` container for that extension.

If uploads are added, the intended valet-key flow is:

1. Store uploaded files in the private `uploads` container.
2. Access Blob Storage from the app using `DefaultAzureCredential` and the App Service managed identity.
3. Grant users access only after authentication and authorization checks.
4. Generate a short-lived SAS URL for the specific blob and return that URL to the browser.
5. Do not expose storage account keys or long-lived container SAS tokens.

Terraform already assigns `Storage Blob Data Contributor` to the App Service managed identity for this purpose.

## CI/CD

The workflow is in `.github/workflows/deploy.yml`.

On every push to `main`, it:

1. Restores and publishes the .NET 6 application.
2. Uploads static assets to the `static` Blob container using `AZURE_STORAGE_CONNECTION_STRING`.
3. Deploys the published app to `hw2-app` using `AZURE_WEBAPP_PUBLISH_PROFILE`.

The repository needs these GitHub Actions secrets:

```text
AZURE_STORAGE_CONNECTION_STRING
AZURE_WEBAPP_PUBLISH_PROFILE
```

The database connection remains secretless at runtime because Azure SQL uses the App Service managed identity.
