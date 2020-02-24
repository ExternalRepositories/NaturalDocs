﻿/* 
 * Class: CodeClear.NaturalDocs.Engine.Tests.Runner
 * ____________________________________________________________________________
 * 
 * A simple class to run NUnit tests from a Visual Studio console project which allows you to set breakpoints
 * in the tests and debug them.  You can pass a test or group of tests to run as a command line option,
 * such as "LinkScoring", or not pass anything to run all tests.
 * 
 */

// This file is part of Natural Docs, which is Copyright © 2003-2020 Code Clear LLC.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using System.Text;
using System.Text.RegularExpressions;
using CodeClear.NaturalDocs.Engine;

namespace CodeClear.NaturalDocs.Engine.Tests
	{
	class Runner
		{
		static void Main (string[] commandLineOptions)
			{
			testGroup = null;
			showNUnitOutput = false;
			pauseOnError = false;
			pauseBeforeExit = false;

			try
				{
				System.Console.WriteLine();
				System.Console.WriteLine("Natural Docs Engine Tests");
				System.Console.WriteLine("Version " + Engine.Instance.VersionString);
				System.Console.WriteLine("-------------------------");

				var assembly = System.Reflection.Assembly.GetExecutingAssembly();
				Path assemblyFolder = Path.FromAssembly(assembly).ParentFolder;
				Path dllPath = assemblyFolder + "/NaturalDocs.Engine.Tests.dll";

				if (!ParseCommandLine(commandLineOptions))
					{  return;  }


				// Build the parameter string

				string nunitParams = "/work:\"" + assemblyFolder + "\"";

				if (testGroup != null)
					{  nunitParams += " /fixture:CodeClear.NaturalDocs.Engine.Tests." + testGroup;  }

				nunitParams += " \"" + dllPath + "\"";


				// Use nunit-console-x86.exe to run the tests.  Capture its output if desired.

				string nunitOutput = null;

				using (System.Diagnostics.Process nunitProcess = new System.Diagnostics.Process())
					{
					nunitProcess.StartInfo.FileName = assemblyFolder + "/nunit-console-x86.exe";
					nunitProcess.StartInfo.Arguments = nunitParams;
					nunitProcess.StartInfo.UseShellExecute = false;
					nunitProcess.StartInfo.RedirectStandardOutput = !showNUnitOutput;

					if (testGroup == null)
						{  System.Console.WriteLine("Running all tests...");  }
					else
						{  System.Console.WriteLine("Running " + testGroup + " tests...");  }

					if (showNUnitOutput)
						{  System.Console.WriteLine();  }

					nunitProcess.Start();

					if (!showNUnitOutput)
						{
						// This MUST be done before WaitForExit(), counterintuitive as that may be.
						// See https://msdn.microsoft.com/en-us/library/system.diagnostics.process.standardoutput(v=vs.110).aspx
						nunitOutput = nunitProcess.StandardOutput.ReadToEnd();
						}

					nunitProcess.WaitForExit();

					if (showNUnitOutput)
						{  System.Console.WriteLine();  }
					}


				// Attempt to extract the failure count and notices from the generated XML file.

				Path xmlPath = assemblyFolder + "/TestResult.xml";
				string xmlContent = System.IO.File.ReadAllText(xmlPath);

				// <failure>
				// <message><![CDATA[1 out of 1 test failed for F:\Projects\Natural Docs 2\Source\Engine.Tests.Data\Comments\XML\Parsing:
				// - Test: Expected output file missing
				// ]]></message>
				// <stack-trace><![CDATA[at CodeClear.NaturalDocs.Engine.Tests.Framework.SourceToTopics.TestFolder(Path testFolder, Path projectConfigFolder) in F:\Projects\Natural Docs 2\Source\Engine.Tests\Source\Framework\SourceToTopics.cs:line 123
				// at CodeClear.NaturalDocs.Engine.Tests.Comments.XML.Parsing.All() in F:\Projects\Natural Docs 2\Source\Engine.Tests\Source\Comments\XML\Parsing.cs:line 20
				// ]]></stack-trace>
				// </failure>

				int foundFailures = 0;
				StringBuilder failures = new StringBuilder();
				MatchCollection failureMatches = Regex.Matches(xmlContent, @"<failure>.*?<message><!\[CDATA\[(.*?)\]\]>.*?</message>.*?</failure>", RegexOptions.Singleline);

				foreach (Match failureMatch in failureMatches)
					{  
					failures.AppendLine(failureMatch.Groups[1].ToString());
					foundFailures++;
					}

				//<test-results 
				//		name="F:\Projects\Natural Docs 2\Source\Engine.Tests.Runner\bin\Debug\NaturalDocs.Engine.Tests.dll"
				//		total="1" errors="0" failures="0" not-run="0" inconclusive="0" ignored="0" skipped="0" invalid="0"
				//		date="2012-03-06" time="11:02:19"
				//>
				Match failureCountMatch = System.Text.RegularExpressions.Regex.Match(xmlContent, @"<test-results.+errors=\""([0-9]+)\"".+failures=\""([0-9]+)\"".*>");
				int failureCount = -1;

				if (failureCountMatch.Success)
					{
					failureCount = int.Parse(failureCountMatch.Groups[1].Value) + int.Parse(failureCountMatch.Groups[2].Value);
					}


				// Display the output, falling back to the captured console output if our XML extraction didn't work.

				if (failureCount == -1 || failureCount != foundFailures)
					{  
					if (!showNUnitOutput)
						{
						System.Console.WriteLine();
						System.Console.Write(nunitOutput);
						}

					if (pauseOnError)
						{  pauseBeforeExit = true;  }
					}
				else if (failureCount != 0)
					{
					if (!showNUnitOutput)
						{
						System.Console.WriteLine();
						System.Console.Write(failures.ToString());
						}

					if (pauseOnError)
						{  pauseBeforeExit = true;  }
					}
				else
					{
					if (!showNUnitOutput)
						{  System.Console.WriteLine("All tests successful.");  }
					}

				System.Console.WriteLine("-------------------------");
				}
			catch (Exception e)
				{
				if (pauseOnError)
					{  pauseBeforeExit = true;  }

				System.Console.WriteLine("-------------------------");
				System.Console.WriteLine();
				System.Console.WriteLine("Exception: " + e.Message);
				}

			if (pauseBeforeExit)
				{
				System.Console.WriteLine();
				System.Console.WriteLine("Press any key to continue...");
				System.Console.ReadKey(false);
				}
			}


		private static bool ParseCommandLine (string[] commandLineSegments)
			{
			Engine.CommandLine commandLine = new CommandLine(commandLineSegments);

			commandLine.AddAliases("--test-group", "--testgroup", "--test", "--tests");
			commandLine.AddAliases("--show-nunit-output", "--shownunitoutput", "--show-nunit", "--shownunit");
			commandLine.AddAliases("--pause-before-exit", "--pausebeforexit", "--pause");
			commandLine.AddAliases("--pause-on-error", "--pauseonerror");
			commandLine.AddAliases("--help", "-h", "-?");
			
			string parameter, parameterAsEntered;
			bool isFirst = true;
				
			while (commandLine.IsInBounds)
				{
				// If the first segment isn't a parameter, it's the test group
				if (isFirst && !commandLine.IsOnParameter)
					{
					parameter = "--test-group";
					parameterAsEntered = parameter;
					}
				else
					{  
					if (!commandLine.GetParameter(out parameter, out parameterAsEntered))
						{
						System.Console.Error.WriteLine("Unrecognized parameter \"" + parameterAsEntered + "\"");
						return false;
						}
					}

				isFirst = false;

					
				// Test Group

				if (parameter == "--test-group")
					{
					if (!commandLine.GetBareWord(out testGroup))
						{
						System.Console.Error.WriteLine("\"" + parameterAsEntered + "\" must be followed by a test group name");
						return false;
						}
					}


				// Show NUnit Output

				else if (parameter == "--show-nunit-output")
					{
					if (!commandLine.NoValue())
						{
						System.Console.Error.WriteLine("\"" + parameterAsEntered + "\" can't be followed by a value");
						return false;
						}
					else
						{
						showNUnitOutput = true;
						}
					}


				// Pause Before Exit

				else if (parameter == "--pause-before-exit")
					{
					if (!commandLine.NoValue())
						{
						System.Console.Error.WriteLine("\"" + parameterAsEntered + "\" can't be followed by a value");
						return false;
						}
					else
						{
						pauseBeforeExit = true;
						}
					}


				// Pause On Error

				else if (parameter == "--pause-on-error")
					{
					if (!commandLine.NoValue())
						{
						System.Console.Error.WriteLine("\"" + parameterAsEntered + "\" can't be followed by a value");
						return false;
						}
					else
						{
						pauseOnError = true;
						}
					}


				// Help
				
				else if (parameter == "--help")
					{
					System.Console.WriteLine("TestRunner [group].  If no group is specified all tests will be run.");
					return false;
					}


				// Everything else

				else
					{
					System.Console.Error.WriteLine("Unrecognized parameter \"" + parameterAsEntered + "\"");
					return false;
					}
				}
				
				
			// Done.

			return true;				
			}



		// Group: Variables
		// __________________________________________________________________________

		private static string testGroup;

		private static bool showNUnitOutput;
		private static bool pauseBeforeExit;
		private static bool pauseOnError;

		}
	}
