using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;

public class RegistryProbe
{
    private readonly string _registryKeyPath;
    private readonly string _registryValueName;





    public RegistryProbe(string registryKeyPath, string registryValueName)
    {
        _registryKeyPath = registryKeyPath;
        _registryValueName = registryValueName;
    }





    public string ReadRegistryValue()
    {
        // In a real application, you would use Microsoft.Win32.Registry to read the value.
        // For demonstration purposes, we'll return a placeholder.
        // Example:
        // using Microsoft.Win32;
        // try
        // {
        //     using (RegistryKey key = Registry.LocalMachine.OpenSubKey(_registryKeyPath))
        //     {
        //         if (key != null)
        //         {
        //             return key.GetValue(_registryValueName)?.ToString();
        //         }
        //     }
        // }
        // catch (System.Exception)
        // {
        //     // Handle exceptions appropriately
        // }

        return $"Placeholder value for Key: {_registryKeyPath}, Value: {_registryValueName}";
    }
}
