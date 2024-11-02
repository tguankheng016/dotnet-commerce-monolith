# Add Migrations
MIGRATION_NAME = Initial
MIGRATION_PROJECT = src/CommerceMono.Application/CommerceMono.Application.csproj
STARTUP_PROJECT = src/CommerceMono.Api/CommerceMono.Api.csproj
DBCONTEXT_WITH_NAMESPACE = CommerceMono.Application.Data.AppDbContext
OUTPUT_DIR = Data/Migrations
add_migration:
	dotnet ef migrations add --project $(MIGRATION_PROJECT) --startup-project $(STARTUP_PROJECT) --context $(DBCONTEXT_WITH_NAMESPACE) --configuration Debug --verbose $(MIGRATION_NAME) --output-dir $(OUTPUT_DIR)