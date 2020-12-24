
<h3 align="center">
  <img alt="Hosta" src="img/project-banner.png" width=500px />
</h3>

<h3 align="center">
  Take back control of your data
</h3>

<div align="center" margin="50px">
  Hosta is an open source, PoC decentralized social media that focuses on privacy, security, and real-world relationships.
</div>

<p align="center" style="font-size:125%">
 📝 🔒 👪
</p>

<h1></h1><br/>

<p align="center">
  <a href="https://github.com/mileswatson/Hosta/blob/master/LICENSE"><img alt="GitHub license" src="https://img.shields.io/github/license/mileswatson/Hosta?color=blue"></a>
  <a href="https://github.com/mileswatson/Hosta/stargazers"><img alt="GitHub stars" src="https://img.shields.io/github/stars/mileswatson/Hosta?color=gold"></a>
  <a href="https://github.com/mileswatson/Hosta/issues"><img alt="GitHub issues" src="https://img.shields.io/github/issues/mileswatson/Hosta"></a>
  <img alt=".NET 5" src="https://img.shields.io/static/v1?label=&message=%2ENET%205&color=5C2D91">
  <img alt="SQLite" src="https://img.shields.io/static/v1?label=&message=SQLite&color=003B57&logo=SQLite">
  <img alt="Windows" src="https://img.shields.io/static/v1?label=&message=Windows&color=0078D6&logo=Windows">
  <img alt="Linux" src="https://img.shields.io/static/v1?label=&message=linux&color=FCC624&logo=Linux&logoColor=black">
</p>

<h3 align="center">
  <a href="https://github.com/mileswatson/Hosta/tree/master/src">Source</a>
  <span> · </span>
  <a href="https://github.com/mileswatson/Hosta/wiki">Docs</a>
  <span> · </span>
  <a href="https://github.com/mileswatson/Hosta/discussions/21">Feature request</a>
  <span> · </span>
  <a href="https://github.com/mileswatson/Hosta/issues">Report a bug</a>
  <span> · </span>
  <a href="https://github.com/mileswatson/Hosta/discussions/20">Support</a>
</h3>

<h1></h1><br/>

### What's the problem?

Modern social media platforms rely on *personalised advertising*. They are designed with two purposes:

1. 📋 Collect and analyse as much user-data as possible
2. ⌚ Keep the user engaged as long as possible

This leads to a time-consuming and stressful user experience that is fraught with relentless notifications, addictive recommendation algorithms, dangerous echo-chambers, and harmful fixations on numbers (likes, followers etc).

### How is Hosta different?

Hosta is a different type of social media to the ones you usually install on your phone. By using the Hosta network:

 - Users are in direct control of their data
 - All communications are end-to-end encrypted
 - There is no central server that controls the network

All of this makes Hosta ideal for private, secure, and censorship resistant communications.

Furthermore, Hosta is not incentivised to maximise user attention - there are no intrusive advertisements, no greedy shareholders, and no creepy trackers.

### What do I need to get started?

Each user should have an always-on-device (called a *node*) to host their content on. The node controls all of the user's data - it stores posts and comments, and controls who can see them.

They can use a *client* program to access the network - clients allow the user to view the posts, comments, and profiles of users on the network.

To get started, it is recommended to use a Raspberry Pi + Ubuntu to run the node, and a desktop computer + Windows to run the client.

# The Technical Section

### DNS

Will try to make it as far as possible:
1. Manual IP entry
2. Full mesh
3. Recursive lookup via adjacent nodes
4. a) DHT, perhaps similar to Kademlia's implementation; b) blockchain

### Architecture

This is the proposed structure of the network. Each client has a corresponding node, which hosts their content and contains the public key. The nodes update each other of their location, which means that the client only has to know the location of one node on the network to be able to connect.

![](img/network_structure.png)

The solution is divided into 4 main projects - a client program, a node program, a database, and a core library (used by both the client and the node).

![](img/namespace_structure.png)

Here is the progress on the core library so far:

![](img/hosta_structure.png)