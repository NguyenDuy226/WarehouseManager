namespace Warehouse.Application.Interfaces
{
    public interface IValidatorName
    {
        string ValidHumanName(string input);
        string ValidName(string input);
    }
}