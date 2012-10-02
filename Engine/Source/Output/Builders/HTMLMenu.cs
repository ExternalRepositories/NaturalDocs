﻿/* 
 * Class: GregValure.NaturalDocs.Engine.Output.Builders.HTMLMenu
 * ____________________________________________________________________________
 * 
 * A reusable helper class to build the JavaScript menu data for <Output.Builders.HTML>.  See <JavaScript Menu Data> 
 * for the output format.
 * 
 * 
 * Usage:
 * 
 *		- Fill in the menu data as described in the documentation for <Menu>.
 *		- Call <Build()>.
 *		- Ta da.
 *	
 * 
 * Threading: Not Thread Safe
 * 
 *		This class is only designed to be used by one thread at a time.  It has an internal state that is used during a call to
 *		<Build()>, and another <Build()> should not be started until it's completed.  Instead each thread should create its 
 *		own object.
 *		
 */

// This file is part of Natural Docs, which is Copyright © 2003-2012 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using System.Collections.Generic;
using System.Text;
using GregValure.NaturalDocs.Engine.Collections;
using GregValure.NaturalDocs.Engine.Output.MenuEntries;


namespace GregValure.NaturalDocs.Engine.Output.Builders
	{
	public class HTMLMenu : Menu
		{

		// Group: Functions
		// __________________________________________________________________________


		/* Constructor: HTMLMenu
		 */
		public HTMLMenu (Builders.HTML htmlBuilder) : base ()
			{
			this.htmlBuilder = htmlBuilder;
			}


		/* Function: Build
		 * Generates JSON files for all entries in the menu.  It returns a <StringTable> mapping the file type strings ("files", 
		 * "classes", etc.) to a <IDObjects.NumberSet> representing all the files that were generated.  So "files.js", "files2.js",
		 * and "files3.js" would map to "files" -> [1-3].
		 */
		public StringTable<IDObjects.NumberSet> Build ()
			{
			try
				{  
				// This will create multiple subdirectories if needed, and will not throw an exception if it already exists.
				System.IO.Directory.CreateDirectory(htmlBuilder.Menu_DataFolder);  
				}
			catch
				{
				throw new Exceptions.UserFriendly( 
					Locale.Get("NaturalDocs.Engine", "Error.CouldNotCreateOutputFolder(name)", htmlBuilder.Menu_DataFolder) 
					);
				}


			// Build menu files

			StringTable<IDObjects.NumberSet> outputFiles = new StringTable<IDObjects.NumberSet>(false, false);

			if (RootFileMenu != null)
				{  
				GenerateJSON(RootFileMenu);
				SegmentMenu(RootFileMenu, "files", ref outputFiles);
				BuildOutput(RootFileMenu);
				}


			// Build tab information file

			StringBuilder tabInformation = new StringBuilder("NDMenu.OnTabsLoaded([");
			#if DONT_SHRINK_FILES
				tabInformation.Append('\n');
			#endif

			List<MenuEntries.Base.Container> tabContainers = new List<MenuEntries.Base.Container>();
			List<string> tabTypes = new List<string>();

			// DEPENDENCY: tabTypes must use the same strings as the NDLocation JavaScript class.
			// DEPENDENCY: tabTypes must use strings safe for including in CSS names.

			if (RootFileMenu != null)
				{
				tabContainers.Add(RootFileMenu);
				tabTypes.Add("File");
				}

			for (int i = 0; i < tabContainers.Count; i++)
				{
				ContainerExtraData extraData = (ContainerExtraData)tabContainers[i].ExtraData;

				#if DONT_SHRINK_FILES
					tabInformation.Append(' ', IndentSpaces);
				#endif

				tabInformation.Append("[\"");
				tabInformation.Append(tabTypes[i]);
				tabInformation.Append("\",\"");
				tabInformation.StringEscapeAndAppend( tabContainers[i].Title.ToHTML() );
				tabInformation.Append("\",");

				if (extraData.HashPath == null)
					{  tabInformation.Append("undefined");  }
				else
					{
					tabInformation.Append('"');
					tabInformation.StringEscapeAndAppend(extraData.HashPath);
					tabInformation.Append('"');
					}

				tabInformation.Append(",\"");
				tabInformation.StringEscapeAndAppend(extraData.DataFileName );
				tabInformation.Append("\"]");

				if (i < tabContainers.Count - 1)
					{  tabInformation.Append(',');  }

				#if DONT_SHRINK_FILES
					tabInformation.Append('\n');
				#endif
				}

			#if DONT_SHRINK_FILES
				tabInformation.Append(' ', IndentSpaces);
			#endif
			tabInformation.Append("]);");

			System.IO.File.WriteAllText( htmlBuilder.Menu_DataFile("tabs", 1), tabInformation.ToString() );


			return outputFiles;
			}


		/* Function: GenerateJSON
		 * Generates JSON for all the entries in the passed container.
		 */
		protected void GenerateJSON (MenuEntries.Base.Container container)
			{
			ContainerExtraData containerExtraData = new ContainerExtraData(container);
			container.ExtraData = containerExtraData;

			containerExtraData.GenerateJSON(htmlBuilder, this);

			foreach (var member in container.Members)
				{
				if (member is MenuEntries.Base.Target)
					{
					TargetExtraData targetExtraData = new TargetExtraData((MenuEntries.Base.Target)member);
					member.ExtraData = targetExtraData;

					targetExtraData.GenerateJSON(htmlBuilder, this);
					}
				else if (member is MenuEntries.Base.Container)
					{
					GenerateJSON((MenuEntries.Base.Container)member);
					}
				}
			}


		/* Function: SegmentMenu
		 * Segments the menu into smaller pieces and generates data file names.
		 */
		protected void SegmentMenu (MenuEntries.Base.Container container, string dataFileType, 
															  ref StringTable<IDObjects.NumberSet> usedDataFiles)
			{
			// Generate the data file name for this container.

			IDObjects.NumberSet usedDataFileNumbers = usedDataFiles[dataFileType];

			if (usedDataFileNumbers == null)
				{
				usedDataFileNumbers = new IDObjects.NumberSet();
				usedDataFiles.Add(dataFileType, usedDataFileNumbers);
				}
			
			int dataFileNumber = usedDataFileNumbers.LowestAvailable;
			usedDataFileNumbers.Add(dataFileNumber);

			ContainerExtraData extraData = (ContainerExtraData)container.ExtraData;
			extraData.DataFileName = htmlBuilder.Menu_DataFileNameOnly(dataFileType, dataFileNumber);


			// The data file has to include all the members in this container no matter what.

			int containerJSONSize = extraData.JSONBeforeMembers.Length + extraData.JSONAfterMembers.Length + 
														extraData.JSONLengthOfMembers;

			List<MenuEntries.Base.Container> subContainers = null;

			foreach (var member in container.Members)
				{
				if (member is MenuEntries.Base.Container)
					{
					if (subContainers == null)
						{  subContainers = new List<MenuEntries.Base.Container>();  }

					subContainers.Add((MenuEntries.Base.Container)member);
					}
				}


			// Now start including the contents of subcontainers until we reach the size limit.  We're going breadth-first instead of
			// depth first.

			List<MenuEntries.Base.Container> nextSubContainers = null;

			for (;;)
				{
				if (subContainers == null || subContainers.Count == 0)
					{
					if (nextSubContainers == null || nextSubContainers.Count == 0)
						{  break;  }
					else
						{
						subContainers = nextSubContainers;
						nextSubContainers = null;
						}
					}

				// Add subcontainers to the file in the order from smallest to largest.  This prevents one very large container early
				// in the list from causing all the other ones to be broken out into separate files.
				// DEPENDENCY: ContainerExtraData.JSONLengthOfMembers must cache its value for this algorithm to be efficient.

				int smallestSubContainerIndex = 0;
				int smallestSubContainerSize = (subContainers[0].ExtraData as ContainerExtraData).JSONLengthOfMembers;

				for (int i = 1; i < subContainers.Count; i++)
					{
					if ((subContainers[i].ExtraData as ContainerExtraData).JSONLengthOfMembers < smallestSubContainerSize)
						{
						smallestSubContainerIndex = i;
						smallestSubContainerSize = (subContainers[i].ExtraData as ContainerExtraData).JSONLengthOfMembers;
						}
					}

				containerJSONSize += smallestSubContainerSize;

				if (containerJSONSize > SegmentLength)
					{  break;  }

				foreach (var member in subContainers[smallestSubContainerIndex].Members)
					{
					if (member is MenuEntries.Base.Container)
						{
						if (nextSubContainers == null)
							{  nextSubContainers = new List<MenuEntries.Base.Container>();  }

						nextSubContainers.Add((MenuEntries.Base.Container)member);
						}
					}

				subContainers.RemoveAt(smallestSubContainerIndex);
				}


			// Now recurse through any remaining subcontainers so they get their own files.

			if (subContainers != null)
				{
				foreach (var subContainer in subContainers)
					{  SegmentMenu(subContainer, dataFileType, ref usedDataFiles);  }
				}

			if (nextSubContainers != null)
				{
				foreach (var subContainer in nextSubContainers)
					{  SegmentMenu(subContainer, dataFileType, ref usedDataFiles);  }
				}
			}


		/* Function: BuildOutput
		 * Generates the output file for the container.  It must have <ContainerExtraData.DataFileName> set.  If it finds
		 * any sub-containers that also have that set, it will recursively generate files for them as well.
		 */
		protected void BuildOutput (MenuEntries.Base.Container container)
			{
			#if DEBUG
			if (container.ExtraData == null || (container.ExtraData as ContainerExtraData).StartsNewDataFile == false)
				{  throw new Exception ("BuildOutput() can only be called on containers with DataFileName set.");  }
			#endif

			Stack<MenuEntries.Base.Container> containersToBuild = new Stack<MenuEntries.Base.Container>();
			containersToBuild.Push(container);

			while (containersToBuild.Count > 0)
				{
				MenuEntries.Base.Container containerToBuild = containersToBuild.Pop();
				string fileName = (containerToBuild.ExtraData as ContainerExtraData).DataFileName;
				
				StringBuilder output = new StringBuilder();
				output.Append("NDMenu.OnSectionLoaded(\"");
				output.StringEscapeAndAppend(fileName);
				output.Append("\",[");

				#if DONT_SHRINK_FILES
				output.AppendLine();
				#endif
				
				AppendMembers(containerToBuild, output, 1, containersToBuild);

				#if DONT_SHRINK_FILES
				output.Append(' ', IndentSpaces);
				#endif

				output.Append("]);");

				System.IO.File.WriteAllText(htmlBuilder.Menu_DataFolder + "/" + fileName, output.ToString());
				}
			}


		/* Function: AppendMembers
		 * A support function for <BuildOutput()>.  Appends the output of the container's members to the string, recursively 
		 * going through sub-containers as well.  This will not include the surrounding brackets, only the comma-separated
		 * member entries.  If it finds any sub-containers that start a new data file, it will add them to containersToBuild.
		 */
		protected void AppendMembers (MenuEntries.Base.Container container, StringBuilder output, int indent, 
																  Stack<MenuEntries.Base.Container> containersToBuild)
			{
			for (int i = 0; i < container.Members.Count; i++)
				{
				var member = container.Members[i];

				#if DONT_SHRINK_FILES
				output.Append(' ', indent * IndentSpaces);
				#endif

				if (member is MenuEntries.Base.Target)
					{
					TargetExtraData targetExtraData = (TargetExtraData)member.ExtraData;
					output.Append(targetExtraData.JSON);
					}
				else if (member is MenuEntries.Base.Container)
					{
					ContainerExtraData containerExtraData = (ContainerExtraData)member.ExtraData;
					output.Append(containerExtraData.JSONBeforeMembers);

					if (containerExtraData.StartsNewDataFile)
						{
						output.Append('"');
						output.StringEscapeAndAppend(containerExtraData.DataFileName);
						output.Append('"');

						containersToBuild.Push((MenuEntries.Base.Container)member);
						}
					else
						{
						output.Append('[');

						#if DONT_SHRINK_FILES
						output.AppendLine();
						#endif

						AppendMembers((MenuEntries.Base.Container)member, output, indent + 1, containersToBuild);

						#if DONT_SHRINK_FILES
						output.Append(' ', (indent + 1) * IndentSpaces);
						#endif

						output.Append(']');
						}

					output.Append(containerExtraData.JSONAfterMembers);
					}
				#if DEBUG
				else
					{  throw new Exception ("Can't append JSON for menu entry " + member.Title + ".");  }
				#endif

				if (i < container.Members.Count - 1)
					{  output.Append(',');  }

				#if DONT_SHRINK_FILES
				output.AppendLine();
				#endif
				}
			}



		// Group: Variables
		// __________________________________________________________________________
			
		/* var: htmlBuilder
			* The <Builders.HTML> object associated with this menu.
			*/
		protected Builders.HTML htmlBuilder;


		// Group: Constants
		// __________________________________________________________________________

		/* Constant: IndentSpaces
		 * The number of spaces to indent each level by when building the output with <DONT_SHRINK_FILES>.
		 */
		protected int IndentSpaces = 3;

		/* const: SegmentLength
		 * The amount of data to try to fit in each JSON file before splitting it off into another one.  This will be
		 * artificially low in debug builds to better test the loading mechanism.
		 */
		#if DEBUG
			protected const int SegmentLength = 1024*3;
		#else
			protected const int SegmentLength = 1024*32;
		#endif



		/* ____________________________________________________________________________
		 * 
		 * Class: GregValure.NaturalDocs.Engine.Output.Builders.HTMLMenu.TargetExtraData
		 * ____________________________________________________________________________
		 * 
		 * A class used to store extra information needed by <HTMLMenu> in each menu entry via the 
		 * ExtraData property.
		 * 
		 */
		private class TargetExtraData
			{

			// Group: Functions
			// _________________________________________________________________________

			/* Function: TargetExtraData
			 */
			public TargetExtraData (MenuEntries.Base.Target menuEntry)
				{
				this.menuEntry = menuEntry;
				this.json = null;
				}

			/* Function: GenerateJSON
			 */
			public void GenerateJSON (Builders.HTML htmlBuilder, HTMLMenu menu)
				{
				#if DEBUG
				if ((menuEntry is MenuEntries.File.File) == false)
					{  throw new Exception("HTMLMenu can only generate JSON for target entries that are files.");  }
				#endif

				StringBuilder output = new StringBuilder();

				output.Append("[1,\"");

				string htmlTitle = menuEntry.Title.ToHTML();
				output.StringEscapeAndAppend(htmlTitle);

				output.Append('"');

				string hashPath = htmlBuilder.Source_OutputFileNameOnlyHashPath( (menuEntry as MenuEntries.File.File).WrappedFile.FileName );

				if (hashPath != htmlTitle)
					{
					output.Append(",\"");
					output.StringEscapeAndAppend(hashPath);
					output.Append('"');
					}

				output.Append(']');

				json = output.ToString();
				}


			// Group: Properties
			// _________________________________________________________________________

			/* Property: JSON
			 * After <GenerateJSON()> is called, this is the JSON output for this entry.
			 */
			public string JSON
				{
				get
					{  return json;  }
				}


			// Group: Variables
			// _________________________________________________________________________

			/* var: menuEntry
			 * The menu entry associated with this object.
			 */
			protected MenuEntries.Base.Target menuEntry;

			/* var: json
			 * The generated JSON for this entry.
			 */
			protected string json;

			}



		/* ____________________________________________________________________________
		 * 
		 * Class: GregValure.NaturalDocs.Engine.Output.Builders.HTMLMenu.ContainerExtraData
		 * ____________________________________________________________________________
		 * 
		 * A class used to store extra information needed by <HTMLMenu> in each menu entry via the 
		 * ExtraData property.
		 * 
		 */
		private class ContainerExtraData
			{

			// Group: Functions
			// _________________________________________________________________________

			public ContainerExtraData (MenuEntries.Base.Container menuEntry)
				{
				this.menuEntry = menuEntry;
				this.jsonBeforeMembers = null;
				this.jsonAfterMembers = null;
				this.jsonLengthOfMembers = -1;
				this.dataFileName = null;
				this.hashPath = null;
				}

			/* Function: GenerateJSON
			 */
			public void GenerateJSON (Builders.HTML htmlBuilder, HTMLMenu menu)
				{
				#if DEBUG
				if (menuEntry is MenuEntries.File.Folder && menuEntry.Parent == null)
					{  throw new Exception("Parent must be defined before generating JSON for a folder.");  }
				#endif

				StringBuilder output = new StringBuilder();

				output.Append("[2,");


				// Title

				if (menuEntry.CondensedTitles == null)
					{
					// xxx This is for single file sources that don't have titles.  This shouldn't be necessary once condensing is turned on.
					if (menuEntry.Title == null)
						{  output.Append("undefined");  }
					else
						{
						output.Append('"');
						output.StringEscapeAndAppend(menuEntry.Title.ToHTML());
						output.Append('"');
						}
					}
				else
					{
					output.Append("[\"");
					output.StringEscapeAndAppend(menuEntry.Title.ToHTML());
					output.Append('"');

					foreach (string condensedTitle in menuEntry.CondensedTitles)
						{
						output.Append(",\"");
						output.StringEscapeAndAppend(condensedTitle.ToHTML());
						output.Append('"');
						}

					output.Append(']');
					}


				// Hash path

				output.Append(',');

				if (menuEntry is MenuEntries.File.FileSource)
					{
					MenuEntries.File.FileSource fileSourceEntry = (MenuEntries.File.FileSource)menuEntry;
					hashPath = htmlBuilder.Source_OutputFolderHashPath( fileSourceEntry.WrappedFileSource.Number,
																															  fileSourceEntry.CondensedPathFromFileSource );
					}
				else if (menuEntry is MenuEntries.File.Folder)
					{
					MenuEntries.Base.Container container = menuEntry.Parent;

					while ((container is MenuEntries.File.FileSource) == false)
						{
						container = container.Parent;

						#if DEBUG
						if (container == null)
							{  throw new Exception ("Couldn't find a file source among the folder's parents when generating JSON.");  }
						#endif
						}

					MenuEntries.File.Folder folderEntry = (MenuEntries.File.Folder)menuEntry;
					MenuEntries.File.FileSource fileSourceEntry = (MenuEntries.File.FileSource)container;

					hashPath = htmlBuilder.Source_OutputFolderHashPath( fileSourceEntry.WrappedFileSource.Number, 
																															  folderEntry.PathFromFileSource );
					}
				else if (menuEntry == menu.RootFileMenu)
					{
					hashPath = null;
					}
				#if DEBUG
				else
					{  throw new Exception ("Could not generate JSON for container " + menuEntry.Title + ".");  }
				#endif

				if (hashPath == null)
					{  output.Append("undefined");  }
				else
					{  
					output.Append('"');
					output.StringEscapeAndAppend(hashPath);
					output.Append('"');
					}

				output.Append(',');

				jsonBeforeMembers = output.ToString();
				jsonAfterMembers = "]";
				}


			// Group: Properties
			// _________________________________________________________________________

			/* Property: StartsNewDataFile
			 * Whether this container starts a new data file.  This property is read-only.  If you need to change
			 * it, set <DataFileName> instead.
			 */
			public bool StartsNewDataFile
				{
				get
					{  return (dataFileName != null);  }
				}

			/* Property: DataFileName
			 * If this container starts a new data file this will be its file name, such as "files2.js" or "classes.js".  It will
			 * not include a path.  If this container doesn't start a new data file, this will be null.
			 */
			public string DataFileName
				{
				get
					{  return dataFileName;  }
				set
					{  dataFileName = value;  }
				}

			/* Property: JSONBeforeMembers
			 * After <GenerateJSON()> is called, this will be the JSON output of this entry up to the point where its members
			 * would appear.
			 */
			public string JSONBeforeMembers
				{
				get
					{  return jsonBeforeMembers;  }
				}

			/* Property: JSONAfterMembers
			 * After <GenerateJSON()> is called, this will be the JSON output of this entry after the point where its members
			 * would appear.
			 */
			public string JSONAfterMembers
				{
				get
					{  return jsonAfterMembers;  }
				}

			/* Property: JSONLengthOfMembers
			 * The calculated total JSON length of all members stored directly in this container.  It does NOT recurse into deeper
			 * containers.
			 */
			public int JSONLengthOfMembers
				{
				get
					{
					// DEPENDENCY: HTMLMenu.SegmentMenu expects this value to only be calculated once despite repeated calls for 
					// its algorithm to be efficient.

					if (jsonLengthOfMembers != -1)
						{  return jsonLengthOfMembers;  }

					jsonLengthOfMembers = 0;

					foreach (var member in menuEntry.Members)
						{
						if (member is MenuEntries.Base.Target)
							{
							jsonLengthOfMembers += (member.ExtraData as TargetExtraData).JSON.Length;
							}
						else // container
							{
							ContainerExtraData extraData = (ContainerExtraData)member.ExtraData;
							jsonLengthOfMembers += extraData.JSONBeforeMembers.Length + extraData.JSONAfterMembers.Length;
							}
						}

					return jsonLengthOfMembers;
					}
				}

			/* Property: HashPath
			 * The hash path of the container, or null if none.  This will only be available after <GenerateJSON()> is called.
			 */
			public string HashPath
				{
				get
					{  return hashPath;  }
				}



			// Group: Variables
			// _________________________________________________________________________

			/* var: menuEntry
			 * The menu entry associated with this object.
			 */
			protected MenuEntries.Base.Container menuEntry;

			/* var: jsonBeforeMembers
			 * The generated JSON for this entry, up to the point where its members would be inserted.
			 */
			protected string jsonBeforeMembers;

			/* var: jsonAfterMembers
			 * The generated JSON for this entry, after the point where its members would be inserted.
			 */
			protected string jsonAfterMembers;

			/* var: jsonLengthOfMembers
			 * The calculated total JSON length of all members directly stored in this container, or -1 if it hasn't been
			 * calculated yet.  It does NOT recurse into deeper levels.
			 */
			protected int jsonLengthOfMembers;

			/* var: dataFileName
			 * If this container starts a new data file this will be its file name, such as "files2.js" or "classes.js".  It will
			 * not include a path.  If this container doesn't start a new data file, this will be null.
			 */
			protected string dataFileName;

			/* var: hashPath
			 * The hash path of the container, or null if none.  This will only be available after <GenerateJSON()> is called.
			 */
			protected string hashPath;

			}

		}
	}
