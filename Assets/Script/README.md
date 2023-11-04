# NetLib
A simple interface to encapsulate low level networking libraries for Unity

https://github.com/Pk-c/NetLib/assets/8162241/3b0f89e8-93b5-4bfe-82d7-a638519c85f2

# Structure

Netlib contains the library content, the rest is some exemples on how to use the interface in a game

![marin](https://github.com/Pk-c/NetLib/assets/8162241/04d28913-213a-4f69-b446-d8c1e7c995c3)

# Concept

-The networkmanager provide an interface to work with different transport library ( Network for game object, Steam, Fakenet for local testing )

-At the moment the serializer provided with network for game object is used to serialize all the data ( wip )

# Usage

General usage :

-Only Server can communicate with different clients.

-Network manager send and recieve all the datas.

-Other systems can subscribe to specific NetPacket, and will be notified upon receive.

-On server side NetPacket can be registered for "propagation" meaning that upon recieving a certain type of NetPacket the server will transmit the data to all connected clients

-Callback to network are available in network manager to decide what do to upon action such as "New client joined" or "A client disconnected".

Please refer to NetworkManager.cs to see an exemple of the interface implementation.





