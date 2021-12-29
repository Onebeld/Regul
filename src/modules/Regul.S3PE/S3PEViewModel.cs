using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using Onebeld.Extensions;
using PleasantUI.Controls.Custom;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Base;
using Regul.Base.Views.Windows;
using Regul.ModuleSystem;
using Regul.S3PE.Interfaces;
using Regul.S3PE.Structures;
using Regul.S3PE.Viewers;
using Regul.S3PE.Windows;
using Regul.S3PE.Windows.Editors;
using Regul.S3PI;
using Regul.S3PI.Extensions;
using Regul.S3PI.Interfaces;
using Regul.S3PI.Package;
using Regul.S3PI.Resources;

namespace Regul.S3PE
{
	public class S3PEViewModel : ViewModelBase
	{
		private enum DuplicateHandling
		{
			/// <summary>
			/// Refuse to create the request resource
			/// </summary>
			Reject,
			/// <summary>
			/// Delete any conflicting resource
			/// </summary>
			Replace,
			/// <summary>
			/// Ignore any conflicting resource
			/// </summary>
			Allow
		}

		public PleasantTabItem PleasantTabItem { get; set; }

		public S3PE S3Pe { get; set; }

		private AvaloniaList<Resource> _resources = new AvaloniaList<Resource>();
		private AvaloniaList<Resource> _allResources = new AvaloniaList<Resource>();
		private AvaloniaList<Resource> _selectedResources = new AvaloniaList<Resource>();
		private Resource _selectedResource;
		private IViewer _viewer;

		private List<IResource> _nameMap;
		private List<IResourceIndexEntry> _nameMapResourceIndexEntries;

		#region Search

		private bool _checkName;
		private bool _checkCompressed;
		private bool _checkTag;
		private bool _checkResourceType;
		private bool _checkResourceGroup;
		private bool _checkInstance;

		private string _name;
		private string _tag;
		private string _resourceType;
		private string _resourceGroup;
		private string _instance;
		private string _compressed;

		#endregion

		private bool _notSelectedResource = true;
		private bool _selectedMultipleResources;
		private bool _cannotResourceView;

		private bool _openedMenu;
		private bool _isEdited;
		private bool _searchEnabled = true;

		#region Properties

		#region Search

		private bool CheckName
		{
			get => _checkName;
			set
			{
				RaiseAndSetIfChanged(ref _checkName, value);
				UpdateResources(false);
			}
		}
		private bool CheckTag
		{
			get => _checkTag;
			set
			{
				RaiseAndSetIfChanged(ref _checkTag, value);
				UpdateResources(false);
			}
		}
		private bool CheckResourceType
		{
			get => _checkResourceType;
			set
			{
				RaiseAndSetIfChanged(ref _checkResourceType, value);
				UpdateResources(false);
			}
		}
		private bool CheckResourceGroup
		{
			get => _checkResourceGroup;
			set
			{
				RaiseAndSetIfChanged(ref _checkResourceGroup, value);
				UpdateResources(false);
			}
		}
		private bool CheckInstance
		{
			get => _checkInstance;
			set
			{
				RaiseAndSetIfChanged(ref _checkInstance, value);
				UpdateResources(false);
			}
		}
		private bool CheckCompressed
		{
			get => _checkCompressed;
			set
			{
				RaiseAndSetIfChanged(ref _checkCompressed, value);
				UpdateResources(false);
			}
		}

		private string Name
		{
			get => _name;
			set
			{
				RaiseAndSetIfChanged(ref _name, value);
				if (_searchEnabled && CheckName)
					UpdateResources(false);
			}
		}
		private string Tag
		{
			get => _tag;
			set
			{
				RaiseAndSetIfChanged(ref _tag, value);
				if (_searchEnabled && CheckTag)
					UpdateResources(false);
			}
		}
		private string ResourceType
		{
			get => _resourceType;
			set
			{
				RaiseAndSetIfChanged(ref _resourceType, value);
				if (_searchEnabled && CheckResourceType)
					UpdateResources(false);
			}
		}
		private string ResourceGroup
		{
			get => _resourceGroup;
			set
			{
				RaiseAndSetIfChanged(ref _resourceGroup, value);
				if (_searchEnabled && CheckResourceGroup)
					UpdateResources(false);
			}
		}
		private string Instance
		{
			get => _instance;
			set
			{
				RaiseAndSetIfChanged(ref _instance, value);
				if (_searchEnabled && CheckInstance)
					UpdateResources(false);
			}
		}
		private string Compressed
		{
			get => _compressed;
			set
			{
				RaiseAndSetIfChanged(ref _compressed, value);
				if (_searchEnabled && CheckCompressed)
					UpdateResources(false);
			}
		}

