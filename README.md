
<h3 align="center">
  <img alt="Hosta" src="img/project-banner.png" width=500px />
</h3>

<h3 align="center">
  Take back control of your data
</h3>

<div align="center" margin="50px">
  Hosta is an open source, decentralized social media which focuses on privacy, security, and real-world relationships.
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
  <a href="https://github.com/mileswatson/Hosta/wiki/Documentation-📃">Docs</a>
  <span> · </span>
  <a href="https://github.com/mileswatson/Hosta/discussions/21">Feature Requests</a>
  <span> · </span>
  <a href="https://github.com/mileswatson/Hosta/issues">Bug Reports</a>
  <span> · </span>
  <a href="https://github.com/mileswatson/Hosta/discussions/20">Support</a>
</h3>

<h1></h1>

### What's the problem?

Modern social media platforms rely on *personalised advertising*. They are designed with two purposes:

1. 📋 Collect and analyse as much user-data as possible
2. ⌚ Keep the user engaged as long as possible

This leads to a *time-consuming* and *stressful* user experience that is fraught with relentless notifications, addictive recommendation algorithms, dangerous echo-chambers, and harmful fixations on numbers (likes, followers etc).

### How is Hosta different?

Hosta is a different type of social media to the ones you usually install on your phone. Defining features of the Hosta network:

 - Users are in **direct control** of their data
 - All communications are **end-to-end encrypted**
 - There is **no central server** that controls the network

All of this makes Hosta ideal for private, secure, and censorship resistant communications.

Furthermore, Hosta is not incentivised to maximise user attention - there are no intrusive advertisements, greedy shareholders, or creepy trackers.

### What do I need to get started?

Each user should have an always-on device (called a *node*) to host their content on. The node controls all of the user's data - it stores posts and comments, and controls who can see them. For beginners, a **Raspberry Pi + Ubuntu** is recommended.

They can use a *client* program to view the posts, comments, and profiles of users on the network. At the moment, only a **Windows 10** client is in the works - however, more platforms may be supported in the future.

# Run 🏃

## Node

This section will guide you through the process of running a Hosta node. It is recommended to use a Raspberry Pi running Ubuntu.

### 1. Install Dependencies

 - Linux: Install [Docker Engine](https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-on-ubuntu-20-04) and [Docker Compose](https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-compose-on-ubuntu-20-04#prerequisites)
 - Windows / MacOS: Install [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 2. Download the configuration folder

Download the latest `node.zip` file, unzip it to a directory of your choosing, and `cd` into the `node` folder. Running `dir` (Windows) or `ls -a` (Linux) should show three files in the current directory:
 - `.env`
 - `docker-compose.yml`
 - `README.md`

### 3. Run

Run the following command:

`docker-compose up`

You can stop the service with `Ctrl+C`.

### 4. Update

Update the node at any time by running `docker pull mileswatson/hosta:node` and then restarting the container.

## Client

This section will guide you through the process of running a Hosta node. Currently, the client can only be run on Windows.

### 1. Download the executable

Download the latest `client.zip` file, and unzip it to a directory of your choosing.

### 2. Run

Run the executable through a terminal, or by double clicking it in the file explorer.
If running through the terminal, the default save folder can be provided with `./ClientWPF.exe PATH_TO_DATA_FOLDER`.

In order to connect the client to your node, you must select a folder that has a `client.identity` file identical to that of the `node.identity`.
You can do this by copying the `node.identity` file from the node folder over to the client folder (perhaps using a USB), before renaming it to `client.identity`).

You must also enter the IP address of your node - if running on the same computer, you can leave this as `127.0.0.1`.

### 3. Update

Currently, the client program must be updated manually - replace the current executable with the executable from the latest `client.zip` release.
