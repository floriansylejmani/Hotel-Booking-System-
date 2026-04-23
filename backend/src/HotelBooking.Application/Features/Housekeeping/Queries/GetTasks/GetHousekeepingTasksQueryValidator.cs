using FluentValidation;

namespace HotelBooking.Application.Features.Housekeeping.Queries.GetTasks;

public sealed class GetHousekeepingTasksQueryValidator : AbstractValidator<GetHousekeepingTasksQuery>
{
    public GetHousekeepingTasksQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
