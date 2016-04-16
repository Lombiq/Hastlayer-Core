# Hastlayer - Communication Readme



Component dealing with the communication between the host and the hardware implementation.


## Using Ethernet

Ethernet is set as the default communication channel so Hastlayer will use it if you don't change anything.

For Hastlayer to be able to communicate with connected devices they should be on the same network as the host PC (or on one of the networks the host PC is connected to). You can achieve this by connecting the devices to a relevant router or switch, or by directly connecting them to the host (peer to peer). If doing the latter make sure to also run a DHCP server for the given network adapters (like [this simple to use one](http://www.dhcpserver.de/)) otherwise the devices won't be able to obtain an IP address and won't be reachable.

When using "DHCP Server for Windows" then first connect the device(s) and then start the DHCP server for the network adapter(s). Use its wizard to set up the server; make sure to set it up for a network adapter that has DHCP disabled (you can set an IP and subnet mask for the adapter manually; you can e.g. use 192.168.10.1/255.255.255.0 as the IP/subnet mask, leaving everything else empty). You can leave everything on default in the wizard but don't forget to write out the INI file. Also make sure to configure the firewall exception. (Although it might not be obvious the server will be only started when not installed as a service if you click "Continue as tray app".)


## Using USB UART (virtual serial port)

Connect the device(s) to the host PC with an USB cable to use USB UART as the communication channel. When generating proxies for you hardware-accelarated objects use `"Serial"` as the `CommunicationChannelName` to select this communication channel.