		#endregion

		public bool NotSelectedResource
		{
			get => _notSelectedResource;
			set => RaiseAndSetIfChanged(ref _notSelectedResource, value);
		}
		public bool SelectedMultipleResources
		{
			get => _selectedMultipleResources;
			set => RaiseAndSetIfChanged(ref _selectedMultipleResources, value);
		}
		public bool CannotResourceView
		{
			get => _cannotResourceView;
			set => RaiseAndSetIfChanged(ref _cannotResourceView, value);
		}

		public bool OpenedMenu
		{
			get => _openedMenu;
			set => RaiseAndSetIfChanged(ref _openedMenu, value);
		}

		public AvaloniaList<Resource> Resources
		{
			get => _resources;
			set => RaiseAndSetIfChanged(ref _resources, value);
		}

		public AvaloniaList<Resource> AllResources
		{
			get => _allResources;
			set => RaiseAndSetIfChanged(ref _allResources, value);
		}

		public string PathToFile { get; set; }

		public IPackage CurrentPackage { get; set; }

		public AvaloniaList<Resource> SelectedResources
		{
			get => _selectedResources;
			set => RaiseAndSetIfChanged(ref _selectedResources, value);
		}

		public Resource SelectedResource
		{
			get => _selectedResource;
			set
			{
				RaiseAndSetIfChanged(ref _selectedResource, value);

				if (value == null)
				{
					Viewer = null;

					if (!SelectedMultipleResources)
						NotSelectedResource = true;
					if (NotSelectedResource)
						SelectedMultipleResources = false;
					CannotResourceView = false;

					return;
				}

				if (SelectedResources.Count > 1)
				{
					Viewer = null;

					SelectedMultipleResources = true;
					CannotResourceView = false;
					NotSelectedResource = false;

					SelectedResource = null;

					return;
				}

				SelectedMultipleResources = false;
				CannotResourceView = false;
				NotSelectedResource = false;

				try
				{
					IResource resource = WrapperDealer.GetResource(CurrentPackage, value.IndexEntry);

					switch (value.Tag)
					{
						case "SNAP":
						case "THUM":
						case "TWNI":
						case "ICON":
						case "IMAG":
						case "TSNP":
							Viewer = new ImageViewer(resource, this);
							break;
						case "_IMG":
						case "_ADS":
							Viewer = new DDSViewer(resource);
							break;
						case "S3SA":
							Viewer = new S3SAViewer(resource, ImportDll);
							break;
						case "_XML":
						case "CNFG":
						case "_CSS":
						case "LAYO":
						case "VOCE":
						case "MIXR":
						case "ITUN":
						case "DMTR":
						case "_INI":
						case "SKIL":
						case "PTRN":
						case "BUFF":
						case "RMLS":
						case "TRIG":
						case "HTML":
							Viewer = new TextViewer(resource, EditTextResource, value.Tag);
							break;
						case "_KEY":
						case "STBL":
							Viewer = new NameMapViewer(resource, EditSTBLResource);
							break;
						default:
							if (HasStringValueContentField(resource))
								Viewer = new TextViewer(resource["Value"]);
							else
							{
								Viewer = null;

								SelectedMultipleResources = false;
								CannotResourceView = true;
								NotSelectedResource = false;
							}
							break;
					}
				}
				catch (Exception e)
				{
					Viewer = new ExceptionViewer(e);
				}
			}
		}

		public IViewer Viewer
		{
			get => _viewer;
			set => RaiseAndSetIfChanged(ref _viewer, value);
		}

		public bool IsEdited
		{
			get => _isEdited;
			set
			{
				RaiseAndSetIfChanged(ref _isEdited, value);
				PleasantTabItem.IsEditedIndicator = value;
			}
		}

		#endregion

		public S3PEViewModel(PleasantTabItem pleasantTabItem, Editor editor, S3PE s3PE, string path = null)
		{
			PleasantTabItem = pleasantTabItem;
			S3Pe = s3PE;
			S3Pe.CurrentEditor = editor;

			PathToFile = path;

			if (string.IsNullOrEmpty(path))
			{
				CurrentPackage = Package.NewPackage();
				IsEdited = true;
			}
			else CurrentPackage = Package.OpenPackage(path, true);

			if (CurrentPackage == null || CurrentPackage.GetResourceList.Count == 0)
				CurrentPackage = Package.NewPackage();
		}
		public S3PEViewModel(PleasantTabItem pleasantTabItem, Editor editor, S3PE s3PE, string path, Project project) : this(pleasantTabItem, editor, s3PE, path)
		{
			S3Pe.CurrentProject = project;

			CreateNameMap();

			UpdateResources(true);
		}

