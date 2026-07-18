using Business_Layer.Mappings;

namespace Api_Layer.DependencyInjection
{
    public static class AutoMapperdependencyinjection
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
         services.AddAutoMapper(cfg => { },
             typeof(UserMappingProfile).Assembly
             );
         return services;
        }
    }
}
