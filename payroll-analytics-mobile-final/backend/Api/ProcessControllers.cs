namespace PayrollAnalytics.Api;

public static class ProcessControllers
{
    public static object GetEfficiency()
    {
        // Demo values: time to run payroll (hours) and accuracy rate (%)
        return new {
            timeToRunPayrollHours = 12.5,
            accuracyRatePct = 98.4
        };
    }
}
