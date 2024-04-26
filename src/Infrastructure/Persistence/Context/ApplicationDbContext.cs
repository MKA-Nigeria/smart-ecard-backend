using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Application.Common.Events;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Context;
public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions options, ICurrentUser currentUser, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(options, currentUser, serializer, dbSettings, events)
    {
    }
}