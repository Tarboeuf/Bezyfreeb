﻿using BezyFB.Configuration;
using CommonPortableLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BezyFB.Helpers
{
    public class MessageDialogService : IMessageDialogService
    {
        public async Task AfficherMessage(string message)
        {
            if (MySettings.Current.AffichageErreurMessageBox)
                MessageBox.Show(message);
            else
                AddMessageBuffer(message);
        }

        public async Task<DialogResult> ShowYesNoDialog(string message)
        {
            if (MySettings.Current.AffichageErreurMessageBox)
                return MessageBox.Show(message, "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes ? DialogResult.Yes : DialogResult.No;
            return DialogResult.No;
        }

        public string MessageBuffer { get; set; }

        public void AddMessageBuffer(string message)
        {
            if (!string.IsNullOrEmpty(MessageBuffer))
                MessageBuffer += Environment.NewLine;
            MessageBuffer += message;
        }
    }
}
