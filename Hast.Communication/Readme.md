# Hastlayer - Communication Readme



Component dealing with the communication between the host and the hardware implementation.


## Using Ethernet

Ethernet is set as the default communication channel so Hastlayer will use it if you don't change anything.

For Hastlayer to be able to communicate with connected devices they should be on the same network as the host PC (or on one of the networks the host PC is connected to). You can achieve this by connecting the devices to a relevant router or switch, or by directly connecting them to the host (peer to peer). If doing the latter make sure to also run a DHCP server for the given network adapters otherwise the devices won't be able to obtain an IP address and won't be reachable.

You can use [Open DHCP Server](http://dhcpserver.sourceforge.net/) as following:

1. Set up at least one network adapter that has DHCP disabled, i.e. has statically configured IP and subnet mask (you can set an IP and subnet mask for the adapter manually: you can e.g. use 192.168.10.1/255.255.255.0 as the IP/subnet mask, leaving everything else empty).
2. Install Open DHCP Server; don't install as service (you won't need that).
3. Locate the `DHCPRange=192.168.0.1-192.168.0.254` line in the OpenDHCPServer.ini file in the installation directory. This is a default config for the range of IPs available to be handed out. Unless you set 192.168.0.0 as the NICs IP this won't work.
4. Change the line to reflect an IP range suitable for your NIC's subnet. To follow the exampe you could use `DHCPRange=192.168.10.2-192.168.10.254`
5. Run RunStandAlone.bat to start the DCHP server.

Also tried [Tftpd32](http://tftpd32.jounin.net/) (needs manual configuration for the given NICs), [Tiny DHCP Server](http://softcab.com/dhcp-server/index.php) (also needs manual configuration), [DHCP Server for Windows](http://www.dhcpserver.de/) (worked sporadically), [haneWIN DHCP Server](http://www.hanewin.net/dhcp-e.htm) (30 days shareware).


## Using USB UART (virtual serial port)

Connect the device(s) to the host PC with an USB cable to use USB UART as the communication channel. When generating proxies for you hardware-accelarated objects use `"Serial"` as the `CommunicationChannelName` to select this communication channel.