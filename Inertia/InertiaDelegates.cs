using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Storage;
using Inertia.Web;
using Inertia.Network;

#region General Events

public delegate void InertiaAction();
public delegate void InertiaAction<T>(T value);
public delegate void LoggerHandler(string log);

#endregion

#region Web Events

public delegate void DownloadStartedHandler(WebFileData webFile);
public delegate void DownloadProgressHandler(WebFileData webFile);
public delegate void DownloadCompletedHandler(WebFileData webFile);
public delegate void DownloadFailedHandler(WebFileData webFile, Exception error);

public delegate void UploadFileStartedHandler(WebUploadFileData file);
public delegate void UploadFileProgressHandler(WebUploadFileData file);
public delegate void UploadFileCompletedHandler(WebUploadFileData file);
public delegate void UploadFileFailedHandler(WebUploadFileData file, Exception exception);

#endregion

#region Storage Events

public delegate void DataStorageCompletedHandler<T>(DataStorage<T> storage);
public delegate void DataStorageProgressHandler<T>(DataStorage<T> storage, StorageAsyncProgression progression);
public delegate void DataStorageProgressFailedHandler<T>(DataStorage<T> storage, Exception e);

public delegate void FileStorageCompletedHandler(FileStorage storage);
public delegate void FileStorageProgressHandler(FileStorage storage, StorageAsyncProgression progression);
public delegate void FileStorageProgressFailedHandler(FileStorage storage, Exception e);
public delegate void FileStorageAddFileCompletedHandler(FileMemoryData file);
public delegate void FileStorageAddFileFailedHandler(FileMemoryData file, Exception e);
public delegate void FileStorageAddDirectoryCompletedHandler(FileStorage storage, FileMemoryData[] files);
public delegate void FileStorageAddDirectoryProgressHandler(FileStorage storage, FileMemoryData file, StorageAsyncProgression progression);
public delegate void FileStorageExtractFailedHandler(FileStorage storage, FileMemoryData file, Exception e);
public delegate void FileStorageExtractProgressHandler(FileStorage storage, FileMemoryData file, StorageAsyncProgression progression);
public delegate void FileStorageExtractFileCompletedHandler(FileStorage storage, FileMemoryData file);

#endregion

#region Network Events

public delegate void ServerStartHandler(Server server);
public delegate void ServerStopHandler(Server server, NetworkDisconnectReason reason);

public delegate void TcpServerAddClientHandler(TcpNetworkUser user);
public delegate void TcpServerRemoveClientHandler(TcpNetworkUser user, NetworkDisconnectReason reason);
public delegate void UdpServerAddClientHandler(UdpNetworkUser user);
public delegate void UdpServerRemoveClientHandler(UdpNetworkUser user, NetworkDisconnectReason reason);

public delegate void TcpClientConnectedHandler(TcpNetworkClient client);
public delegate void TcpClientDisconnectedHandler(TcpNetworkClient client, NetworkDisconnectReason reason);
public delegate void UdpClientConnectedHandler(UdpNetworkClient client);
public delegate void UdpClientDisconnectedHandler(UdpNetworkClient client, NetworkDisconnectReason reason);

public delegate void NetworkMessageSenderHandler(byte[] data);

#endregion