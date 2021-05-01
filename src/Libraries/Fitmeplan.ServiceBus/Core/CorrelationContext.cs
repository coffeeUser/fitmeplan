using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using Fitmeplan.Identity;

namespace Fitmeplan.ServiceBus.Core
{
    public class CorrelationContext : ICorrelationContext
    {
        public Guid Id { get; }
        public int UserId { get; }
        public int? CurrentOrgId { get; set; }
        public Guid ResourceId { get; }
        public string Name { get; }
        public string Origin { get; }
        public string Resource { get; }
        public string Culture { get; }
        public DateTime CreatedAt { get; }
        [JsonConverter(typeof(ClaimsPrincipalConverter))]
        public ClaimsPrincipal ClaimsPrincipal { get; }

        public CorrelationContext()
        {
            Id = Trace.CorrelationManager.ActivityId;
        }

        [JsonConstructor]
        private CorrelationContext(Guid id, ClaimsPrincipal claimsPrincipal, Guid resourceId, string name,
            string origin, string culture, string resource)
        {
            Id = id;
            UserId = claimsPrincipal?.GetUserId() ?? 0;
            CurrentOrgId = claimsPrincipal?.GetOrganisationId() ?? 0;
            ClaimsPrincipal = claimsPrincipal;
            ResourceId = resourceId;
            Name = string.IsNullOrWhiteSpace(name) ? string.Empty : GetName(name);
            Origin = string.IsNullOrWhiteSpace(origin) ? string.Empty : 
                origin.StartsWith("/") ? origin.Remove(0, 1) : origin;
            Culture = culture;
            Resource = resource;
            CreatedAt = DateTime.UtcNow;
        }

        public static ICorrelationContext Empty 
            => new CorrelationContext();

        public static ICorrelationContext From<T>(ICorrelationContext context)
            => Create<T>(context.Id, context.ClaimsPrincipal, context.ResourceId, context.Origin, context.Culture, context.Resource);

        public static ICorrelationContext Create<T>(Guid id, ClaimsPrincipal principal, Guid resourceId, string origin, string culture, string resource = "")
            => new CorrelationContext(id, principal, resourceId, typeof(T).Name, origin, culture, resource);

        private static string GetName(string name)
            => Underscore(name).ToLowerInvariant();

        private static string Underscore(string value)
            => string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
    }
}
