using Regul.S3PI.Interfaces;
using System;

namespace Regul.S3PE.Interfaces
{
    public interface IViewer
    {
        IResource Resource { get; set; }

        Action EditResource { get; set; }
    }
}