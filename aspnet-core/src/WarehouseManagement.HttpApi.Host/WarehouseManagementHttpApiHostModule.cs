using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using WarehouseManagement.EntityFrameworkCore;
using WarehouseManagement.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Microsoft.OpenApi;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace WarehouseManagement;

[DependsOn(
    typeof(WarehouseManagementHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(WarehouseManagementApplicationModule),
    typeof(WarehouseManagementEntityFrameworkCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class WarehouseManagementHttpApiHostModule : AbpModule
{
    private const string OpenIddictPemKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAuPTvi0ZhnvYMvjtSuMAIbEIy8PPjQYPzM/CIGfPw69ZUhyLL
4WtaGGTJvpDpaYVvSWu3sUFxLUcZw9/VnQSBU1A/il1rDNGubjxkts7XrgA4FLFe
UY2oBWB+h+4TabzWllOjZM70wSRpTuXQu9oI73U3alcZDvqXZ+htETa+2AwP+hdL
xRwvLQupfDFiRC833aM0u/AJ/a2vZLCTEShVvH4Em2E2V7iSjZSX04Xb4UTdvjhh
1KGpmszA+hdWV2YVYpzi1uAflALkRmTcMXEEtn8n+BXpGxlcKWxpb02jJYCwLICQ
tWlsSkFooYYOZbsSxA0RJz+thQM5CZ9v3/vmXQIDAQABAoIBAFoDYn1ewG3Tg3ol
dunRE4hUY2Re3Y/p/redbS9sSCkOw06NQbowO8W5b3HeJqqjNPj/goOyvLjzbHem
fNiQ87uvR204b7vqZEDkBYPugIr/Mkvn0mZQUaBkSNAwrqP8Mlcw10lda0KATPwH
3qE9yLF5U4HeZ6ShkoQQWtxQz2pnCJSRGyXQZhmdrAJmdx3s7LDq6fYImV0q7G9V
F1rURQj9CGzx848CSSs966smWiabZiOPnWkEF+7X6IdtLAOlQURX5C96sf/nZAKy
+3ZFaCR3z31aL5pwm7Eo48hZh0HEM7ifbOtFYWq1aLUY/jRmgmCUk7KPILZq/1vT
3RU8YYUCgYEA30RcyvhRqxjNMMk+iDEQAMHklARSe4RVHa5RY2n/aPeevqrgvSmy
OlfdjuL7mAWKHPPrgmcgvugRIJ5aFxBkGLN+a63ra0DMrlJh0zoXryQIMdKZoqH+
esxDVtimE9NMcdK/JoACa6wDh2qea5ZmHmn5TKc51w1JKFoIGrNXR0sCgYEA1BK3
VcNtkEl5/WG+DOscPZY+6ejqr7qfMjR4KJk0f/xsAO/tz+WKScAKRtqtGuBM05Bs
ohn6PDru3x8igP+cgLRXxvyo090d/OrUbL9h4xPTVkKwF/rAsgKoqONDbzivRn2f
wY/I1oLhnydWDUatgLlHKHFkZuGUmZvL4cE/N/cCgYB9ykptoNKCj4qr/fZZTSsz
AgsCyhyessQx4NDCLipUvY6W5jeQ3DkqXe7aXlBaKmTSQythYTE13hIoSo9/hyCe
OiL0EB0fZHbtkETFeWl2p9pmF72vv41dKlCWUaHgcaAUUSSZUG4Ms/GZDh0LiMUo
RZTL9zvDShz5ux/tyzVVWQKBgE9vg0SYox18qDkEt49XMYjQEljDacTQ/nhrtuZe
HJAkGMBxHn7U1+uwBylrF5QNmDlWy5ocaOLOoFLq8p2nIRQEeSyLzyEsneLygwcI
2L2Z23tgF12kCzGiVVKNbeDnNqAMxh5WoT/YXnKi5VSruKmSvNilQJpYvLSc6s1w
+SEZAoGBAMpWSsjsVAtq4qcZL8ciF3yn3CCOBL42z1cdmdggUBYgJlaT8O9rKD88
0NQBAwvOco5pczkZRficQNjmwsMvnGrbsjfApqsGoAwTxISTi1HBJEXC5in37Ksu
3oeQKBuAWSEcdR31vYJENzSmcF/0r7q51csxMs8TAFKQY87xUK1d
-----END RSA PRIVATE KEY-----";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var authority = configuration["AuthServer:Authority"]?.TrimEnd('/');

        PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
        {
            options.AddDevelopmentEncryptionAndSigningCertificate = false;
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("WarehouseManagement");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
            builder.AddServer(options =>
            {
                options.UseAspNetCore().DisableTransportSecurityRequirement();

                var rsa = RSA.Create();
                rsa.ImportFromPem(OpenIddictPemKey);
                var rsaKey = new RsaSecurityKey(rsa) { KeyId = "WarehouseManagement-SigningKey" };
                options.AddSigningKey(rsaKey);
                options.AddEncryptionKey(rsaKey);
                options.DisableAccessTokenEncryption();

                if (!string.IsNullOrEmpty(authority))
                {
                    options.SetIssuer(new Uri(authority + "/"));
                }
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        Configure<OpenIddictServerAspNetCoreOptions>(options =>
        {
            options.DisableTransportSecurityRequirement = true;
        });

        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });

        ConfigureAuthentication(context);
        ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureConventionalControllers();
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);
        ConfigureSwaggerServices(context, configuration);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());

            options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
        });
    }

    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<WarehouseManagementDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}WarehouseManagement.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<WarehouseManagementDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}WarehouseManagement.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<WarehouseManagementApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}WarehouseManagement.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<WarehouseManagementApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}WarehouseManagement.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(WarehouseManagementApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"]!,
            new Dictionary<string, string>
            {
                    {"WarehouseManagement", "WarehouseManagement API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "WarehouseManagement API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(configuration["App:CorsOrigins"]?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => o.RemovePostFix("/"))
                        .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.Use(async (httpContext, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentType = "text/plain; charset=utf-8";
                await httpContext.Response.WriteAsync($"SERVER EXCEPTION:\nType: {ex.GetType().FullName}\nMessage: {ex.Message}\nStack:\n{ex.StackTrace}\nInner:\n{ex.InnerException?.ToString()}");
            }
        });

        app.UseForwardedHeaders();
        app.UseDeveloperExceptionPage();

        app.Use(async (httpContext, next) =>
        {
            var path = httpContext.Request.Path.Value;
            if (string.Equals(path, "/favicon.svg", StringComparison.OrdinalIgnoreCase))
            {
                var faviconSvgPath = Path.Combine(env.ContentRootPath, "wwwroot", "favicon.svg");
                if (File.Exists(faviconSvgPath))
                {
                    httpContext.Response.ContentType = "image/svg+xml";
                    httpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                    var bytes = await File.ReadAllBytesAsync(faviconSvgPath);
                    await httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    return;
                }
            }
            else if (string.Equals(path, "/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                var faviconIcoPath = Path.Combine(env.ContentRootPath, "wwwroot", "favicon.ico");
                if (File.Exists(faviconIcoPath))
                {
                    httpContext.Response.ContentType = "image/x-icon";
                    httpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                    var bytes = await File.ReadAllBytesAsync(faviconIcoPath);
                    await httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    return;
                }
            }

            await next();
        });

        app.UseAbpRequestLocalization();
        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "WarehouseManagement API");

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            c.OAuthScopes("WarehouseManagement");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
