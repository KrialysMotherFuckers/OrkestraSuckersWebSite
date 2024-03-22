#if EJ2_DNX
using System.Web.Mvc;
using System.IO.Packaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
#endif
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Krialys.Orkestra.WebApi.Services.EJ2FileManager.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Krialys.Orkestra.WebApi.Services.EJ2FileManager.PhysicalFileProvider;

public sealed class PhysicalFileProvider : IPhysicalFileProviderBase
{
    private string _contentRootPath;
    private readonly string[] _allowedExtension = { "*" };
    private AccessDetails _accessDetails;
    private string _rootName = string.Empty;
    private string _hostPath;
    private string _hostName;
    private string _accessMessage = string.Empty;

    public void RootFolder(string name)
    {
        _contentRootPath = name;
        _hostName = new Uri(_contentRootPath).Host;
        if (!string.IsNullOrEmpty(_hostName))
        {
            _hostPath = $"{Path.DirectorySeparatorChar}{_hostName}{Path.DirectorySeparatorChar}{_contentRootPath[(_contentRootPath.ToLower().IndexOf(_hostName, StringComparison.Ordinal) + _hostName.Length + 1)..]}";
        }
    }

    public void SetRules(AccessDetails details)
    {
        _accessDetails = details;
        var root = new DirectoryInfo(_contentRootPath);
        _rootName = root.Name;
    }

