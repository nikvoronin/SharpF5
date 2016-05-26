# SharpF5

A .NET Ethernet Communication Library for KEB F5 Combiverts. Library allows read and write registers of the F5 Combivert with help of the DIN66019-II protocol.

- Tcp and BootP protocols.
- Read/write parameters.
- Parameter sets.
- Display standarts.
- Operator panels.
- Searching combiverts (over BootP protocol).


# Examples

## Connect

```c#
NetworkStream stream = TcpHelper.Connect("192.168.5.12");
F5 combivert = new F5(stream);
combivert.SelectInverter(0);
```


## Read/Write parameter

```c#
ParameterAddress op10 = new ParameterAddress( "op10", 2));

int value = combivert.GetParameter(op10);
value *= 2;
combivert.SetParameter(op10, value);
```


## Display standart

```c#
ParameterAddress ru25 = new ParameterAddress( "ru25", 0 ));
DisplayStandart std = combivert.GetDisplayStandart(ru25);

int value = combivert.GetParameter( ru25 );
string humanReadableValue = std.ToDisplayValue(value);

Console.WriteLine( humanReadableValue );
```


## Searching combiverts

```c#
IPAddress localIp = BootpHelper.GetLocalIp();

List<BootpHelper.BootpResponse> combiverts =
	BootpHelper.BroadcastSearch(localIp, 1000);

Console.WriteLine(
	"Found " + combiverts.Count.ToString() + " combivert(s)" );

foreach (BootpHelper.BootpResponse f5 in combiverts)
	Console.WriteLine( "\t" + f5.IpAddress.ToString() );
```


# History

2.4
- Only four of the first sets are available at this moment: 0, 1, 2, 3.

2.2-3
- Refactoring.

2.1
- Fixed bug with parameters of the operator panel.
- Searching over BootP protocol.

2.0
- Refactoring.
- Removed asynchronous operations.
- Errors throw exceptions.

1.2
- Parameter sets.

1.1
- Display standarts.

1.0
- Read/write parameters.