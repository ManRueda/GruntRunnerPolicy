using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Diagnostics;

namespace GruntRunnerPolicy {

	[Serializable]
	public class GruntRunnerPolicy: PolicyBase {
        GruntRunner runner;

        //Description of the policy
		public override string Description {
			get {
				return@"This policy runs a custom Grunt Task. If the task fails, the check-in is rejected.";
			}
		}

        //Text to display to the user when he doesn't have the policy in his system.
		public override string InstallationInstructions {
			get {
				return@"Instructions abount how to get the policy.";
			}
		}

        //Type of policy, this string will be displayed in the policies combo
		public override string Type {
			get {
				return "Grunt validations";
			}
		}

        //Description of the type of policy
		public override string TypeDescription {
			get {
                return @"This policy runs a custom Grunt Task. If the task fails, the check-in is rejected.";
			}
		}

		public override bool Edit(IPolicyEditArgs args) {
			// Do not need any custom configuration
			return true;
		}

        //This method perfomance the check-in evaluation
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

        //This method is executed when the user double-clicks the UI of the policy
		public override void Activate(PolicyFailure failure) {
			MessageBox.Show("Run the grunt configurated task to see the errors in your code.", "How to fix your policy failure");
		}

        //This method is executed when the user ask for the policy help (F1)
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