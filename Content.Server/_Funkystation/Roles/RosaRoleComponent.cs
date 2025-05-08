using Content.Shared.Roles;

namespace Content.Server._Funkystation.Roles;

[RegisterComponent]
public sealed partial class RosaRoleComponent : BaseMindRoleComponent
{
    [ViewVariables]
    public RosaAgentRole Role { get; set; } = RosaAgentRole.Pending;
}

public enum RosaAgentRole
{
    Pending,

    Red,
    Green,
    Blue,
}
