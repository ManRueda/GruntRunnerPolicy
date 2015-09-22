using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Diagnostics;

namespace GruntRunnerPolicy {
	[Serializable]
	class GruntRunnerPolicy: PolicyBase {
        GruntRunner runner;

		public override string Description {
			get {
				return@"This policy runs a custom Grunt Task. If the task fails, the check-in is rejected.";
			}
		}

		// This is a string that is stored with the policy definition on the source
		// control server. If a user does not have the policy plug-in installed, this string
		// is displayed.  You can use this to explain to the user how they should 
		// install the policy plug-in.
		public override string InstallationInstructions {
			get {

				return@"Instructions abount how to get the policy.";
			}
		}

		// This string identifies the type of policy. It is displayed in the 
		// policy list when you add a new policy to a Team Project.
		public override string Type {
			get {
				return "Grunt validations";
			}
		}

		// This string is a description of the type of policy. It is displayed 
		// when you select the policy in the Add Check-in Policy dialog box.
		public override string TypeDescription {
			get {
                return @"This policy runs a custom Grunt Task. If the task fails, the check-in is rejected.";
			}
		}

		// This method is called by the policy framework when you create 
		// a new check-in policy or edit an existing check-in policy.
		// You can use this to display a UI specific to this policy type 
		// allowing the user to change the parameters of the policy.
		public override bool Edit(IPolicyEditArgs args) {
			// Do not need any custom configuration
			return true;
		}

		// This method performs the actual policy evaluation. 
		// It is called by the policy framework at various points in time
		// when policy should be evaluated. In this example, the method 
		// is invoked when various asyc events occur that may have 
		// invalidated the current list of failures.

		public override PolicyFailure[] Evaluate() {

			var configs = new List<string>();

			foreach(var file in this.PendingCheckin.PendingChanges.CheckedPendingChanges) {

                var configFilePath = Options.SearchGruntParentDirectory(Path.GetDirectoryName(file.LocalItem));

                if (configFilePath != null)
                {
                    if (configs.IndexOf(configFilePath) == -1)
                    {
                        configs.Add(configFilePath);
					}
				}
			}

			var success = true;

			foreach(var config in configs) {
                var opts = Options.Deserialize(config);
                runner = new GruntRunner(opts);
                var exitCode = runner.Run();

				success = exitCode == 0;
				if (!success) {
					break;
				}
			}
			if (!success) {
				return new PolicyFailure[] {
					new PolicyFailure("The grunt task fails. View log -> ", this)
				};
			} else {
				return new PolicyFailure[0];
			}
		}

		// This method is called if the user double-clicks on 
		// a policy failure in the UI. In this case a message telling the user 
		// to supply some comments is displayed.
		public override void Activate(PolicyFailure failure) {
			MessageBox.Show("Run the grunt configurated task to see the errors in your code.", "How to fix your policy failure");
		}

		// This method is called if the user presses F1 when a policy failure 
		// is active in the UI. In this example, a message box is displayed.
		public override void DisplayHelp(PolicyFailure failure) {
            if (runner != null)
            {
                if (String.IsNullOrEmpty(runner.Log))
                {
                    MessageBox.Show("Not log found", "Grunt task log");
                }
                else
                {
                    MessageBox.Show(runner.Log, "Grunt task log");
                }
            }
		}
	}
}