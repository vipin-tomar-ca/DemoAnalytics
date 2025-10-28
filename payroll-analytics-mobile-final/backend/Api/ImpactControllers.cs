namespace PayrollAnalytics.Api;

public static class ImpactControllers
{
    public static object GetFinanceImpact()
    {
        var rnd = new Random(41);
        string[] orgs = ["Sales","Engineering","Ops","Support","Finance","HR","Marketing"];
        var data = orgs.Select(o => new {
            org = o,
            prRatio = Math.Round(30 + rnd.NextDouble()*25, 1), // payroll to revenue %
            salesPerEmp = rnd.Next(120000, 380000),
            productivityIndex = rnd.Next(1,10)
        });
        return new { orgs = data };
    }
}
