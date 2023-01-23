#region Using directives
using System;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.Core;
using FTOptix.UI;
using FTOptix.Alarm;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using FTOptix.DataLogger;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.EventLogger;
using FTOptix.WebUI;
#endregion

public class ChangeUserButtonLogic : BaseNetLogic
{
    public override void Start()
    {
        ComboBox comboBox = Owner.Owner.Get<ComboBox>("Username");
        if (Project.Current.AuthenticationMode == AuthenticationMode.ModelOnly)
        {
            comboBox.Mode = ComboBoxMode.Normal;
        }
        else
        {
            comboBox.Mode = ComboBoxMode.Editable;
        }
    }

    [ExportMethod]
    public void PerformChangeUser(string username, string password)
    {
        var usersAlias = LogicObject.GetAlias("Users");
        if (usersAlias == null || usersAlias.NodeId == NodeId.Empty)
        {
            Log.Error("ChangeUserButtonLogic", "Missing Users alias");
            return;
        }

        var passwordExpiredDialogType = LogicObject.GetAlias("PasswordExpiredDialogType") as DialogType;
        if (passwordExpiredDialogType == null)
        {
            Log.Error("ChangeUserButtonLogic", "Missing PasswordExpiredDialogType alias");
            return;
        }

        Button changeUserButton = (Button)Owner;
        changeUserButton.Enabled = false;

        try
        {
            var outputMessageLabel = Owner.Owner.GetObject("ChangeUserFormOutputMessage");
            var outputMessageLabelResultCode = outputMessageLabel.GetVariable("LoginResultCode");

            var loginResult = Session.ChangeUser(username, password);
            outputMessageLabelResultCode.Value = (int)loginResult.ResultCode;

            switch (loginResult.ResultCode)
            {
                case ChangeUserResultCode.Success:
                    TextBox passwordTextBox = Owner.Owner.Get<TextBox>("Password");
                    passwordTextBox.Text = String.Empty;
                    break;
                case ChangeUserResultCode.PasswordExpired:
                    changeUserButton.Enabled = true;
                    var user = usersAlias.Get<User>(username);
                    var ownerButton = (Button)Owner;
                    ownerButton.OpenDialog(passwordExpiredDialogType, user.NodeId);
                    return;
                case ChangeUserResultCode.UserNotFound:
                    changeUserButton.Enabled = true;
                    Log.Error("ChangeUserButtonLogic", "Could not find user " + username);
                    return;
                case ChangeUserResultCode.UnableToCreateUser:
                    changeUserButton.Enabled = true;
                    Log.Error("ChangeUserButtonLogic", "Unable to create user " + username);
                    return;
                case ChangeUserResultCode.LoginAttemptBlocked:
                    changeUserButton.Enabled = true;
                    Log.Error("ChangeUserButtonLogic", "Login attempt blocked");
                    return;
            }

            string outputMessage = outputMessageLabel.GetVariable("Message").Value;
            var outputMessageLogic = outputMessageLabel.GetObject("ChangeUserFormOutputMessageLogic");
            outputMessageLogic.ExecuteMethod("SetOutputMessage", new object[] { outputMessage });
        }
        catch (Exception e)
        {
            Log.Error("ChangeUserButtonLogic", e.Message);
        }

        changeUserButton.Enabled = true;
    }

}
