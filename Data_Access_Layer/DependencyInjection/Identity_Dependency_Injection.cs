using Data_Access_Layer.Data;
using Domain_Layer.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace Infrastructure.DependencyInjection
{

        public static class IdentityDependencyInjection
        {
            public static IServiceCollection AddIdentityServices(
                this IServiceCollection services,
                ConfigurationManager configuration)
            {
                services.AddIdentity<AppUser, IdentityRole>(options =>
                        options.User.RequireUniqueEmail = true)
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["JWT:Issuer"],

                        ValidateAudience = false,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                    };
                });

                return services;
            }
        }
    }