    public FileManagerResponse GetFiles(string path, bool showHiddenItems, params FileManagerDirectoryContent[] data)
    {
        var readResponse = new FileManagerResponse();
        try
        {
            path ??= string.Empty;
            string fullPath = (_contentRootPath + path);
            fullPath = fullPath.Replace("../", "");
            var directory = new DirectoryInfo(fullPath);
            string[] extensions = _allowedExtension;
            var cwd = new FileManagerDirectoryContent();

            string rootPath = string.IsNullOrEmpty(_hostPath)
                ? _contentRootPath
                : new DirectoryInfo(_hostPath).FullName;

            string parentPath = string.IsNullOrEmpty(_hostPath)
                ? directory.Parent?.FullName
                : new DirectoryInfo(_hostPath + (path != "/" ? path : "")).Parent?.FullName;

            cwd.Name = string.IsNullOrEmpty(_hostPath)
                ? directory.Name
                : new DirectoryInfo(_hostPath + path).Name;

            cwd.Size = 0;
            cwd.IsFile = false;
            cwd.DateModified = directory.LastWriteTime;
            cwd.DateCreated = directory.CreationTime;
            cwd.HasChild = CheckChild(directory.FullName);
            cwd.Type = directory.Extension;
            cwd.FilterPath = GetRelativePath(rootPath, parentPath + Path.DirectorySeparatorChar);
            cwd.Permission = GetPathPermission(path);
            readResponse.Cwd = cwd;

            if (!HasAccess(directory.FullName) || cwd.Permission is { Read: false })
            {
                readResponse.Files = null;
                _accessMessage = cwd.Permission.Message;
                throw new UnauthorizedAccessException(
                    $"'{cwd.Name}' is not accessible. You need permission to perform the read action.");
            }

            readResponse.Files = ReadDirectories(directory, extensions, showHiddenItems, data);
            readResponse.Files = readResponse.Files.Concat(ReadFiles(directory, extensions, showHiddenItems, data));
            return readResponse;
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Message = e.Message
            };
            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            readResponse.Error = er;
            return readResponse;
        }
    }

    private IEnumerable<FileManagerDirectoryContent> ReadFiles(DirectoryInfo directory, string[] extensions, bool showHiddenItems, params FileManagerDirectoryContent[] data)
    {
        var readFiles = new FileManagerResponse();
        if (!showHiddenItems)
        {
            var files = extensions.SelectMany(directory.GetFiles)
                .Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                .Select(file => new FileManagerDirectoryContent
                {
                    Name = file.Name,
                    IsFile = true,
                    Size = file.Length,
                    DateModified = file.LastWriteTime,
                    DateCreated = file.CreationTime,
                    HasChild = false,
                    Type = file.Extension,
                    FilterPath = GetRelativePath(_contentRootPath, directory.FullName),
                    Permission = GetPermission(directory.FullName, file.Name, true)
                });
            readFiles.Files = files;
        }
        else
        {
            var files = extensions.SelectMany(directory.GetFiles)
                .Select(file => new FileManagerDirectoryContent
                {
                    Name = file.Name,
                    IsFile = true,
                    Size = file.Length,
                    DateModified = file.LastWriteTime,
                    DateCreated = file.CreationTime,
                    HasChild = false,
                    Type = file.Extension,
                    FilterPath = GetRelativePath(_contentRootPath, directory.FullName),
                    Permission = GetPermission(directory.FullName, file.Name, true)
                });
            readFiles.Files = files;
        }

        return readFiles.Files;
    }

    private string GetRelativePath(string rootPath, string fullPath)
    {
        switch (string.IsNullOrEmpty(rootPath))
        {
            case false when !string.IsNullOrEmpty(fullPath):
                {
                    DirectoryInfo rootDirectory;
                    if (!string.IsNullOrEmpty(_hostName))
                    {
                        if (rootPath.Contains(_hostName) || rootPath.ToLower().Contains(_hostName) ||
                            rootPath.ToUpper().Contains(_hostName))
                        {
                            rootPath = rootPath[
                                (rootPath.IndexOf(_hostName, StringComparison.CurrentCultureIgnoreCase) +
                                 _hostName.Length)..];
                        }

                        if (fullPath.Contains(_hostName) || fullPath.ToLower().Contains(_hostName) ||
                            fullPath.ToUpper().Contains(_hostName))
                        {
                            fullPath = fullPath[
                                (fullPath.IndexOf(_hostName, StringComparison.CurrentCultureIgnoreCase) +
                                 _hostName.Length)..];
                        }

                        rootDirectory = new DirectoryInfo(rootPath);
                        fullPath = new DirectoryInfo(fullPath).FullName;
                        rootPath = new DirectoryInfo(rootPath).FullName;
                    }
                    else
                    {
                        rootDirectory = new DirectoryInfo(rootPath);
                    }

                    if (rootDirectory.FullName.Substring(rootDirectory.FullName.Length - 1) ==
                        Path.DirectorySeparatorChar.ToString())
                    {
                        if (fullPath.Contains(rootDirectory.FullName))
                        {
                            return fullPath.Substring(rootPath.Length - 1);
                        }
                    }
                    else if (fullPath.Contains(rootDirectory.FullName + Path.DirectorySeparatorChar))
                    {
                        return Path.DirectorySeparatorChar + fullPath[(rootPath.Length + 1)..];
                    }

                    break;
                }
        }

        return string.Empty;
    }

    private IEnumerable<FileManagerDirectoryContent> ReadDirectories(DirectoryInfo directory, string[] extensions, bool showHiddenItems, params FileManagerDirectoryContent[] data)
    {
        var readDirectory = new FileManagerResponse();
        if (!showHiddenItems)
        {
            var directories = directory.GetDirectories().Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                .Select(subDirectory => new FileManagerDirectoryContent
                {
                    Name = subDirectory.Name,
                    Size = 0,
                    IsFile = false,
                    DateModified = subDirectory.LastWriteTime,
                    DateCreated = subDirectory.CreationTime,
                    HasChild = CheckChild(subDirectory.FullName),
                    Type = subDirectory.Extension,
                    FilterPath = GetRelativePath(_contentRootPath, directory.FullName),
                    Permission = GetPermission(directory.FullName, subDirectory.Name, false)
                });
            readDirectory.Files = directories;
        }
        else
        {
            var directories = directory.GetDirectories().Select(subDirectory => new FileManagerDirectoryContent
            {
                Name = subDirectory.Name,
                Size = 0,
                IsFile = false,
                DateModified = subDirectory.LastWriteTime,
                DateCreated = subDirectory.CreationTime,
                HasChild = CheckChild(subDirectory.FullName),
                Type = subDirectory.Extension,
                FilterPath = GetRelativePath(_contentRootPath, directory.FullName),
                Permission = GetPermission(directory.FullName, subDirectory.Name, false)
            });
            readDirectory.Files = directories;
        }

        return readDirectory.Files;
    }

    public FileManagerResponse Create(string path, string name, params FileManagerDirectoryContent[] data)
    {
        var createResponse = new FileManagerResponse();
        try
        {
            var pathPermission = GetPathPermission(path);
            if (pathPermission != null && (!pathPermission.Read || !pathPermission.WriteContents))
            {
                _accessMessage = pathPermission.Message;
                throw new UnauthorizedAccessException(
                    $"'{GetFileNameFromPath(_rootName + path)}' is not accessible. You need permission to perform the writeContents action.");
            }

            string newDirectoryPath = Path.Combine(_contentRootPath + path, name);
            newDirectoryPath = newDirectoryPath.Replace("../", "");

            bool directoryExist = Directory.Exists(newDirectoryPath);

            if (directoryExist)
            {
                var exist = new DirectoryInfo(newDirectoryPath);
                var er = new ErrorDetails
                {
                    Code = "400",
                    Message = $"A file or folder with the name {exist.Name} already exists."
                };
                createResponse.Error = er;

                return createResponse;
            }

            string physicalPath = GetPath(path);
            Directory.CreateDirectory(newDirectoryPath);
            var directory = new DirectoryInfo(newDirectoryPath);
            var createData = new FileManagerDirectoryContent
            {
                Name = directory.Name,
                IsFile = false,
                Size = 0,
                DateModified = directory.LastWriteTime,
                DateCreated = directory.CreationTime,
                HasChild = CheckChild(directory.FullName),
                Type = directory.Extension,
                Permission = GetPermission(physicalPath, directory.Name, false)
            };
            var newData = new[] { createData };
            createResponse.Files = newData;
            return createResponse;
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Code = null,
                Message = null,
                FileExists = null
            };
            er.Message = e.Message;
            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            createResponse.Error = er;
            return createResponse;
        }
    }

    public FileManagerResponse Details(string path, string[] names, params FileManagerDirectoryContent[] data)
    {
        var getDetailResponse = new FileManagerResponse();
        try
        {
            FileDetails detailFiles;
            if (names.Length == 0 || names.Length == 1)
            {
                path ??= string.Empty;
                string fullPath;
                if (names.Length == 0)
                {
                    fullPath = (_contentRootPath + path.Substring(0, path.Length - 1));
                }
                else if (string.IsNullOrEmpty(names[0]))
                {
                    fullPath = (_contentRootPath + path);
                }
                else
                {
                    fullPath = Path.Combine(_contentRootPath + path, names[0]);
                    fullPath = fullPath.Replace("../", "");
                }

                string physicalPath = GetPath(path);
                var directory = new DirectoryInfo(fullPath);
                var info = new FileInfo(fullPath);
                var fileDetails = new FileDetails();
                var baseDirectory =
                    new DirectoryInfo(string.IsNullOrEmpty(_hostPath) ? _contentRootPath : _hostPath);
                fileDetails.Name = info.Name == "" ? directory.Name : info.Name;
                fileDetails.IsFile = (File.GetAttributes(fullPath) & FileAttributes.Directory) !=
                                     FileAttributes.Directory;
                fileDetails.Size =
                    (File.GetAttributes(fullPath) & FileAttributes.Directory) != FileAttributes.Directory
                        ? ByteConversion(info.Length)
                        : ByteConversion(GetDirectorySize(new DirectoryInfo(fullPath), 0));
                fileDetails.Created = info.CreationTime;
                fileDetails.Location =
                    GetRelativePath(
                            string.IsNullOrEmpty(_hostName)
                                ? baseDirectory.Parent?.FullName
                                : baseDirectory.Parent?.FullName + Path.DirectorySeparatorChar, info.FullName)
                        .Substring(1);
                fileDetails.Modified = info.LastWriteTime;
                fileDetails.Permission = GetPermission(physicalPath, fileDetails.Name, fileDetails.IsFile);
                detailFiles = fileDetails;
            }
            else
            {
                bool isVariousFolders = false;
                string previousPath = "";
                string previousName = "";
                var fileDetails = new FileDetails
                {
                    Size = "0"
                };
                var baseDirectory =
                    new DirectoryInfo(string.IsNullOrEmpty(_hostPath) ? _contentRootPath : _hostPath);
                string parentPath = baseDirectory.Parent?.FullName;
                string baseDirectoryParentPath = string.IsNullOrEmpty(_hostName)
                    ? parentPath
                    : parentPath + Path.DirectorySeparatorChar;
                for (int i = 0; i < names.Length; i++)
                {
                    string fullPath;
                    if (names[i] == null)
                    {
                        fullPath = (_contentRootPath + path);
                    }
                    else
                    {
                        fullPath = Path.Combine(_contentRootPath + path, names[i]);
                        fullPath = fullPath.Replace("../", "");
                    }

                    var info = new FileInfo(fullPath);
                    fileDetails.Name = previousName == ""
                        ? previousName = data[i].Name
                        : previousName = $"{previousName}, {data[i].Name}";
                    fileDetails.Size = (long.Parse(fileDetails.Size) +
                                        (((File.GetAttributes(fullPath) & FileAttributes.Directory) !=
                                          FileAttributes.Directory)
                                            ? info.Length
                                            : GetDirectorySize(new DirectoryInfo(fullPath), 0))).ToString();
                    string relativePath = GetRelativePath(baseDirectoryParentPath, info.Directory?.FullName);
                    previousPath = previousPath == "" ? relativePath : previousPath;
                    if (previousPath == relativePath && !isVariousFolders)
                    {
                        previousPath = relativePath;
                    }
                    else
                    {
                        isVariousFolders = true;
                    }
                }

                fileDetails.Size = ByteConversion(long.Parse(fileDetails.Size));
                fileDetails.MultipleFiles = true;
                detailFiles = fileDetails;
            }

            getDetailResponse.Details = detailFiles;
            return getDetailResponse;
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Message = e.Message
            };
            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            getDetailResponse.Error = er;
            return getDetailResponse;
        }
    }

    public FileManagerResponse Delete(string path, string[] names, params FileManagerDirectoryContent[] data)
    {
        var deleteResponse = new FileManagerResponse();
        var removedFiles = new List<FileManagerDirectoryContent>();
        try
        {
            string physicalPath = GetPath(path);
            string result = string.Empty;
            foreach (var t in names)
            {
                bool isFile = !IsDirectory(physicalPath, t);
                var permission = GetPermission(physicalPath, t, isFile);
                if (permission != null && (!permission.Read || !permission.Write))
                {
                    _accessMessage = permission.Message;
                    throw new UnauthorizedAccessException(
                        $"'{GetFileNameFromPath(_rootName + path + t)}' is not accessible.  you need permission to perform the write action.");
                }
            }

            foreach (var t in names)
            {
                string fullPath = Path.Combine((_contentRootPath + path), t);
                fullPath = fullPath.Replace("../", "");
                if (!string.IsNullOrEmpty(t))
                {
                    var attr = File.GetAttributes(fullPath);
                    var removingFile = GetFileDetails(fullPath);
                    //detect whether its a directory or file
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        result = DeleteDirectory(fullPath);
                    }
                    else
                    {
                        try
                        {
                            File.Delete(fullPath);
                        }
                        catch (Exception e)
                        {
                            if (e.GetType().Name == "UnauthorizedAccessException")
                            {
                                result = fullPath;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    if (result != string.Empty)
                    {
                        break;
                    }

                    removedFiles.Add(removingFile);
                }
                else
                {
                    throw new ArgumentNullException($"name should not be null");
                }
            }

            deleteResponse.Files = removedFiles;
            if (result == string.Empty)
            {
                return deleteResponse;
            }

            string deniedPath = result[_contentRootPath.Length..];
            var er = new ErrorDetails
            {
                Message =
                    $"'{GetFileNameFromPath(deniedPath)}' is not accessible.  you need permission to perform the write action.",
                Code = "401"
            };
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            deleteResponse.Error = er;
            return deleteResponse;

        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Message = e.Message
            };
            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            deleteResponse.Error = er;
            return deleteResponse;
        }
    }

    public FileManagerResponse Rename(string path, string name, string newName, bool replace = false, bool showFileExtension = true, params FileManagerDirectoryContent[] data)
    {
        var renameResponse = new FileManagerResponse();
        try
        {
            string physicalPath = GetPath(path);
            if (!showFileExtension)
            {
                name = name + data[0].Type;
                newName = newName + data[0].Type;
            }

            bool IsFile = !IsDirectory(physicalPath, name);
            var permission = GetPermission(physicalPath, name, IsFile);
            if (permission != null && (!permission.Read || !permission.Write))
            {
                _accessMessage = permission.Message;
                throw new UnauthorizedAccessException();
            }

            string tempPath = (_contentRootPath + path);
            tempPath = tempPath.Replace("../", "");
            string oldPath = Path.Combine(tempPath, name);
            string newPath = Path.Combine(tempPath, newName);
            File.GetAttributes(oldPath);

            var info = new FileInfo(oldPath);
            bool isFile = (File.GetAttributes(oldPath) & FileAttributes.Directory) != FileAttributes.Directory;
            if (isFile)
            {
                if (File.Exists(newPath) && !oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    var exist = new FileInfo(newPath);
                    var er = new ErrorDetails
                    {
                        Code = "400",
                        Message = $"Cannot rename {exist.Name} to {newName}: destination already exists."
                    };
                    renameResponse.Error = er;
                    return renameResponse;
                }

                info.MoveTo(newPath);
            }
            else
            {
                bool directoryExist = Directory.Exists(newPath);
                if (directoryExist && !oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    var exist = new DirectoryInfo(newPath);
                    var er = new ErrorDetails
                    {
                        Code = "400",
                        Message = $"Cannot rename {exist.Name} to {newName}: destination already exists."
                    };
                    renameResponse.Error = er;

                    return renameResponse;
                }

                if (oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    tempPath = Path.Combine($"{tempPath}Syncfusion_TempFolder");
                    Directory.Move(oldPath, tempPath);
                    Directory.Move(tempPath, newPath);
                }
                else
                {
                    Directory.Move(oldPath, newPath);
                }
            }

            var addedData = new[]
            {
                GetFileDetails(newPath)
            };
            renameResponse.Files = addedData;
            return renameResponse;
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Message = (e.GetType().Name == "UnauthorizedAccessException")
                    ? $"'{GetFileNameFromPath(_rootName + path + name)}' is not accessible. You need permission to perform the write action."
                    : e.Message
            };
            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            renameResponse.Error = er;
            return renameResponse;
        }
    }

    public FileManagerResponse Copy(string path, string targetPath, string[] names, string[] renameFiles, FileManagerDirectoryContent targetData, params FileManagerDirectoryContent[] data)
    {
        var copyResponse = new FileManagerResponse();
        try
        {
            string result = string.Empty;
            renameFiles ??= Array.Empty<string>();
            string physicalPath = GetPath(path);

            foreach (var t in names)
            {
                bool isFile = !IsDirectory(physicalPath, t);
                var permission = GetPermission(physicalPath, t, isFile);

                if (permission == null || (permission.Read && permission.Copy))
                {
                    continue;
                }

                _accessMessage = permission.Message;
                throw new UnauthorizedAccessException(
                    $"'{GetFileNameFromPath(_rootName + path + t)}' is not accessible. You need permission to perform the copy action.");
            }

            var pathPermission = GetPathPermission(targetPath);
            if (pathPermission != null && (!pathPermission.Read || !pathPermission.WriteContents))
            {
                _accessMessage = pathPermission.Message;
                throw new UnauthorizedAccessException(
                    $"'{GetFileNameFromPath(_rootName + targetPath)}' is not accessible. You need permission to perform the writeContents action.");
            }

            var existFiles = new List<string>();
            var missingFiles = new List<string>();
            var copiedFiles = new List<FileManagerDirectoryContent>();
            string tempPath = path;
            for (int i = 0; i < names.Length; i++)
            {
                string fullname = names[i];
                int name = names[i].LastIndexOf("/", StringComparison.Ordinal);
                if (name >= 0)
                {
                    path = tempPath + names[i][..(name + 1)];
                    names[i] = names[i][(name + 1)..];
                }
                else
                {
                    path = tempPath;
                }

                string itemPath = Path.Combine(_contentRootPath + path, names[i]);
                itemPath = itemPath.Replace("../", "");
                if (Directory.Exists(itemPath) || File.Exists(itemPath))
                {
                    if ((File.GetAttributes(itemPath) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        string directoryName = names[i];
                        string oldPath = Path.Combine(_contentRootPath + path, directoryName);
                        oldPath = oldPath.Replace("../", "");
                        string newPath = Path.Combine(_contentRootPath + targetPath, directoryName);
                        newPath = newPath.Replace("../", "");
                        bool exist = Directory.Exists(newPath);
                        if (exist)
                        {
                            int index = -1;
                            if (renameFiles.Length > 0)
                            {
                                index = Array.FindIndex(renameFiles, row => row.Contains(directoryName));
                            }

                            if ((newPath == oldPath) || (index != -1))
                            {
                                newPath = DirectoryRename(newPath);
                                result = DirectoryCopy(oldPath, newPath);
                                if (result != string.Empty)
                                {
                                    break;
                                }

                                var detail = GetFileDetails(newPath);
                                detail.PreviousName = names[i];
                                copiedFiles.Add(detail);
                            }
                            else
                            {
                                existFiles.Add(fullname);
                            }
                        }
                        else
                        {
                            result = DirectoryCopy(oldPath, newPath);
                            if (result != string.Empty)
                            {
                                break;
                            }

                            var detail = GetFileDetails(newPath);
                            detail.PreviousName = names[i];
                            copiedFiles.Add(detail);
                        }
                    }
                    else
                    {
                        string fileName = names[i];
                        string oldPath = Path.Combine(_contentRootPath + path, fileName);
                        oldPath = oldPath.Replace("../", "");
                        string newPath = Path.Combine(_contentRootPath + targetPath, fileName);
                        newPath = newPath.Replace("../", "");
                        bool fileExist = File.Exists(newPath);
                        try
                        {
                            if (fileExist)
                            {
                                int index = -1;
                                if (renameFiles.Length > 0)
                                {
                                    index = Array.FindIndex(renameFiles, row => row.Contains(fileName));
                                }

                                if ((newPath == oldPath) || (index != -1))
                                {
                                    newPath = FileRename(newPath, fileName);
                                    File.Copy(oldPath, newPath);
                                    var detail = GetFileDetails(newPath);
                                    detail.PreviousName = names[i];
                                    copiedFiles.Add(detail);
                                }
                                else
                                {
                                    existFiles.Add(fullname);
                                }
                            }
                            else
                            {
                                if (renameFiles.Length > 0)
                                {
                                    File.Delete(newPath);
                                }

                                File.Copy(oldPath, newPath);
                                var detail = GetFileDetails(newPath);
                                detail.PreviousName = names[i];
                                copiedFiles.Add(detail);
                            }
                        }
                        catch (Exception e)
                        {
                            if (e.GetType().Name == "UnauthorizedAccessException")
                            {
                                result = newPath;
                                break;
                            }

                            throw;
                        }
                    }
                }
                else
                {
                    missingFiles.Add(names[i]);
                }
            }

            copyResponse.Files = copiedFiles;
            if (result != string.Empty)
            {
                string deniedPath = result.Substring(_contentRootPath.Length);
                var er = new ErrorDetails
                {
                    Message =
                        $"'{GetFileNameFromPath(deniedPath)}' is not accessible. You need permission to perform the copy action.",
                    Code = "401"
                };
                copyResponse.Error = er;
                return copyResponse;
            }

            if (existFiles.Count > 0)
            {
                var er = new ErrorDetails
                {
                    FileExists = existFiles,
                    Code = "400",
                    Message = "File Already Exists"
                };
                copyResponse.Error = er;
            }

            if (missingFiles.Count == 0)
            {
                return copyResponse;
            }

            string namelist = missingFiles[0];
            for (int k = 1; k < missingFiles.Count; k++)
            {
                namelist = $"{namelist}, {missingFiles[k]}";
            }

            throw new FileNotFoundException($"{namelist} not found in given location.");
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Message = e.Message
            };
            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            er.FileExists = copyResponse.Error?.FileExists;
            copyResponse.Error = er;
            return copyResponse;
        }
    }

    public FileManagerResponse Move(string path, string targetPath, string[] names, string[] renameFiles, FileManagerDirectoryContent targetData, params FileManagerDirectoryContent[] data)
    {
        var moveResponse = new FileManagerResponse();
        try
        {
            string result = string.Empty;
            renameFiles ??= Array.Empty<string>();

            string physicalPath = GetPath(path);
            foreach (var t in names)
            {
                bool isFile = !IsDirectory(physicalPath, t);
                var permission = GetPermission(physicalPath, t, isFile);

                if (permission == null || (permission.Read && permission.Write))
                {
                    continue;
                }

                _accessMessage = permission.Message;
                throw new UnauthorizedAccessException(
                    $"'{GetFileNameFromPath(_rootName + path + t)}' is not accessible. You need permission to perform the write action.");
            }

            var pathPermission = GetPathPermission(targetPath);
            if (pathPermission != null && (!pathPermission.Read || !pathPermission.WriteContents))
            {
                _accessMessage = pathPermission.Message;
                throw new UnauthorizedAccessException(
                    $"'{GetFileNameFromPath(_rootName + targetPath)}' is not accessible. You need permission to perform the writeContents action.");
            }

            var existFiles = new List<string>();
            var missingFiles = new List<string>();
            var movedFiles = new List<FileManagerDirectoryContent>();
            string tempPath = path;
            for (int i = 0; i < names.Length; i++)
            {
                string fullName = names[i];
                int name = names[i].LastIndexOf("/", StringComparison.Ordinal);
                if (name >= 0)
                {
                    path = tempPath + names[i][..(name + 1)];
                    names[i] = names[i][(name + 1)..];
                }
                else
                {
                    path = tempPath;
                }

                string itemPath = Path.Combine(_contentRootPath + path, names[i]);
                itemPath = itemPath.Replace("../", "");
                if (Directory.Exists(itemPath) || File.Exists(itemPath))
                {
                    if ((File.GetAttributes(itemPath) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        string directoryName = names[i];
                        string oldPath = Path.Combine(_contentRootPath + path, directoryName);
                        oldPath = oldPath.Replace("../", "");
                        string newPath = Path.Combine(_contentRootPath + targetPath, directoryName);
                        newPath = newPath.Replace("../", "");
                        bool exist = Directory.Exists(newPath);
                        if (exist)
                        {
                            int index = -1;
                            if (renameFiles.Length > 0)
                            {
                                index = Array.FindIndex(renameFiles, row => row.Contains(directoryName));
                            }

                            if (newPath == oldPath || (index != -1))
                            {
                                newPath = DirectoryRename(newPath);
                                result = DirectoryCopy(oldPath, newPath);
                                if (result != string.Empty)
                                {
                                    break;
                                }

                                bool isExist = Directory.Exists(oldPath);
                                if (isExist)
                                {
                                    result = DeleteDirectory(oldPath);
                                    if (result != string.Empty)
                                    {
                                        break;
                                    }
                                }

                                var detail = GetFileDetails(newPath);
                                detail.PreviousName = names[i];
                                movedFiles.Add(detail);
                            }
                            else
                            {
                                existFiles.Add(fullName);
                            }
                        }
                        else
                        {
                            result = DirectoryCopy(oldPath, newPath);
                            if (result != string.Empty)
                            {
                                break;
                            }

                            bool isExist = Directory.Exists(oldPath);
                            if (isExist)
                            {
                                result = DeleteDirectory(oldPath);
                                if (result != string.Empty)
                                {
                                    break;
                                }
                            }

                            var detail = GetFileDetails(newPath);
                            detail.PreviousName = names[i];
                            movedFiles.Add(detail);
                        }
                    }
                    else
                    {
                        string fileName = names[i];
                        string oldPath = Path.Combine(_contentRootPath + path, fileName);
                        oldPath = oldPath.Replace("../", "");
                        string newPath = Path.Combine(_contentRootPath + targetPath, fileName);
                        newPath = newPath.Replace("../", "");
                        bool fileExist = File.Exists(newPath);
                        try
                        {
                            if (fileExist)
                            {
                                int index = -1;
                                if (renameFiles.Length > 0)
                                {
                                    index = Array.FindIndex(renameFiles, row => row.Contains(fileName));
                                }

                                if (newPath == oldPath || (index != -1))
                                {
                                    newPath = FileRename(newPath, fileName);
                                    File.Copy(oldPath, newPath);
                                    bool isExist = File.Exists(oldPath);
                                    if (isExist)
                                    {
                                        File.Delete(oldPath);
                                    }

                                    var detail = GetFileDetails(newPath);
                                    detail.PreviousName = names[i];
                                    movedFiles.Add(detail);
                                }
                                else
                                {
                                    existFiles.Add(fullName);
                                }
                            }
                            else
                            {
                                File.Copy(oldPath, newPath);
                                bool isExist = File.Exists(oldPath);
                                if (isExist)
                                {
                                    File.Delete(oldPath);
                                }

                                var detail = GetFileDetails(newPath);
                                detail.PreviousName = names[i];
                                movedFiles.Add(detail);
                            }
                        }
                        catch (Exception e)
                        {
                            if (e.GetType().Name != "UnauthorizedAccessException")
                            {
                                throw;
                            }

                            result = newPath;
                            break;

                        }
                    }
                }
                else
                {
                    missingFiles.Add(names[i]);
                }
            }

            moveResponse.Files = movedFiles;
            if (result != string.Empty)
            {
                string deniedPath = result.Substring(_contentRootPath.Length);
                var er = new ErrorDetails
                {
                    Message =
                        $"'{GetFileNameFromPath(deniedPath)}' is not accessible. You need permission to perform this action.",
                    Code = "401"
                };
                moveResponse.Error = er;
                return moveResponse;
            }

            if (existFiles.Count > 0)
            {
                var er = new ErrorDetails
                {
                    FileExists = existFiles,
                    Code = "400",
                    Message = "File Already Exists"
                };
                moveResponse.Error = er;
            }

            if (missingFiles.Count == 0)
            {
                return moveResponse;
            }

            string namelist = missingFiles[0];
            for (int k = 1; k < missingFiles.Count; k++)
            {
                namelist = $"{namelist}, {missingFiles[k]}";
            }

            throw new FileNotFoundException($"{namelist} not found in given location.");
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Message = e.Message,
                Code = e.Message.Contains("is not accessible. You need permission") ? "401" : "417",
                FileExists = moveResponse.Error?.FileExists
            };
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            moveResponse.Error = er;
            return moveResponse;
        }
    }

    public FileManagerResponse Search(string path, string searchString, bool showHiddenItems = false, bool caseSensitive = false, params FileManagerDirectoryContent[] data)
    {
        var searchResponse = new FileManagerResponse();
        try
        {
            path ??= string.Empty;

            string searchPath = (_contentRootPath + path);
            var directory = new DirectoryInfo(_contentRootPath + path);
            var cwd = new FileManagerDirectoryContent
            {
                Name = directory.Name,
                Size = 0,
                IsFile = false,
                DateModified = directory.LastWriteTime,
                DateCreated = directory.CreationTime
            };
            string rootPath = string.IsNullOrEmpty(_hostPath)
                ? _contentRootPath
                : new DirectoryInfo(_hostPath).FullName;
            string parentPath = string.IsNullOrEmpty(_hostPath)
                ? directory.Parent?.FullName
                : new DirectoryInfo(_hostPath + (path != "/" ? path : "")).Parent?.FullName;
            cwd.HasChild = CheckChild(directory.FullName);
            cwd.Type = directory.Extension;
            cwd.FilterPath = GetRelativePath(rootPath, parentPath + Path.DirectorySeparatorChar);
            cwd.Permission = GetPathPermission(path);
            if (cwd.Permission is { Read: false })
            {
                _accessMessage = cwd.Permission.Message;
                throw new UnauthorizedAccessException(
                    $"'{GetFileNameFromPath(_rootName + path)}' is not accessible. You need permission to perform the read action.");
            }

            searchResponse.Cwd = cwd;

            var foundedFiles = new List<FileManagerDirectoryContent>();
            var searchDirectory = new DirectoryInfo(searchPath);
            var files = new List<FileInfo>();
            var directories = new List<DirectoryInfo>();
            if (showHiddenItems)
            {
                var filteredFileList = GetDirectoryFiles(searchDirectory, files).Where(item =>
                    new Regex(WildcardToRegex(searchString),
                        (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name));
                var filteredDirectoryList = GetDirectoryFolders(searchDirectory, directories).Where(item =>
                    new Regex(WildcardToRegex(searchString),
                        (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name));
                foreach (var file in filteredFileList)
                {
                    var fileDetails =
                        GetFileDetails(Path.Combine(_contentRootPath, file.DirectoryName ?? string.Empty,
                            file.Name));
                    bool hasPermission = ParentsHavePermission(fileDetails);
                    if (hasPermission)
                    {
                        foundedFiles.Add(fileDetails);
                    }
                }

                foundedFiles.AddRange(from dir in filteredDirectoryList
                                      select GetFileDetails(Path.Combine(_contentRootPath, dir.FullName))
                    into dirDetails
                                      let hasPermission = ParentsHavePermission(dirDetails)
                                      where hasPermission
                                      select dirDetails);
            }
            else
            {
                var filteredFileList = GetDirectoryFiles(searchDirectory, files).Where(item =>
                    new Regex(WildcardToRegex(searchString),
                        (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name) &&
                    (item.Attributes & FileAttributes.Hidden) == 0);

                var filteredDirectoryList = GetDirectoryFolders(searchDirectory, directories).Where(item =>
                    new Regex(WildcardToRegex(searchString),
                        (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase)).IsMatch(item.Name) &&
                    (item.Attributes & FileAttributes.Hidden) == 0);

                foundedFiles.AddRange(from file in filteredFileList
                                      select GetFileDetails(Path.Combine(_contentRootPath, file.DirectoryName ?? string.Empty,
                                          file.Name))
                    into fileDetails
                                      let hasPermission = ParentsHavePermission(fileDetails)
                                      where hasPermission
                                      select fileDetails);

                foundedFiles.AddRange(from dir in filteredDirectoryList
                                      select GetFileDetails(Path.Combine(_contentRootPath, dir.FullName))
                    into dirDetails
                                      let hasPermission = ParentsHavePermission(dirDetails)
                                      where hasPermission
                                      select dirDetails);
            }

            searchResponse.Files = foundedFiles;
            return searchResponse;
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Code = null,
                Message = null,
                FileExists = null
            };
            er.Message = e.Message;
            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            searchResponse.Error = er;
            return searchResponse;
        }
    }

    private static string ByteConversion(long fileSize)
    {
        string[] index = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (fileSize == 0)
        {
            return $"0 {index[0]}";
        }

        long bytes = Math.Abs(fileSize);
        int loc = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, loc), 1);
        return $"{(Math.Sign(fileSize) * num).ToString(CultureInfo.InvariantCulture)} {index[loc]}";
    }

    private static string WildcardToRegex(string pattern)
    {
        return $"^{Regex.Escape(pattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".")}$";
    }

    public FileStreamResult GetImage(string path, string id, bool allowCompress, ImageSize size, params FileManagerDirectoryContent[] data)
    {
        try
        {
            if (GetFilePermission(path) is { Read: false })
                return null;

            string fullPath = (_contentRootPath + path);
            fullPath = fullPath.Replace("../", "");
#if EJ2_DNX
                if (allowCompress)
                {
                    size = new ImageSize { Height = 14, Width = 16 };
                    CompressImage(fullPath, size);
                }
#endif
            var fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
            return fileStreamResult;
        }
        catch (Exception)
        {
            return null;
        }
    }

