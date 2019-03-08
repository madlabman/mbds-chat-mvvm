/*
 * This class present simple demo of using Protocol.
 */

using System;
using System.Collections.ObjectModel;
using System.Windows;
using ChatApp.Core;

namespace ChatApp.Client
{
    public static class Example
    {
        public static void Run()
        {
            Protocol.SignUp("J", "agent-j", "j_password");
            Protocol.SignUp("K", "agent-k", "k_password");
            Protocol.SignIn("agent-k", "k_password");
            var jDialog = Protocol.LoadDialog("agent-j");
            if (jDialog != null)
            {
                Protocol.SendMessageToTheDialog(jDialog, "Hello, space invaders are coming!");
            }

            Protocol.SignIn("agent-j", "j_password");
            var kDialog = Protocol.LoadDialog("agent-k");
            if (kDialog != null)
            {
                kDialog.Messages = Protocol.GetDialogMessages(kDialog);
                foreach (var message in Protocol.DecryptDialogMessages(kDialog))
                {
                    MessageBox.Show(message.Body);
                }
            }
        }
    }
}