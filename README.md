# SharpF5

A .NET Ethernet Communication Library for [KEB F5 Combiverts](https://www.keb.de/en/products/frequency-inverters/combivert-f5.html). Library allows read and write registers of the F5 Combivert with help of the DIN66019-II protocol.

- Tcp and BootP protocols.
- Read/write parameters.
- Sets of parameters.
- Display standarts.
- Operator panels.
- Searching combiverts over BootP protocol (in draft state).


# Examples

## Connect

```c#
NetworkStream stream = TcpHelper.Connect("192.168.5.12");
F5 combivert = new F5(stream);
combivert.SelectInverter(0);
```


## Read/Write parameter

```c#
int sy50_value = combivert.ReadValue("sy50");
int op10_value = combivert.ReadValue("op10", Paramater.SET_1);
combivert.WriteValue(1, "di01");
```

Frequent use of the parameter with help of "Parameter" class and use of inner integer field "Value".

```c#
Parameter op10 = new Parameter( "op10", Parameter.SET_2);

combivert.ReadValue(op10);
op10.Value *= 2;
combivert.WriteValue(op10);

Thread.Sleep(1000);

combivert.WriteValue(0, op10);
```


## Display standart

```c#
Parameter ru25 = new Parameter( "ru25" );
DisplayStandart std = combivert.GetDisplayStandart(ru25);

int value = combivert.ReadValue( ru25 );
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

2.5
- Refactoring.

2.4
- Only four of the first sets are available at this moment: 0, 1, 2, 3.

2.2-2.3
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