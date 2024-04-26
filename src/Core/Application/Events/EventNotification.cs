
using Domain.Common.Events;
using MediatR;

namespace Application.Common.Events;
public class EventNotification<TEvent> : INotification
    where TEvent : IEvent
{
    public EventNotification(TEvent @event) => Event = @event;

    public TEvent Event { get; }
}