using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.Sdk;

namespace Sample8_CommandLibrary;

public class CommandLibraryExtension : PixiEditorExtension
{
    public override void OnInitialized()
    {
        CommandMetadata publicCommand = new CommandMetadata("PrintHelloWorld")
        {
            // All extensions can invoke this command
            InvokePermissions = InvokePermissions.Public
        };

        CommandMetadata internalCommand = new CommandMetadata("PrintHelloWorldFamily")
        {
            // All extensions with unique name starting with "yourCompany" can invoke this command
            InvokePermissions = InvokePermissions.Family
        };

        CommandMetadata privateCommand = new CommandMetadata("PrintHelloWorldPrivate")
        {
            // Only this extension can invoke this command
            InvokePermissions = InvokePermissions.Owner
        };

        CommandMetadata explicitCommand = new CommandMetadata("PrintHelloWorldExplicit")
        {
            // Only this extension and the ones listed in ExplicitlyAllowedExtensions can invoke this command
            InvokePermissions = InvokePermissions.Explicit,
            ExplicitlyAllowedExtensions = "yourCompany.Samples.Commands" // You can put multiple extensions by separating with ;
        };

        Api.Commands.RegisterCommand(publicCommand, () =>
        {
            Api.Logger.Log("Hello World from public command!");
        });

        Api.Commands.RegisterCommand(internalCommand, () =>
        {
            Api.Logger.Log("Hello World from internal command!");
        });

        Api.Commands.RegisterCommand(privateCommand, () =>
        {
            Api.Logger.Log("Hello World from private command!");
        });

        Api.Commands.RegisterCommand(explicitCommand, () =>
        {
            Api.Logger.Log("Hello World from explicit command!");
        });
    }
}