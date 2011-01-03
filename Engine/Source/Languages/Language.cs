﻿/* 
 * Class: GregValure.NaturalDocs.Engine.Languages.Language
 * ____________________________________________________________________________
 * 
 * A class encapsulating information about a language.  This differs from <ConfigFileLanguage> in that its meant to 
 * represent the final combined settings of a language rather than its entry in a config file.  For example, this class
 * doesn't store the language's extensions or shebang strings.
 * 
 */

// This file is part of Natural Docs, which is Copyright © 2003-2011 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;
using System.Collections.Generic;
using GregValure.NaturalDocs.Engine.Tokenization;
using GregValure.NaturalDocs.Engine.Comments;


namespace GregValure.NaturalDocs.Engine.Languages
	{
	public partial class Language : IDObjects.Base
		{
		
		// Group: Types
		// __________________________________________________________________________
		
		
		/* Enum: EnumValues
		 * Can be Global, UnderType, or UnderParent.
		 */
		public enum EnumValues : byte
			{  Global, UnderType, UnderParent  };
		
		
		/* Enum: LanguageType
		 * 
		 * The type of language or file this is.
		 *
		 * FullSupport - The language is fully supported.
		 * BasicSupport - The language has basic support.
		 * TextFile - The file is a text file.
		 * Container - The file is a container, meaning it's contents may contain code from other languages, such as .cgi and .asp files.
		 */
		public enum LanguageType : byte
			{  FullSupport, BasicSupport, TextFile, Container  };
		
		
		/* Enum: LanguageFlags
		 * 
		 * InSystemFile - Set if the language was defined in the system config file <Languages.txt>.
		 * InProjectFile - Set if the language was defined in the project config file <Languages.txt>.  Not set for Alter Language.
		 * 
		 * InConfigFiles - A combination of <InSystemFile> and <InProjectFile> used for testing if either are set.
		 * 
		 * InBinaryFile - Set if the language was present in <Languages.nd>
		 * Predefined - Set if the language is predefined by Natural Docs.
		 */
		protected enum LanguageFlags : byte
			{
			InSystemFile = 0x01,
			InProjectFile = 0x02,
			
			InConfigFiles = InSystemFile | InProjectFile,
			
			InBinaryFile = 0x04,
			Predefined = 0x08
			}
		
		
		
		// Group: Functions
		// __________________________________________________________________________
		
		
		/* Constructor: Language
		 * Creates a new language object.
		 */
		public Language (string newName) : base()
			{
			name = newName;
			
			simpleIdentifier = null;			
			type = LanguageType.BasicSupport;
			lineCommentStrings = null;
			blockCommentStringPairs = null;
			javadocLineCommentStringPairs = null;
			javadocBlockCommentStringPairs = null;
			xmlLineCommentStrings = null;
			memberOperator = ".";
			prototypeEnders = null;
			lineExtender = null;
			enumValue = EnumValues.Global;			
			flags = 0;
			}



		// Group: Language Properties
		// __________________________________________________________________________
			
			
		/* Property: Name
		 * The name of the language.
		 */
		override public string Name
			{
			get
				{  return name;  }
			}
			
		/* Property: SimpleIdentifier
		 * The name of the language using only the letters A to Z.
		 */
		public string SimpleIdentifier
			{
			get
				{
				// Generate and store the default.  Since Name can't be changed, we don't have to worry
				// about keeping simpleIdentifier null so it can be regenerated again.
				if (simpleIdentifier == null)
					{  simpleIdentifier = name.OnlyAToZ();  }
					
				// A fallback if that still didn't work.
				if (simpleIdentifier == null)
					{  simpleIdentifier = "LanguageID" + ID;  }
					
				return simpleIdentifier;
				}
			set
				{  simpleIdentifier = value;  }
			}
			
		/* Property: Type
		 * The type of the language or file.
		 */
		public LanguageType Type
			{
			get
				{  return type;  }
			set
				{  type = value;  }
			}
			
		/* Property: LineCommentStrings
		 * An array of strings representing line comment symbols.  Will be null if none are defined.
		 */
		public string[] LineCommentStrings
			{
			get
				{  return lineCommentStrings;  }
			set
				{
				if (value != null && value.Length != 0)
					{  lineCommentStrings = value;  }
				else
					{  lineCommentStrings = null;  }
				}
			}
			
		/* Property: BlockCommentStringPairs
		 * An array of string pairs representing start and stop block comment symbols.  Will be null if none are defined.
		 */
		public string[] BlockCommentStringPairs
			{
			get
				{  return blockCommentStringPairs;  }
			set
				{
				if (value != null && value.Length != 0)
					{  
					if (value.Length % 2 == 1)
						{  throw new Exceptions.ArrayDidntHaveEvenLength("BlockCommentStringPairs");  }

					blockCommentStringPairs = value;  
					}
				else
					{  blockCommentStringPairs = null;  }
				}
			}
			
		/* Property: JavadocLineCommentStringPairs
		 * An array of string pairs representing Javadoc line comment symbols.  The first are are the symbols that must start the
		 * comment, and the second are the symbols that must be used on every following line.  Will be null if none are defined.
		 */
		public string[] JavadocLineCommentStringPairs
			{
			get
				{  return javadocLineCommentStringPairs;  }
			set
				{
				if (value != null && value.Length != 0)
					{  
					if (value.Length % 2 == 1)
						{  throw new Exceptions.ArrayDidntHaveEvenLength("JavadocLineCommentStringPairs");  }

					javadocLineCommentStringPairs = value;  
					}
				else
					{  javadocLineCommentStringPairs = null;  }
				}
			}
			
		/* Property: JavadocBlockCommentStringPairs
		 * An array of string pairs representing start and stop Javadoc block comment symbols.  Will be null if none are defined.
		 */
		public string[] JavadocBlockCommentStringPairs
			{
			get
				{  return javadocBlockCommentStringPairs;  }
			set
				{
				if (value != null && value.Length != 0)
					{  
					if (value.Length % 2 == 1)
						{  throw new Exceptions.ArrayDidntHaveEvenLength("JavadocBlockCommentStringPairs");  }

					javadocBlockCommentStringPairs = value;  
					}
				else
					{  javadocBlockCommentStringPairs = null;  }
				}
			}
			
		/* Property: XMLLineCommentStrings
		 * An array of strings representing XML line comment symbols.  Will be null if none are defined.
		 */
		public string[] XMLLineCommentStrings
			{
			get
				{  return xmlLineCommentStrings;  }
			set
				{
				if (value != null && value.Length != 0)
					{  xmlLineCommentStrings = value;  }
				else
					{  xmlLineCommentStrings = null;  }
				}
			}
			
		/* Property: MemberOperator
		 * A string representing the default member operator symbol.
		 */
		public string MemberOperator
			{
			get
				{  return memberOperator;  }
			set
				{  memberOperator = value;  }
			}
		
		/* Property: LineExtender
		 * A string representing the line extender symbol if line breaks are significant to the language.
		 */
		 public string LineExtender
			{
			get
				{  return lineExtender;  }
			set
				{  lineExtender = value;  }
			}
			
		/* Function: GetPrototypeEnders
		 * Returns an array of strings representing the prototype enders for the passed topic type, or null if there
		 * are none.
		 */
		public string[] GetPrototypeEnders (int topicTypeID)
			{
			if (prototypeEnders == null)
				{  return null;  }
			else
				{  return prototypeEnders[topicTypeID];  }
			}
			
		/* Function: SetPrototypeEnders
		 * Sets the array of strings representing the prototype enders for the passed topic type.
		 */
		public void SetPrototypeEnders (int topicTypeID, string[] prototypeEnderStrings)
			{
			if (prototypeEnderStrings == null || prototypeEnderStrings.Length == 0)
				{
				if (prototypeEnders != null)
					{
					prototypeEnders.Remove(topicTypeID);
					if (prototypeEnders.Count == 0)
						{  prototypeEnders = null;  }
					}
				}
			else
				{
				if (prototypeEnders == null)
					{  prototypeEnders = new Dictionary<int,string[]>();  }
					
				prototypeEnders[topicTypeID] = prototypeEnderStrings;
				}
			}
			
		/* Function: GetTopicTypesWithPrototypeEnders
		 * Returns an array of all the topic types that have prototype enders defined, or null if none.
		 */
		public int[] GetTopicTypesWithPrototypeEnders()
			{
			if (prototypeEnders == null)
				{  return null;  }
			else
				{
				int[] result = new int[ prototypeEnders.Keys.Count ];
				prototypeEnders.Keys.CopyTo(result, 0);
				return result;  
				}
			}
			
			
		/* Property: EnumValue
		 * How enum values are referenced.
		 */
		public EnumValues EnumValue
			{
			get
				{  return enumValue;  }
			set
				{  enumValue = value;  }
			}			

			
			
		
		// Group: Flags
		// These properties do not affect the equality operators.
		// __________________________________________________________________________
		
		
		/* Property: InSystemFile
		 * Whether this language was defined in the system <Languages.txt> file.
		 */
		public bool InSystemFile
			{
			get
				{  return ( (flags & LanguageFlags.InSystemFile) != 0);  }
			set
				{  
				if (value == true)
					{  flags |= LanguageFlags.InSystemFile;  }
				else
					{  flags &= ~LanguageFlags.InSystemFile;  }
				}
			}
			
		/* Property: InProjectFile
		 * Whether this language was defined in the project <Languages.txt> file.
		 */
		public bool InProjectFile
			{
			get
				{  return ( (flags & LanguageFlags.InProjectFile) != 0);  }
			set
				{  
				if (value == true)
					{  flags |= LanguageFlags.InProjectFile;  }
				else
					{  flags &= ~LanguageFlags.InProjectFile;  }
				}
			}
			
		/* Property: InConfigFiles
		 * Whether this language was defined in either of the <Languages.txt> files.
		 */
		public bool InConfigFiles
			{
			get
				{  return ( (flags & LanguageFlags.InConfigFiles) != 0);  }
			}
			
		/* Property: InBinaryFile
		 * Whether this language was present in <Languages.nd>.
		 */
		public bool InBinaryFile
			{
			get
				{  return ( (flags & LanguageFlags.InBinaryFile) != 0);  }
			set
				{
				if (value == true)
					{  flags |= LanguageFlags.InBinaryFile;  }
				else
					{  flags &= ~LanguageFlags.InBinaryFile;  }
				}
			}
			
		/* Property: Predefined
		 * Whether this language is predefined by Natural Docs.
		 */
		public bool Predefined
			{
			get
				{  return ( (flags & LanguageFlags.Predefined) != 0);  }
			set
				{
				if (value == true)
					{  flags |= LanguageFlags.Predefined;  }
				else
					{  flags &= ~LanguageFlags.Predefined;  }
				}
			}



		// Group: Operators
		// __________________________________________________________________________
		
		
		/* Function: operator ==
		 * Returns whether all the properties of the two topic types are equal, including Name and ID, but excluding flags.
		 */
		public static bool operator == (Language language1, Language language2)
			{
			if ((object)language1 == null && (object)language2 == null)
				{  return true;  }
			else if ((object)language1 == null || (object)language2 == null)
				{  return false;  }
			else
				{
				// Deliberately does not include Flags
				return ( language1.ID == language2.ID &&
							language1.Type == language2.Type &&
							language1.EnumValue == language2.EnumValue &&
						  
							language1.Name == language2.Name &&
							language1.SimpleIdentifier == language2.SimpleIdentifier &&
							language1.MemberOperator == language2.MemberOperator &&
							language1.LineExtender == language2.LineExtender &&

							StringArraysAreEqual (language1.lineCommentStrings, language2.lineCommentStrings) &&
							StringPairArraysAreEqual (language1.blockCommentStringPairs, language2.blockCommentStringPairs) &&
							PrototypeEndersAreEqual (language1.prototypeEnders, language2.prototypeEnders) &&
							StringPairArraysAreEqual (language1.javadocLineCommentStringPairs, language2.javadocLineCommentStringPairs) &&
							StringPairArraysAreEqual (language1.javadocBlockCommentStringPairs, language2.javadocBlockCommentStringPairs) &&
							StringArraysAreEqual (language1.xmlLineCommentStrings, language2.xmlLineCommentStrings) );
				}
			}
			
		
		/* Function: operator !=
		 * Returns if any of the properties of the two languages are inequal, including Name and ID, but excluding flags.
		 */
		public static bool operator != (Language language1, Language language2)
			{
			return !(language1 == language2);
			}
			
			
		/* Function: StringArraysAreEqual
		 * Compares two arrays of strings, ignoring the order they exist in.  Is case sensitive and safe to use with nulls.
		 */
		public static bool StringArraysAreEqual (string[] array1, string[] array2)
			{
			if (array1 == null && array2 == null)
				{  return true;  }
			else if (array1 == null || array2 == null)
				{  return false;  }
			else if (array1.Length != array2.Length)
				{  return false;  }
			else
				{
				Collections.StringSet array1set = new Collections.StringSet(false, false);
				
				foreach (string array1item in array1)
					{  array1set.Add(array1item);  }
					
				foreach (string array2item in array2)
					{
					if (!array1set.Contains(array2item))
						{  return false;  }
					}
					
				return true;
				}
			}
			
			
		/* Function: StringPairArraysAreEqual
		 * Compares two arrays of string pairs, ignoring the order they exist in.  Is case sensitive and safe to use with nulls.
		 */
		public static bool StringPairArraysAreEqual (string[] array1, string[] array2)
			{
			if (array1 == null && array2 == null)
				{  return true;  }
			else if (array1 == null || array2 == null)
				{  return false;  }
			else if (array1.Length != array2.Length)
				{  return false;  }
			else
				{
				Collections.StringSet array1set = new Collections.StringSet(false, false);
				
				for (int i = 0; i < array1.Length; i += 2)
					{  array1set.Add( array1[i] + array1[i+1] );  }
					
				for (int i = 0; i < array2.Length; i += 2)
					{
					if (!array1set.Contains( array2[i] + array2[i+1] ))
						{  return false;  }
					}
					
				return true;
				}
			}
			
			
		/* Function: PrototypeEndersAreEqual
		 * Compares two prototype ender dictionaries.  Is case sensitive and safe to use with nulls.
		 */
		protected static bool PrototypeEndersAreEqual (Dictionary<int, string[]> prototypeEnders1, 
																			Dictionary<int, string[]> prototypeEnders2)
			{
			if (prototypeEnders1 == null && prototypeEnders2 == null)
				{  return true;  }
			else if (prototypeEnders1 == null || prototypeEnders2 == null)
				{  return false;  }
			else if (prototypeEnders1.Count != prototypeEnders2.Count)
				{  return false;  }
			else
				{
				foreach (KeyValuePair<int, string[]> prototypeEnders1Pair in prototypeEnders1)
					{
					if (!StringArraysAreEqual(prototypeEnders1Pair.Value, prototypeEnders2[ prototypeEnders1Pair.Key ]))
						{  return false;  }
					}
					
				return true;
				}
			}


			
		// Group: Interface Functions
		// __________________________________________________________________________
		
		
		public override bool Equals (object o)
			{
			if (o is Language)
				{  return (this == (Language)o);  }
			else
				{  return false;  }
			}


		public override int GetHashCode ()
			{
			return Name.GetHashCode();
			}
			
			
		
		// Group: Variables
		// __________________________________________________________________________
		
		/* var: name
		 * The language name.
		 */
		protected string name;
		
		/* var: simpleIdentifier
		 * The language's name using only the letters A to Z, or null if it's not defined.
		 */
		protected string simpleIdentifier;
		
		/* var: type
		 * The type of the language or file.
		 */
		protected LanguageType type;

		/* array: lineCommentStrings
		 * An array of strings that start line comments.
		 */
		protected string[] lineCommentStrings;
		
		/* array: blockCommentStringPairs
		 * An array of string pairs that start and end block comments.
		 */
		protected string[] blockCommentStringPairs;
		
		/* array: javadocLineCommentStringPairs
		 * An array of string pairs that start Javadoc line comments.  The first will be the symbol that must start it, and
		 * the second will be the symbol that must be used on every following line.
		 */
		protected string[] javadocLineCommentStringPairs;
		
		/* array: javadocBlockCommentStringPairs
		 * An array of string pairs that start and end Javadoc black comments.
		 */
		protected string[] javadocBlockCommentStringPairs;
		
		/* array: xmlLineCommentStrings
		 * An array of strings that start XML line comments.
		 */
		protected string[] xmlLineCommentStrings;
		
		/* string: memberOperator
		 * A string representing the default member operator symbol.
		 */
		protected string memberOperator;
		
		/* object: prototypeEnders
		 * A dictionary mapping topic type IDs to arrays of symbols representing their prototype enders.  Line breaks are
		 * represented with "\n".
		 */
		protected Dictionary<int, string[]> prototypeEnders;
		
		/* string: lineExtender
		 * A string representing the line extender symbol if line breaks are significant to the language.
		 */
		protected string lineExtender;
		
		/* var: enumValue
		 * How the language handles enum values.
		 */
		protected EnumValues enumValue;
		
		/* var: flags
		 * A combination of <FlagValues> describing the language.
		 */
		protected LanguageFlags flags;
		}
	}