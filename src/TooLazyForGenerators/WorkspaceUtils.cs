using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace TooLazyForGenerators;

internal static class WorkspaceUtils
{
    /// <summary>
    /// Registers an MSBuild locator if one is not already registered
    /// and returns a new <see cref="MSBuildWorkspace"/>.
    /// Remember to dispose the workspace!
    /// </summary>
    public static MSBuildWorkspace CreateWorkspace()
    {
        if (!MSBuildLocator.IsRegistered)
            MSBuildLocator.RegisterDefaults();
        
        return MSBuildWorkspace.Create();
    }
}