#if EJ2_DNX
        protected virtual void CompressImage(string path, ImageSize targetSize)
        {
            using (var image = Image.FromStream(System.IO.File.OpenRead(path)))
            {
                var originalSize = new ImageSize { Height = image.Height, Width = image.Width };
                var size = FindRatio(originalSize, targetSize);
                using (var thumbnail = new Bitmap(size.Width, size.Height))
                {
                    using (var graphics = Graphics.FromImage(thumbnail))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.PixelOffsetMode = PixelOffsetMode.Default;
                        graphics.InterpolationMode = InterpolationMode.Bicubic;
                        graphics.DrawImage(image, 0, 0, thumbnail.Width, thumbnail.Height);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        thumbnail.Save(memoryStream, ImageFormat.Png);
                        HttpResponse response = HttpContext.Current.Response;
                        response.Buffer = true;
                        response.Clear();
                        response.ContentType = "image/png";
                        response.BinaryWrite(memoryStream.ToArray());
                        response.Flush();
                        response.End();
                    }
                }
            }
        }
      
        protected virtual ImageSize FindRatio(ImageSize originalSize, ImageSize targetSize)
        {
            var aspectRatio = (float)originalSize.Width / (float)originalSize.Height;
            var width = targetSize.Width;
            var height = targetSize.Height;

            if (originalSize.Width > targetSize.Width || originalSize.Height > targetSize.Height)
            {
                if (aspectRatio > 1)
                {
                    height = (int)(targetSize.Height / aspectRatio);
                }
                else
                {
                    width = (int)(targetSize.Width * aspectRatio);
                }
            }
            else
            {
                width = originalSize.Width;
                height = originalSize.Height;
            }

            return new ImageSize
            {
                Width = Math.Max(width, 1),
                Height = Math.Max(height, 1)
            };
        }
