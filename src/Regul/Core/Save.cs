using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.VisualBasic;
using Regul.S3PI.Interfaces;
using Regul.S3PI.Package;
using System.IO;
using System.Text;

namespace Regul.Core
{
    public class Save
    {
        public IImage FamilyIcon;
        private readonly string nhdPath;

        public string WorldName;
        public ulong ImgInstance;

        public string Hash => nhdPath.GetHashCode().ToString();

        public Save(string strPath)
        {
            nhdPath = strPath;
            Package package = (Package)Package.OpenPackage(Path.Combine(Path.GetDirectoryName(strPath), "Meta.data"));
            IResourceIndexEntry rie = package.Find((R => R.ResourceType == 1653241999U));
            UnParse(S3PI.WrapperDealer.GetResource(package, rie).Stream);
        }

        private void UnParse(Stream s)
        {
            BinaryReader binaryReader = new BinaryReader(s);
            binaryReader.ReadInt32();
            StringBuilder stringBuilder = new StringBuilder();

            int num1 = checked(binaryReader.ReadInt32() - 1);
            int num2 = 0;

            while (num2 <= num1)
            {
                short num3 = binaryReader.ReadInt16();
                stringBuilder.Append(Strings.ChrW(num3));
                checked { ++num2; }
            }

            stringBuilder.Append(' ');
            int num4 = checked(binaryReader.ReadInt32() - 1);
            int num5 = 0;

            while (num5 <= num4)
            {
                short num3 = binaryReader.ReadInt16();
                stringBuilder.Append(Strings.ChrW(num3));
                checked { ++num5; }
            }

            WorldName = stringBuilder.ToString();
            binaryReader.ReadInt32();

            IPackage pkg = Package.OpenPackage(nhdPath);
            ImgInstance = binaryReader.ReadUInt64();
            IResourceIndexEntry rie = pkg.Find(entry => (long)entry.Instance == (long)ImgInstance & entry.ResourceType == 1802339198U);

            if (rie != null)
                FamilyIcon = new Bitmap(S3PI.WrapperDealer.GetResource(pkg, rie).Stream);
            else FamilyIcon = (DrawingImage)Application.Current.FindResource("UnknownIcon");

            binaryReader.ReadUInt64();
        }
    }
}
