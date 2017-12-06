﻿// The MIT License (MIT) 
// Copyright (c) 1994-2017 The Sage Group plc or its licensors.  All rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of 
// this software and associated documentation files (the "Software"), to deal in 
// the Software without restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
// Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using Sage.CA.SBS.ERP.Sage300.UpgradeWizard.Properties;

namespace Sage.CA.SBS.ERP.Sage300.UpgradeWizard
{
    /// <summary> UI for Sage 300 Upgrade Wizard </summary>
    public partial class Upgrade : Form
    {
        #region Private Vars

        /// <summary> Process Upgrade logic </summary>
        private ProcessUpgrade _upgrade;

        /// <summary> Wizard Steps </summary>
        private readonly List<WizardStep> _wizardSteps = new List<WizardStep>();

        /// <summary> Current Wizard Step </summary>
        private int _currentWizardStep;

        /// <summary> Settings for Processing </summary>
        private Settings _settings;

        /// <summary> Log file </summary>
        private readonly StringBuilder _log = new StringBuilder();

        /// <summary> Source Folder </summary>
        private readonly string _sourceFolder;

        /// <summary> Destination Folder </summary>
        private readonly string _destinationFolder;

        /// <summary> Destination Web Folder </summary>
        private readonly string _destinationWebFolder;

        /// <summary> Destination Web </summary>
        private readonly string _destinationWeb;

        #endregion

        #region Private Constants
        /// <summary> Splitter Distance </summary>
        private const int SplitterDistance = 375;
        #endregion

        #region Delegates

        /// <summary> Delegate to update UI with name of the step being processed </summary>
        /// <param name="text">Text for UI</param>
        private delegate void ProcessingCallback(string text);

        /// <summary> Delegate to update log with status of the step being processed </summary>
        /// <param name="text">Text for UI</param>
        private delegate void LogCallback(string text);


        #endregion

        #region Constructor

        /// <summary> Upgrade Class </summary>
        /// <param name="destination">Destination Default</param>
        /// <param name="destinationWeb">Destination Web Default</param>
        /// <param name="templatePath">Upgrade Web Items template Path </param>
        public Upgrade(string destination, string destinationWeb, string templatePath)
        {
            InitializeComponent();
            Localize();
            InitWizardSteps();
            InitEvents();
            ProcessingSetup(true);
            Processing("");

            // Setup local vars
            _destinationFolder = destination;
            _destinationWeb = destinationWeb;
            _sourceFolder = Path.GetDirectoryName(templatePath);
            _destinationWebFolder = Directory.GetDirectories(_destinationFolder).FirstOrDefault(dir => dir.ToLower().Contains(ProcessUpgrade.WebSuffix));

        }

        #endregion

        #region Button Events

        /// <summary> Next/Upgrade toolbar button </summary>
        /// <param name="sender">Sender object </param>
        /// <param name="e">Event Args </param>
        /// <remarks>Next wizard step or Upgrade if last step</remarks>
        private void btnNext_Click(object sender, EventArgs e)
        {
            NextStep();
        }

        /// <summary> Back toolbar button </summary>
        /// <param name="sender">Sender object </param>
        /// <param name="e">Event Args </param>
        /// <remarks>Back wizard step</remarks>
        private void btnBack_Click(object sender, EventArgs e)
        {
            BackStep();
        }

        #endregion

        #region Private Methods/Routines/Events

        /// <summary> Initialize wizard steps </summary>
        private void InitWizardSteps()
        {
            // Default
            btnBack.Enabled = false;

            // Current Step
            _currentWizardStep = -1;

            // Init wizard steps
            _wizardSteps.Clear();

            // Same for all upgrades, but the content will be specific to the release
            AddStep(Resources.StepTitleMain, string.Format(Resources.StepDescriptionMain, ProcessUpgrade.FromReleaseNumber, ProcessUpgrade.ToReleaseNumber), 
                BuildMainContentStep());
            AddStep(Resources.ReleaseAllTitleSyncWebFiles, Resources.ReleaseAllDescSyncWebFiles, 
                Resources.ReleaseAllSyncWebFiles);
            // This step can be commented if no accpac update this release
            AddStep(Resources.ReleaseAllTitleSyncAccpacLibs, Resources.ReleaseAllDescSyncAccpacLibs, 
                string.Format(Resources.ReleaseAllSyncAccpacLibs, ProcessUpgrade.FromAccpacNumber, ProcessUpgrade.ToAccpacNumber));


            // Specific to release steps go here
            // 2018.1 Release: Manual step to modify Login function
            AddStep(Resources.ReleaseSpecificTitleModifyLogin, Resources.ReleaseSpecificDescModifyLogin,
                Resources.ReleaseSpecificModifyLogin);

            // Same for all upgrades, but the content will be specific to the release
            AddStep(Resources.ReleaseAllTitleConfirmation, Resources.ReleaseAllDescConfirmation,
                Resources.ReleaseAllUpgrade);
            AddStep(Resources.ReleaseAllTitleRecompile, Resources.ReleaseAllDescRecompile,
                string.Format(Resources.ReleaseAllUpgraded, Resources.ShowLog, ProcessUpgrade.ToReleaseNumber));

            // Display first step
            NextStep();
        }