		private async void AddResource()
		{
			// Hack, without it hotkeys will only be processed for the first tab
			if (!PleasantTabItem.IsSelected) return;

			ResourceDetails resourceDetails = WindowsManager.CreateModalWindow<ResourceDetails>();

			if (resourceDetails == null) return;

			if (await resourceDetails.Show<bool>(WindowsManager.MainWindow))
			{
				ResourceDetailsViewModel viewModel = resourceDetails.GetDataContext<ResourceDetailsViewModel>();

				IResourceIndexEntry rie;

				IResource resource = WrapperDealer.CreateNewResource("0x" + viewModel.ResourceType.ToString("x8").ToUpper());

				if (!string.IsNullOrEmpty(viewModel.FileName))
				{
					MemoryStream ms = new MemoryStream();
					BinaryWriter w = new BinaryWriter(ms);
					BinaryReader r = new BinaryReader(new FileStream(viewModel.FileName, FileMode.Open, FileAccess.Read));

					w.Write(r.ReadBytes((int)r.BaseStream.Length));
					r.Close();
					w.Flush();

					if (resource is ScriptResource scriptResource)
					{
						scriptResource.Assembly = new BinaryReader(ms);

						rie = NewResource(viewModel, ms, viewModel.Replace ? DuplicateHandling.Replace : DuplicateHandling.Reject, viewModel.Compress);

						CurrentPackage.ReplaceResource(rie, resource);
					}
					else
					{
						rie = NewResource(viewModel, ms,
							viewModel.Replace ? DuplicateHandling.Replace : DuplicateHandling.Reject, viewModel.Compress);
					}
				}
				else
				{
					rie = NewResource(viewModel, null,
					viewModel.Replace ? DuplicateHandling.Replace : DuplicateHandling.Reject, viewModel.Compress);

					CurrentPackage.ReplaceResource(rie, resource);
				}

				if (rie == null) return;

				if (viewModel.UseResourceName)
					GetResourceName(viewModel.Instance, viewModel.ResourceName, true, viewModel.Replace);

				UpdateResources(true);
			}

			((UserControl)PleasantTabItem.Content).Focus();
		}

		private void CopyResource()
		{
			if (!PleasantTabItem.IsSelected || SelectedResources.Count < 1) return;

			try
			{
				List<ClipboardDataFormat> dataFormats = new List<ClipboardDataFormat>();

				foreach (Resource selectedResource in SelectedResources)
				{
					dataFormats.Add(new ClipboardDataFormat
					{
						Data = Convert.ToBase64String(WrapperDealer.GetResource(CurrentPackage, selectedResource.IndexEntry, true).AsBytes),
						Tgin = new SerializableTGIN(selectedResource.IndexEntry as AResourceIndexEntry, selectedResource.Name)
					});
				}

				using (StringWriter writer = new StringWriter())
				{
					new XmlSerializer(typeof(List<ClipboardDataFormat>)).Serialize(writer, dataFormats);

					Application.Current.Clipboard.SetTextAsync(writer.ToString());
				}
			}
			catch { }
		}

		private async void PasteResource()
		{
			if (!PleasantTabItem.IsSelected) return;

			try
			{
				List<ClipboardDataFormat> dataFormats;

				using (StringReader reader = new StringReader(await Application.Current.Clipboard.GetTextAsync()))
				{
					dataFormats =
						(List<ClipboardDataFormat>)new XmlSerializer(typeof(List<ClipboardDataFormat>)).Deserialize(reader);
				}

				foreach (ClipboardDataFormat dataFormat in dataFormats)
				{
					IResourceIndexEntry entry = CurrentPackage.AddResource(new TGIBlock(null, dataFormat.Tgin.ResType, dataFormat.Tgin.ResGroup, dataFormat.Tgin.ResInstance), new MemoryStream(Convert.FromBase64String(dataFormat.Data)), true);

					GetResourceName(entry.Instance, dataFormat.Tgin.ResName, true, false);
				}
			}
			catch { }

			UpdateResources(true);
			IsEdited = true;
		}

		private void CompressedResources()
		{
			for (var index = 0; index < SelectedResources.Count; index++)
			{
				Resource selectedResource = SelectedResources[index];
				ushort target = 0xffff;
				if (CompressedCheck(selectedResource)) target = 0;

				selectedResource.IndexEntry.Compressed = target;
			}

			UpdateResources(true);
			IsEdited = true;
		}

		private void DeleteResources()
		{
			foreach (Resource resource in SelectedResources)
				CurrentPackage.GetResourceList.Remove(resource.IndexEntry);

			UpdateResources(true);
			IsEdited = true;
		}

