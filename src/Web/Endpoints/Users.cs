using DotNetEvaluation.Infrastructure.Identity;

namespace DotNetEvaluation.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder
            .MapIdentityApi<ApplicationUser>();
    }
}
