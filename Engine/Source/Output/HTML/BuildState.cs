﻿/* 
 * Class: CodeClear.NaturalDocs.Engine.Output.HTML.BuildState
 * ____________________________________________________________________________
 * 
 * A class tracking information about a HTML output target.
 *		
 */

// This file is part of Natural Docs, which is Copyright © 2003-2020 Code Clear LLC.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using CodeClear.NaturalDocs.Engine.Collections;
using CodeClear.NaturalDocs.Engine.IDObjects;


namespace CodeClear.NaturalDocs.Engine.Output.HTML
	{
	public class BuildState
		{

		// Group: Functions
		// __________________________________________________________________________
		
		
		/* Constructor: BuildState
		 */
		public BuildState ()
			{
			sourceFilesWithContent = new NumberSet();
			classesWithContent = new NumberSet();
			usedImageFiles = new NumberSet();

			usedMenuDataFiles = new NumberSetTable<Hierarchy>();

			accessLock = new object();
			}


		/* Function: Lock
		 * 
		 * Locks the object to apply multiple changes efficiently or to directly access the variables.  This prevents other threads
		 * from accessing the object until you call <Unlock()>.
		 * 
		 * Use of the other functions will lock and unlock this object automatically, so it is not necessary for most use cases.
		 */
		public void Lock ()
			{
			System.Threading.Monitor.Enter(accessLock);
			}


		/* Function: Unlock
		 * Releases the thread lock applied by <Lock()>.
		 */
		public void Unlock ()
			{
			System.Threading.Monitor.Exit(accessLock);
			}



		// Group: Source File Functions
		// __________________________________________________________________________


		/* Function: SourceFileHasContent
		 * Returns whether the passed source file ID has content this build target can use.
		 */
		public bool SourceFileHasContent (int fileID)
			{
			lock (accessLock)
				{  return sourceFilesWithContent.Contains(fileID);  }
			}

		/* Function: AddSourceFileWithContent
		 * Tells the class that the passed file ID has content this build target can use.  Returns true if the number didn't 
		 * already exist in the set and was added, false if it was already in the set.
		 */
		public bool AddSourceFileWithContent (int fileID)
			{
			lock (accessLock)
				{  return sourceFilesWithContent.Add(fileID);  }
			}

		/* Function: RemoveSourceFileWithContent
		 * Tells the class that the passed file ID does not have content this build target can use.  Returns true if the number
		 * existed in the set and was removed, false if it wasn't part of the set.
		 */
		public bool RemoveSourceFileWithContent (int fileID)
			{
			lock (accessLock)
				{  return sourceFilesWithContent.Remove(fileID);  }
			}



		// Group: Class Functions
		// __________________________________________________________________________


		/* Function: ClassHasContent
		 * Returns whether the passed class ID has content this build target can use.
		 */
		public bool ClassHasContent (int classID)
			{
			lock (accessLock)
				{  return classesWithContent.Contains(classID);  }
			}

		/* Function: AddClassWithContent
		 * Tells the class that the passed class ID has content this build target can use.  Returns true if the number didn't
		 * already exist in the set and was added, false if it was already in the set.
		 */
		public bool AddClassWithContent (int classID)
			{
			lock (accessLock)
				{  return classesWithContent.Add(classID);  }
			}

		/* Function: RemoveClassWithContent
		 * Tells the class that the passed class ID does not have content this build target can use.  Returns true if the 
		 * number existed in the set and was removed, false if it wasn't part of the set.
		 */
		public bool RemoveClassWithContent (int classID)
			{
			lock (accessLock)
				{  return classesWithContent.Remove(classID);  }
			}



		// Group: Image File Functions
		// __________________________________________________________________________


		/* Function: ImageFileIsUsed
		 * Returns whether the passed image file ID is used in the output.
		 */
		public bool ImageFileIsUsed (int imageFileID)
			{
			lock (accessLock)
				{  return usedImageFiles.Contains(imageFileID);  }
			}

		/* Function: AddUsedImageFile
		 * Tells the class that the passed image file ID is used in the output.  Returns true if the number didn't already exist in the set 
		 * and was added, false if it was already in the set.
		 */
		public bool AddUsedImageFile (int imageFileID)
			{
			lock (accessLock)
				{  return usedImageFiles.Add(imageFileID);  }
			}

		/* Function: RemoveUsedImageFile
		 * Tells the class that the passed image file ID is not used in the output.  Returns true if the number existed in the set and was 
		 * removed, false if it wasn't part of the set.
		 */
		public bool RemoveUsedImageFile (int imageFileID)
			{
			lock (accessLock)
				{  return usedImageFiles.Remove(imageFileID);  }
			}



		// Group: Menu Data File Functions
		// __________________________________________________________________________


		/* Function: MenuDataFileIsUsed
		 * Returns whether the passed <Hierarchy> and number are used by the menu data files.
		 */
		public bool MenuDataFileIsUsed (Hierarchy hierarchy, int number)
			{
			lock (accessLock)
				{  
				NumberSet hierarchyNumbers = usedMenuDataFiles[hierarchy];

				if (hierarchyNumbers == null)
					{  return false;  }

				return hierarchyNumbers.Contains(number);
				}
			}

		/* Function: AddUsedMenuDataFile
		 * Adds a <Hierarchy>/number combination to the list of used menu data files.
		 */
		public void AddUsedMenuDataFile (Hierarchy hierarchy, int number)
			{
			lock (accessLock)
				{
				NumberSet hierarchyNumbers = usedMenuDataFiles[hierarchy];

				if (hierarchyNumbers == null)
					{
					hierarchyNumbers = new NumberSet();
					usedMenuDataFiles[hierarchy] = hierarchyNumbers;
					}

				hierarchyNumbers.Add(number);
				}
			}

		/* Function: RemoveUsedMenuDataFile
		 * Remove a <Hierarchy>/number combination to the list of used menu data files.
		 */
		public void RemoveUsedMenuDataFile (Hierarchy hierarchy, int number)
			{
			lock (accessLock)
				{
				NumberSet hierarchyNumbers = usedMenuDataFiles[hierarchy];

				if (hierarchyNumbers != null)
					{  hierarchyNumbers.Remove(number);  }
				}
			}



		// Group: Variables
		// __________________________________________________________________________
		//
		// These variables are protected internal because some code may need to access them directly.  You should use the
		// access functions instead of doing this whenever possible.  All direct access to the variables must be surrounded by
		// calls to <Lock()> and <Unlock()>.
		//

				
		/* var: sourceFilesWithContent
		 * 
		 * A set of the source file IDs that contain content this output target can use.  This is different from all the files with
		 * content in <CodeDB.Manager> because it is after all filters have been applied.
		 * 
		 * This variable is protected internal because some code may need to access it directly.  You should use the access
		 * functions instead of doing this whenever possible.  All direct access to the variable must be surrounded by calls to
		 * <Lock()> and <Unlock()>.
		 */
		protected internal NumberSet sourceFilesWithContent;


		/* var: classesWithContent
		 * 
		 * A set of the class IDs that contain content this output target can use.  This is different from all the classes with 
		 * content in <CodeDB.Manager> because it is after all filters have been applied.
		 * 
		 * This variable is protected internal because some code may need to access it directly.  You should use the access
		 * functions instead of doing this whenever possible.  All direct access to the variable must be surrounded by calls to
		 * <Lock()> and <Unlock()>.
		 */
		protected internal NumberSet classesWithContent;


		/* var: usedImageFiles
		 * 
		 * A set of the image file IDs that are used in the output.
		 * 
		 * This variable is protected internal because some code may need to access it directly.  You should use the access
		 * functions instead of doing this whenever possible.  All direct access to the variable must be surrounded by calls to
		 * <Lock()> and <Unlock()>.
		 */
		protected internal NumberSet usedImageFiles;


		/* var: usedMenuDataFiles
		 * 
		 * The menu data files created the last time the menu was built.  It maps <Hierarchies> to <NumberSets>, so
		 * files.js, files2.js, and files3.js would map to <Hierarchy.File> and {1-3}.
		 * 
		 * This variable is protected internal because some code may need to access it directly.  You should use the access
		 * functions instead of doing this whenever possible.  All direct access to the variable must be surrounded by calls to
		 * <Lock()> and <Unlock()>.
		 */
		protected internal NumberSetTable<Hierarchy> usedMenuDataFiles;


		/* var: accessLock
		 * An object used for a monitor that prevents more than one thread from accessing any of the variables at a time.
		 */
		protected object accessLock;

		}
	}

