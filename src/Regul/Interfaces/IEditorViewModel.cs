using System;
using Regul.Enums;
using Regul.Structures;

namespace Regul.Interfaces;

public interface IEditorViewModel
{
    Workbench Workbench { get; set; }

    Type EditorType { get; }

    void Execute();

    SaveResult Save();
}