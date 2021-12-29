using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using PleasantUI.Media;

namespace PleasantUI
{
    public static class Extensions
    {
        public static IBrush ToBursh(this Color color) => new SolidColorBrush(color);

        public static string ToAxaml(this Theme theme)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("<Style xmlns=\"https://github.com/avaloniaui\"");
            sb.AppendLine("       xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");
            sb.AppendLine("    <Style.Resources>");

			foreach (PropertyInfo property in typeof(Theme).GetProperties())
			{
                if (property.Name == "Name" || property.Name == "ID")
                    continue;

                sb.AppendLine($"        <Color x:Key=\"{property.Name}\">#{(uint)property.GetValue(theme):X8}</Color>");
            }

            sb.AppendLine("    </Style.Resources>");
            sb.AppendLine("</Style>");
            
            return sb.ToString();
        }

        public static IBitmap ToBitmap(this WindowIcon icon)
        {
            if (icon != null)
            {
                MemoryStream stream = new MemoryStream();
                icon.Save(stream);
                stream.Position = 0;
                try
                {
                    return new Bitmap(stream);
                }
                catch
                {
                    return null;
                }
            }
            else
                return null;
        }
        
        public static T GetParentTOfLogical<T>(this ILogical logical) where T : class => logical.GetSelfAndLogicalAncestors().OfType<T>().FirstOrDefault<T>();

        public static T GetParentTOfVisual<T>(this IVisual visual) where T : class => visual.GetSelfAndVisualAncestors().OfType<T>().FirstOrDefault<T>();
        
        /// <summary>
        /// Get an control in the indicated template, this method can be within "protected overrive void OnTemplateApplied(e)" method only
        /// </summary>
        /// <typeparam name="T">Type of the Control to return</typeparam>
        /// <param name="templatedControl">TemplatedControl owner of the IndicatedControl</param>
        /// <param name="e">The TemplateAppliedEventArgs</param>
        /// <param name="name">The Name of the Control to return</param>
        /// <returns>a control with the indicated params</returns>
        public static T GetControl<T>(this TemplatedControl templatedControl,
            TemplateAppliedEventArgs e,
            string name) 
            where T : AvaloniaObject
            => e.NameScope.Find<T>(name);
        
        /// <summary>
        /// Removes the TabItem.
        /// </summary>
        /// <param name="tabControl">The TabControl Parent</param>
        /// <param name="tabItem">The TabItem to Remove</param>
        public static void CloseTab(this TabControl tabControl, TabItem tabItem)
        {
            try
            {
                if (tabItem == null)
                {
                }
                else
                {
                    //var n_index = NewIndex(tabControl, tabItem);
                    ((IList)tabControl.Items).Remove(tabItem); //removes the tabitem itself
                    //tabControl.SelectedIndex = n_index;
                }
            }
            catch (Exception e)
            {
                throw new Exception("The TabItem does not exist", e);
            }
        }

        /// <summary>
        /// Removes a TabItem with its index number.
        /// </summary>
        /// <param name="tabControl">A TabControl Parent</param>
        /// <param name="index">The TabItem Index</param>
        public static void CloseTab(this TabControl tabControl, int index)
        {
            index--;
            try
            {
                if (index < 0) { }
                else
                {
                    //var item = (tabControl.Items as List<TabItem>).Select(x => x.IsSelected == true);
                    //tabControl.SelectedIndex = NewIndex(tabControl, index);
                    ((IList)tabControl.Items).RemoveAt(index);
                }
            }
            catch (Exception e)
            {
                throw new Exception("the index must be greater than 0", e);
            }
        }

        /// <summary>
        /// Add a TabItem
        /// </summary>
        /// <param name="tabControl">The TabControl Parent</param>
        /// <param name="TabItemToAdd">The TabItem to Add</param>
        /// <returns>If the method has been done correctly,returns bool if it has been done correctly or false if it has been done incorrectly</returns>
        public static bool AddTab(this TabControl tabControl, TabItem TabItemToAdd, bool Focus = true)
        {
            try
            {
                //Thanks to Grooky this is possible
                ((IList)tabControl.Items).Add(TabItemToAdd);
                switch (Focus)
                {
                    case true:
                        TabItemToAdd.IsSelected = true;
                        break;
                }

                return true;
            }
            catch (SystemException e)
            {
                throw new SystemException("The Item to add is null", e);
            }
        }
    }
}