		private void DuplicateResource()
		{
			if (SelectedResource == null) return;

			byte[] buffer = WrapperDealer.GetResource(CurrentPackage, SelectedResource.IndexEntry, true).AsBytes;

			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(buffer, 0, buffer.Length);

			IResourceIndexEntry rie = CurrentPackage.AddResource(SelectedResource.IndexEntry, memoryStream, false);
			rie.Compressed = SelectedResource.IndexEntry.Compressed;

			IResource res = WrapperDealer.GetResource(CurrentPackage, rie, true);
			CurrentPackage.ReplaceResource(rie, res);

			UpdateResources(true);
			IsEdited = true;
		}

		private void SelectAllResources() => S3Pe.DataGrid.SelectAll();

		private async void ResourceDetails()
		{
			if (!PleasantTabItem.IsSelected || SelectedResource == null) return;

			ResourceDetails resourceDetails = WindowsManager.CreateModalWindow<ResourceDetails>(null, SelectedResource.IndexEntry, !string.IsNullOrEmpty(SelectedResource.Name));

			if (resourceDetails == null) return;

			ResourceDetailsViewModel viewModel = resourceDetails.GetDataContext<ResourceDetailsViewModel>();

			viewModel.Compress = SelectedResource.IndexEntry.Compressed != 0;
			viewModel.Replace = true;

			if (viewModel.UseResourceName) viewModel.ResourceName = SelectedResource.Name;

			if (await resourceDetails.Show<bool>(WindowsManager.MainWindow))
			{
				IResourceIndexEntry rie = CurrentPackage.Find(x => x.Equals(SelectedResource.IndexEntry));
				IResource resource = WrapperDealer.GetResource(CurrentPackage, rie);

				CurrentPackage.GetResourceList.Remove(rie);

				IResourceIndexEntry newRie;

				if (!string.IsNullOrEmpty(viewModel.FileName))
				{
					MemoryStream ms = new MemoryStream();
					BinaryWriter w = new BinaryWriter(ms);
					BinaryReader r = new BinaryReader(new FileStream(viewModel.FileName, FileMode.Open, FileAccess.Read));

					w.Write(r.ReadBytes((int)r.BaseStream.Length));
					r.Close();
					w.Flush();

					newRie = NewResource(viewModel, ms,
						viewModel.Replace ? DuplicateHandling.Replace : DuplicateHandling.Reject, viewModel.Compress);
				}
				else
				{
					newRie = NewResource(viewModel, null,
						viewModel.Replace ? DuplicateHandling.Replace : DuplicateHandling.Reject, viewModel.Compress);

					CurrentPackage.ReplaceResource(newRie, resource);
				}

				if (viewModel.UseResourceName && !string.IsNullOrEmpty(viewModel.ResourceName))
					GetResourceName(viewModel.Instance, viewModel.ResourceName, true, viewModel.AllowRename);

				UpdateResources(true);

				S3Pe.DataGrid.SelectedItem = AllResources.First(x => x.IndexEntry.Equals(newRie));
			}

			((UserControl)PleasantTabItem.Content).Focus();
		}

		#region Import

		private async void ImportResources()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				AllowMultiple = true,
				Filters = new List<FileDialogFilter>
				{
					new FileDialogFilter
					{
						Name = App.GetResource<string>("AllFiles"),
						Extensions = {"*"}
					}
				}
			};
			string[] paths = await dialog.ShowAsync(WindowsManager.MainWindow);

