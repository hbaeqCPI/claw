using System;
using System.Security.Claims;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Localization.SqlLocalizer.DbStringLocalizer;
using AutoMapper;
using R10.Infrastructure.Data;
using R10.Infrastructure.Identity;
using R10.Core.Interfaces;
using R10.Core.Identity;
using R10.Web.Security;
using R10.Web.Interfaces;
using R10.Web.Services;
using R10.Web.Extensions;
using R10.Web.Areas;
using R10.Web.Filters;
using Microsoft.Extensions.FileProviders;
using System.IO;
using R10.Core.Services;
using R10.Web.MiddleWares;
using R10.Core.Helpers;
using System.Linq;
using Microsoft.AspNetCore.DataProtection;
// using ActiveQueryBuilder.Web.Core; // Removed during debloat
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2;
using System.Security.Cryptography.X509Certificates;
using Sustainsys.Saml2.Configuration;
using GleamTech.AspNet.Core;
using GleamTech.DocumentUltimate;
using GleamTech.DocumentUltimate.AspNet;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.CookiePolicy;
using R10.Core.Entities;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
// using ActiveQueryBuilder.Web.Server.Infrastructure.Providers; // Removed during debloat
using R10.Web.Areas.Admin.Services;
using System.Net;
using OpenIddict.Server;
using Sustainsys.Saml2.AspNetCore2;
using WaterTrans.AzureBlobFileProvider;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;

namespace R10.Web
{
    public class Startup
    {
        private readonly bool _autoCreateLocalizedString = false;
        private readonly string _emailAddInCORSPolicy = "EmailAddInCORSPolicy";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;

            if (env.IsDevelopment())
            {
               // _autoCreateLocalizedString = true;
            }

            //Column encryption config
            Configuration.SetColumnEncryption();

            //Trade secret config
            Configuration.SetTradeSecret();

            //Task scheduler config
            Configuration.SetTaskScheduler();
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDataProtection().SetApplicationName("R10").PersistKeysToFileSystem(new DirectoryInfo("Keys")); //in a web farm, server instances should share same key
            services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDistributedSqlServerCache(options => {
                options.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
                options.SchemaName = "dbo";
                options.TableName = "tblSQLSessions";
                options.DefaultSlidingExpiration = TimeSpan.FromDays(1);
            });

