using Data_Access_Layer.Data;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api_Layer.DependencyInjection
{
    public static class IdentityDependencyInjection
    {
        public static IServiceCollection AddIdentity(this IServiceCollection services ,ConfigurationManager configuration )
        {
            services.AddIdentity<AppUser, IdentityRole>
                (op => op.User.RequireUniqueEmail = true)
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(
                o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

                }
                ).AddJwtBearer(
                 o =>
                 {
                     o.RequireHttpsMetadata = false;
                     o.SaveToken = true;
                     o.TokenValidationParameters = new TokenValidationParameters()
                     {
                         ValidateIssuer = true,
                         ValidIssuer = configuration["JWT:Issuer"],
                         ValidateAudience = false,
                         ValidateIssuerSigningKey = true,
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))

                     };
                 }
                );
            return services;
        }
    }
}
