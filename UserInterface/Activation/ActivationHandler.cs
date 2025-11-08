// Created:  2025/10/29
// Solution: WindowsConfigurationAnalyzer
// Project:  UserInterface
// File:  ActivationHandler.cs
// 
// All Rights Reserved 2025
// Kyle L Crowder



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Activation;



// Extend this class to implement new ActivationHandlers. See DefaultActivationHandler for an example.
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/activation.md
public abstract class ActivationHandler<T> : IActivationHandler
    where T : class
{
    public bool CanHandle(object args)
    {
        return args is T && CanHandleInternal((args as T)!);
    }





    public async Task HandleAsync(object args)
    {
        await HandleInternalAsync((args as T)!);
    }





    // Override this method to add the logic for whether to handle the activation.
    protected virtual bool CanHandleInternal(T args)
    {
        return true;
    }





    // Override this method to add the logic for your activation handler.
    protected abstract Task HandleInternalAsync(T args);
}