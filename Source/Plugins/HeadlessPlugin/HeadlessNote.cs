﻿using AlephNote.PluginInterface;
using AlephNote.PluginInterface.Impl;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using AlephNote.PluginInterface.Util;

namespace AlephNote.Plugins.Headless
{
	class HeadlessNote : BasicHierachicalNote
	{
		private Guid _id;

		private string _title = "";
		public override string Title {get { return _title; } set { _title = value; OnPropertyChanged(); }}

		private string _text = "";
		public override string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

		private DirectoryPath _path = DirectoryPath.Root();
		public override DirectoryPath Path { get { return _path; } set { _path = value; OnPropertyChanged(); } }

		private DateTimeOffset _modificationDate = DateTimeOffset.Now;
		public override DateTimeOffset ModificationDate { get { return _modificationDate; } set { _modificationDate = value; OnPropertyChanged(); } }

		private DateTimeOffset _creationDate = DateTimeOffset.Now;
		public override DateTimeOffset CreationDate { get { return _creationDate; } set { _creationDate = value; OnPropertyChanged(); } }

		private bool _isPinned = false;
		public override bool IsPinned { get { return _isPinned; } set { _isPinned = value; OnPropertyChanged(); } }

		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();

		public override ObservableCollection<string> Tags { get { return _tags; } }

		public HeadlessNote(Guid uid)
		{
			_id = uid;
		}

		public override string GetUniqueName()
		{
			return _id.ToString("B");
		}

		public override XElement Serialize()
		{
			var data = new object[]
			{
				new XElement("ID", _id),
				new XElement("Title", Title),
				new XElement("Text", XHelper.ConvertToC80Base64(Text)),
				new XElement("Tags", Tags.Select(p => new XElement("Tag", p)).Cast<object>().ToArray()),
				new XElement("ModificationDate", ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("CreationDate", CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")),
				new XElement("Path", Path.Serialize()),
				new XElement("IsPinned", IsPinned),
			};

			var r = new XElement("localnote", data);
			r.SetAttributeValue("plugin", HeadlessPlugin.Name);
			r.SetAttributeValue("pluginversion", HeadlessPlugin.Version.ToString());

			return r;
		}

		public override void Deserialize(XElement input)
		{
			_id = XHelper.GetChildValueGUID(input, "ID");
			Title = XHelper.GetChildValueString(input, "Title");
			Text = XHelper.GetChildBase64String(input, "Text");
			Path = DirectoryPath.Deserialize(XHelper.GetChildrenOrEmpty(input, "Path", "PathComponent"));
			Tags.Synchronize(XHelper.GetChildValueStringList(input, "Tags", "Tag"));
			CreationDate = XHelper.GetChildValueDateTimeOffset(input, "CreationDate");
			ModificationDate = XHelper.GetChildValueDateTimeOffset(input, "ModificationDate");
			IsPinned = XHelper.GetChildValue(input, "IsPinned", false);
		}

		protected override BasicHierachicalNote CreateClone()
		{
			var n = new HeadlessNote(_id);
			n._title = _title;
			n._text = _text;
			n._tags.Synchronize(_tags);
			n._path = _path;
			n._isPinned = _isPinned;
			return n;
		}

		public override void ApplyUpdatedData(INote iother)
		{
			var other = (HeadlessNote)iother;

			using (SuppressDirtyChanges())
			{
				_title = other.Title;
				_text = other.Text;
				_tags.Synchronize(other.Tags);
				_path = other._path;
				_isPinned = other._isPinned;
			}
		}

		public override void OnAfterUpload(INote clonenote)
		{
			//
		}
	}
}