        /// <summary> Build Main Content Step </summary>
        /// <returns>Content for main screen</returns>
        private static string BuildMainContentStep()
        {
            var content = new StringBuilder();
            var step = 0;

            // Same for all upgrades
            content.AppendLine(Resources.FollowingSteps);
            content.AppendLine("");
            content.AppendLine(string.Format("{0} {1}. {2}", Resources.Step, ++step, Resources.ReleaseAllTitleSyncWebFiles));
            content.AppendLine(string.Format("{0} {1}. {2}", Resources.Step, ++step, Resources.ReleaseAllTitleSyncAccpacLibs));

            // Specific to release
            content.AppendLine(string.Format("{0} {1}. {2}", Resources.Step, ++step, Resources.ReleaseSpecificTitleModifyLogin));

            // Same for all upgrades
            content.AppendLine(string.Format("{0} {1}. {2}", Resources.Step, ++step, Resources.ReleaseAllTitleConfirmation));
            content.AppendLine(string.Format("{0} {1}. {2}", Resources.Step, ++step, Resources.ReleaseAllTitleRecompile));
            content.AppendLine("");
            content.AppendLine(Resources.EnsureBackup);

            return content.ToString();
        }

        /// <summary> Add wizard step </summary>
        /// <param name="title">Title for wizard step</param>
        /// <param name="description">Description for wizard step</param>
        /// <param name="content">Content for wizard step</param>
        /// <param name="showCheckbox">Optional. True to show checkbox otherwise false</param>
        /// <param name="checkboxText">Optional. Checkbox text</param>
        /// <param name="checkboxValue">Optional. Checkbox value</param>
        private void AddStep(string title, string description, string content, 
            bool showCheckbox = false, string checkboxText = "", bool checkboxValue = false)
        {
            _wizardSteps.Add(new WizardStep
            {
                Title = title,
                Description = description,
                Content = content,
                ShowCheckbox = showCheckbox,
                CheckboxText = checkboxText,
                CheckboxValue = checkboxValue
            });
        }

        /// <summary> Next/Generate Navigation </summary>
        /// <remarks>Next wizard step or Generate if last step</remarks>
        private void NextStep()
        {
            // Finished?
            if (!_currentWizardStep.Equals(-1) && _currentWizardStep.Equals(_wizardSteps.Count - 1))
            {
                Close();
            }
            else
            {
                // Proceed to next wizard step or start upgrade if upgrade step
                if (!_currentWizardStep.Equals(-1) &&
                    _currentWizardStep.Equals(_wizardSteps.Count - 2))
                {
                    // Setup display before processing
                    ProcessingSetup(false);

                    _settings = new Settings
                    {
                        WizardSteps = _wizardSteps,
                        SourceFolder = _sourceFolder,
                        DestinationWebFolder = _destinationWebFolder
                    };

                    // Start background worker for processing (async)
                    wrkBackground.RunWorkerAsync(_settings);
                }
                else
                {
                    // Proceed to next step
                    if (!_currentWizardStep.Equals(-1))
                    {
                        // Enable back button
                        btnBack.Enabled = true;
                    }

                    // Increment step
                    _currentWizardStep++;

                    // Update title and text for step
                    ShowStep();

                    // Update text of Next button?
                    if (_currentWizardStep.Equals(_wizardSteps.Count - 2))
                    {
                        btnNext.Text = Resources.Upgrade;
                    }

                    // Update text of Next and buttons?
                    if (_currentWizardStep.Equals(_wizardSteps.Count - 1))
                    {
                        btnBack.Text = Resources.ShowLog;
                        btnNext.Text = Resources.Finish;
                    }
                }
            }
        }

        /// <summary> Back Navigation </summary>
        /// <remarks>Back wizard step</remarks>
        private void BackStep()
        {
            // Proceed if not on first step
            if (!_currentWizardStep.Equals(0))
            {
                // Show the log
                if (_currentWizardStep.Equals(_wizardSteps.Count - 1))
                {
                    var logPath = Path.Combine(_destinationFolder, ProcessUpgrade.LogFileName);
                    System.Diagnostics.Process.Start(logPath);
                    return;
                }

                btnNext.Text = Resources.Next;
                _currentWizardStep--;

                // Update title and text for step
                ShowStep();

                // Enable back button?
                if (_currentWizardStep.Equals(0))
                {
                    btnBack.Enabled = false;
                    btnNext.Enabled = true;
                }

            }
        }
        
