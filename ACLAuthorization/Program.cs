using ACLAuthorization.Helper;
using ACLAuthorization.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ACLAuthorization.Controllers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<Context>(options =>
{
    options.UseMySQL(connectionString);

});

// add authentication bearer
var key = builder.Configuration["Jwt:AccessKey"]; // get key from app settings
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // check issue default = true
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            // check audience default = true
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            // check expire
            RequireExpirationTime = true,
            ValidateLifetime = true,
            // check token signkey
            IssuerSigningKey = signingKey,
            RequireSignedTokens = true
        };
    });

builder.Services.AddTransient<IJWTTokenServices, JWTTokenServices>();

builder.Services.AddSwaggerGen(c =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. <br/>
                      Enter your token in the text input below.
                      <br/>Example: '12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TodoAPI", Version = "v1" });
    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        { jwtSecurityScheme, new List<string>() }
      });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ACLs", policy =>
        policy.RequireAssertion(async context =>
        {
            var canAccess = false;
            var httpContext = context.Resource as HttpContext;

            // get method
            var method = httpContext.Request.Method.ToString();
            // get endpoint /api/controller/...
            var endpoint = httpContext.GetEndpoint();
            var routePattern = (endpoint as RouteEndpoint)?.RoutePattern;
            var endpointValue = "";

            // convert endpoint from /api/{id}/... to /api/:id to match resource in permission
            if (routePattern != null)
            {
                endpointValue = routePattern.RawText;

            }

            if (httpContext.User.Identity.IsAuthenticated)
            {
                // get dbContext from httpContext
                var dbContext = httpContext.RequestServices.GetRequiredService<Context>();
                // get roleClaims and check role can access endpoint
                var roleClaims = httpContext.User.Claims.Where(claim => claim.Type == ClaimTypes.Role);
                foreach (var claim in roleClaims)
                {
                    if (canAccess)
                        break;
                    var roleName = claim.Value;
                    var roleEntity = dbContext.roles.Include(role => role.Permissions).FirstOrDefault(role => role.Name == roleName);
                    foreach (var permission in roleEntity.Permissions)
                    {
                        if (permission.Resource == StringConverter.ConvertToColonFormat(endpointValue) && MethodConverter.ConvertToString(permission.Method).ToLower() == method.ToLower())
                        {
                            canAccess = true;
                            break;
                        }
                    }
                }

            }
            //httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            //httpContext.Response.ContentType = "application/json";
            //httpContext.Response.StatusCode = 403;
            //await httpContext.Response.WriteAsync("Bạn không có quyền truy cập vào tài nguyên này.");
            //httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            //await httpContext.Response.WriteAsync("Bạn không có quyền truy cập vào tài nguyên này.");
            return canAccess;
        }));
});

var app = builder.Build();

app.UseStatusCodePages(async statusCodeContext =>
{
    var exceptionFeature = statusCodeContext.HttpContext.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionFeature?.Error;
    var problemDetails = new ProblemDetails
    {
        Status = statusCodeContext.HttpContext.Response.StatusCode,
        Type = exception?.GetType().Name,
        Title = "An unhandled error occurred",
        Detail = exception?.Message
    };
    switch (statusCodeContext.HttpContext.Response.StatusCode)
    {
        case 401:
            problemDetails.Detail = "Unauthorized";
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Title = "Unauthorized";
            break;
        case 403:
            problemDetails.Detail = "Forbidden";
            problemDetails.Status = StatusCodes.Status403Forbidden;
            problemDetails.Title = "Forbidden";
            break;
        case 500:
            problemDetails.Detail = "Internal Server Error";
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Internal Server Error";
            break;
    }
    statusCodeContext.HttpContext.Response.ContentType = "application/json";
    await statusCodeContext.HttpContext.Response.WriteAsJsonAsync(problemDetails);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
