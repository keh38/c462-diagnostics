# HTS TCP Message Contract

## 1. HTSController → HTS: Fire-and-Forget (SendMessage)

| Command | Payload | Notes |
|---------|---------|-------|
| `Disconnect` | _(none)_ | Sent on app shutdown |
| `ChangeScene` | `string` scene name | e.g. "SpeechReception", "Pupil Dynamic Range", "Gaze Calibration", "Admin Tools" |
| `Begin` | _(none)_ | Start the active measurement |
| `Abort` | _(none)_ | Stop/abort the active measurement |
| `SetParams` | `string` XML | Serialized settings object (type depends on scene) |
| `SetScriptArguments` | `string` extra args | Turandot only |
| `Initialize` | `string` XML | Serialized measurement config (type depends on scene) |
| `StartSynchronizing` | `string` data file path | Begin clock sync for recording |
| `StopSynchronizing` | _(none)_ | End clock sync |
| `SendSyncLog` | _(none)_ | Asks HTS to push sync log via RemoteMessageHandler |
| `SendData` | _(none)_ | Asks HTS to push data file via RemoteMessageHandler |
| `SendResourceList` | _(none)_ | Asks HTS to push file list via RemoteMessageHandler |
| `StartResourceSync` | _(none)_ | Prepares HTS for file sync session |
| `DeleteFile` | `string` relative file path | Resource sync: remove a file on HTS |
| `RunInstaller` | `string` filename | App update: run installer from Downloads folder |
| `TransferFile` | `TransferFilePayload` | Send a file to HTS (config files, etc.) |
| `SetSubjectInfo` | `string` "Project/Subject" | Set active project and subject |
| `SetSubjectMetadata` | `string` XML | Serialized SubjectMetadata |
| `SetSubjectMetrics` | `string` XML | Serialized metrics dictionary |
| `CreateProject` | `string` project name | Create a new project on HTS |
| `SetDataRoot` | `string` folder path | Local-connection only: set data root |
| `SetProject` | `string` project name | Local-connection only: set active project |
| `Location` | `string` "x,y" | Gaze calibration: send target point |
| `SetActive` | `string` "channel=0\|1" | Interactive: toggle a channel |
| `SetProperty` | `string` "channel.property=value" | Interactive: set a parameter |
| `ShowSliders` | `string` "True\|False" | Interactive: show/hide slider overlay |

---

## 2. HTSController → HTS: Request/Response (SendRequest\<T\>)

| Command | Request Payload | Response Type | Notes |
|---------|----------------|---------------|-------|
| `Connect` | `ConnectionRequestPayload` { Address, Port } | `ConnectionResponsePayload` { HostName, VersionNumber, SceneName } | Sent on discovery; establishes connection |
| `GetLEDColors` | _(none)_ | `string` "r,g,b,w" | Returns "none" if no LED hardware |
| `GetSubjectInfo` | _(none)_ | `string` "Project/Subject" | Retrieve current subject state from HTS |
| `GetProjectList` | _(none)_ | `List<string>` JSON | All projects on HTS |
| `GetSubjectList` | `string` project name | `List<string>` JSON | Subjects in project |
| `GetSubjectMetadata` | _(none)_ | `SubjectMetadata` XML | Transducer + metrics for current subject |
| `GetTransducers` | _(none)_ | `List<string>` XML | Available transducer names |
| `GetAdapterMap` | _(none)_ | `AdapterMap` XML | Audio adapter channel mapping (Interactive) |
| `GetScreenSize` | _(none)_ | `string` "width,height" | Tablet screen resolution (Gaze Calibration) |
| `GetSyncLog` | _(none)_ | `TextFilePayload` { Filename, Content } | Returns sync log; Filename empty if none |
| `GetLog` | _(none)_ | `TextFilePayload` { Filename, Content } | Returns HTS application log |
| `FileExists` | `string` relative path | `string` "404" or DateTime JSON | Resource sync: check file presence/timestamp |

---

## 3. HTS → HTSController: Push Messages (RemoteMessageHandler)

These are the **Phase 2** string-based push messages HTS sends back unprompted.
Format: `"Target:Command:Info:Data"` (split on `:` up to 4 parts).

### Scene: Turandot (target = `"Turandot"`)
| Command | Info | Data | Notes |
|---------|------|------|-------|
| `File` | data file path | _(empty)_ | Sent after Initialize; unblocks InitializeTurandot() |
| `Trial` | trial description | _(empty)_ | Update log display |
| `Progress` | int 0–100 | _(empty)_ | Update progress bar |
| `State` | state string | _(empty)_ | Update status display |
| `ReceiveData` | filename | file content | Save file to SubjectDataFolder |
| `Error` | error message | _(empty)_ | Triggers EndRun("Error") |
| `Finished` | status message | _(empty)_ | Triggers EndRun("Finished") |

