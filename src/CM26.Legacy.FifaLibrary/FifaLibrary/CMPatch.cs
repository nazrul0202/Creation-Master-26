using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FifaLibrary;

[Serializable]
[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
[HelpKeyword("vs.data.DataSet")]
[DesignerCategory("code")]
[ToolboxItem(true)]
[XmlRoot("Patch")]
[XmlSchemaProvider("GetTypedDataSetSchema")]
public class CMPatch : DataSet
{
	public delegate void PatchIdentityRowChangeEventHandler(object sender, PatchIdentityRowChangeEvent e);

	public delegate void PatchElementsRowChangeEventHandler(object sender, PatchElementsRowChangeEvent e);

	[Serializable]
	[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
	[XmlSchemaProvider("GetTypedTableSchema")]
	public class PatchIdentityDataTable : DataTable, IEnumerable
	{
		private DataColumn columnName;

		private DataColumn columnVersion;

		private DataColumn columnDescription;

		private DataColumn columnChecksum;

		private DataColumn columnCMS;

		[DebuggerNonUserCode]
		public DataColumn NameColumn => columnName;

		[DebuggerNonUserCode]
		public DataColumn VersionColumn => columnVersion;

		[DebuggerNonUserCode]
		public DataColumn DescriptionColumn => columnDescription;

		[DebuggerNonUserCode]
		public DataColumn ChecksumColumn => columnChecksum;

		[DebuggerNonUserCode]
		public DataColumn CMSColumn => columnCMS;

		[Browsable(false)]
		[DebuggerNonUserCode]
		public int Count => base.Rows.Count;

		[DebuggerNonUserCode]
		public PatchIdentityRow this[int index] => (PatchIdentityRow)base.Rows[index];

		[CompilerGenerated]
		public event PatchIdentityRowChangeEventHandler PatchIdentityRowChanging;

		[CompilerGenerated]
		public event PatchIdentityRowChangeEventHandler PatchIdentityRowChanged;

		[CompilerGenerated]
		public event PatchIdentityRowChangeEventHandler PatchIdentityRowDeleting;

		[CompilerGenerated]
		public event PatchIdentityRowChangeEventHandler PatchIdentityRowDeleted;

		[DebuggerNonUserCode]
		public PatchIdentityDataTable()
		{
			base.TableName = "PatchIdentity";
			BeginInit();
			InitClass();
			EndInit();
		}

		[DebuggerNonUserCode]
		internal PatchIdentityDataTable(DataTable table)
		{
			base.TableName = table.TableName;
			if (table.CaseSensitive != table.DataSet.CaseSensitive)
			{
				base.CaseSensitive = table.CaseSensitive;
			}
			if (table.Locale.ToString() != table.DataSet.Locale.ToString())
			{
				base.Locale = table.Locale;
			}
			if (table.Namespace != table.DataSet.Namespace)
			{
				base.Namespace = table.Namespace;
			}
			base.Prefix = table.Prefix;
			base.MinimumCapacity = table.MinimumCapacity;
		}

		[DebuggerNonUserCode]
		protected PatchIdentityDataTable(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			InitVars();
		}

		[DebuggerNonUserCode]
		public void AddPatchIdentityRow(PatchIdentityRow row)
		{
			base.Rows.Add(row);
		}

		[DebuggerNonUserCode]
		public PatchIdentityRow AddPatchIdentityRow(string Name, string Version, string Description, int Checksum, string CMS)
		{
			PatchIdentityRow patchIdentityRow = (PatchIdentityRow)NewRow();
			object[] itemArray = new object[5] { Name, Version, Description, Checksum, CMS };
			patchIdentityRow.ItemArray = itemArray;
			base.Rows.Add(patchIdentityRow);
			return patchIdentityRow;
		}

		[DebuggerNonUserCode]
		public virtual IEnumerator GetEnumerator()
		{
			return base.Rows.GetEnumerator();
		}

		[DebuggerNonUserCode]
		public override DataTable Clone()
		{
			PatchIdentityDataTable obj = (PatchIdentityDataTable)base.Clone();
			obj.InitVars();
			return obj;
		}

		[DebuggerNonUserCode]
		protected override DataTable CreateInstance()
		{
			return new PatchIdentityDataTable();
		}

		[DebuggerNonUserCode]
		internal void InitVars()
		{
			columnName = base.Columns["Name"];
			columnVersion = base.Columns["Version"];
			columnDescription = base.Columns["Description"];
			columnChecksum = base.Columns["Checksum"];
			columnCMS = base.Columns["CMS"];
		}

		[DebuggerNonUserCode]
		private void InitClass()
		{
			columnName = new DataColumn("Name", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnName);
			columnVersion = new DataColumn("Version", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnVersion);
			columnDescription = new DataColumn("Description", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnDescription);
			columnCMS = new DataColumn("CMS", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnCMS);
		}

		[DebuggerNonUserCode]
		public PatchIdentityRow NewPatchIdentityRow()
		{
			return (PatchIdentityRow)NewRow();
		}

		[DebuggerNonUserCode]
		protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
		{
			return new PatchIdentityRow(builder);
		}

		[DebuggerNonUserCode]
		protected override Type GetRowType()
		{
			return typeof(PatchIdentityRow);
		}

		[DebuggerNonUserCode]
		protected override void OnRowChanged(DataRowChangeEventArgs e)
		{
			base.OnRowChanged(e);
			if (this.PatchIdentityRowChanged != null)
			{
				this.PatchIdentityRowChanged(this, new PatchIdentityRowChangeEvent((PatchIdentityRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		protected override void OnRowChanging(DataRowChangeEventArgs e)
		{
			base.OnRowChanging(e);
			if (this.PatchIdentityRowChanging != null)
			{
				this.PatchIdentityRowChanging(this, new PatchIdentityRowChangeEvent((PatchIdentityRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		protected override void OnRowDeleted(DataRowChangeEventArgs e)
		{
			base.OnRowDeleted(e);
			if (this.PatchIdentityRowDeleted != null)
			{
				this.PatchIdentityRowDeleted(this, new PatchIdentityRowChangeEvent((PatchIdentityRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		protected override void OnRowDeleting(DataRowChangeEventArgs e)
		{
			base.OnRowDeleting(e);
			if (this.PatchIdentityRowDeleting != null)
			{
				this.PatchIdentityRowDeleting(this, new PatchIdentityRowChangeEvent((PatchIdentityRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		public void RemovePatchIdentityRow(PatchIdentityRow row)
		{
			base.Rows.Remove(row);
		}

		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
		{
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			CMPatch cMPatch = new CMPatch();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
			xmlSchemaAny.MinOccurs = 0m;
			xmlSchemaAny.MaxOccurs = decimal.MaxValue;
			xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
			xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
			xmlSchemaAny2.MinOccurs = 1m;
			xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
			xmlSchemaSequence.Items.Add(xmlSchemaAny2);
			xmlSchemaComplexType.Attributes.Add(new XmlSchemaAttribute
			{
				Name = "namespace",
				FixedValue = cMPatch.Namespace
			});
			xmlSchemaComplexType.Attributes.Add(new XmlSchemaAttribute
			{
				Name = "tableTypeName",
				FixedValue = "PatchIdentityDataTable"
			});
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = cMPatch.GetSchemaSerializable();
			if (xs.Contains(schemaSerializable.TargetNamespace))
			{
				MemoryStream memoryStream = new MemoryStream();
				MemoryStream memoryStream2 = new MemoryStream();
				try
				{
					schemaSerializable.Write(memoryStream);
					foreach (XmlSchema item in xs.Schemas(schemaSerializable.TargetNamespace))
					{
						memoryStream2.SetLength(0L);
						item.Write(memoryStream2);
						if (memoryStream.Length == memoryStream2.Length)
						{
							memoryStream.Position = 0L;
							memoryStream2.Position = 0L;
							while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
							{
							}
							if (memoryStream.Position == memoryStream.Length)
							{
								return xmlSchemaComplexType;
							}
						}
					}
				}
				finally
				{
					memoryStream?.Close();
					memoryStream2?.Close();
				}
			}
			xs.Add(schemaSerializable);
			return xmlSchemaComplexType;
		}
	}

	[Serializable]
	[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
	[XmlSchemaProvider("GetTypedTableSchema")]
	public class PatchElementsDataTable : DataTable, IEnumerable
	{
		private DataColumn columnComment;

		private DataColumn columnType;

		private DataColumn columnID;

		private DataColumn columnName;

		private DataColumn columnChecksum;

		[DebuggerNonUserCode]
		public DataColumn CommentColumn => columnComment;

		[DebuggerNonUserCode]
		public DataColumn TypeColumn => columnType;

		[DebuggerNonUserCode]
		public DataColumn IDColumn => columnID;

		[DebuggerNonUserCode]
		public DataColumn NameColumn => columnName;

		[DebuggerNonUserCode]
		public DataColumn ChecksumColumn => columnChecksum;

		[Browsable(false)]
		[DebuggerNonUserCode]
		public int Count => base.Rows.Count;

		[DebuggerNonUserCode]
		public PatchElementsRow this[int index] => (PatchElementsRow)base.Rows[index];

		[CompilerGenerated]
		public event PatchElementsRowChangeEventHandler PatchElementsRowChanging;

		[CompilerGenerated]
		public event PatchElementsRowChangeEventHandler PatchElementsRowChanged;

		[CompilerGenerated]
		public event PatchElementsRowChangeEventHandler PatchElementsRowDeleting;

		[CompilerGenerated]
		public event PatchElementsRowChangeEventHandler PatchElementsRowDeleted;

		[DebuggerNonUserCode]
		public PatchElementsDataTable()
		{
			base.TableName = "PatchElements";
			BeginInit();
			InitClass();
			EndInit();
		}

		[DebuggerNonUserCode]
		internal PatchElementsDataTable(DataTable table)
		{
			base.TableName = table.TableName;
			if (table.CaseSensitive != table.DataSet.CaseSensitive)
			{
				base.CaseSensitive = table.CaseSensitive;
			}
			if (table.Locale.ToString() != table.DataSet.Locale.ToString())
			{
				base.Locale = table.Locale;
			}
			if (table.Namespace != table.DataSet.Namespace)
			{
				base.Namespace = table.Namespace;
			}
			base.Prefix = table.Prefix;
			base.MinimumCapacity = table.MinimumCapacity;
		}

		[DebuggerNonUserCode]
		protected PatchElementsDataTable(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			InitVars();
		}

		[DebuggerNonUserCode]
		public void AddPatchElementsRow(PatchElementsRow row)
		{
			base.Rows.Add(row);
		}

		[DebuggerNonUserCode]
		public PatchElementsRow AddPatchElementsRow(string Comment, string Type, string ID, string Name, int Checksum)
		{
			PatchElementsRow patchElementsRow = (PatchElementsRow)NewRow();
			object[] itemArray = new object[5] { Comment, Type, ID, Name, Checksum };
			patchElementsRow.ItemArray = itemArray;
			base.Rows.Add(patchElementsRow);
			return patchElementsRow;
		}

		[DebuggerNonUserCode]
		public virtual IEnumerator GetEnumerator()
		{
			return base.Rows.GetEnumerator();
		}

		[DebuggerNonUserCode]
		public override DataTable Clone()
		{
			PatchElementsDataTable obj = (PatchElementsDataTable)base.Clone();
			obj.InitVars();
			return obj;
		}

		[DebuggerNonUserCode]
		protected override DataTable CreateInstance()
		{
			return new PatchElementsDataTable();
		}

		[DebuggerNonUserCode]
		internal void InitVars()
		{
			columnComment = base.Columns["Comment"];
			columnType = base.Columns["Type"];
			columnID = base.Columns["ID"];
			columnName = base.Columns["Name"];
			columnChecksum = base.Columns["Checksum"];
		}

		[DebuggerNonUserCode]
		private void InitClass()
		{
			columnComment = new DataColumn("Comment", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnComment);
			columnType = new DataColumn("Type", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnType);
			columnID = new DataColumn("ID", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnID);
			columnName = new DataColumn("Name", typeof(string), null, MappingType.Element);
			base.Columns.Add(columnName);
			columnChecksum = new DataColumn("Checksum", typeof(int), null, MappingType.Element);
			base.Columns.Add(columnChecksum);
		}

		[DebuggerNonUserCode]
		public PatchElementsRow NewPatchElementsRow()
		{
			return (PatchElementsRow)NewRow();
		}

		[DebuggerNonUserCode]
		protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
		{
			return new PatchElementsRow(builder);
		}

		[DebuggerNonUserCode]
		protected override Type GetRowType()
		{
			return typeof(PatchElementsRow);
		}

		[DebuggerNonUserCode]
		protected override void OnRowChanged(DataRowChangeEventArgs e)
		{
			base.OnRowChanged(e);
			if (this.PatchElementsRowChanged != null)
			{
				this.PatchElementsRowChanged(this, new PatchElementsRowChangeEvent((PatchElementsRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		protected override void OnRowChanging(DataRowChangeEventArgs e)
		{
			base.OnRowChanging(e);
			if (this.PatchElementsRowChanging != null)
			{
				this.PatchElementsRowChanging(this, new PatchElementsRowChangeEvent((PatchElementsRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		protected override void OnRowDeleted(DataRowChangeEventArgs e)
		{
			base.OnRowDeleted(e);
			if (this.PatchElementsRowDeleted != null)
			{
				this.PatchElementsRowDeleted(this, new PatchElementsRowChangeEvent((PatchElementsRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		protected override void OnRowDeleting(DataRowChangeEventArgs e)
		{
			base.OnRowDeleting(e);
			if (this.PatchElementsRowDeleting != null)
			{
				this.PatchElementsRowDeleting(this, new PatchElementsRowChangeEvent((PatchElementsRow)e.Row, e.Action));
			}
		}

		[DebuggerNonUserCode]
		public void RemovePatchElementsRow(PatchElementsRow row)
		{
			base.Rows.Remove(row);
		}

		[DebuggerNonUserCode]
		public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
		{
			XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
			XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
			CMPatch cMPatch = new CMPatch();
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.Namespace = "http://www.w3.org/2001/XMLSchema";
			xmlSchemaAny.MinOccurs = 0m;
			xmlSchemaAny.MaxOccurs = decimal.MaxValue;
			xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
			xmlSchemaSequence.Items.Add(xmlSchemaAny);
			XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
			xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
			xmlSchemaAny2.MinOccurs = 1m;
			xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
			xmlSchemaSequence.Items.Add(xmlSchemaAny2);
			xmlSchemaComplexType.Attributes.Add(new XmlSchemaAttribute
			{
				Name = "namespace",
				FixedValue = cMPatch.Namespace
			});
			xmlSchemaComplexType.Attributes.Add(new XmlSchemaAttribute
			{
				Name = "tableTypeName",
				FixedValue = "PatchElementsDataTable"
			});
			xmlSchemaComplexType.Particle = xmlSchemaSequence;
			XmlSchema schemaSerializable = cMPatch.GetSchemaSerializable();
			if (xs.Contains(schemaSerializable.TargetNamespace))
			{
				MemoryStream memoryStream = new MemoryStream();
				MemoryStream memoryStream2 = new MemoryStream();
				try
				{
					schemaSerializable.Write(memoryStream);
					foreach (XmlSchema item in xs.Schemas(schemaSerializable.TargetNamespace))
					{
						memoryStream2.SetLength(0L);
						item.Write(memoryStream2);
						if (memoryStream.Length == memoryStream2.Length)
						{
							memoryStream.Position = 0L;
							memoryStream2.Position = 0L;
							while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
							{
							}
							if (memoryStream.Position == memoryStream.Length)
							{
								return xmlSchemaComplexType;
							}
						}
					}
				}
				finally
				{
					memoryStream?.Close();
					memoryStream2?.Close();
				}
			}
			xs.Add(schemaSerializable);
			return xmlSchemaComplexType;
		}
	}

	[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
	public class PatchIdentityRow : DataRow
	{
		private PatchIdentityDataTable tablePatchIdentity;

		[DebuggerNonUserCode]
		public string Name
		{
			get
			{
				try
				{
					return (string)base[tablePatchIdentity.NameColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Name' in table 'PatchIdentity' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchIdentity.NameColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public string Version
		{
			get
			{
				try
				{
					return (string)base[tablePatchIdentity.VersionColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Version' in table 'PatchIdentity' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchIdentity.VersionColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public string Description
		{
			get
			{
				try
				{
					return (string)base[tablePatchIdentity.DescriptionColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Description' in table 'PatchIdentity' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchIdentity.DescriptionColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public int Checksum
		{
			get
			{
				try
				{
					return (int)base[tablePatchIdentity.ChecksumColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Checksum' in table 'PatchIdentity' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchIdentity.ChecksumColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public string CMS
		{
			get
			{
				if (IsCMSNull())
				{
					return string.Empty;
				}
				return (string)base[tablePatchIdentity.CMSColumn];
			}
			set
			{
				base[tablePatchIdentity.CMSColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		internal PatchIdentityRow(DataRowBuilder rb)
			: base(rb)
		{
			tablePatchIdentity = (PatchIdentityDataTable)base.Table;
		}

		[DebuggerNonUserCode]
		public bool IsNameNull()
		{
			return IsNull(tablePatchIdentity.NameColumn);
		}

		[DebuggerNonUserCode]
		public void SetNameNull()
		{
			base[tablePatchIdentity.NameColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsVersionNull()
		{
			return IsNull(tablePatchIdentity.VersionColumn);
		}

		[DebuggerNonUserCode]
		public void SetVersionNull()
		{
			base[tablePatchIdentity.VersionColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsDescriptionNull()
		{
			return IsNull(tablePatchIdentity.DescriptionColumn);
		}

		[DebuggerNonUserCode]
		public void SetDescriptionNull()
		{
			base[tablePatchIdentity.DescriptionColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsChecksumNull()
		{
			return IsNull(tablePatchIdentity.ChecksumColumn);
		}

		[DebuggerNonUserCode]
		public void SetChecksumNull()
		{
			base[tablePatchIdentity.ChecksumColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsCMSNull()
		{
			return IsNull(tablePatchIdentity.CMSColumn);
		}

		[DebuggerNonUserCode]
		public void SetCMSNull()
		{
			base[tablePatchIdentity.CMSColumn] = Convert.DBNull;
		}
	}

	[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
	public class PatchElementsRow : DataRow
	{
		private PatchElementsDataTable tablePatchElements;

		[DebuggerNonUserCode]
		public string Comment
		{
			get
			{
				try
				{
					return (string)base[tablePatchElements.CommentColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Comment' in table 'PatchElements' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchElements.CommentColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public string Type
		{
			get
			{
				try
				{
					return (string)base[tablePatchElements.TypeColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Type' in table 'PatchElements' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchElements.TypeColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public string ID
		{
			get
			{
				if (IsIDNull())
				{
					return string.Empty;
				}
				return (string)base[tablePatchElements.IDColumn];
			}
			set
			{
				base[tablePatchElements.IDColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public string Name
		{
			get
			{
				try
				{
					return (string)base[tablePatchElements.NameColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Name' in table 'PatchElements' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchElements.NameColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		public int Checksum
		{
			get
			{
				try
				{
					return (int)base[tablePatchElements.ChecksumColumn];
				}
				catch (InvalidCastException innerException)
				{
					throw new StrongTypingException("The value for column 'Checksum' in table 'PatchElements' is DBNull.", innerException);
				}
			}
			set
			{
				base[tablePatchElements.ChecksumColumn] = value;
			}
		}

		[DebuggerNonUserCode]
		internal PatchElementsRow(DataRowBuilder rb)
			: base(rb)
		{
			tablePatchElements = (PatchElementsDataTable)base.Table;
		}

		[DebuggerNonUserCode]
		public bool IsCommentNull()
		{
			return IsNull(tablePatchElements.CommentColumn);
		}

		[DebuggerNonUserCode]
		public void SetCommentNull()
		{
			base[tablePatchElements.CommentColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsTypeNull()
		{
			return IsNull(tablePatchElements.TypeColumn);
		}

		[DebuggerNonUserCode]
		public void SetTypeNull()
		{
			base[tablePatchElements.TypeColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsIDNull()
		{
			return IsNull(tablePatchElements.IDColumn);
		}

		[DebuggerNonUserCode]
		public void SetIDNull()
		{
			base[tablePatchElements.IDColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsNameNull()
		{
			return IsNull(tablePatchElements.NameColumn);
		}

		[DebuggerNonUserCode]
		public void SetNameNull()
		{
			base[tablePatchElements.NameColumn] = Convert.DBNull;
		}

		[DebuggerNonUserCode]
		public bool IsChecksumNull()
		{
			return IsNull(tablePatchElements.ChecksumColumn);
		}

		[DebuggerNonUserCode]
		public void SetChecksumNull()
		{
			base[tablePatchElements.ChecksumColumn] = Convert.DBNull;
		}
	}

	[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
	public class PatchIdentityRowChangeEvent : EventArgs
	{
		private PatchIdentityRow eventRow;

		private DataRowAction eventAction;

		[DebuggerNonUserCode]
		public PatchIdentityRow Row => eventRow;

		[DebuggerNonUserCode]
		public DataRowAction Action => eventAction;

		[DebuggerNonUserCode]
		public PatchIdentityRowChangeEvent(PatchIdentityRow row, DataRowAction action)
		{
			eventRow = row;
			eventAction = action;
		}
	}

	[GeneratedCode("System.Data.Design.TypedDataSetGenerator", "2.0.0.0")]
	public class PatchElementsRowChangeEvent : EventArgs
	{
		private PatchElementsRow eventRow;

		private DataRowAction eventAction;

		[DebuggerNonUserCode]
		public PatchElementsRow Row => eventRow;

		[DebuggerNonUserCode]
		public DataRowAction Action => eventAction;

		[DebuggerNonUserCode]
		public PatchElementsRowChangeEvent(PatchElementsRow row, DataRowAction action)
		{
			eventRow = row;
			eventAction = action;
		}
	}

	private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

	private PatchIdentityDataTable tablePatchIdentity;

	private PatchElementsDataTable tablePatchElements;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[DebuggerNonUserCode]
	public PatchIdentityDataTable PatchIdentity => tablePatchIdentity;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[DebuggerNonUserCode]
	public PatchElementsDataTable PatchElements => tablePatchElements;

	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[DebuggerNonUserCode]
	public override SchemaSerializationMode SchemaSerializationMode
	{
		get
		{
			return _schemaSerializationMode;
		}
		set
		{
			_schemaSerializationMode = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DebuggerNonUserCode]
	public new DataTableCollection Tables => base.Tables;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DebuggerNonUserCode]
	public new DataRelationCollection Relations => base.Relations;

	[DebuggerNonUserCode]
	public CMPatch()
	{
		BeginInit();
		InitClass();
		CollectionChangeEventHandler value = SchemaChanged;
		base.Tables.CollectionChanged += value;
		base.Relations.CollectionChanged += value;
		EndInit();
	}

	[DebuggerNonUserCode]
	protected CMPatch(SerializationInfo info, StreamingContext context)
		: base(info, context, ConstructSchema: false)
	{
		if (IsBinarySerialized(info, context))
		{
			InitVars(initTable: false);
			CollectionChangeEventHandler value = SchemaChanged;
			Tables.CollectionChanged += value;
			Relations.CollectionChanged += value;
			return;
		}
		string s = (string)info.GetValue("XmlSchema", typeof(string));
		if (DetermineSchemaSerializationMode(info, context) == SchemaSerializationMode.IncludeSchema)
		{
			DataSet dataSet = new DataSet();
			dataSet.ReadXmlSchema(new XmlTextReader(new StringReader(s)));
			if (dataSet.Tables["PatchIdentity"] != null)
			{
				base.Tables.Add(new PatchIdentityDataTable(dataSet.Tables["PatchIdentity"]));
			}
			if (dataSet.Tables["PatchElements"] != null)
			{
				base.Tables.Add(new PatchElementsDataTable(dataSet.Tables["PatchElements"]));
			}
			base.DataSetName = dataSet.DataSetName;
			base.Prefix = dataSet.Prefix;
			base.Namespace = dataSet.Namespace;
			base.Locale = dataSet.Locale;
			base.CaseSensitive = dataSet.CaseSensitive;
			base.EnforceConstraints = dataSet.EnforceConstraints;
			Merge(dataSet, preserveChanges: false, MissingSchemaAction.Add);
			InitVars();
		}
		else
		{
			ReadXmlSchema(new XmlTextReader(new StringReader(s)));
		}
		GetSerializationData(info, context);
		CollectionChangeEventHandler value2 = SchemaChanged;
		base.Tables.CollectionChanged += value2;
		Relations.CollectionChanged += value2;
	}

	[DebuggerNonUserCode]
	protected override void InitializeDerivedDataSet()
	{
		BeginInit();
		InitClass();
		EndInit();
	}

	[DebuggerNonUserCode]
	public override DataSet Clone()
	{
		CMPatch obj = (CMPatch)base.Clone();
		obj.InitVars();
		obj.SchemaSerializationMode = SchemaSerializationMode;
		return obj;
	}

	[DebuggerNonUserCode]
	protected override bool ShouldSerializeTables()
	{
		return false;
	}

	[DebuggerNonUserCode]
	protected override bool ShouldSerializeRelations()
	{
		return false;
	}

	[DebuggerNonUserCode]
	protected override void ReadXmlSerializable(XmlReader reader)
	{
		if (DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
		{
			Reset();
			DataSet dataSet = new DataSet();
			dataSet.ReadXml(reader);
			if (dataSet.Tables["PatchIdentity"] != null)
			{
				base.Tables.Add(new PatchIdentityDataTable(dataSet.Tables["PatchIdentity"]));
			}
			if (dataSet.Tables["PatchElements"] != null)
			{
				base.Tables.Add(new PatchElementsDataTable(dataSet.Tables["PatchElements"]));
			}
			base.DataSetName = dataSet.DataSetName;
			base.Prefix = dataSet.Prefix;
			base.Namespace = dataSet.Namespace;
			base.Locale = dataSet.Locale;
			base.CaseSensitive = dataSet.CaseSensitive;
			base.EnforceConstraints = dataSet.EnforceConstraints;
			Merge(dataSet, preserveChanges: false, MissingSchemaAction.Add);
			InitVars();
		}
		else
		{
			ReadXml(reader);
			InitVars();
		}
	}

	[DebuggerNonUserCode]
	protected override XmlSchema GetSchemaSerializable()
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteXmlSchema(new XmlTextWriter(memoryStream, null));
		memoryStream.Position = 0L;
		return XmlSchema.Read(new XmlTextReader(memoryStream), null);
	}

	[DebuggerNonUserCode]
	internal void InitVars()
	{
		InitVars(initTable: true);
	}

	[DebuggerNonUserCode]
	internal void InitVars(bool initTable)
	{
		tablePatchIdentity = (PatchIdentityDataTable)base.Tables["PatchIdentity"];
		if (initTable && tablePatchIdentity != null)
		{
			tablePatchIdentity.InitVars();
		}
		tablePatchElements = (PatchElementsDataTable)base.Tables["PatchElements"];
		if (initTable && tablePatchElements != null)
		{
			tablePatchElements.InitVars();
		}
	}

	[DebuggerNonUserCode]
	private void InitClass()
	{
		base.DataSetName = "Patch";
		base.Prefix = "";
		base.Namespace = "http://tempuri.org/Patch.xsd";
		base.Locale = new CultureInfo("");
		base.EnforceConstraints = true;
		SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
		tablePatchIdentity = new PatchIdentityDataTable();
		base.Tables.Add(tablePatchIdentity);
		tablePatchElements = new PatchElementsDataTable();
		base.Tables.Add(tablePatchElements);
	}

	[DebuggerNonUserCode]
	private bool ShouldSerializePatchIdentity()
	{
		return false;
	}

	[DebuggerNonUserCode]
	private bool ShouldSerializePatchElements()
	{
		return false;
	}

	[DebuggerNonUserCode]
	private void SchemaChanged(object sender, CollectionChangeEventArgs e)
	{
		if (e.Action == CollectionChangeAction.Remove)
		{
			InitVars();
		}
	}

	[DebuggerNonUserCode]
	public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
	{
		CMPatch cMPatch = new CMPatch();
		XmlSchemaComplexType xmlSchemaComplexType = new XmlSchemaComplexType();
		XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
		xmlSchemaSequence.Items.Add(new XmlSchemaAny
		{
			Namespace = cMPatch.Namespace
		});
		xmlSchemaComplexType.Particle = xmlSchemaSequence;
		XmlSchema schemaSerializable = cMPatch.GetSchemaSerializable();
		if (xs.Contains(schemaSerializable.TargetNamespace))
		{
			MemoryStream memoryStream = new MemoryStream();
			MemoryStream memoryStream2 = new MemoryStream();
			try
			{
				schemaSerializable.Write(memoryStream);
				foreach (XmlSchema item in xs.Schemas(schemaSerializable.TargetNamespace))
				{
					memoryStream2.SetLength(0L);
					item.Write(memoryStream2);
					if (memoryStream.Length == memoryStream2.Length)
					{
						memoryStream.Position = 0L;
						memoryStream2.Position = 0L;
						while (memoryStream.Position != memoryStream.Length && memoryStream.ReadByte() == memoryStream2.ReadByte())
						{
						}
						if (memoryStream.Position == memoryStream.Length)
						{
							return xmlSchemaComplexType;
						}
					}
				}
			}
			finally
			{
				memoryStream?.Close();
				memoryStream2?.Close();
			}
		}
		xs.Add(schemaSerializable);
		return xmlSchemaComplexType;
	}
}
