csharp-upload-api-client
========================

## C# Client for the LAMA Upload API

In order to interact with the API you must first instantiate a Client object.

```csharp
var client = new Client("user", "password", "https://api.agile.lldns.net");
```

The Client object provides typed access to the API.  For instance, the following will list the first ten files in your root directory:

```csharp
var files = client.ListFile("/", 10, 0, true);
foreach (var file in files) 
{
    Console.WriteLine("Found {0}", file.Name);
}
```

Take note that there is no need to explicitly authenticate.  Internally the Client manages your login tokens.  However, it's still possible to manually authenticate.

```csharp
var login = client.Login();
Console.WriteLine("Found Token:{0} Path:{1}", login.Token, login.Path);
```

Files can be uploaded using the MakeFile method.  The result includes the checksum and size as calculated by the server as well as the full Agile path.

```csharp
var result =client.MakeFile("/home/bill/data.txt", "/my-data.txt");
Console.WriteLine("Found Checksum:{0} Path:{1} Size:{2}", result.Checksum, result.Path, result.Size);
```

### Supported Api Methods

- CopyFile (Deprecated)
- DeleteDir
- DeleteFile
- DeleteObject
- ListDir
- ListFile
- ListPath
- Login
- Logout
- MakeDir
- MakeDir2
- MakeFile
- Rename
- SetMTime
- Stat

### Supported Multipart Api Methods

- AbortMultipart
- CompleteMultipart
- CreateMultipart
- CreateMultipartPiece
- GetMultipartStatus
- ListMultipart
- ListMultipartPiece
- RestartMultipart

### Executing JSON-RPC calls directly

In addition to the typed methods, the ability to execute methods directly is also available.  If a new API function named "recycleFile" were to be added, it could be called using the code below.  In this example, we're expecting the JSON-RPC result to contain nothing but the integer code.  If it were to return a dictionary containing 'code', we would use a DictCodeGetter.

```csharp
object result = client.ExecRawJson("recycleFile", new CodeGetter(), 1, "a");
Console.WriteLine("Found {0}", result);
```

### Retries

Upon finding an expired authentication token, the client object will attempt to retry all JSON-RPC and HTTP POST actions.  By default, actions are retried five times.   Each time, a new authentication token is requested.

### Tracking Progress

The Client object exposes a single event named OnProgress.  As files are uploaded using MakeFile or CreateMultipartPiece events are fired containing the following information:

- LastBytesRead
- TotalBytesRead
- TotalBytes
- Tag (remotePath for MakeFile or <MpId>-<Part> for CreateMultupartPiece


## SmartUpload

In additional to the Client, the library provides a simple way to leverage the multipart upload feature without having to code for it.  The simplest form is as follows:

```csharp
var uploader = new SmartUpload(client);
var mpId = uploader.MakeFile("/home/bill/data.txt", "/my-data.txt", 10000);
```

In the example above, /home/bill/data.txt will be created as a multipart file, then uploaded in 10000 byte pieces.  After the last piece has finished, multipart complete will be executed. 

### Tracking Progress

SmartUpload tracks progress on the file as a whole rather than on the individual parts.

# TODO
- Setup maximum for concurrent background workers
- Can we use listFiles rather than getStatdata for size / existence checks?
- Can we use listDirs rather than getStatData for existence checks?
- Is it feasible to add a local cache to speed up existence check comparisons?
- Or maybe a switch that allows "no check first" that always overwrites
- Add support for files and masks