			if (paths != null && paths.Length > 0)
			{
				List<string> foo = new List<string>();
				foreach (string path in paths)
				{
					if (Directory.Exists(path)) foo.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
					else if (File.Exists(path)) foo.Add(path);
				}
				paths = foo.ToArray();


			}
		}

		private bool ImportFile(string fileName,
			TGIN tgin,
			bool useName,
			bool rename,
			bool compress,
			DuplicateHandling dup,
			bool select)
		{
			IResourceKey resourceKey = (TGIBlock)tgin;

			string resName = tgin.ResName;
			bool nmOK = true;

			MemoryStream ms = new MemoryStream();
			BinaryWriter w = new BinaryWriter(ms);
			BinaryReader r = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));

			w.Write(r.ReadBytes((int)r.BaseStream.Length));
			r.Close();
			w.Flush();

			if (useName && !string.IsNullOrEmpty(resName))
				nmOK = GetResourceName(resourceKey.Instance, resName, true, rename);

			NewResource(resourceKey, ms, dup, compress);

			return nmOK;
		}

		public async void ImportDll()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filters = new List<FileDialogFilter>
				{
					new FileDialogFilter {Name = $"DLL-{App.GetResource<string>("Files")}", Extensions = {"dll"}}
				}
			};

			string file = (await dialog.ShowAsync(WindowsManager.MainWindow)).FirstOrDefault();

			if (!string.IsNullOrEmpty(file))
			{
				IResourceIndexEntry rie = CurrentPackage.Find(x => x.Equals(SelectedResource.IndexEntry));
				int index = Resources.IndexOf(SelectedResource);

				ScriptResource resource = WrapperDealer.GetResource(CurrentPackage, rie) as ScriptResource;

				using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
				{
					resource.Assembly = new BinaryReader(fileStream);
				}

				CurrentPackage.ReplaceResource(rie, resource);

				UpdateResources(true);

				IsEdited = true;

				S3Pe.DataGrid.SelectedItem = Resources.ElementAt(index);

				WindowsManager.MainWindow.GetDataContext<MainViewModel>().NotificationManager.Show(
					new Notification(App.GetResource<string>("Successful"), App.GetResource<string>("FileImported"), NotificationType.Success));
			}
		}

		public async void ImportImag()
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filters = new List<FileDialogFilter>
				{
					new FileDialogFilter {Name = $"PNG-{App.GetResource<string>("Files")}", Extensions = {"png"}}
				}
			};

			string file = (await dialog.ShowAsync(WindowsManager.MainWindow)).FirstOrDefault();

			if (!string.IsNullOrEmpty(file))
			{
				IResourceIndexEntry rie = CurrentPackage.Find(x => x.Equals(SelectedResource.IndexEntry));
				int index = Resources.IndexOf(SelectedResource);

				MemoryStream ms = new MemoryStream();
				BinaryWriter w = new BinaryWriter(ms);
				BinaryReader r = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read));

				w.Write(r.ReadBytes((int)r.BaseStream.Length));
				r.Close();
				w.Flush();

				CurrentPackage.ReplaceResource(rie, new DefaultResource(ms));

				UpdateResources(true);

				IsEdited = true;

				S3Pe.DataGrid.SelectedItem = Resources.ElementAt(index);

				WindowsManager.MainWindow.GetDataContext<MainViewModel>().NotificationManager.Show(
					new Notification(App.GetResource<string>("Successful"), App.GetResource<string>("FileImported"), NotificationType.Success));
			}
		}

		#endregion

		private IResource ReadResource(string fileName)
		{
			MemoryStream ms = new MemoryStream();
			using (BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open)))
			{
				ms.Write(br.ReadBytes((int)br.BaseStream.Length), 0, (int)br.BaseStream.Length);
				br.Close();
			}

			IResource resource = WrapperDealer.CreateNewResource("*");
			ConstructorInfo constructorInfo = resource.GetType().GetConstructor(new Type[] { typeof(Stream) });
			return (IResource)constructorInfo.Invoke(new object[] { ms });
		}

		private async void ExportResources()
		{
			if (SelectedResources.Count == 0) return;

			OpenFolderDialog dialog = new OpenFolderDialog();
			string folder = await dialog.ShowAsync(WindowsManager.MainWindow);

			if (!string.IsNullOrEmpty(folder))
			{
				bool overwriteAll = false;
				bool skipAll = false;

				try
				{
					foreach (Resource resource in SelectedResources)
					{
						if (!(resource.IndexEntry is AResourceIndexEntry entry)) return;

						TGIN tgin = entry;
						tgin.ResName = resource.Name;

						string file = Path.Combine(folder, tgin);
						if (File.Exists(file))
						{
							if (skipAll) continue;
							if (!overwriteAll)
							{
								string resultMb = await MessageBox.Show(WindowsManager.MainWindow,
									App.GetResource<string>("Message"), "Allo", new List<MessageBoxButton>
									{
										new MessageBoxButton
										{
											Text = App.GetResource<string>("No"),
											Default = true,
											Result = "No",
											IsKeyDown = true
										},
										new MessageBoxButton
										{
											Text = App.GetResource<string>("NoToAll"),
											Result = "NoToAll"
										},
										new MessageBoxButton
										{
											Text = App.GetResource<string>("Yes"),
											Result = "Yes"
										},
										new MessageBoxButton
										{
											Text = App.GetResource<string>("YesToAll"),
											Result = "YesToAll"
										},
										new MessageBoxButton
										{
											Text = App.GetResource<string>("Abandon"),
											Result = "Abandon"
										}
									}, MessageBox.MessageBoxIcon.Question);

								switch (resultMb)
								{
									case "No":
										continue;
									case "NoToAll":
										skipAll = true;
										continue;
									case "Yes":
										break;
									case "YesToAll":
										overwriteAll = true;
										break;
									case "Abandon":
										return;
								}
							}
						}
						ExportFile(entry, file);
					}
				}
				catch { }
			}
		}

		private IResourceIndexEntry NewResource(IResourceKey rk, Stream s, DuplicateHandling dups, bool compress)
		{
			IResourceIndexEntry rie = CurrentPackage.Find(rk.Equals);

			if (rie != null)
			{
				switch (dups)
				{
					case DuplicateHandling.Reject:
						return null;
					case DuplicateHandling.Replace:
						CurrentPackage.DeleteResource(rie);
						break;
				}
			}

			rie = CurrentPackage.AddResource(rk, s, false);
			if (rie == null) return null;

			rie.Compressed = (ushort)(compress ? 0xFFFF : 0);

			IsEdited = true;

			return rie;
		}

		private string GetResourceTag(IResourceIndexEntry rie)
		{
			string key = rie["ResourceType"];
			if (ExtList.Ext.ContainsKey(key)) return ExtList.Ext[key][0];
			if (ExtList.Ext.ContainsKey("*")) return ExtList.Ext["*"][0];
			return "";
		}

		private void ExportFile(IResourceIndexEntry rie, string path)
		{
			IResource res = WrapperDealer.GetResource(CurrentPackage, rie, true);

			Stream s = res.Stream;
			s.Position = 0;

			BinaryWriter w = new BinaryWriter(new FileStream(path, FileMode.Create));
			w.Write(new BinaryReader(s).ReadBytes((int)s.Length));
			w.Close();
		}

		private void CopyRK()
		{
			if (SelectedResources.Count > 0)
				Application.Current.Clipboard.SetTextAsync(string.Join("\r\n", SelectedResources.Select(r => r.IndexEntry.ToString())));
		}

		private bool CompressedCheck(Resource resource)
		{
			if (resource == null) return false;
			return resource.IndexEntry.Compressed != 0;
		}

		private void UpdateResources(bool updateAllResources)
		{
			if (updateAllResources)
			{
				AllResources = new AvaloniaList<Resource>(CurrentPackage.GetResourceList.Count);

				for (int index = 0; index < CurrentPackage.GetResourceList.Count; index++)
				{
					IResourceIndexEntry indexEntry = CurrentPackage.GetResourceList[index];
					AllResources.Add(new Resource
					{
						IndexEntry = indexEntry,
						Tag = GetResourceTag(indexEntry),
						Name = GetResourceName(indexEntry)
					});
				}
			}

			if (!CheckName && !CheckTag && !CheckResourceType && !CheckResourceGroup && !CheckInstance && !CheckCompressed)
			{
				Resources = AllResources;
				return;
			}

			List<Resource> res = AllResources.ToList();

			if (CheckName)
				res = res.FindAll(x => x.Name.ToLower().Contains(Name.ToLower()));
			if (CheckTag)
				res = res.FindAll(x => x.Tag.ToLower().Contains(Tag.ToLower()));
			if (CheckResourceType)
				res = res.FindAll(x => x.IndexEntry.ResourceType == Convert.ToUInt32(ResourceType, ResourceType.StartsWith("0x") ? 16 : 10));
			if (CheckResourceGroup)
				res = res.FindAll(x => x.IndexEntry.ResourceGroup == Convert.ToUInt32(ResourceGroup, ResourceGroup.StartsWith("0x") ? 16 : 10));
			if (CheckInstance)
				res = res.FindAll(x => x.IndexEntry.Instance == Convert.ToUInt64(Instance, Instance.StartsWith("0x") ? 16 : 10));
			if (CheckCompressed)
				res = res.FindAll(x =>
				{
					if (App.GetResource<string>("Yes").ToLower() == Compressed.ToLower()
						|| Compressed.ToLower() == "Yes".ToLower()
						|| Compressed == "+"
						|| Compressed == "1"
						|| Convert.ToUInt16(Compressed, Compressed.StartsWith("0x") ? 16 : 10) == 0xffff)
					{
						return x.IndexEntry.Compressed == 0xffff;
					}

					if (App.GetResource<string>("No").ToLower() == Compressed.ToLower()
						|| Compressed.ToLower() == "No".ToLower()
						|| Compressed == "-"
						|| Convert.ToUInt16(Compressed, Compressed.StartsWith("0x") ? 16 : 10) == 0)
					{
						return x.IndexEntry.Compressed == 0;
					}

					return true;
				});

			Resources = new AvaloniaList<Resource>(res);
		}

		#region NameMap

		private string GetResourceName(IResourceIndexEntry rie)
		{
			if (_nameMap == null || _nameMap.Count == 0) return "";
			for (int index = 0; index < _nameMap.Count; index++)
			{
				IDictionary<ulong, string> nmap = (IDictionary<ulong, string>)_nameMap[index];
				if (nmap.ContainsKey(rie.Instance)) return nmap[rie.Instance];
			}

			return "";
		}

		private bool GetResourceName(ulong instance, string resourceName, bool create, bool replace) =>
			MergeNameMap(new[] { new KeyValuePair<ulong, string>(instance, resourceName) }, create, replace);

		private void CreateNameMap()
		{
			_nameMap = new List<IResource>();
			_nameMapResourceIndexEntries = new List<IResourceIndexEntry>();

			IList<IResourceIndexEntry> lrie = CurrentPackage.FindAll(_key => _key.ResourceType == 0x0166038C);
			for (int index = 0; index < lrie.Count; index++)
			{
				IResourceIndexEntry rie = lrie[index];
				IResource res = WrapperDealer.GetResource(CurrentPackage, rie);
				switch (res)
				{
					case null:
						continue;
					case IDictionary<ulong, string> _:
						_nameMap.Add(res);
						_nameMapResourceIndexEntries.Add(rie);
						break;
				}
			}
		}

		private bool MergeNameMap(IEnumerable<KeyValuePair<ulong, string>> newMap, bool create, bool replace)
		{
			if (_nameMap == null) CreateNameMap();
			if (_nameMap == null || _nameMap.Count == 0 && create)
			{
				CurrentPackage.AddResource(new TGIBlock(null, 0x0166038C, 0, 0), null, false);

				CreateNameMap();

				if (_nameMap == null) return false;
			}

			try
			{
				IDictionary<ulong, string> nmap = _nameMap[0] as IDictionary<ulong, string>;
				if (nmap == null) return false;

				foreach (KeyValuePair<ulong, string> keyValuePair in newMap)
				{
					if (nmap.ContainsKey(keyValuePair.Key))
					{
						if (replace) nmap[keyValuePair.Key] = keyValuePair.Value;
					}
					else nmap.Add(keyValuePair.Key, keyValuePair.Value);
				}

				CurrentPackage.ReplaceResource(_nameMapResourceIndexEntries[0], _nameMap[0]);

				IsEdited = true;
			}
			catch
			{
				return false;
			}

			return true;
		}

		private void ClearNameMap()
		{
			if (_nameMap == null) return;
			_nameMap.Clear();
			_nameMap = null;

			if (_nameMapResourceIndexEntries == null) return;
			_nameMapResourceIndexEntries.Clear();
			_nameMapResourceIndexEntries = null;
		}

		#endregion

		#region EditResource

		private async void EditTextResource()
		{
			TextEditor textEditor = WindowsManager.CreateModalWindow<TextEditor>();

			if (textEditor == null) return; 

			IResource resource = WrapperDealer.GetResource(CurrentPackage, SelectedResource.IndexEntry);

			TextEditorViewModel viewModel = textEditor.GetDataContext<TextEditorViewModel>();
			viewModel.TextDocument = new TextDocument
			{
				Text = await new StreamReader(resource.Stream).ReadToEndAsync()
			};

			switch (SelectedResource.Tag)
			{
				case "CNFG":
				case "LAYO":
				case "VOCE":
				case "MIXR":
				case "ITUN":
				case "DMTR":
				case "_INI":
				case "SKIL":
				case "PTRN":
				case "BUFF":
				case "RMLS":
				case "TRIG":
				case "S3SA":
				case "TXTC":
				case "_XML":
				case "HTML":
					textEditor.AvaloniaEdit.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
					break;
				case "_CSS":
					textEditor.AvaloniaEdit.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("CSS");
					break;
			}

			string result = await textEditor.Show<string>(WindowsManager.MainWindow);
			if (result != null)
			{
				IResourceIndexEntry rie = CurrentPackage.Find(x => x.Equals(SelectedResource.IndexEntry));

				CurrentPackage.GetResourceList.Remove(rie);

				IResourceIndexEntry newRie;

				MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(viewModel.TextDocument.Text));

				newRie = NewResource((TGIBlock)new TGIN
				{
					ResGroup = rie.ResourceGroup,
					ResInstance = rie.Instance,
					ResType = rie.ResourceType
				}, ms, DuplicateHandling.Reject, rie.Compressed == 0xffff);

				UpdateResources(true);

				S3Pe.DataGrid.SelectedItem = Resources.First(x => x.IndexEntry.Equals(newRie));

				IsEdited = true;
			}
		}

		private async void EditSTBLResource()
		{
			STBLEditor stblEditor = WindowsManager.CreateModalWindow<STBLEditor>();

			if (stblEditor == null) return;

			stblEditor.GetDataContext<STBLEditorViewModel>().Initialize(WrapperDealer.GetResource(CurrentPackage, SelectedResource.IndexEntry) as IDictionary<ulong, string>);

			IDictionary<ulong, string> dictionary = await stblEditor.Show<IDictionary<ulong, string>>(WindowsManager.MainWindow);
			if (dictionary != null)
			{
				IResourceIndexEntry rie = CurrentPackage.Find(x => x.Equals(SelectedResource.IndexEntry));
				int index = Resources.IndexOf(SelectedResource);
				IResource resource = WrapperDealer.GetResource(CurrentPackage, rie);
				IDictionary<ulong, string> resourceDictionary = resource as IDictionary<ulong, string>;

				resourceDictionary.Clear();
				foreach (KeyValuePair<ulong, string> item in dictionary)
					resourceDictionary.Add(item);

				CurrentPackage.ReplaceResource(rie, resource);

				if (resource is NameMapResource)
					CreateNameMap();

				UpdateResources(true);

				S3Pe.DataGrid.SelectedItem = Resources.ElementAt(index);

				IsEdited = true;
			}
		}

		private async void EditWithDataGrid()
		{

		}

		#endregion

		#region SearchPanel

		private void QBE()
		{
			if (SelectedResource != null)
			{
				_searchEnabled = false;

				Name = SelectedResource.Name;
				Tag = SelectedResource.Tag;
				Instance = "0x" + SelectedResource.IndexEntry.Instance.ToString("x16").ToUpper();
				ResourceType = "0x" + SelectedResource.IndexEntry.ResourceType.ToString("x8").ToUpper();
				ResourceGroup = "0x" + SelectedResource.IndexEntry.ResourceGroup.ToString("x8").ToUpper();
				Compressed = "0x" + SelectedResource.IndexEntry.Compressed.ToString("x4").ToUpper();

				_searchEnabled = true;

				if (CheckName || CheckTag || CheckInstance || CheckResourceType || CheckResourceGroup || CheckCompressed)
					UpdateResources(false);
			}
		}

		private async void PasteRK()
		{
			TGIBlock tgiBlock = new TGIBlock(null);
			if (!AResourceKey.TryParse(await Application.Current.Clipboard.GetTextAsync(), tgiBlock)) return;

			_searchEnabled = false;

			ResourceType = "0x" + tgiBlock.ResourceType.ToString("x8").ToUpper();
			ResourceGroup = "0x" + tgiBlock.ResourceGroup.ToString("x8").ToUpper();
			Instance = "0x" + tgiBlock.Instance.ToString("x16").ToUpper();

			_searchEnabled = true;

			if (CheckInstance || CheckResourceType || CheckResourceGroup)
				UpdateResources(false);
		}

		private void Revise()
		{
			_searchEnabled = false;

			Name = string.Empty;
			Tag = string.Empty;
			Instance = string.Empty;
			ResourceType = string.Empty;
			ResourceGroup = string.Empty;
			Compressed = string.Empty;

			_searchEnabled = true;

			UpdateResources(false);
		}

		#endregion

		private bool HasStringValueContentField(IResource resource)
		{
			if (!resource.ContentFields.Contains("Value")) return false;

			Type t = AApiVersionedFields.GetContentFieldTypes(resource.GetType())["Value"];
			return typeof(string).IsAssignableFrom(t);
		}

		public async void OpenAdvancedSearch()
		{
			AdvancedSearchResources window = WindowsManager.CreateModalWindow<AdvancedSearchResources>();

			if (window == null) return;

			AdvancedSearchResourcesViewModel viewModel = window.GetDataContext<AdvancedSearchResourcesViewModel>();
			viewModel.Package = CurrentPackage;
			viewModel.Resources = AllResources;
			
			Resource resource = await window.Show<Resource>(WindowsManager.MainWindow);

			if (resource != null)
			{
				S3Pe.DataGrid.SelectedItem = resource;
			}
		}

		public bool SavePackage(string path)
		{
			try
			{
				string oldPath = PathToFile;
				PathToFile = path;

				if (oldPath != PathToFile) CurrentPackage.SaveAs(path);
				else CurrentPackage.SavePackage();

				IsEdited = false;

				return true;
			}
			catch (Exception)
			{
				try
				{
					CurrentPackage.SaveAs(path);

					IsEdited = false;

					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public void Release()
		{
			ClearNameMap();
			AllResources.Clear();
			Resources.Clear();
			Package.ClosePackage(CurrentPackage);
		}
	}
}