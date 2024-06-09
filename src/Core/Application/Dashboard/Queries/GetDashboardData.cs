namespace Application.Cards.Dashboard.Queries;
public class DashboardData
{
    public int TotalActiveCards { get; set; }
    public int TotalPrintedCards { get; set; }
    public int PendingPrintedCards { get; set; }
    public int TotalInactiveCards { get; set; }
    public int CardRenewalsThisMonth { get; set; }
    public int NewCardsIssued { get; set; }
    public int PendingApprovals { get; set; }
    public int TotalRequests { get; set; }
}
public class GetDashboardData : IRequest<DashboardData>
{

}

public class GetDashboardDataHandler(IRepository<Card> _cardRepository, IRepository<CardRequest> _cardRequestRepository) : IRequestHandler<GetDashboardData, DashboardData>
{
    public async Task<DashboardData> Handle(GetDashboardData request, CancellationToken cancellationToken)
    {
        int activeCards = await _cardRepository.CountAsync(x => x.Status == Domain.Enums.CardStatus.Active, cancellationToken);
        int inactiveCards = await _cardRepository.CountAsync(x => x.Status == Domain.Enums.CardStatus.InActive, cancellationToken);
        int issued = await _cardRepository.CountAsync(x => x.IsCollected, cancellationToken);
        int pendingPrinted = await _cardRepository.CountAsync(x => x.PrintStatus == Domain.Enums.PrintStatus.ReadyForPrint, cancellationToken);
        int printed = await _cardRepository.CountAsync(x => x.PrintStatus == Domain.Enums.PrintStatus.Printed, cancellationToken);
        int pendingApprovals = await _cardRequestRepository.CountAsync(x => x.Status == Domain.Enums.CardRequestStatus.Pending, cancellationToken);
        int totalCardRequest = await _cardRequestRepository.CountAsync(cancellationToken);
        return new DashboardData
        {
            TotalActiveCards = activeCards,
            TotalPrintedCards = printed,
            PendingPrintedCards = pendingApprovals,
            CardRenewalsThisMonth = 0,
            PendingApprovals = pendingApprovals,
            TotalInactiveCards = inactiveCards,
            NewCardsIssued = issued,
            TotalRequests = totalCardRequest
        };
    }
}

