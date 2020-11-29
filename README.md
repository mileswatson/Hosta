![](img/project-banner.png)

## Crypto

Using AES, ECDH, ECDsa.

## DNS

Will try to make it as far as possible:
1. Manual IP entry
2. Full mesh
3. Recursive lookup via adjacent nodes
4. a) DHT, perhaps similar to Kademlia's implementation; b) blockchain

## Architecture

This is the proposed structure of the network. Each client has a corresponding node, which hosts their content and contains the public key. The nodes update each other of their location, which means that the client only has to know the location of one node on the network to be able to connect.

![](img/network_structure.png)

The solution is divided into 4 main projects - a client program, a node program, a database, and a core library (used by both the client and the node).

![](img/namespace_structure.png)

Here is the progress on the core library so far:

![](img/hosta_structure.png)
