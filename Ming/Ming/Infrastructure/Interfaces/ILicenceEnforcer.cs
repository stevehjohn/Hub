namespace Ming.Infrastructure.Interfaces
{
    internal enum LicenceStatus
    {
        TrialPeriod,
        TrialExpired,
        Licensed
    }

    internal interface ILicenceEnforcer
    {
        LicenceStatus Status { get; }

        int TrialDaysRemaining { get; }

        bool IsValidModule(int moduleId);

        bool ValidateLicence(string emailAddress, string licence);
    }
}
