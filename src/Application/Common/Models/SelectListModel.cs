namespace Application.Common.Models;

public class SelectListModel
{
    public dynamic Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class SelectListModelDynamic : SelectListModel
{
    public dynamic ValueOne { get; set; }
    public dynamic ValueTwo { get; set; }
    public dynamic ValueThree { get; set; }
    public dynamic ValueFour { get; set; }
    public dynamic ValueFive { get; set; }
    public dynamic ValueSix { get; set; }
    public dynamic ValueSeven { get; set; }

}
