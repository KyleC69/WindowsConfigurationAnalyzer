using System.Collections.Specialized;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