        /// <summary> Show Step Page</summary>
        private void ShowStep()
        {
            // Update title and text for step
            var step = _currentWizardStep.Equals(0) ? "" : Resources.Step + _currentWizardStep.ToString("#0") + Resources.Dash;

            lblStepTitle.Text = step + _wizardSteps[_currentWizardStep].Title;
            lblStepDescription.Text = _wizardSteps[_currentWizardStep].Description;

            // Display information
            lblContent.Text = _wizardSteps[_currentWizardStep].Content;

            // Checkbox
            checkBox.Text = _wizardSteps[_currentWizardStep].CheckboxText;
            checkBox.Checked = _wizardSteps[_currentWizardStep].CheckboxValue;
            splitStep.Panel2Collapsed = !_wizardSteps[_currentWizardStep].ShowCheckbox;

            splitSteps.SplitterDistance = SplitterDistance;
        }

        /// <summary>
        /// Write log file to upgrade solution folder
        /// </summary>
        private void WriteLogFile()
        {
            var logFilePath = Path.Combine(_destinationFolder, ProcessUpgrade.LogFileName);
            File.WriteAllText(logFilePath, _log.ToString());
        }


        /// <summary> Setup processing display </summary>
        /// <param name="enableToolbar">True to enable otherwise false</param>
        private void ProcessingSetup(bool enableToolbar)
        {
            pnlButtons.Enabled = enableToolbar;
            pnlButtons.Refresh();
        }

        /// <summary> Update processing display </summary>
        /// <param name="text">Text to display in status bar</param>
        private void Processing(string text)
        {
            lblProcessing.Text = string.IsNullOrEmpty(text) ? text : string.Format(Resources.ProcessingStep, text);

            pnlButtons.Refresh();
        }

        /// <summary> Update processing display </summary>
        /// <param name="text">Text to display</param>
        /// <remarks>Invoked from threaded process</remarks>
        private void ProcessingEvent(string text)
        {
            var callBack = new ProcessingCallback(Processing);
            Invoke(callBack, text);
        }

        /// <summary> Background worker started event </summary>
        /// <param name="sender">Sender object </param>
        /// <param name="e">Event Args </param>
        /// <remarks>Background worker will run process</remarks>
        private void wrkBackground_DoWork(object sender, DoWorkEventArgs e)
        {
            _upgrade.Process((Settings)e.Argument);
        }

        /// <summary> Background worker completed event </summary>
        /// <param name="sender">Sender object </param>
        /// <param name="e">Event Args </param>
        /// <remarks>Background worker has completed process</remarks>
        private void wrkBackground_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProcessingSetup(true);
            Processing("");

            _currentWizardStep++;

            // Update title and text for step
            ShowStep();

            // Display final step
            btnBack.Text = Resources.ShowLog;
            btnNext.Text = Resources.Finish;

            // Write out log file with upgrade being complete
            WriteLogFile();

        }

        /// <summary> Help Button</summary>
        /// <param name="sender">Sender object </param>
        /// <param name="e">Event Args </param>
        /// <remarks>Disabled help until DPP wiki is available</remarks>
        private void Generation_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            // Display wiki link
            //System.Diagnostics.Process.Start(Resources.Browser, Resources.WikiLink);
        }

        /// <summary> Localize </summary>
        private void Localize()
        {
            Text = Resources.SolutionUpgrade;

            btnBack.Text = Resources.Back;
            btnBack.Enabled = false;

            btnNext.Text = Resources.Next;
        }

        /// <summary> Initialize events for process generation class </summary>
        private void InitEvents()
        {
            // Processing Events
            _upgrade = new ProcessUpgrade();
            _upgrade.ProcessingEvent += ProcessingEvent;
            _upgrade.LogEvent += LogEvent;
        }

        /// <summary> Update Log </summary>
        /// <param name="text">Text for Log</param>
        /// <remarks>Invoked from threaded process</remarks>
        private void Log(string text)
        {
            _log.AppendLine(text);
        }

        /// <summary> Log Event </summary>
        /// <param name="text">Text for log</param>
        /// <remarks>Invoked from threaded process</remarks>
        private void LogEvent(string text)
        {
            var callBack = new LogCallback(Log);
            Invoke(callBack, text);
        }

        /// <summary> Store value selected in Wizard step</summary>
        /// <param name="sender">Sender object </param>
        /// <param name="e">Event Args </param>
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            // Stores value in step
            _wizardSteps[_currentWizardStep].CheckboxValue = checkBox.Checked;
        }

        #endregion

    }
}