#endif
#if EJ2_DNX
    public virtual FileManagerResponse Upload(string path, IList<System.Web.HttpPostedFileBase> uploadFiles, string action, params FileManagerDirectoryContent[] data)
#else
    public FileManagerResponse Upload(string path, IList<IFormFile> uploadFiles, string action, params FileManagerDirectoryContent[] data)
#endif
    {
        var uploadResponse = new FileManagerResponse();
        try
        {
            var pathPermission = GetPathPermission(path);
            if (pathPermission != null && (!pathPermission.Read || !pathPermission.Upload))
            {
                _accessMessage = pathPermission.Message;
                throw new UnauthorizedAccessException(
                    $"'{GetFileNameFromPath(_rootName + path)}' is not accessible. You need permission to perform the upload action.");
            }

            var existFiles = new List<string>();
#if EJ2_DNX
            foreach (System.Web.HttpPostedFileBase file in uploadFiles)
#else
            foreach (var file in uploadFiles)
#endif
            {
#if EJ2_DNX
                var name = System.IO.Path.GetFileName(file.FileName);
                var fullName = Path.Combine((this.contentRootPath + path), name);
#else
                var name = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim().ToString();
                string[] folders = name.Split('/');
                string fileName = folders[^1];
                var fullName = Path.Combine((_contentRootPath + path), fileName);
                fullName = fullName.Replace("../", "");
#endif
                switch (action)
                {
                    case "save" when !File.Exists(fullName):
                        {
#if !EJ2_DNX
                            using var fs = File.Create(fullName);
                            file.CopyTo(fs);
                            fs.Flush();
#else
                        file.SaveAs(fullName);
#endif
                            break;
                        }

                    case "save":
                        existFiles.Add(fullName);
                        break;

                    case "remove" when File.Exists(fullName):
                        File.Delete(fullName);
                        break;

                    case "remove":
                        {
                            uploadResponse.Error = new ErrorDetails
                            {
                                Code = "404",
                                Message = "File not found."
                            };
                            break;
                        }

                    case "replace":
                        {
                            if (File.Exists(fullName))
                            {
                                File.Delete(fullName);
                            }
#if !EJ2_DNX
                            using var fs = File.Create(fullName);
                            file.CopyTo(fs);
                            fs.Flush();
#else
                        file.SaveAs(fullName);
#endif
                            break;
                        }

                    case "keepboth":
                        {
                            string newName = fullName;
                            int index = newName.LastIndexOf(".", StringComparison.Ordinal);
                            if (index >= 0)
                                newName = newName.Substring(0, index);
                            int fileCount = 0;
                            while (File.Exists(newName + (fileCount > 0
                                       ? $"({fileCount}){Path.GetExtension(name)}"
                                       : Path.GetExtension(name))))
                            {
                                fileCount++;
                            }

                            newName = $"{newName}{(fileCount > 0 ? $"({fileCount})" : "")}{Path.GetExtension(name)}";
#if !EJ2_DNX
                            using var fs = File.Create(newName);
                            file.CopyTo(fs);
                            fs.Flush();
#else
                        file.SaveAs(newName);
#endif
                            break;
                        }
                }
            }

            if (existFiles.Count == 0)
            {
                return uploadResponse;
            }

            uploadResponse.Error = new ErrorDetails
            {
                Code = "400",
                Message = "File already exists.",
                FileExists = existFiles
            };

            return uploadResponse;
        }
        catch (Exception e)
        {
            var er = new ErrorDetails
            {
                Message = (e.GetType().Name == "UnauthorizedAccessException")
                    ? $"'{GetFileNameFromPath(path)}' is not accessible. You need permission to perform the upload action."
                    : e.Message
            };

            er.Code = er.Message.Contains("is not accessible. You need permission") ? "401" : "417";
            if ((er.Code == "401") && !string.IsNullOrEmpty(_accessMessage))
            {
                er.Message = _accessMessage;
            }

            uploadResponse.Error = er;

            return uploadResponse;
        }
    }
