﻿using StudentManagement.Commands;
using StudentManagement.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using StudentManagement.Services;
using FileInfo = StudentManagement.Objects.FileInfo;
using StudentManagement.Models;
using System.Diagnostics;
using System.IO.Compression;

namespace StudentManagement.ViewModels
{
    public class FileManagerClassDetailViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private ObservableCollection<FileInfo> _fileData;

        private ListCollectionView _fileDataGroup;
        public ObservableCollection<FileInfo> FileData { get => _fileData; set => _fileData = value; }
        public ObservableCollection<FileInfo> BindingFileData { get; set; }
        public ListCollectionView FileDataGroup { get => _fileDataGroup; set { _fileDataGroup = value; OnPropertyChanged(); } }

        public ICommand AddFile { get; set; }
        public ICommand AddFolder { get; set; }
        public ICommand CreateFolder { get; set; }
        public ICommand DeleteFile { get; set; }
        public ICommand DeleteFolder { get; set; }
        public ICommand SearchFile { get; set; }
        public ICommand RenameFolder { get; set; }
        public ICommand SubmitFolderName { get; set; }
        public ICommand DownloadMultipleFiles { get; set; }

        public Guid? FolderEditingId { get => _folderEditingId; set { _folderEditingId = value; OnPropertyChanged(); } }
        private Guid? _folderEditingId;

