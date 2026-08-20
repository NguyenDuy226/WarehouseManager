namespace Warehouse.Application.Report.DTO;

public record ProfileDTO(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    IList<string> Roles
);