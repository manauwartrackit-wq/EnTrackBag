# Verified schema notes from MFT Script Latest.txt

The supplied 14/08/2026 MFT script is the source of truth for the existing `BLTSMFT` operational schema. It is reference input only: the application must not run it to recreate or migrate that database.

Verified operational objects:

- Tables: `AlarmList`, `LogicalDevice`, `LogicalDeviceMap`, `Readers`, `SuspectBags`, `Antennas`, `Controllers`, `LogicalDeviceType`, `ReaderControllerMap`, `ServerList`, and `SystemSettings`.
- Views: `vwDeviceLog`, `vwTags`, and `vwtemp`.
- Integer keys include `AlarmList.AlarmID`, `SuspectBags.ID`, `Readers.ID`, `Antennas.ID`, `Controllers.ID`, `LogicalDevice.ID`, `LogicalDeviceMap.ID`, `ReaderControllerMap.TID`, `ServerList.ID`, and `SystemSettings.ID`.
- `AlarmList.CancelledBy`, `SuspectBags.LastSeenAt`, and `LogicalDevice.DeviceType` are nullable integers.
- `SuspectBags.TagID` is required (`varchar(50)`).

The EF model maps the two reusable operational views (`vwDeviceLog` and `vwTags`) as keyless read-only entities. `vwtemp` is not mapped because its definition contains fixed November 2018 dates and is not an application data source.

Do not infer physical antenna count from reader ports.
