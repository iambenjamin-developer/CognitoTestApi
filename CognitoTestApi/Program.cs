using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

            // 1. Configuración de variables de Cognito
            var region = builder.Configuration["AWS:Region"];
            var poolId = builder.Configuration["AWS:UserPoolId"];
            var authority = $"https://cognito-idp.{region}.amazonaws.com/{poolId}";

            // 2. Autenticación
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            #region CONFIGURACIÓN OPENAPI / SCALAR
            // 3. Configurar OpenAPI (el "motor" que alimenta a Scalar)
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    // Usamos nombres completos para evitar errores de namespace
                    var scheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Pega tu IdToken de Cognito"
                    };

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes.Add("Bearer", scheme);

                    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }] = Array.Empty<string>()
                    });

                    return Task.CompletedTask;
                });
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
