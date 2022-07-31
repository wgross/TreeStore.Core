using System.Collections.ObjectModel;
using System.Management.Automation.Host;
using System.Security.AccessControl;

namespace TreeStore.Core.Providers;

public interface ICmdletProvider
{
    public char AltItemSeparator { get; }

    public PSCredential Credential { get; }

    public PSTransactionContext CurrentPSTransaction { get; }

    public Collection<string> Exclude { get; }

    public string Filter { get; }

    public object DynamicParameters { get; }

    public SwitchParameter Force { get; }

    public PSHost Host { get; }

    public Collection<string> Include { get; }

    public CommandInvocationIntrinsics InvokeCommand { get; }

    public ProviderIntrinsics InvokeProvider { get; }

    public char ItemSeparator { get; }

    public SessionState SessionState { get; }

    public bool Stopping { get; }

    public string GetResourceString(string baseName, string resourceId);

    public bool ShouldContinue(string query, string caption);

    public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll);

    public bool ShouldProcess(string target);

    public bool ShouldProcess(string target, string action);

    public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption);

    public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason);

    public void ThrowTerminatingError(ErrorRecord errorRecord);

    public bool TransactionAvailable();

    public void WriteDebug(string text);

    public void WriteError(ErrorRecord errorRecord);

    public void WriteInformation(InformationRecord record);

    public void WriteInformation(object messageData, string[] tags);

    public void WriteItemObject(object item, string path, bool isContainer);

    public void WriteProgress(ProgressRecord progressRecord);

    public void WritePropertyObject(object propertyValue, string path);

    public void WriteSecurityDescriptorObject(ObjectSecurity securityDescriptor, string path);

    public void WriteVerbose(string text);

    public void WriteWarning(string text);
}