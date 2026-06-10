using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

namespace CognitoTestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            #region CONFIGURACIÓN COGNITO
            var region = builder.Configuration["AWS:Region"];
            var poolId = builder.Configuration["AWS:UserPoolId"];
            var authority = $"https://cognito-idp.{region}.amazonaws.com/{poolId}";

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authority,
                        ValidateAudience = false,
                        ValidateLifetime = true
                    };
                });

            builder.Services.AddAuthorization();

            #endregion


            builder.Services.AddControllers();

            #region OpenAPI + Scalar
            // 3. Configurar OpenAPI (el "motor" que alimenta a Scalar)
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });

            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // Genera el endpoint /openapi/v1.json
                app.MapOpenApi();

                // Configura Scalar
                app.MapScalarApiReference(options =>
                {
                    options.WithTitle("Cognito API Test")
                           .WithTheme(ScalarTheme.DeepSpace)
                           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                });
            }

            app.UseHttpsRedirection();

            // IMPORTANTE: El orden importa. Primero Auth, luego Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
