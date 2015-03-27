# serilog-sinks-xsockets

[![Build status](https://ci.appveyor.com/api/projects/status/d3cgfr4fm94w74b6/branch/master?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-xsockets/branch/master)

A Serilog sink that writes events to [XSockets.NET](http://xsockets.net).

You can get logs to anything that can connect to the XSockets server. So you will be able to get data to anything that has TCP/IP.

In the samples below we show how C#, JavaScript, Putty (raw sockets) and NodeJS can be used.

### Configuration

By default XSockets will log to ColoredConsole, but by creating a custom module like shown below we can use the XSockets sink for Serilog.

    /// <summary>
    /// Sample configuration.
    /// This will write both to the colored console as well as the XSockets sink
    /// </summary>
    public class MyLogger : XLogger
    {
        public MyLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.XSockets().MinimumLevel.Verbose().CreateLogger();           
        }
    }
    
### CSharp Client

#### PreReq

Install package XSockets.Client from nuget

    PM> Install-Package XSockets.Client
    
#### Setup the client

    //Start a client and hook up only to the log controller...
    var client = new XSocketClient("ws://localhost:4502","http://localhost","log");

    client.Open();
                
    //Listen for log events, by default the event will be Information and higher.
    client.Controller("log").On<LogEventWrapper>("logEvent", logEvent =>
    {                    
        Console.WriteLine("Log: {0}",logEvent.Level);
        Console.WriteLine("Log: {0}", logEvent.RenderedMessage);
    });

#### Change the LogEventLevel

By changing the level on the connection each connected client can decide what level to get from Serilog. Changing to versbose will send all messages to this client since verbose is the lowest level
    
    client.Controller("log").SetEnum("LogEventLevel", LogEventLevel.Verbose.ToString());
    
    
### JavaScript Client

#### PreReq

Install package XSockets.JsApi from nuget

    PM> Install-Package XSockets.JsApi
    
#### Setup the client

    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head>
        <title></title>
        <script src="Scripts/XSockets.latest.js"></script>
    </head>
    <body>
        <script>
            //Start a client and hook up only to the log controller...
            var conn = new XSockets.WebSocket("ws://localhost:4502",["log"]);        
                
            //Listen for log events, by default the event will be Information and higher.
            conn.controller("log").on("logEvent", function(e) {
                console.log("Log: ", e.Level);
                console.log("Log: ", e.RenderedMessage);
            });                             
        </script>
    </body>
    </html>

#### Change the LogEventLevel

By changing the level on the connection each connected client can decide what level to get from Serilog. Changing to versbose will send all messages to this client since verbose is the lowest level
    
    conn.controller("log").setEnum("LogEventLevel", "Verbose");
    
### Putty (raw sockets) Client
XSockets have support for cross protocol communication. As an example of this we have added a very simple protocol for Putty. The first two examples use the WebSocket protocol, but here we want to use Raw sockets... The very basic protocol looks like this

    controller|topic|data

This will ofcourse have some limitations, like sending complex data structures for example. But this is just a example and will work fine with the Serilog sink for XSockets.

#### PreReq

Install/Download Putty
    
#### Setup the client

 - Open putty.
 - Enter the adress (127.0.0.1) and port (4502) of the XSockets server.
 - Choose connection type "Raw"
 - Click Open
 - Enter "PuttyProtocol" and hit enter (case sensetive)
 - You will now see a welcome message

#### Change the LogEventLevel

Since we are only connected to the server (and not the any controller) with putty, we need to connect to the "log" controller and also set a level to start getting information/logs.

Type 
    
    log|set_LogEventLevel|Error
    
The first tome you send the command over Putty you will get a "open message" back. You can now change the LogEventLevel by using the same statement as above but using for example Verbose insteaad of Error.

Like...

    log|set_LogEventLevel|Verbose

### NodeJS

#### PreReq

Just make sure you have node installed on your machine. Then paste the code below into a file and name it serilog.js.

Then copy the file into the nodejs installation folder.

Open the command prompt, navigate to the folder and run:

    //In this example the installation was made to c:\nodejs
    c:\nodejs>node serilog.js
    
#### Setup the client

The contect below should be in the serilog.js file.

    //
    // Basic example of how to get Serilog.Sinks.XSockets data to NodeJS
    //

    var net = require('net');
    var HOST = '127.0.0.1';
    var PORT = 4502;
    var client = new net.Socket();

    client.connect(PORT, HOST, function () {
        console.log('CONNECTED TO: ' + HOST + ':' + PORT);
        console.log('Sending handshake...')
        client.write('JsonProtocol');
    });

    client.on('data', function (data) {
        var message = parse(data);

        //If protocol open message
        if (message == 'Welcome to JsonProtocol'){
            console.log('Handshake completed!')

	    //Tell the Serilog sink that we want messages for Warning and higher
            var b = prepare("{'C':'log', 'T':'set_logeventlevel','D':'\"Warning\"'}");

            //Send prepared buffer
            client.write(b);
        }
        else {
	    try{
                var json = JSON.parse(message);
                if(json.T == 'logevent'){
                    console.log(JSON.parse(json.D).RenderedMessage);
	        }
	    }catch(ex){
            }
        }
    });

    //Ugly hack to get the message, we should look for endbyte here
    var parse = function(d){
        var s = d.toString();
        return s.substring(1,s.length-1);
    }

    //Wrap the message in start/end bytes
    var prepare = function(json){
        var buf = new Buffer(json.length + 2);
        buf[0] = 0x00;
        buf.write(json,1);
        buf[json.length+1] = 0xff;
        return buf;
    }

#### Change the LogEventLevel

By changing the level on the connection each connected client can decide what level to get from Serilog. Replace the "Warning" part below with Error, Fatal, Information etc to get another LogEventLevel for this client.
    
    var b = prepare("{'C':'log', 'T':'set_logeventlevel','D':'\"Warning\"'}");
    