            //set sso cookie samesite to none to prevent getting blocked
            services.ConfigureExternalCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });


            services.AddSingleton(Configuration);
            services.AddMemoryCache(); // to avoid error in Outlook Add-in
            services.AddEntityFrameworkSqlServer();
            services.AddTransient<ISqlServerConnection, EntityFilterConnection>();
            services.AddDbContext<ApplicationDbContext>((sp, options) => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions =>
                {
                    sqlServerOptions.CommandTimeout(900);
                    //sqlServerOptions.EnableRetryOnFailure(
                    //  maxRetryCount: 10,
                    //  maxRetryDelay: TimeSpan.FromSeconds(10),
                    //  errorNumbersToAdd: null);
                }
                );
                options.UseInternalServiceProvider(sp);
            });

            //services.AddDbContext<ApplicationDbContext>(options =>
            //   options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());

            services.AddDbContext<CPiUserDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            //TODO: ENABLE CORS?
            //services.AddCors(options => {
            //    options.AddPolicy("CorsPolicy", builder => builder
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .AllowCredentials()
            //    );
            //});

            // refer to https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.1
            // specifically, more secure cors: https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.1#enable-cors-with-attributes-1
            if (!string.IsNullOrEmpty(Configuration["EmailAddIn:OutlookOriginUrl"]) || !string.IsNullOrEmpty(Configuration["EmailAddIn:GmailOriginUrl"])) {
                services.AddCors(options => {
                    options.AddPolicy(_emailAddInCORSPolicy, builder => {
                        string origins = "";
                        // outlook add-in
                        if (!string.IsNullOrEmpty(Configuration["EmailAddIn:OutlookOriginUrl"]))
                        {
                            origins += "|" + Configuration["EmailAddIn:OutlookOriginUrl"];
                        }

                        // gmail add-in
                        if (!string.IsNullOrEmpty(Configuration["EmailAddIn:GmailOriginUrl"]))
                        {
                            origins += "|" + Configuration["EmailAddIn:GmailOriginUrl"];
                        }

                        origins = origins.Substring(1);
                        builder.WithOrigins(origins.Split("|"))
                                          .AllowAnyHeader()
                                          //.AllowAnyMethod()
                                          .WithMethods("GET", "POST");
                    });
                });
            }

            services.AddCPiDbContext<ApplicationDbContext>();

            services.AddIdentity<CPiUser, CPiRole>()
                .AddEntityFrameworkStores<CPiUserDbContext>()
                .AddUserManager<CPiUserManager>()
                .AddSignInManager<CPiSignInManager>()
                .AddClaimsPrincipalFactory<CPiUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders();

            //Load Identity options from appsettings.json
            services.Configure<IdentityOptions>(Configuration.GetSection("Identity"));

            services.ConfigureApplicationCookie(options =>
            {
                //Cookie settings
                options.ExpireTimeSpan = TimeSpan.Parse(Configuration["Cpi:Cookie:StaySignedInTimeSpan"]);
                options.SlidingExpiration = true;
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.AccessDeniedPath = "/AccessDenied";
            });

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                //Set to TimeSpan.Zero to enable immediate logout after updating the user profile.
                options.ValidationInterval = TimeSpan.Parse(Configuration["Cpi:Cookie:ValidationTimeSpan"]);
            });

            services.AddScoped<ICPiUserPasswordHistoryRepository, CPiUserPasswordHistoryStore>();
            services.AddScoped<ICPiUserPermissionManager, CPiUserPermissionManager>();
            services.AddScoped<ICPiUserEntityFilterRepository, CPiUserEntityFilterStore>();
            services.AddScoped<ICPiExternalLoginManager, CPiExternalLoginManager>();

            services.AddScoped<ICPiMenuItemManager, CPiMenuItemManager>();
            services.AddScoped<ICPiMenuPageManager, CPiMenuPageManager>();

            services.AddScoped<ILocalizationRecordsManager, LocalizationRecordsManager>();

            services.AddScoped<ICPiUserSettingManager, CPiUserSettingManager>();
            services.AddScoped<INotificationSettingManager, NotificationSettingManager>();
            services.AddScoped<ICPiUserDefaultPageManager, CPiUserDefaultPageManager>();

            services.AddScoped<ICPiSystemSettingManager, CPiSystemSettingManager>();
            services.AddScoped<ICPiUserGroupManager, CPiUserGroupManager>();

            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<ILoggerService<Log>, LoggerService<Log, ApplicationDbContext>>();
            services.AddScoped<ILoggerService<ApiLog>, LoggerService<ApiLog, ApplicationDbContext>>();

            services.AddTransient<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>().HttpContext.User);

            // Add systems services
            services.AddAdmin();
            services.AddPatent();
            services.AddTrademark();
            services.AddShared(Configuration);
            services.AddReportScheduler();


            // Debloat stubs: no-op implementations for removed services still referenced in DI
            services.AddScoped<Interfaces.IInventionViewModelService, Interfaces.NoOpInventionViewModelService>();
            services.AddScoped<Interfaces.ICountryApplicationViewModelService, Interfaces.NoOpCountryApplicationViewModelService>();
            services.AddScoped<Interfaces.IPatActionDueViewModelService, Interfaces.NoOpPatActionDueViewModelService>();
            services.AddScoped<Interfaces.IPatActionDueInvViewModelService, Interfaces.NoOpPatActionDueInvViewModelService>();
            services.AddScoped<Interfaces.IPatCostTrackingViewModelService, Interfaces.NoOpPatCostTrackingViewModelService>();
            services.AddScoped<Interfaces.IPatCostTrackingInvViewModelService, Interfaces.NoOpPatCostTrackingInvViewModelService>();
            services.AddScoped<Interfaces.IPatImageInvViewModelService, Interfaces.NoOpPatImageInvViewModelService>();
            services.AddScoped<Interfaces.IPatImageAppViewModelService, Interfaces.NoOpPatImageAppViewModelService>();
            services.AddScoped<Interfaces.IPatImageActViewModelService, Interfaces.NoOpPatImageActViewModelService>();
            services.AddScoped<Interfaces.IPatImageActInvViewModelService, Interfaces.NoOpPatImageActInvViewModelService>();
            services.AddScoped<Interfaces.IPatImageCostViewModelService, Interfaces.NoOpPatImageCostViewModelService>();
            services.AddScoped<Interfaces.IPatImageCostInvViewModelService, Interfaces.NoOpPatImageCostInvViewModelService>();
            services.AddScoped<Interfaces.IPatInventorRemunerationService, Interfaces.NoOpPatInventorRemunerationService>();
            services.AddScoped<Interfaces.IPatInventorFRRemunerationService, Interfaces.NoOpPatInventorFRRemunerationService>();
            services.AddScoped<Interfaces.IPatInventorAppAwardUpdateService, Interfaces.NoOpPatInventorAppAwardUpdateService>();
            services.AddScoped<Interfaces.IEPOService, Interfaces.NoOpEPOService>();
            services.AddScoped<Interfaces.ITmkTrademarkViewModelService, Interfaces.NoOpTmkTrademarkViewModelService>();
            services.AddScoped<Interfaces.ITmkActionDueViewModelService, Interfaces.NoOpTmkActionDueViewModelService>();
            services.AddScoped<Interfaces.ITmkCostTrackingViewModelService, Interfaces.NoOpTmkCostTrackingViewModelService>();
            services.AddScoped<Interfaces.ITmkConflictViewModelService, Interfaces.NoOpTmkConflictViewModelService>();
            services.AddScoped<Interfaces.ITmkImageViewModelService, Interfaces.NoOpTmkImageViewModelService>();
            services.AddScoped<Interfaces.ITmkImageCostViewModelService, Interfaces.NoOpTmkImageCostViewModelService>();
            services.AddScoped<Interfaces.ITmkImageActViewModelService, Interfaces.NoOpTmkImageActViewModelService>();

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<SmtpSettings>(Configuration.GetSection("Smtp"));
            services.Configure<DocuSignSettings>(Configuration.GetSection("DocuSign"));
            services.Configure<GraphSettings>(Configuration.GetSection("Graph"));
            services.Configure<iManageSettings>(Configuration.GetSection("iManage"));
            services.Configure<NetDocumentsSettings>(Configuration.GetSection("NetDocuments"));
            services.Configure<CPiIdentitySettings>(Configuration.GetSection("Cpi"));
            services.Configure<ServiceAccount>(Configuration.GetSection("ServiceAccount"));
            services.Configure<EPOMailboxSettings>(Configuration.GetSection("MyEPO:Mailbox"));
            services.Configure<EPOOPSSettings>(Configuration.GetSection("MyEPO:OPS"));
            services.Configure<GoogleIDSSettings>(Configuration.GetSection("GoogleIDS"));

            //globalization
            services.AddDbContext<LocalizationModelContext>(options =>
                  options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("R10.Infrastructure")),
                  ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            services.AddSqlLocalization(options => options.UseSettings(useTypeFullNames: false, useOnlyPropertyNames: false, returnOnlyKeyIfNotFound: true,
                                                                       createNewRecordWhenLocalisedStringDoesNotExist: _autoCreateLocalizedString));
            services.Configure<RequestLocalizationOptions>(options =>
                      {
                          var di = new DirectoryInfo(Path.Combine(Environment.WebRootPath, @"lib\cldr-data\main"));
                          var supportedCultures = di.GetDirectories().Where(x => x.Name != "root").Select(x => new CultureInfo(x.Name)).ToList();

                          options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en");
                          options.SupportedCultures = supportedCultures;
                          options.SupportedUICultures = supportedCultures;

                          options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                          {
                              return new ProviderCultureResult(context.User.GetLocale());
                          }));
                      });


            // Add security
            services.AddSecurity(Configuration);

            #region Register External Authentication
            if (!string.IsNullOrEmpty(Configuration["Authentication:WSFederation:Realm"]))
            {
                services.AddAuthentication().AddWsFederation(options =>
                {
                    options.MetadataAddress = Configuration["Authentication:WSFederation:Metadata"];
                    options.Wtrealm = Configuration["Authentication:WSFederation:Realm"];
                });
            }
            if (!string.IsNullOrEmpty(Configuration["Authentication:Google:ClientId"]))
            {
                services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                });
            }
            if (!string.IsNullOrEmpty(Configuration["Authentication:Microsoft:ApplicationId"]))
            {
                services.AddAuthentication().AddMicrosoftAccount(options =>
                {
                    var authEndPpoint = Configuration["Authentication:Microsoft:AuthorizationEndpoint"];
                    if (!string.IsNullOrEmpty(authEndPpoint))
                        options.AuthorizationEndpoint = authEndPpoint;

                    var tokenEndPpoint = Configuration["Authentication:Microsoft:TokenEndpoint"];
                    if (!string.IsNullOrEmpty(tokenEndPpoint))
                        options.TokenEndpoint = tokenEndPpoint;

                    options.ClientId = Configuration["Authentication:Microsoft:ApplicationId"];
                    options.ClientSecret = Configuration["Authentication:Microsoft:Password"];
                });
            }
            if (!string.IsNullOrEmpty(Configuration["Authentication:OIDC:ClientId"]))
            {
                services.AddAuthentication()
                    .AddOpenIdConnect("OIDC", Configuration["Authentication:OIDC:Name"], options =>
                    {
                        //most oidc
                        //options.Authority = Configuration["Authentication:OIDC:Authority"] +"/oauth2/default";
                        //azure ad oidc
                        //options.Authority = Configuration["Authentication:OIDC:Authority"] + "/v2.0";
                        //include full path in appsettings
                        options.Authority = Configuration["Authentication:OIDC:Authority"];
                        options.ClientId = Configuration["Authentication:OIDC:ClientId"];
                        options.ClientSecret = Configuration["Authentication:OIDC:ClientSecret"];
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("email");

                        options.Events = new OpenIdConnectEvents()
                        {
                            OnRemoteFailure = context => {
                                context.HandleResponse();

                                if (context.Properties.Items.ContainsKey(".redirect"))
                                {
                                    var url = context.Properties.Items.FirstOrDefault(i => i.Key == ".redirect").Value;
                                    url = url + (url.Contains("?") ? "&" : "?") + "remoteError=Access denied."; // + HttpUtility.UrlEncode(context.Failure.Message ?? "");

                                    context.Response.Redirect(url);
                                }
                                else
                                    context.Response.Redirect("Login");

                                return Task.FromResult(0);
                            }
                        };
                    });
            }
            if (!string.IsNullOrEmpty(Configuration["Authentication:Saml2:SP:EntityId"]))
            {
                services.AddAuthentication()
                    .AddSaml2(Saml2Defaults.Scheme, Configuration["Authentication:Saml2:Name"], options =>
                    {
                        options.SPOptions = new SPOptions()
                        {
                            EntityId = new EntityId(Configuration["Authentication:Saml2:SP:EntityId"])
                        };

                        //idp-initiated login
                        if (!string.IsNullOrEmpty(Configuration["Authentication:Saml2:SP:ReturnUrl"]))
                            options.SPOptions.ReturnUrl = new Uri(Configuration["Authentication:Saml2:SP:ReturnUrl"]);

                        //saml2 slo
                        var serviceCert = Configuration["Authentication:Saml2:SP:ServiceCertificate"];
                        if (!string.IsNullOrEmpty(serviceCert))
                        {
                            var certPath = Path.Combine(Environment.ContentRootPath, serviceCert);
                            if (File.Exists(certPath))
                                options.SPOptions.ServiceCertificates.Add(new ServiceCertificate
                            {
                                Certificate = new X509Certificate2(certPath, Configuration["Authentication:Saml2:SP:ServiceCertificatePassword"]),
                                Use = CertificateUse.Signing,
                                Status = CertificateStatus.Current
                            });
                        }

                        //requested attributes
                        var attributeConsumingService = new AttributeConsumingService()
                        {
                            IsDefault = true,
                        };
                        //attribute names
                        var roleAttributeName = Configuration["Cpi:ExternalLogin:RoleAttributeName"];
                        var emailAttributeName = Configuration["Cpi:ExternalLogin:EmailAttributeName"];
                        var firstNameAttributeName = Configuration["Cpi:ExternalLogin:FirstNameAttributeName"];
                        var lastNameAttributeName = Configuration["Cpi:ExternalLogin:LastNameAttributeName"];

                        emailAttributeName = string.IsNullOrEmpty(emailAttributeName) ? ClaimTypes.Email : emailAttributeName;
                        firstNameAttributeName = string.IsNullOrEmpty(firstNameAttributeName) ? ClaimTypes.GivenName : firstNameAttributeName;
                        lastNameAttributeName = string.IsNullOrEmpty(lastNameAttributeName) ? ClaimTypes.Surname : lastNameAttributeName;

                        //role attribute
                        if (bool.Parse(Configuration["Cpi:ExternalLogin:RequireRoleAttribute"]))
                            attributeConsumingService.RequestedAttributes.Add(
                                new RequestedAttribute(roleAttributeName)
                                {
                                    FriendlyName = "CPI Role",
                                    IsRequired = true,
                                    NameFormat = RequestedAttribute.AttributeNameFormatUnspecified
                                });

                        //email attribute
                        attributeConsumingService.RequestedAttributes.Add(
                            new RequestedAttribute(emailAttributeName)
                            {
                                FriendlyName = "Email Address",
                                IsRequired = true,
                                NameFormat = RequestedAttribute.AttributeNameFormatUnspecified
                            });
                        //first name attribute
                        attributeConsumingService.RequestedAttributes.Add(
                            new RequestedAttribute(firstNameAttributeName)
                            {
                                FriendlyName = "First Name",
                                IsRequired = true,
                                NameFormat = RequestedAttribute.AttributeNameFormatUnspecified
                            });
                        //last name attribute
                        attributeConsumingService.RequestedAttributes.Add(
                            new RequestedAttribute(lastNameAttributeName)
                            {
                                FriendlyName = "Last Name",
                                IsRequired = true,
                                NameFormat = RequestedAttribute.AttributeNameFormatUnspecified
                            });
                        //add required attributes
                        options.SPOptions.AttributeConsumingServices.Add(attributeConsumingService);

                        if (!string.IsNullOrEmpty(Configuration["Authentication:Saml2:EntityId"]))
                        {
                            var idp = new IdentityProvider(
                                    new EntityId(Configuration["Authentication:Saml2:EntityId"]), options.SPOptions)
                            {
                                SingleSignOnServiceUrl = new Uri(Configuration["Authentication:Saml2:SingleSignOnServiceUrl"]),
                                AllowUnsolicitedAuthnResponse = true,
                            };

                            //saml2 slo
                            if (!string.IsNullOrEmpty(Configuration["Authentication:Saml2:SingleLogOutServiceUrl"]))
                            {
                                idp.SingleLogoutServiceUrl = new Uri(Configuration["Authentication:Saml2:SingleLogOutServiceUrl"]);
                                idp.DisableOutboundLogoutRequests = false;
                            }

                            if (!string.IsNullOrEmpty(Configuration["Authentication:Saml2:SigningKey"]))
                            {
                                idp.Binding = Sustainsys.Saml2.WebSso.Saml2BindingType.HttpRedirect;
                                idp.SigningKeys.AddConfiguredKey(new X509Certificate2(Path.Combine(Environment.ContentRootPath, Configuration["Authentication:Saml2:SigningKey"])));
                            }

                            if (!string.IsNullOrEmpty(Configuration["Authentication:Saml2:MetadataLocation"]))
                            {
                                idp.MetadataLocation = Configuration["Authentication:Saml2:MetadataLocation"];
                                idp.LoadMetadata = true;
                            }

                            options.IdentityProviders.Add(idp);
                        }
                    });
            }
            #endregion -- External Authentication

            #region Register Graph API authentication used by OnBehalfOf flow
            if (!string.IsNullOrEmpty(Configuration["Graph:Mail:Authority"]))
            {
                services.AddAuthentication()
                    .AddOpenIdConnect(Configuration["Graph:Mail:ProviderName"], Configuration["Graph:Mail:ProviderName"], options =>
                    {
                        options.Authority = Configuration["Graph:Mail:Authority"];
                        options.ClientId = Configuration["Graph:Mail:ClientId"];
                        options.ClientSecret = Configuration["Graph:Mail:ClientSecret"];
                        options.Scope.Add("openid");

                        //graph tokens
                        options.SaveTokens = true; //includes auth tokens in externallogininfo
                        options.ResponseType = OpenIdConnectResponseType.Code; //includes all tokens (default is id_token only)

                        //set unique callback path to avoid conflict with oidc external login provider (/signin-oidc)
                        options.CallbackPath = "/signin-mail";
                    });
            }
            if (!string.IsNullOrEmpty(Configuration["Graph:SharePoint:Authority"]))
            {
                services.AddAuthentication()
                    .AddOpenIdConnect(Configuration["Graph:SharePoint:ProviderName"], Configuration["Graph:SharePoint:ProviderName"], options =>
                    {
                        options.Authority = Configuration["Graph:SharePoint:Authority"];
                        options.ClientId = Configuration["Graph:SharePoint:ClientId"];
                        options.ClientSecret = Configuration["Graph:SharePoint:ClientSecret"];
                        options.Scope.Add("openid");

                        foreach (var scope in Configuration["Graph:SharePoint:Scopes"].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            options.Scope.Add(scope);
                        }

                        //needed to get a refresh token
                        options.Scope.Add("offline_access");

                        //includes auth tokens in externallogininfo
                        options.SaveTokens = true;

                        //includes all tokens (default is id_token only)
                        options.ResponseType = OpenIdConnectResponseType.Code; 
                        
                        //set unique callback path to avoid conflict with default oidc redirect uri (/signin-oidc)
                        options.CallbackPath = "/signin-sharepoint";
                    });
            }
            #endregion -- Graph API Authentication

            services.Configure<FormOptions>(options =>
            {
                //Request form size limit
                options.ValueCountLimit = int.MaxValue;
            });

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.MaxModelBindingCollectionSize = 20000; //default is 1024
                //options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
            })

                //.AddViewOptions(options =>
                //{
                //    //remove temp data key prefix "TempDataProperty-"
                //    options.SuppressTempDataAttributePrefix = true;

                //})
                //.AddJsonOptions(options =>
                //{
                //    options.SerializerSettings.ContractResolver = new DefaultContractResolver(); // Maintain property names during serialization. See: https://github.com/aspnet/Announcements/issues/194
                //    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // Self referencing loop detected. See: https://www.newtonsoft.com/json/help/html/ReferenceLoopHandlingIgnore.htm and https://stackoverflow.com/questions/13510204/json-net-self-referencing-loop-detected
                //}) 
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                })
                .AddDataAnnotationsLocalization()
                .AddViewLocalization()
                .AddXmlSerializerFormatters()
                .AddRazorRuntimeCompilation();

            services.AddKendo(); // Add Kendo UI services to the services container

            services.AddAutoMapper(typeof(Startup)); //Add AutoMapper services
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>(); //default is Session
            services.AddScoped<ExceptionFilter>();
            services.AddScoped<RequestHeaderFilter>();
            services.AddScoped<SharePointAuthorizationFilter>();
            services.AddScoped<MailAuthorizationFilter>();

            IFileProvider physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            services.AddSingleton<IFileProvider>(physicalProvider);

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                                               .ActionContext;
                return new UrlHelper(actionContext);
            });
            services.AddHttpClient();

            #region Register the OpenIddict services for Web API auth
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and entities.
                    options.UseEntityFrameworkCore()
                                   .UseDbContext<CPiUserDbContext>();
                })

                .AddServer(options =>
                {                   

                    // Set the endpoints
                    options.SetAuthorizationEndpointUris("connect/authorize");      // used by authorization flow
                    options.SetTokenEndpointUris("connect/token");                  // used by password flow, authorization flow
                    options.SetLogoutEndpointUris("connect/logout");                // logout
                    options.SetUserinfoEndpointUris("connect/userinfo");            // used by oidc-client in Outlook add-in

                    // Allow flows for client applications
                    // Outlook Add-in uses authorization code
                    options.AllowPasswordFlow();                                    // password flow; grant_type=password
                    options.AllowAuthorizationCodeFlow();                           // authorization code flow; response_type=code; implement PKCE on client-side
                    options.AllowClientCredentialsFlow();                           // client credentials flow; grant_type=client_credentials
                    options.AllowRefreshTokenFlow();                                // refresh token for access token renewal; include in scope 'offline_access'

                    //options.SetAccessTokenLifetime(TimeSpan.FromMinutes(2));      // for testing token expiration/refresh only
                    //options.SetRefreshTokenLifetime(null);                        // null -> refresh token never expires (discouraged)

                    if (string.IsNullOrEmpty(Configuration["OpenIddict:SigningThumbprint"]) && string.IsNullOrEmpty(Configuration["OpenIddict:EncryptionThumbprint"]))
                    {
                        options.AddEncryptionCertificate(new X509Certificate2(Path.Combine(Environment.ContentRootPath, "Resources", "openiddict-encryption-certificate.pfx"), Configuration["OpenIddict:CertificatePassword"]));
                        options.AddSigningCertificate(new X509Certificate2(Path.Combine(Environment.ContentRootPath, "Resources", "openiddict-signing-certificate.pfx"), Configuration["OpenIddict:CertificatePassword"]));
                    }
                    else
                    {
                        options.AddEncryptionCertificate(Configuration["OpenIddict:EncryptionThumbprint"], StoreName.My, StoreLocation.CurrentUser);
                        options.AddSigningCertificate(Configuration["OpenIddict:SigningThumbprint"], StoreName.My, StoreLocation.CurrentUser);
                    }

                    options.AcceptAnonymousClients();   //no client_id parameter needed when request a token
                    options.UseAspNetCore()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough();
                })

                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });       // note: this works only for default token format but NOT for JWT       // note: this works only for default token format but NOT for JWT
            #endregion -- Register OpenIddict

            services.AddSignalR();


            // Add GleamTech document viewer
            services.AddGleamTech();
            if (!string.IsNullOrEmpty(Configuration["DocumentUltimate:LicenseKeyV7"]))
            {
                DocumentUltimateConfiguration.Current.LicenseKey = Configuration["DocumentUltimate:LicenseKeyV7"];    // supposedly this is not needed but Gleamtech seems unable to pull license info from appSettings automatically
            }

            //help files azure blob file provider
            //https://github.com/watertrans/AzureBlobFileProvider
            if (!string.IsNullOrEmpty(Configuration["Help:AzureStorage:ServiceUri"]))
            {
                services.AddAzureBlobFileProvider(options =>
                {
                    options.ServiceUri = new Uri(Configuration["Help:AzureStorage:ServiceUri"]);
                    options.Token = Configuration["Help:AzureStorage:Token"];
                    options.ContainerName = Configuration["Help:AzureStorage:ContainerName"];
                    options.LocalCacheTimeout = int.Parse(Configuration["Help:AzureStorage:LocalCacheTimeout"] ?? "300");
                });
            }

            // The default CacheLocation value is "~/App_Data/DocumentCache"
            // Both virtual and physical paths are allowed (or a Location instance for one of the supported file systems like Amazon S3 and Azure Blob).
            DocumentUltimateWebConfiguration.Current.CacheLocation = Environment.ContentRootPath + @"\UserFiles\Documents\ViewerCache";
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //option to show HTML error responses to aid in debugging
                if (!string.IsNullOrEmpty(Configuration["Debug:UseDeveloperExceptionPage"]) && bool.Parse(Configuration["Debug:UseDeveloperExceptionPage"]))                    
                    app.UseDeveloperExceptionPage();
                else
                    app.UseExceptionHandler("/Error");
            }

            app.UseHsts();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.None
            });

            // Security headers
            app.Use((context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                return next.Invoke();
            });

            // AQB removed during debloat

            // Register GleamTech document viewer (SHOULD come before app.UseStaticFiles)
            app.UseGleamTech();

            //app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx => {
                    ctx.Context.Response.Headers.Append(
                         "strict-transport-security", $"max-age={30 * 24 * 60 * 60}");
                }
            });
            //app.UseMiddleware<SecurityHeadersMiddleware>();

            // CORS can be made more secure by adding CORS policy to the controller
            // see: https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-2.1#enable-cors-with-attributes-1
            if (!string.IsNullOrEmpty(Configuration["EmailAddIn:OutlookOriginUrl"]) || !string.IsNullOrEmpty(Configuration["EmailAddIn:GmailOriginUrl"]))
            {
                app.UseCors(_emailAddInCORSPolicy);
            }

            app.UseAuthentication();
            //app.UseProtectFolder(new ProtectFolderOptions { Path = "/UserFiles", PolicyName = "CanAccessShared" });

            //should be avoided, userfiles is a protected folder
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(
            //    Path.Combine(Directory.GetCurrentDirectory(), @"UserFiles")),
            //    RequestPath = new PathString("/UserFiles")
            //});

            //help files
            IFileProvider? helpFileProvider = null;
            if (string.IsNullOrEmpty(Configuration["Help:AzureStorage:ServiceUri"])) {
                //unc path file provider
                var helpFilePath = Configuration["Help:FilePath"];
                if (string.IsNullOrEmpty(helpFilePath) || !Directory.Exists(helpFilePath))
                    helpFilePath = Path.Combine(Directory.GetCurrentDirectory(), @"Help");
                helpFileProvider = new PhysicalFileProvider(helpFilePath);
            } 
            else
            {
                //azure blob file provider
                helpFileProvider = app.ApplicationServices.GetRequiredService<AzureBlobFileProvider>();
            }
            if (helpFileProvider != null)
            {
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = helpFileProvider,
                    RequestPath = new PathString("/Help"),
                    OnPrepareResponse = ctx => {
                        var user = ctx.Context.User;
                        var request = ctx.Context.Request;
                        var response = ctx.Context.Response;

                        if (!user.Identity.IsAuthenticated)
                        {
                            var returnUrl = WebUtility.UrlEncode($"{request.PathBase}{request.Path}");

                            response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            response.Redirect($"{request.Scheme}://{request.Host}{request.PathBase}/Login?ReturnUrl={returnUrl}");
                        }
                        else
                        {
                            if (!request.Path.StartsWithSegments($"/Help/{user.GetHelpFolder()}", StringComparison.OrdinalIgnoreCase))
                            {
                                response.StatusCode = (int)HttpStatusCode.NotFound;
                                response.ContentLength = 0;
                                response.Body = Stream.Null;
                            }
                        }
                    }
                });
            }

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                   name: "detailLink",
                   template: "{area:exists}/{controller}/DetailLink",
                   defaults: new { action = "DetailLink" });

                routes.MapRoute(
                   name: "areaRoute",
                   template: "{area:exists}/{controller}/{action=Index}/{id?}");

                //routes.MapRoute(
                //    name: "detail",
                //    template: "{area:exists}/{controller}/{id?}",
                //    defaults: new { action = "Detail" });

                routes.MapRoute(
                    name: "root",
                    template: "{action=Index}",
                    defaults: new { controller = "Home" });

                routes.MapRoute(
                    name: "userProfile",
                    template: "Account/{action=Index}",
                    defaults: new { controller = "Manage" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "notifications",
                    template: "Notifications",
                    defaults: new { controller = "Notification", action = "Index", area = "Admin" });
            });

            //app.UseKendo(env); // Configure Kendo UI

            //app.UseSignalR(builder =>
            //{
            //    builder.MapHub<NotificationHub>("/notify",configureOptions=> {
            //        configureOptions.Transports = HttpTransportType.WebSockets;
            //    });

            //});

            //Show Personal Identifiable Information (PII) in error logs.
            //Useful in debugging.
            //No longer needed in .NET 6.0??
            //Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

       
    }
}