#if SyncfusionFramework4_0
        public virtual void Download(string path, string[] names, params FileManagerDirectoryContent[] data)
        {
            try
            {
                string physicalPath = GetPath(path);
                String extension;
                int count = 0;
                for (var i = 0; i < names.Length; i++)
                {
                    bool IsFile = !IsDirectory(physicalPath, names[i]);
                    AccessPermission FilePermission = GetPermission(physicalPath, names[i], IsFile);
                    if (FilePermission != null && (!FilePermission.Read || !FilePermission.Download))
                     throw new UnauthorizedAccessException("'" + this.getFileNameFromPath(this.rootName + path + names[i]) + "' is not accessible. You need permission to perform the download action.");

                    extension = Path.GetExtension(names[i]);
                    if (extension != "")
                    {
                        count++;
                    }
                }
                if (names.Length > 1)
                    DownloadZip(path, names);

                if (count == names.Length)
                {
                    DownloadFile(path, names);
                }

            }
            catch (Exception)
            {

            }
        }

        private FileStreamResult fileStreamResult;
        protected virtual void DownloadFile(string path, string[] names = null)
        {
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    path = (Path.Combine(contentRootPath + path, names[0]));
                    HttpResponse response = HttpContext.Current.Response;
                    response.Buffer = true;
                    response.Clear();
                    response.ContentType = "APPLICATION/octet-stream";
                    string extension = System.IO.Path.GetExtension(path);
                    response.AddHeader("content-disposition", string.Format("attachment; filename = \"{0}\"", System.IO.Path.GetFileName(path)));
                    response.WriteFile(path);
                    response.Flush();
                    response.End();
                }
                catch (Exception ex) { throw ex; }
            }
            else throw new ArgumentNullException("name should not be null");
        }

        protected virtual void DownloadZip(string path, string[] names)
        {
            HttpResponse response = HttpContext.Current.Response;
            string tempPath = Path.Combine(Path.GetTempPath(), "temp.zip");

            for (int i = 0; i < names.Count(); i++)
            {
                string fullPath = Path.Combine(contentRootPath + path, names[0]);
                if (!string.IsNullOrEmpty(fullPath))
                {
                    try
                    {
                        var physicalPath = Path.Combine(contentRootPath + path, names[0]);
                        AddFileToZip(tempPath, physicalPath);
                    }
                    catch (Exception ex) { throw ex; }
                }
                else throw new ArgumentNullException("name should not be null");
            }
            try
            {
                System.Net.WebClient net = new System.Net.WebClient();
                response.ClearHeaders();
                response.Clear();
                response.Expires = 0;
                response.Buffer = true;
                response.AddHeader("Content-Disposition", "Attachment;FileName=Files.zip");
                response.ContentType = "application/zip";
                response.BinaryWrite(net.DownloadData(tempPath));
                response.End();
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
            catch (Exception ex) { throw ex; }
        }

        protected virtual void AddFileToZip(string zipFileName, string fileToAdd)
        {
            using (Package zip = System.IO.Packaging.Package.Open(zipFileName, FileMode.OpenOrCreate))
            {
                string destFilename = ".\\" + Path.GetFileName(fileToAdd);
                Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                if (zip.PartExists(uri))
                    zip.DeletePart(uri);
                PackagePart pkgPart = zip.CreatePart(uri, System.Net.Mime.MediaTypeNames.Application.Zip, CompressionOption.Normal);
                Byte[] bites = System.IO.File.ReadAllBytes(fileToAdd);
                pkgPart.GetStream().Write(bites, 0, bites.Length);
                zip.Close();
            }
        }
#else
    public FileStreamResult Download(string path, string[] names, params FileManagerDirectoryContent[] data)
    {
        try
        {
            string physicalPath = GetPath(path);
            int count = 0;

            foreach (var t in names)
            {
                bool isFile = !IsDirectory(physicalPath, t);
                var filePermission = GetPermission(physicalPath, t, isFile);
                if (filePermission != null && (!filePermission.Read || !filePermission.Download))
                    throw new UnauthorizedAccessException($"'{_rootName}{path}{t}' is not accessible. Access is denied.");

                string fullPath = Path.Combine(_contentRootPath + path, t);
                fullPath = fullPath.Replace("../", "");
                if ((File.GetAttributes(fullPath) & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    count++;
                }
            }

            return count == names.Length ? DownloadFile(path, names) : DownloadFolder(path, names, count);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private FileStreamResult _fileStreamResult;

    public PhysicalFileProvider()
    {
        _accessDetails = new AccessDetails();
    }

    private FileStreamResult DownloadFile(string path, string[] names = null)
    {
        try
        {
            path = Path.GetDirectoryName(path);
            string tempPath = Path.Combine(Path.GetTempPath(), "temp.zip");
            string fullPath;
            if (names == null || names.Length == 0)
            {
                fullPath = (_contentRootPath + path);
                fullPath = fullPath.Replace("../", "");
                File.ReadAllBytes(fullPath);
                var fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                _fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream");
            }
            else
                switch (names.Length)
                {
                    case 1:
                        {
                            fullPath = Path.Combine($"{_contentRootPath}{path}", names[0]);
                            fullPath = fullPath.Replace("../", "");
                            var fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                            _fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream")
                            {
                                FileDownloadName = names[0]
                            };
                            break;
                        }
                    case > 1:
                        {
                            string fileName = $"{Guid.NewGuid()}temp.zip";
                            string newFileName = fileName.Substring(36);
                            tempPath = Path.Combine(Path.GetTempPath(), newFileName);
                            tempPath = tempPath.Replace("../", "");
                            if (File.Exists(tempPath))
                            {
                                File.Delete(tempPath);
                            }

                            for (int i = 0; i < names.Count(); i++)
                            {
                                fullPath = Path.Combine((_contentRootPath + path), names[i]);
                                fullPath = fullPath.Replace("../", "");
                                if (!string.IsNullOrEmpty(fullPath))
                                {
                                    try
                                    {
                                        ZipArchive archive;
                                        using (archive = ZipFile.Open(tempPath, ZipArchiveMode.Update))
                                        {
                                            string currentDirectory = Path.Combine((_contentRootPath + path), names[i]);
                                            currentDirectory = currentDirectory.Replace("../", "");
#if SyncfusionFramework4_5
                                        zipEntry = archive.CreateEntryFromFile(Path.Combine(this.contentRootPath, currentDirectory), names[i]);
#else
                                            archive.CreateEntryFromFile(Path.Combine(_contentRootPath, currentDirectory), names[i], CompressionLevel.Fastest);
#endif
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    throw new ArgumentNullException($"name should not be null");
                                }
                            }

                            try
                            {
                                var fileStreamInput = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
                                _fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream")
                                {
                                    FileDownloadName = "files.zip"
                                };
                            }
                            catch (Exception)
                            {
                                return null;
                            }

                            break;
                        }
                }

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            return _fileStreamResult;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private FileStreamResult DownloadFolder(string path, string[] names, int count)
    {
        try
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = Path.GetDirectoryName(path);
            }

            FileStreamResult fileStreamResult;
            // create a temp.Zip file intially 
            string tempPath = Path.Combine(Path.GetTempPath(), "temp.zip");
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            if (names.Length == 1)
            {
                string fullPath = Path.Combine($"{_contentRootPath}{path}", names[0]);
                fullPath = fullPath.Replace("../", "");
                var directoryName = new DirectoryInfo(fullPath);
#if SyncfusionFramework4_5
                ZipFile.CreateFromDirectory(fullPath, tempPath);
#else
                ZipFile.CreateFromDirectory(fullPath, tempPath, CompressionLevel.Fastest, true);
#endif
                var fileStreamInput = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
                fileStreamResult = new FileStreamResult(fileStreamInput, "APPLICATION/octet-stream")
                {
                    FileDownloadName = $"{directoryName.Name}.zip"
                };
            }
            else
            {
                ZipArchive archive;
                using (archive = ZipFile.Open(tempPath, ZipArchiveMode.Update))
                    foreach (var t in names)
                    {
                        string currentDirectory = Path.Combine((_contentRootPath + path), t);
                        currentDirectory = currentDirectory.Replace("../", "");
                        if ((File.GetAttributes(currentDirectory) & FileAttributes.Directory) ==
                            FileAttributes.Directory)
                        {
                            string[] files = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories);
                            if (files.Length == 0)
                            {
                                archive.CreateEntry($"{t}/");
                            }
                            else
                            {
                                foreach (string filePath in files)
                                {
#if SyncfusionFramework4_5
                                    zipEntry = archive.CreateEntryFromFile(filePath, names[i] + filePath.Substring(currentDirectory.Length));
#else
                                    archive.CreateEntryFromFile(filePath, $"{t}{filePath[currentDirectory.Length..]}", CompressionLevel.Fastest);
#endif
                                }
                            }

                            foreach (string filePath in Directory.GetDirectories(currentDirectory, "*", SearchOption.AllDirectories))
                            {
                                if (Directory.GetFiles(filePath).Length == 0)
                                {
#if SyncfusionFramework4_5
                                    zipEntry = archive.CreateEntryFromFile(Path.Combine(this.contentRootPath, filePath), filePath.Substring(path.Length));
#else
                                    archive.CreateEntry($"{t}{filePath[currentDirectory.Length..]}/");
#endif
                                }
                            }
                        }
                        else
                        {
#if SyncfusionFramework4_5
                            zipEntry = archive.CreateEntryFromFile(Path.Combine(this.contentRootPath, currentDirectory), names[i]);
#else
                            archive.CreateEntryFromFile(Path.Combine(_contentRootPath, currentDirectory), t,
                                CompressionLevel.Fastest);
#endif
                        }
                    }

                var fileStreamInput = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Delete);
                fileStreamResult = new FileStreamResult(fileStreamInput, "application/force-download")
                {
                    FileDownloadName = "folders.zip"
                };
            }

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            return fileStreamResult;
        }
        catch (Exception)
        {
            return null;
        }
    }
#endif

    private static string DirectoryRename(string newPath)
    {
        int directoryCount = 0;
        while (Directory.Exists($"{newPath}{(directoryCount > 0
            ? $"({directoryCount})"
            : "")}"))
        {
            directoryCount++;
        }

        newPath = $"{newPath}{(directoryCount > 0
            ? $"({directoryCount})"
            : "")}";

        return newPath;
    }

    private static string FileRename(string newPath, string fileName)
    {
        int name = newPath.LastIndexOf(".", StringComparison.Ordinal);
        if (name >= 0)
        {
            newPath = newPath[..name];
        }

        int fileCount = 0;
        while (File.Exists(newPath + (fileCount > 0
                   ? $"({fileCount}){Path.GetExtension(fileName)}"
                   : Path.GetExtension(fileName))))
        {
            fileCount++;
        }

        newPath = $"{newPath}{(fileCount > 0 ? $"({fileCount})" : "")}{Path.GetExtension(fileName)}";

        return newPath;
    }

    private static string DirectoryCopy(string sourceDirName, string destDirName)
    {
        string result = string.Empty;
        try
        {
            // Gets the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, creates it.
            if (!Directory.Exists(destDirName))
            {
                try
                {
                    Directory.CreateDirectory(destDirName ?? string.Empty);
                }
                catch (Exception e)
                {
                    if (e.GetType().Name == "UnauthorizedAccessException")
                    {
                        return destDirName;
                    }
                    throw;
                }
            }

            // Gets the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                try
                {
                    string oldPath = Path.Combine(sourceDirName, file.Name);
                    oldPath = oldPath.Replace("../", "");
                    string temppath = Path.Combine(destDirName ?? string.Empty, file.Name);
                    temppath = temppath.Replace("../", "");
                    File.Copy(oldPath, temppath);
                }
                catch (Exception e)
                {
                    if (e.GetType().Name == "UnauthorizedAccessException")
                    {
                        return file.FullName;
                    }
                    throw;
                }
            }

            foreach (var direc in dirs)
            {
                string oldPath = Path.Combine(sourceDirName, direc.Name);
                oldPath = oldPath.Replace("../", "");
                string temppath = Path.Combine(destDirName ?? string.Empty, direc.Name);
                temppath = temppath.Replace("../", "");
                result = DirectoryCopy(oldPath, temppath);
                if (result != string.Empty)
                {
                    return result;
                }
            }

            return result;
        }
        catch (Exception e)
        {
            if (e.GetType().Name == "UnauthorizedAccessException")
            {
                return sourceDirName;
            }
            throw;
        }
    }

    private static string DeleteDirectory(string path)
    {
        try
        {
            string result = string.Empty;
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string file in files)
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    if (e.GetType().Name == "UnauthorizedAccessException")
                    {
                        return file;
                    }
                    throw;
                }
            }

            foreach (string dir in dirs)
            {
                result = DeleteDirectory(dir);
                if (result != string.Empty)
                {
                    return result;
                }
            }

            Directory.Delete(path, true);
            return result;
        }
        catch (Exception e)
        {
            if (e.GetType().Name == "UnauthorizedAccessException")
            {
                return path;
            }
            throw;
        }
    }

    private FileManagerDirectoryContent GetFileDetails(string path)
    {
        var info = new FileInfo(path);
        var attr = File.GetAttributes(path);
        var detailPath = new FileInfo(info.FullName);
        bool isFile = (attr & FileAttributes.Directory) != FileAttributes.Directory;

        if (!isFile)
        {
            if (detailPath.Directory != null)
            {
                _ = detailPath.Directory.GetDirectories().Length;
            }
        }

        string filterPath = GetRelativePath(_contentRootPath, info.DirectoryName + Path.DirectorySeparatorChar);

        return new FileManagerDirectoryContent
        {
            Name = info.Name,
            Size = isFile ? info.Length : 0,
            IsFile = isFile,
            DateModified = info.LastWriteTime,
            DateCreated = info.CreationTime,
            Type = info.Extension,
            HasChild = !isFile && (CheckChild(info.FullName)),
            FilterPath = filterPath,
            Permission = GetPermission(GetPath(filterPath), info.Name, isFile)
        };
    }

    private AccessPermission GetPermission(string location, string name, bool isFile)
    {
        var filePermission = new AccessPermission();
        if (isFile)
        {
            if (_accessDetails.AccessRules == null) return null;
            string nameExtension = Path.GetExtension(name).ToLower();
            string fileName = Path.GetFileNameWithoutExtension(name);
            string currentPath = GetFilePath($"{location}{name}");

            foreach (var fileRule in _accessDetails.AccessRules)
            {
                switch (string.IsNullOrEmpty(fileRule.Path))
                {
                    case false when fileRule.IsFile && (fileRule.Role == null || fileRule.Role == _accessDetails.Role):
                        {
                            if (fileRule.Path.IndexOf("*.*", StringComparison.Ordinal) > -1)
                            {
                                string parentPath = fileRule.Path[..fileRule.Path.IndexOf("*.*", StringComparison.Ordinal)];
                                if (currentPath.IndexOf(GetPath(parentPath), StringComparison.Ordinal) == 0 || parentPath == "")
                                {
                                    filePermission = UpdateFileRules(filePermission, fileRule);
                                }
                            }
                            else if (fileRule.Path.IndexOf("*.", StringComparison.Ordinal) > -1)
                            {
                                string pathExtension = Path.GetExtension(fileRule.Path)?.ToLower();
                                string parentPath = fileRule.Path?[..fileRule.Path.IndexOf("*.", StringComparison.Ordinal)];
                                if ((GetPath(parentPath) == currentPath || parentPath == "") &&
                                    nameExtension == pathExtension)
                                {
                                    filePermission = UpdateFileRules(filePermission, fileRule);
                                }
                            }
                            else if (fileRule.Path.IndexOf(".*", StringComparison.Ordinal) > -1)
                            {
                                string pathName = Path.GetFileNameWithoutExtension(fileRule.Path);
                                string parentPath = fileRule.Path?[..fileRule.Path.IndexOf($"{pathName}.*", StringComparison.Ordinal)];
                                if ((GetPath(parentPath) == currentPath || parentPath == "") && fileName == pathName)
                                {
                                    filePermission = UpdateFileRules(filePermission, fileRule);
                                }
                            }
                            else if (GetPath(fileRule.Path) == GetValidPath(location + name))
                            {
                                filePermission = UpdateFileRules(filePermission, fileRule);
                            }
                            break;
                        }
                }
            }

            return filePermission;
        }

        if (_accessDetails.AccessRules == null)
        {
            return null;
        }

        foreach (var folderRule in _accessDetails.AccessRules)
        {
            if (folderRule.Path == null || folderRule.IsFile || (folderRule.Role != null && folderRule.Role != _accessDetails.Role))
            {
                continue;
            }

            switch (folderRule.Path.IndexOf("*", StringComparison.Ordinal))
            {
                case > -1:
                    {
                        string parentPath = folderRule.Path[..folderRule.Path.IndexOf("*", StringComparison.Ordinal)];
                        if (GetValidPath($"{location}{name}").IndexOf(GetPath(parentPath), StringComparison.Ordinal) == 0 || parentPath == "")
                        {
                            filePermission = UpdateFolderRules(filePermission, folderRule);
                        }

                        break;
                    }

                default:
                    {
                        if (GetPath(folderRule.Path) == GetValidPath($"{location}{name}") || GetPath(folderRule.Path) == GetValidPath($"{location}{name}{Path.DirectorySeparatorChar}"))
                        {
                            filePermission = UpdateFolderRules(filePermission, folderRule);
                        }
                        else if (GetValidPath(location + name).IndexOf(GetPath(folderRule.Path), StringComparison.Ordinal) == 0)
                        {
                            filePermission.Write = HasPermission(folderRule.WriteContents);
                            filePermission.WriteContents = HasPermission(folderRule.WriteContents);
                        }
                        break;
                    }
            }
        }

        return filePermission;
    }

    private string GetPath(string path)
    {
        var fullPath = (_contentRootPath + path);
        fullPath = fullPath.Replace("../", "");
        var directory = new DirectoryInfo(fullPath);

        return directory.FullName;
    }

    private static string GetValidPath(string path)
    {
        var directory = new DirectoryInfo(path);

        return directory.FullName;
    }

    private static string GetFilePath(string path)
    {
        return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
    }

    private static string[] GetFolderDetails(string path)
    {
        string[] strArray = path.Split('/'), fileDetails = new string[2];
        string parentPath = "";
        for (int i = 0; i < strArray.Length - 2; i++)
        {
            parentPath += $"{strArray[i]}/";
        }

        fileDetails[0] = parentPath;
        fileDetails[1] = strArray[^2];

        return fileDetails;
    }

    private AccessPermission GetPathPermission(string path)
    {
        string[] fileDetails = GetFolderDetails(path);

        return GetPermission(GetPath(fileDetails[0]), fileDetails[1], false);
    }

    private AccessPermission GetFilePermission(string path)
    {
        string parentPath = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal) + 1);
        string fileName = Path.GetFileName(path);
        return GetPermission(GetPath(parentPath), fileName, true);
    }

    private static bool IsDirectory(string path, string fileName)
    {
        string fullPath = Path.Combine(path, fileName);
        fullPath = fullPath.Replace("../", "");
        return ((File.GetAttributes(fullPath) & FileAttributes.Directory) == FileAttributes.Directory);
    }

    private bool HasPermission(Permission rule)
    {
        return rule == Permission.Allow;
    }

    private AccessPermission UpdateFileRules(AccessPermission filePermission, AccessRule fileRule)
    {
        filePermission.Copy = HasPermission(fileRule.Copy);
        filePermission.Download = HasPermission(fileRule.Download);
        filePermission.Write = HasPermission(fileRule.Write);
        filePermission.Read = HasPermission(fileRule.Read);
        filePermission.Message = string.IsNullOrEmpty(fileRule.Message) ? string.Empty : fileRule.Message;
        return filePermission;
    }

    private AccessPermission UpdateFolderRules(AccessPermission folderPermission, AccessRule folderRule)
    {
        folderPermission.Copy = HasPermission(folderRule.Copy);
        folderPermission.Download = HasPermission(folderRule.Download);
        folderPermission.Write = HasPermission(folderRule.Write);
        folderPermission.WriteContents = HasPermission(folderRule.WriteContents);
        folderPermission.Read = HasPermission(folderRule.Read);
        folderPermission.Upload = HasPermission(folderRule.Upload);
        folderPermission.Message = string.IsNullOrEmpty(folderRule.Message) ? string.Empty : folderRule.Message;
        return folderPermission;
    }

    private bool ParentsHavePermission(FileManagerDirectoryContent fileDetails)
    {
        string parentPath = fileDetails.FilterPath.Replace(Path.DirectorySeparatorChar, '/');
        string[] parents = parentPath.Split('/');
        string currPath = "/";
        bool hasPermission = true;
        for (int i = 0; i <= parents.Length - 2; i++)
        {
            currPath = (parents[i] == "") ? currPath : $"{currPath}{parents[i]}/";
            var pathPermission = GetPathPermission(currPath);
            if (pathPermission == null)
            {
                break;
            }

            if (pathPermission.Read)
            {
                continue;
            }
            hasPermission = false;
            break;
        }

        return hasPermission;
    }

    public static string ToCamelCase(FileManagerResponse userData)
    {
        return JsonConvert.SerializeObject(userData, new JsonSerializerSettings
        {
#if EJ2_DNX
            ContractResolver = new CamelCasePropertyNamesContractResolver()
#else
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
#endif
        });
    }

    FileStreamResult IFileProviderBase.Download(string path, string[] names, params FileManagerDirectoryContent[] data)
    {
        throw new NotImplementedException();
    }

    private static bool CheckChild(string path)
    {
        bool hasChild;
        try
        {
            var directory = new DirectoryInfo(path);
            var dir = directory.GetDirectories();
            hasChild = dir.Length != 0;
        }
        catch (Exception e)
        {
            if (e.GetType().Name == "UnauthorizedAccessException")
            {
                hasChild = false;
            }
            else
            {
                throw;
            }
        }

        return hasChild;
    }

    private static bool HasAccess(string path)
    {
        bool hasAcceess;
        try
        {
            var directory = new DirectoryInfo(path);
            _ = directory.GetDirectories();
            hasAcceess = true;
        }
        catch (Exception e)
        {
            if (e.GetType().Name == "UnauthorizedAccessException")
            {
                hasAcceess = false;
            }
            else
            {
                throw;
            }
        }

        return hasAcceess;
    }

    private static long GetDirectorySize(DirectoryInfo dir, long size)
    {
        try
        {
            size = dir.GetDirectories().Aggregate(size, (current, subdir)
                => GetDirectorySize(subdir, current)) + dir.GetFiles().Sum(file => file.Length);
        }
        catch (Exception e)
        {
            if (e.GetType().Name != "UnauthorizedAccessException")
            {
                throw;
            }
        }

        return size;
    }

    private static List<FileInfo> GetDirectoryFiles(DirectoryInfo dir, List<FileInfo> files)
    {
        try
        {
            files = dir.GetDirectories().Aggregate(files, (current, subdir) => GetDirectoryFiles(subdir, current));
            files.AddRange(dir.GetFiles());
        }
        catch (Exception e)
        {
            if (e.GetType().Name != "UnauthorizedAccessException")
            {
                throw;
            }
        }

        return files;
    }

    private static List<DirectoryInfo> GetDirectoryFolders(DirectoryInfo dir, List<DirectoryInfo> files)
    {
        try
        {
            files = dir.GetDirectories().Aggregate(files, (current, subdir) => GetDirectoryFolders(subdir, current));
            files.AddRange(dir.GetDirectories());
        }
        catch (Exception e)
        {
            if (e.GetType().Name != "UnauthorizedAccessException")
            {
                throw;
            }
        }

        return files;
    }

    private static string GetFileNameFromPath(string path)
    {
        int index = path.LastIndexOf("/", StringComparison.Ordinal);
        return path[(index + 1)..];
    }
}