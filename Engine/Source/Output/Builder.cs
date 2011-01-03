﻿/* 
 * Class: GregValure.NaturalDocs.Engine.Output.Builder
 * ____________________________________________________________________________
 * 
 * The base class for an output builder.
 */

// This file is part of Natural Docs, which is Copyright © 2003-2011 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using System.Collections.Generic;
using System.IO;
using GregValure.NaturalDocs.Engine.Collections;


namespace GregValure.NaturalDocs.Engine.Output
	{
	abstract public class Builder : CodeDB.IChangeWatcher
		{
		
		// Group: Functions
		// __________________________________________________________________________
		
		
		/* Function: Builder
		 */
		public Builder ()
			{
			}
			
		
		/* Function: Start
		 * Initializes the builder and returns whether all the settings are correct and that execution is ready to begin.  
		 * If there are problems they are added as <Errors> to the errorList parameter.
		 */
		virtual public bool Start (Errors.ErrorList errorList)
			{  
			return true;
			}
			
			
		/* Function: WorkOnUpdatingOutput
		 * 
		 * Works on the task of updating the output files for any changes it has detected so far.  This is a parallelizable task, so
		 * multiple threads can call this function and the work will be divided up between them.
		 * 
		 * This function returns if it's cancelled or there is no more work to be done.  If there is only one thread working on this 
		 * then the task is complete, but if there are multiple threads the task isn't complete until they all have returned.  This one 
		 * may have returned because there was no more work for this thread to do, but other threads are still working.
		 */
		abstract public void WorkOnUpdatingOutput (CancelDelegate cancelDelegate);

			
		/* Function: Cleanup
		 * Cleans up the builder's internal data when everything is up to date.  The default implementation does nothing.  You
		 * can pass a <CancelDelegate> to interrupt the process if necessary.
		 */
		virtual public void Cleanup (CancelDelegate cancelDelegate)
			{
			}
			
			
			
		// Group: Path Functions
		// __________________________________________________________________________
		
		
		/* Function: CreateTextFileAndPath
		 * Creates a UTF-8 text file at the specified path, creating any subfolders as required.
		 */
		public StreamWriter CreateTextFileAndPath (Path path)
			{
			try
				{  
				// This will create multiple subdirectories if needed, and will not throw an exception if it already exists.
				Directory.CreateDirectory(path.ParentFolder);  
				}
			catch
				{
				throw new Exceptions.UserFriendly( Locale.Get("NaturalDocs.Engine", "Error.CouldNotCreateOutputFolder(name)", path.ParentFolder) );
				}
				
			StreamWriter streamWriter = null;
			
			try
				{  streamWriter = File.CreateText(path);  }
			catch
				{
				throw new Exceptions.UserFriendly( Locale.Get("NaturalDocs.Engine", "Error.CouldNotCreateOutputFile(name)", path) );
				}
				
			return streamWriter;
			}



		// Group: CodeDB.IChangeWatcher Functions
		// __________________________________________________________________________
		
		
		abstract public void OnAddTopic (Topic topic, CodeDB.EventAccessor eventAccessor);

		abstract public void OnUpdateTopic (Topic oldTopic, int newCommentLineNumber, int newCodeLineNumber, string newBody, 
															CodeDB.EventAccessor eventAccessor);

		abstract public void OnDeleteTopic (Topic topic, CodeDB.EventAccessor eventAccessor);



		// Group: Properties
		// __________________________________________________________________________


		/* Property: Styles
		 * A list of <Styles> that apply to this builder, or null if none.
		 */
		virtual public IList<Style> Styles
			{
			get
				{  return null;  }
			}

		}
	}