### Scene: Speech Reception (target = `"SpeechReception"`)
| Command | Info | Data | Notes |
|---------|------|------|-------|
| `File` | data file path | _(empty)_ | Unblocks InitializeRemoteMeasurement() |
| `Progress` | int 0–100 | _(empty)_ | Update progress bar |
| `ReceiveData` | filename | file content | Save file to SubjectDataFolder |
| `Status` | status message | _(empty)_ | Append to log |
| `Error` | error message | _(empty)_ | Triggers EndRun("Error") |
| `Finished` | status message | _(empty)_ | "Measurement aborted" sets _runAborted |

### Scene: Pupil Dynamic Range / Gaze Calibration (target = `"Pupil Dynamic Range"` or `"Gaze Calibration"`)
| Command | Info | Data | Notes |
|---------|------|------|-------|
| `File` | data file path | _(empty)_ | Unblocks InitializeDynamicRangeMeasurement() |
| `Progress` | int 0–100 | _(empty)_ | Update progress bar |
| `ReceiveData` | filename | file content | Save file; sets _dataReceived = true |
| `Response` | _(empty)_ | _(empty)_ | Gaze cal: EyeLink acknowledge keypress |
| `Error` | error message | _(empty)_ | Triggers EndRun("Error") |
| `Finished` | status message | _(empty)_ | "Measurement aborted" sets _runAborted |
| `GazeCalibrationFinished` | _(empty)_ | _(empty)_ | Sets _stopCal = true |

### File Sync (target = `"FileSync"`)
| Command | Info | Data | Notes |
|---------|------|------|-------|
| `ReceiveFileList` | _(empty)_ | JSON List\<string\> | Response to SendResourceList |

### Interactive (target = `"TurandotInteractive"`)
| Command | Info | Data | Notes |
|---------|------|------|-------|
| `Error` | error message | _(empty)_ | Display in error tab |

### Scene change (implicit — target = HTSNetwork)
The `CurrentScene` property on HTSNetwork is updated via RemoteMessageHandler.
Controller forms poll `_network.CurrentScene` after sending `ChangeScene` to confirm the transition.

---

## 4. HTSController → DataStream (per-stream TCP)

These go directly to each data stream's `IPEndPoint`, not through HTSNetwork.

| Command | Notes |
|---------|-------|
| `Record` with `RecordPayload { Path }` | Start recording to file |
| `Stop` | Stop recording |
| `Abort` | Abort (sent to EYELINK stream) |
| `Free Run` | Restart EYELINK in free-run mode after gaze calibration |
| `GetStatus` | Poll stream status (returns `DataStreamStatusPayload { Status }`) |
| `ClockSync` with `ClockSyncPayload { T1, T2 }` | Synchronize clocks |

---

## DTOs (namespace HTS.Tcp)

```csharp
ConnectionRequestPayload  { string Address, int Port }
ConnectionResponsePayload { string HostName, string VersionNumber, string SceneName }
ChangeScenePayload        { string SceneName }           // currently unused — scene name sent as raw string
RecordPayload             { string Path }
DataStreamStatusPayload   { int Status }
ClockSyncPayload          { long T1, long T2 }
TextFilePayload           { string Filename, string Content }
ErrorPayload              { string Message }             // available, not widely used yet
FilenamePayload           { string Filename }            // available, not widely used yet
TransferFilePayload       { string Folder, string Filename, string Content }
```

---

## Notes / Open Items

- **Phase 2:** RemoteMessageHandler push messages are still string-based (`"Target:Command:Info:Data"`). These could be migrated to TcpMessage with typed payloads — e.g. `File`, `Progress`, `Finished` could each have their own DTO.
- **`GetSubjectMetadata` / `GetTransducers`** currently return XML strings deserialized by the caller — these could be standardized to JSON DTOs.
- **`GetSubjectList`** sends a payload (project name as raw string) but is called via `SendRequest<T>` — confirm the KLib.Net API handles a request payload here.
- **`ChangeScenePayload`** DTO exists but scene name is currently passed as a raw string — these could be unified.
- **Discovery** (DiscoveryBeacon on HTS side): HTS broadcasts `ServerBeacon { name, addr, port, ver }` on UDP port 10001. HTSController listens via `DiscoveryListener` and initiates `Connect` on discovery.