        public bool IsShowDialog { get => _isShowDialog; set { _isShowDialog = value; OnPropertyChanged(); } }
        private bool _isShowDialog;
        public string NewFolderName
        {
            get => _newFolderName;
            set
            {
                _newFolderName = value;

                // Validation
                _errorBaseViewModel.ClearErrors();
                if (NewFolderName == string.Empty)
                {
                    _errorBaseViewModel.AddError(nameof(NewFolderName), "Vui lòng nhập tên thư mục!");
                }

                OnPropertyChanged();
            }
        }
        private string _newFolderName;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                SearchFileFunction();
                OnPropertyChanged();
            }
        }
        private string _searchQuery;

        public object SelectedFile { get => _selectedFile; set { _selectedFile = value; OnPropertyChanged(); } }
        private object _selectedFile;

        SubjectClass SubjectClassDetail { get; set; }

        // Current user
        User publisher = LoginServices.CurrentUser;

        public FileManagerClassDetailViewModel(SubjectClass subjectClass)
        {
            SubjectClassDetail = subjectClass;

            _errorBaseViewModel = new ErrorBaseViewModel();
            _errorBaseViewModel.ErrorsChanged += ErrorBaseViewModel_ErrorsChanged;

            FirstLoadDataFromDatabase();
            FileData.CollectionChanged += FileData_CollectionChanged;
            BindingFileData = new ObservableCollection<FileInfo>(FileData);
            BindingFileData.CollectionChanged += BindingFileData_CollectionChanged;
            BindingDataToView();

            AddFile = new RelayCommand<object>((p) => true, (p) => AddFileFunction(p));
            DeleteFile = new RelayCommand<object>((p) => true, (p) => DeleteFileFunction(p));
            AddFolder = new RelayCommand<object>((p) => true, (p) => AddFolderFunction());
            CreateFolder = new RelayCommand<object>((p) => true, (p) => CreateFolderFunction());
            DeleteFolder = new RelayCommand<object>((p) => true, (p) => DeleteFolderFunction(p));
            SearchFile = new RelayCommand<object>((p) => true, (p) => SearchFileFunction());
            RenameFolder = new RelayCommand<object>((p) => true, (p) => RenameFolderFunction(p));
            SubmitFolderName = new RelayCommand<object>((p) => true, (p) => SubmitFolderNameFunction());
            DownloadMultipleFiles = new RelayCommand<object>((p) => true, (p) => DownloadMultipleFilesFunction());
        }

        private void FirstLoadDataFromDatabase()
        {
            try
            {
                FileData = new ObservableCollection<FileInfo>();

                var docs = FileServices.Instance.GetListFilesOfSubjectClass(SubjectClassDetail.Id);
                Parallel.ForEach(docs, doc =>
                {
                    FileData.Add(FileServices.Instance.ConvertDocumentToFileInfo(doc));
                });

                var folders = FileServices.Instance.GetListSingleFoldersOfSubjectClass(SubjectClassDetail.Id);
                Parallel.ForEach(folders, folder =>
                {
                    FileData.Add(FileServices.Instance.ConvertFolderToFileInfo(folder));
                });
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra!",
                                  "Lỗi rồi",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            }  
        }

        private async void SubmitFolderNameFunction()
        {
            try
            {
                if (FolderEditingId != null)
                {
                    var folder = FileData.FirstOrDefault(file => file.FolderId == FolderEditingId);
                    await FileServices.Instance.UpdateFolderAsync(folder);
                    FolderEditingId = null;
                }
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Không thể đổi tên thư mục.",
                                  "Lỗi rồi",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            }
            
        }

        private void RenameFolderFunction(object p)
        {
            if (p is CollectionViewGroup collectionViewGroup)
                FolderEditingId = (Guid?)collectionViewGroup.Name;
        }

        private void BindingFileData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BindingDataToView();
        }

        private void BindingDataToView()
        {
            FileDataGroup = new ListCollectionView(BindingFileData);
            FileDataGroup.GroupDescriptions.Add(new PropertyGroupDescription("FolderId"));
        }

        private void SearchFileFunction()
        {
            if (SearchQuery == null)
            {
                SearchQuery = "";
            }
            var searchResult = FileData.Where
            (
                file => 
                    VietnameseStringNormalizer.Instance.Normalize(file.Name)
                    .Contains(VietnameseStringNormalizer.Instance.Normalize(SearchQuery))
                    ||
                    VietnameseStringNormalizer.Instance.Normalize(file.FolderName)
                    .Contains(VietnameseStringNormalizer.Instance.Normalize(SearchQuery))
            );
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                BindingFileData.Clear();
                foreach (var item in searchResult)
                {
                    BindingFileData.Add(item);
                }
            });
        }

        private async void DeleteFolderFunction(object parameter)
        {
            try
            {
                if (parameter is CollectionViewGroup collectionViewGroup)
                {
                    if (MyMessageBox.Show($"Tất cả {collectionViewGroup.ItemCount} tài liệu sẽ bị xóa." +
                                          $" Bạn có chắc chắn muốn xóa thư mục này không?",
                                          "Xóa thư mục",
                                          MessageBoxButton.OKCancel,
                                          MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        var collection = collectionViewGroup.Items.ToList();
                        FileInfo folderToDeleted = new FileInfo((FileInfo)collection[0]);
                        var listOfTasks = new List<Task<int>>();

                        Parallel.ForEach(collection, item =>
                        {
                            FileInfo fileInfo = item as FileInfo;
                            FileInfo fileToBeDeleted = FileData.FirstOrDefault(file => file.Id == fileInfo.Id && file.FolderId == fileInfo.FolderId);
                            FileData.Remove(fileToBeDeleted);
                            if (fileInfo.Id == null)
                            {
                                return;
                            }
                            listOfTasks.Add(FileServices.Instance.DeleteFileAsync(fileToBeDeleted));
                        });

                        await Task.WhenAll(listOfTasks);
                        await FileServices.Instance.DeleteFolderAsync(folderToDeleted);
                    }
                }
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Không thể xóa thư mục.",
                                  "Xóa thư mục",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            }
        }

        public void DeleteFileFunction(object parameter)
        {
            try
            {
                if (parameter != null)
                {
                    var collection = ((IList)parameter).Cast<FileInfo>().ToList();
                    MessageBoxResult userResponse = collection.Count > 1
                        ? MyMessageBox.Show($"Bạn có chắc chắn muốn xóa {collection.Count} tài liệu này không?",
                                          "Xóa tài liệu",
                                          MessageBoxButton.OKCancel,
                                          MessageBoxImage.Question)
                        : MyMessageBox.Show("Bạn có chắc chắn muốn xóa tài liệu này không?",
                                          "Xóa tài liệu",
                                          MessageBoxButton.OKCancel,
                                          MessageBoxImage.Question);
                    if (userResponse == MessageBoxResult.OK)
                    {
                        List<Task<int>> listOfTasks = new List<Task<int>>();
                        Parallel.ForEach(collection, async item => {
                            if (item.FolderId != null && (FileData.Where(file => file.FolderId == item.FolderId).Count() == 1))
                            {
                                FileData.Add(new FileInfo(null, "", item.PublisherId, item.Publisher, "", item.UploadTime, 0, item.FolderId, item.FolderName, SubjectClassDetail.Id));
                            }
                            var fileToBeDeleted = FileData.FirstOrDefault(file => file.Id == item.Id && file.FolderId == item.FolderId);
                            await FileServices.Instance.DeleteFileAsync(fileToBeDeleted);
                            FileData.Remove(fileToBeDeleted);
                        });
                    }
                }
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Không thể xóa tài liệu.",
                                  "Xóa tài liệu",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            }
        }

        private async void CreateFolderFunction()
        {
            if (!IsValidString(NewFolderName))
            {
                return;
            }
            try
            {
                var existFile = FileData.FirstOrDefault(fileInfo => fileInfo.FolderName == NewFolderName);
                if (existFile != null)
                {
                    MyMessageBox.Show("Thư mục này đã tồn tại trong lớp học",
                                      "Thêm thư mục",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    return;
                }
                IsShowDialog = false;
                FileInfo newFolder = new FileInfo(
                                         id: null,
                                         name: "",
                                         publisherId: publisher.Id,
                                         publisher: publisher.DisplayName,
                                         content: "",
                                         uploadTime: DateTime.Now,
                                         size: 0,
                                         folderId: Guid.NewGuid(),
                                         folderName: NewFolderName,
                                         idSubjectClass: SubjectClassDetail.Id); 
                await FileServices.Instance.SaveFolderOfSubjectClassToDatabaseAsync(newFolder);
                FileData.Add(newFolder);
                NewFolderName = null;
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Không thể thêm thư mục.",
                                  "Thêm thư mục",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            }
        }

        private void FileData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SearchFileFunction();
        }

        private async void AddFileFunction(object folder)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "All files (*.*)|*.*"
                };
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get data here to prevent error when data was removed
                    Guid? folderId = null;
                    string folderName = "";
                    if (folder != null)
                    {
                        folderId = (Guid)((CollectionViewGroup)folder).Name;
                        folderName = (((CollectionViewGroup)folder).Items[0] as FileInfo).FolderName;
                    }

                    int existFileCount = 0;
                    int notValidFileSizeCount = 0;

                    MainWindow.Notify.ShowBalloonTip(3000, "Đang tải lên...", "Vui lòng chờ chút bạn nhé!", ToolTipIcon.Info);

                    var listOfLinks = await UploadFileToCloud(openFileDialog.FileNames, folderId);
                    
                    MainWindow.Notify.ShowBalloonTip(3000, "Tải lên hoàn tất", "Enjoy your files", ToolTipIcon.Info);

                    int index = 0;
                    foreach (string file in openFileDialog.FileNames)
                    {
                        string name = Path.GetFileName(file);
                        if (!string.IsNullOrEmpty(name))
                        {
                            // File size limit: 10MB
                            long fileSize = GetFileSize(file);
                            if (!IsValidFileSize(fileSize))
                            {
                                notValidFileSizeCount++;
                                continue;
                            }

                            // Detect exist file
                            var existFile = FileData.FirstOrDefault(fileInfo => fileInfo.Name == name && fileInfo.FolderId == folderId);
                            if (existFile != null)
                            {
                                existFileCount++;
                                continue;
                            }

                            FileInfo newFile = null;

                            if (folder != null)
                            {
                                // Delete pseudo file info used for display folder only
                                var pseudoFileInfo = FileData.FirstOrDefault(fileInfo => fileInfo.FolderId == folderId && fileInfo.Id == null);
                                if (pseudoFileInfo != null)
                                {
                                    FileData.Remove(pseudoFileInfo);
                                }
                                newFile = new FileInfo(
                                              id: Guid.NewGuid(), 
                                              name: name,
                                              publisherId: publisher.Id,
                                              publisher: publisher.DisplayName,
                                              content: listOfLinks[index],
                                              uploadTime: DateTime.Now,
                                              size: fileSize,
                                              folderId: folderId,
                                              folderName: folderName,
                                              idSubjectClass: SubjectClassDetail.Id);
                            }
                            else
                            {
                                newFile = new FileInfo(
                                              id: Guid.NewGuid(),
                                              name: name,
                                              publisherId: publisher.Id,
                                              publisher: publisher.DisplayName,
                                              content: listOfLinks[index],
                                              uploadTime: DateTime.Now,
                                              size: fileSize,
                                              folderId: null,
                                              folderName: "",
                                              idSubjectClass: SubjectClassDetail.Id);
                            }

                            if (newFile != null)
                            {
                                FileData.Add(newFile);
                                await FileServices.Instance.SaveFileOfSubjectClassToDatabaseAsync(newFile);
                            }

                        }
                    }

                    if (existFileCount > 0)
                    {
                        MainWindow.Notify.ShowBalloonTip(3000, "Thêm tài liệu",
                            $"Có {existFileCount} tài liệu đã tồn tại.", ToolTipIcon.Error);


                        //MyMessageBox.Show($"Có {existFileCount} tài liệu đã tồn tại.",
                        //                  "Thêm tài liệu",
                        //                  MessageBoxButton.OK,
                        //                  MessageBoxImage.Error);
                    }

                    if (notValidFileSizeCount > 0)
                    {
                        MainWindow.Notify.ShowBalloonTip(3000, "Thêm tài liệu",
                            $"Không thể tải lên {notValidFileSizeCount} tài liệu có dung lượng > 10MB.", ToolTipIcon.Error);

                        //MyMessageBox.Show($"Không thể tải lên {notValidFileSizeCount} tài liệu có dung lượng > 10MB.",
                        //                  "Thêm tài liệu",
                        //                  MessageBoxButton.OK,
                        //                  MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception)
            {
                MyMessageBox.Show("Đã có lỗi xảy ra! Không thể thêm tài liệu.",
                                  "Thêm tài liệu",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
            }

        }

        private async Task<string[]> UploadFileToCloud(string[] filePaths, Guid? folderId)
        {
            List<Task<string>> listOfTasks = new List<Task<string>>();

            Parallel.ForEach(filePaths, file =>
            {
                string name = Path.GetFileName(file);
                if (!string.IsNullOrEmpty(name))
                {
                    // File size limit: 10MB
                    long fileSize = GetFileSize(file);
                    if (!IsValidFileSize(fileSize))
                    {
                        return;
                    }

                    // Detect exist file
                    var existFile = FileData.FirstOrDefault(fileInfo => fileInfo.Name == name && fileInfo.FolderId == folderId);
                    if (existFile != null)
                    {
                        return;
                    }

                    listOfTasks.Add(FileUploader.Instance.UploadAsync(file));
                }
            });

            return await Task.WhenAll(listOfTasks);
        }

        private long GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                return new System.IO.FileInfo(filePath).Length;
            }
            return 0;
        }

        private bool IsValidFileSize(long fileSizeInBytes)
        {
            // File limit: 10MB
            long FILE_SIZE_LIMIT = 10485760;
            return fileSizeInBytes <= FILE_SIZE_LIMIT;
        }

        private void AddFolderFunction()
        {
            IsShowDialog = true;
        }

        private async void DownloadMultipleFilesFunction()
        {
            string fileName = $"Documents_{SubjectClassDetail.Code}.zip";
            var dialog = new SaveFileDialog
            {
                Filter = "Zip file (*.zip)|*.zip",
                FileName = fileName
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    MainWindow.Notify.ShowBalloonTip(3000, "Đang nén file...", "Vui lòng chờ chút bạn nhé!", ToolTipIcon.Info);

                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                        {
                            // Folder cover entire files
                            var mainFolderName = $"Documents_{SubjectClassDetail.Code}/";
                            archive.CreateEntry(mainFolderName);

                            MainWindow.Notify.ShowBalloonTip(3000, "Đang tải xuống...", "Vui lòng chờ chút bạn nhé!", ToolTipIcon.Info);

                            foreach (var file in FileData)
                            {
                                // Real file
                                if (file.Id != null)
                                {
                                    var fileData = await FileUploader.Instance.DownloadFileAsByteAsync(file.Content);
                                    ZipArchiveEntry zipEntry;
                                    if (file.FolderId == null)
                                    {
                                        zipEntry = archive.CreateEntry(mainFolderName + file.Name);
                                    }
                                    else
                                    {
                                        zipEntry = archive.CreateEntry(mainFolderName + file.FolderName + "/" + file.Name);
                                    }
                                    using (var originalFileStream = new MemoryStream(fileData))
                                    using (var zipEntryStream = zipEntry.Open())
                                    {
                                        //Copy the attachment stream to the zip entry stream
                                        await originalFileStream.CopyToAsync(zipEntryStream);
                                    }
                                }
                                else
                                {
                                    archive.CreateEntry(mainFolderName + file.FolderName + "/");
                                }
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    MyMessageBox.Show("Server hiện đang bận! Vui lòng thử lại sau!", "Không thể tải tài liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                try
                {
                    Process.Start("explorer.exe", Path.GetDirectoryName(dialog.FileName));
                }
                catch (Exception)
                {
                    MyMessageBox.Show("Đường dẫn không tồn tại!", "Không thể mở tài liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        #region Validation

        private readonly ErrorBaseViewModel _errorBaseViewModel;
        public bool CanCreate => !HasErrors;
        public bool HasErrors => _errorBaseViewModel.HasErrors;

        private bool IsValidString(string propertyName)
        {
            return !string.IsNullOrWhiteSpace(propertyName);
        }

        private void ErrorBaseViewModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            ErrorsChanged?.Invoke(this, e);
            OnPropertyChanged(nameof(CanCreate));
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return _errorBaseViewModel.GetErrors(propertyName);
        }
        #endregion
    }
}
