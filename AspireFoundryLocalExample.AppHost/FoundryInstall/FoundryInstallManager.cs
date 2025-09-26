using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class FoundryLocalInstallManager
{
    private bool alreadyCheched;
    private bool isFoundryInstalled;

    private ILogger<FoundryLocalInstallManager> logger;

    public FoundryLocalInstallManager(ILogger<FoundryLocalInstallManager> logger)
    {
        this.logger = logger;
    }

    public void Install()
    {
        if (!IsInstalled())
        {
            logger.LogInformation("Foundry Local not yet installed. Installing... (this might take a few minutes)");

            if (System.OperatingSystem.IsMacOS())
            {
                Process installProcess = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "brew",
                        Arguments = "install foundrylocal",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                installProcess.StartInfo.EnvironmentVariables.Add("NONINTERACTIVE", "1");
                installProcess.Start();
                installProcess.WaitForExit();

            }
            else if (System.OperatingSystem.IsWindows())
            {
                Process installProcess = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "install Microsoft.FoundryLocal --accept-package-agreements --accept-source-agreements --silent",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                installProcess.Start();
                installProcess.WaitForExit();
            }
            else
            {
                logger.LogInformation("OS Not supported");
                return;
            }
        }
        else
        {
            logger.LogInformation("Foundry Local already installed.");
        }
    }

    public bool IsInstalled()
    {
        if (alreadyCheched) return isFoundryInstalled;

        if (System.OperatingSystem.IsMacOS())
        {
            string packageId = "FoundryLocal";
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "brew",
                    Arguments = $"info {packageId}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            isFoundryInstalled = !output.Contains("Not Installed", StringComparison.OrdinalIgnoreCase);
            alreadyCheched = true;
            return isFoundryInstalled;

        }
        else if (System.OperatingSystem.IsWindows())
        {
            string packageId = "Microsoft.FoundryLocal";
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = $"list --id={packageId}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            isFoundryInstalled = output.Contains(packageId, StringComparison.OrdinalIgnoreCase);
            alreadyCheched = true;
            return isFoundryInstalled;
        }
        else
        {
            throw new PlatformNotSupportedException("Foundry Local installazion only work on Windows or MacOS platform");
        }
    }

}

public static class FoundryLocalInstallExtensions
{
    public static IResourceBuilder<Aspire.Hosting.Azure.AzureAIFoundryResource> FoundryLocalInstall(this IResourceBuilder<Aspire.Hosting.Azure.AzureAIFoundryResource> builder)
    {
        builder.ApplicationBuilder.Services.AddSingleton<FoundryLocalInstallManager>();
        
        builder.OnInitializeResource(static (resource, @event, cancellationToken) =>
        {
            var logger = @event.Services.GetRequiredService<ILogger<FoundryLocalInstallManager>>();
            var foundryLocalInstallManager = @event.Services.GetRequiredService<FoundryLocalInstallManager>();

            logger.LogInformation("Ensure Foundry Local as installed");

            foundryLocalInstallManager.Install();

            return Task.CompletedTask;
        });

        return builder;
    }
}
