#region Using directives
using System;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.OPCUAServer;
using FTOptix.Alarm;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using FTOptix.DataLogger;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.EventLogger;
#endregion

public class ChangeUserFormOutputMessageLogic : BaseNetLogic
{
    public override void Start()
    {
        HideMessageLabel();
        messageVariable = Owner.GetVariable("Message");

        task = new DelayedTask(() =>
        {
            HideMessageLabel();
            taskStarted = false;
        }, 10000, LogicObject);
    }

    public override void Stop()
    {
        task?.Dispose();
    }

    [ExportMethod]
    public void SetOutputMessage(string message)
    {
        if (messageVariable == null)
        {
            Log.Error("ChangeUserOutputMessageLogic", "Unable to find Message variable in LoginFormOutputMessage label");
            return;
        }
        ShowMessageLabel();
        messageVariable.Value = message;

        if (taskStarted)
        {
            task?.Cancel();
            taskStarted = false;
        }

        task.Start();
        taskStarted = true;
    }

    private void ShowMessageLabel()
    {
        var messageLabel = (Label)Owner;
        messageLabel.Visible = true;
    }

    private void HideMessageLabel()
    {
        var messageLabel = (Label)Owner;
        messageLabel.Visible = false;
    }

    private DelayedTask task;
    private bool taskStarted = false;
    private IUAVariable messageVariable;
}
