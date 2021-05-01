using System;
using System.Security.Claims;

namespace Fitmeplan.ServiceBus.Core
{
    public interface ICorrelationContext
    {
        Guid Id { get; }
        int UserId { get; }
        int? CurrentOrgId { get; set; }
        Guid ResourceId { get; }
        string Name { get; }
        string Origin { get; }
        string Resource { get; }
        string Culture { get; }
        DateTime CreatedAt { get; }
        ClaimsPrincipal ClaimsPrincipal { get; }
    }
}