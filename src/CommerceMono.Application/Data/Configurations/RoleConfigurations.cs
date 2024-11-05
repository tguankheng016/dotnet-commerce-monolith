using CommerceMono.Application.Roles.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceMono.Application.Data.Configurations;

public class RoleConfigurations : IEntityTypeConfiguration<Role>
{
	public void Configure(EntityTypeBuilder<Role> builder)
	{
		builder.Property(r => r.Version).IsConcurrencyToken();
	}
}
