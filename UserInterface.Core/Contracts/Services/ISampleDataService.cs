using KC.WindowsConfigurationAnalyzer.UserInterface.Core.Models;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}
