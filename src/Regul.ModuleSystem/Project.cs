﻿#region

using System.Runtime.Serialization;
using System.Xml.Serialization;
using Onebeld.Extensions;

#endregion

namespace Regul.ModuleSystem
{
    public class Project : ViewModelBase
    {
        private string _idEditor;
        private string _path;

        [XmlAttribute]
        [DataMember]
        public string Path
        {
            get => _path;
            set => RaiseAndSetIfChanged(ref _path, value);
        }

        [XmlAttribute]
        [DataMember]
        public string IdEditor
        {
            get => _idEditor;
            set => RaiseAndSetIfChanged(ref _idEditor, value);
        }
    }
}