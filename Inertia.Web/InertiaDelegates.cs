using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Web;

#region Web Events

/// <summary>
/// Web downloading updating handler
/// </summary>
/// <param name="webFile"></param>
public delegate void DownloadUpdateHandler(WebProgressFile webFile);
/// <summary>
/// Web downloading failing handler
/// </summary>
/// <param name="webFile"></param>
/// <param name="error"></param>
public delegate void DownloadFailedHandler(WebProgressFile webFile, Exception error);

/// <summary>
/// Web uploading updating handler
/// </summary>
/// <param name="file"></param>
public delegate void UploadFileUpdateHandler(WebProgressFile file);
/// <summary>
/// Web uploading failing handler
/// </summary>
/// <param name="file"></param>
/// <param name="exception"></param>
public delegate void UploadFileFailedHandler(WebProgressFile file, Exception exception);

#endregion